using System.Numerics;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Starlight.Automation;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EntityMoverComponent : Component
{
    /* [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<TagPrototype>, KnockbackData> DoestContain = new(); */

    [DataField, AutoNetworkedField]
    public float PickupRange = 0.25f;

    [DataField, AutoNetworkedField]
    public float DropSearchRange = 0.125f;

    [DataField, AutoNetworkedField]
    public Vector2 PickupLocation = new(0, 1f);

    [DataField, AutoNetworkedField]
    public Vector2 DropLocation = new(0, -1f);

    [DataField, AutoNetworkedField]
    public bool LeaveItemIfNoSpace = true;
}
