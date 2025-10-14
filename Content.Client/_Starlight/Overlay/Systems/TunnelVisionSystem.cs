using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Mech;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Content.Shared.Flash.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory.Events;
using Content.Shared.Starlight.Overlay;
using Content.Shared._Starlight.Actions.Components;
using Content.Shared._Starlight.Actions.EntitySystems;
using Content.Shared._Starlight.Actions.Events;

namespace Content.Client._Starlight.Overlay;

public sealed class TunnelVisionSystem : SharedTunnelVisionSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly TransformSystem _xformSys = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly FlashImmunitySystem _flashImmunity = default!;

    private TunnelVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TunnelVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<TunnelVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new(_prototypeManager.Index<ShaderPrototype>("TunnelVisionShader"));
        //_overlay = new(_prototypeManager.Index<ShaderPrototype>("ModernNightVisionShader"));
    }

    private void OnPlayerAttached(Entity<TunnelVisionComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        AttemptAddVision(ent.Owner);
    }

    private void OnPlayerDetached(Entity<TunnelVisionComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        AttemptRemoveVision(ent.Owner, true);
    }

    public override void OnTunnelVisionToggle(EntityUid uid, TunnelVisionComponent comp, ToggleTunnelVisionEvent args)
    {
        base.OnTunnelVisionToggle(uid, comp, args);
        if (comp.Active)
        {
            AttemptAddVision(uid);
        }
        else
        {
            AttemptRemoveVision(uid);
        }
    }

    private void AttemptAddVision(EntityUid uid)
    {
        if (_player.LocalSession?.AttachedEntity != uid) return;

        //only add if its active
        if (!TryComp<TunnelVisionComponent>(uid, out var tunnelVision) || !tunnelVision.Active) return;

        _overlayMan.AddOverlay(_overlay);
    }

    /// <summary>
    /// Attempt to remove the overlay from the local player.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="force">Use if you need to forcefully remove the overlay no matter what. Only should be used with events that ONLY the local player can fire, like attach/detach</param>
    private void AttemptRemoveVision(EntityUid uid, bool force = false)
    {
        //ENSURE this is the local player
        if (_player.LocalSession?.AttachedEntity != uid && !force) return;

        _overlayMan.RemoveOverlay(_overlay);
    }
}
