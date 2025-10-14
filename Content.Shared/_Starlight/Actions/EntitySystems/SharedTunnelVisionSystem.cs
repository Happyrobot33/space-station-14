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
using Content.Shared.Movement.Components;
using Robust.Shared.Network;

namespace Content.Shared._Starlight.Actions.EntitySystems;

public abstract class SharedTunnelVisionSystem : EntitySystem
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
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TunnelVisionComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<TunnelVisionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TunnelVisionComponent, ToggleTunnelVisionEvent>(OnTunnelVisionToggle);
    }

    public void OnStartup(Entity<TunnelVisionComponent> ent, ref MapInitEvent args)
    {
        _action.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);

        Dirty(ent, ent.Comp);
    }

    public void OnShutdown(Entity<TunnelVisionComponent> ent, ref ComponentShutdown args)
    {
        if (Deleted(ent) || ent.Comp.ActionEntity is null)
            return;

        _action.RemoveAction((ent, null), ent.Comp.ActionEntity);
    }

    public virtual void OnTunnelVisionToggle(EntityUid uid, TunnelVisionComponent comp, ToggleTunnelVisionEvent args)
    {
        comp.Active = !comp.Active;
        Dirty(uid, comp);

        //this is semi fucked and causes jitter when it happens initially. idfk good enough for now

        if (comp.Active)
        {
            //add the eyecursoroffset component to the entity
            var eyeoffset = AddComp<EyeCursorOffsetComponent>(uid);

            //set it up
            eyeoffset.MaxOffset = comp.MaxOffset;
            eyeoffset.OffsetSpeed = comp.OffsetSpeed;
            eyeoffset.PvsIncrease = comp.PvsIncrease;

            //dirty it
            Dirty(uid, eyeoffset);
        }
        else
        {
            //remove the eyecursoroffset component from the entity
            RemComp<EyeCursorOffsetComponent>(uid);
        }
    }
}
