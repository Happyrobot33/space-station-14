using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared._Starlight.Actions.Components;
using Content.Shared._Starlight.Actions.Events;
using Content.Shared.Atmos.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared.Stunnable;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;

namespace Content.Shared._Starlight.Actions.EntitySystems;

//idea taked from VigersRay
public sealed class TunnelVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedChargesSystem _chargesSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TunnelVisionComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<TunnelVisionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TunnelVisionComponent, ToggleTunnelVisionEvent>(OnTunnelVisionToggle);
    }

    private void OnStartup(EntityUid uid, TunnelVisionComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.ActionEntity, component.Action);

        Dirty(uid, component);
    }

    private void OnShutdown(EntityUid uid, TunnelVisionComponent component, ComponentShutdown args)
    {
        if (Deleted(uid) || component.ActionEntity is null)
            return;

        _action.RemoveAction((uid, null), component.ActionEntity);
    }

    private void OnTunnelVisionToggle(EntityUid uid, TunnelVisionComponent comp, ToggleTunnelVisionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        comp.IsActive = !comp.IsActive;
    }
}
