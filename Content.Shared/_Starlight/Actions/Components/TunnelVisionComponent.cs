using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Actions.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TunnelVisionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "TunnelVision";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public bool Active = false;

    //RIPPED FROM EYE CURSOR OFFSET COMPONENT
    //THIS IS MESSY BUT WHATEVER FUCK IT
    /// <summary>
    /// The amount the view will be displaced when the cursor is positioned at/beyond the max offset distance.
    /// Measured in tiles.
    /// </summary>
    [DataField]
    public float MaxOffset = 3f;

    /// <summary>
    /// The speed which the camera adjusts to new positions. 0.5f seems like a good value, but can be changed if you want very slow/instant adjustments.
    /// </summary>
    [DataField]
    public float OffsetSpeed = 0.5f;

    /// <summary>
    /// The amount the PVS should increase to account for the max offset.
    /// Should be 1/10 of MaxOffset most of the time.
    /// </summary>
    [DataField]
    public float PvsIncrease = 0.3f;

}
