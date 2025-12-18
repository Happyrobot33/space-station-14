using Content.Shared.Damage;
using Robust.Shared.Physics.Collision.Shapes;

namespace Content.Shared.Starlight.EntityEffects.Components;

[RegisterComponent]
public sealed partial class DissolvableComponent : Component
{
    # region Resisting
    [DataField]
    public bool Resisting;
    
    [DataField]
    public TimeSpan? ResistingStartedOn = null;
    
    [DataField]
    public TimeSpan ResistingTime = TimeSpan.FromSeconds(2);
    # endregion
    
    # region Update
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public TimeSpan LastTimeUpdated = TimeSpan.Zero;
    # endregion
    
    [DataField]
    public EntityUid? Effect = null;
    [DataField]
    public bool OnDissolve;
    [DataField]
    public float DissolveStacks;
    [DataField]
    public float MaximumDissolveStacks = 10f;
    [DataField]
    public float MinimumDissolveStacks = -10f;
    [DataField]
    public string DissolvableFixtureID = "dissolvable";
    [DataField]
    public float MinDissolveTemperature = 373.15f;
    [DataField]
    public bool CanResistDissolve { get; private set; } = false;

    [DataField(required: true)]
    public DamageSpecifier Damage = new(); // Empty by default, we don't want any funny NREs.

    /// <summary>
    ///     Used for the fixture created to handle passing dissolvestacks when two dissolvable objects collide.
    /// </summary>
    [DataField]
    public IPhysShape DissolvableCollisionShape = new PhysShapeCircle(0.35f);
    [DataField]
    public bool AlwaysCombustible = false;
    [DataField]
    public bool CanExtinguish = true;
    [DataField]
    public float DissolveStacksOnIgnite = 2.0f;

    /// <summary>
    /// Determines how quickly the object will fade out. With positive values, the object will flare up instead of going out.
    /// </summary>
    [DataField]
    public float DissolveStacksFade = -0.1f;
}
