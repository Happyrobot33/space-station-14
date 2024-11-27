using Content.Shared.Actions;
using Content.Shared.Alien;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Chemistry.Reagent;
using System.Numerics;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Alien;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBrainHuggingSystem))]
public sealed class BrainHuggingComponent : Component
{

    [ViewVariables(VVAccess.ReadWrite)] public FixedPoint2 SlugGenes = 140;

    [DataField("stolenEssenceCurrencyPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<CurrencyPrototype>))]
    public string StolenEssenceCurrencyPrototype = "StolenGenes";


    [ViewVariables] public float Accumulator = 0;

    [ViewVariables] public float AccumulatorStarveNotify = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("maxEssence")]
    public FixedPoint2 EssenceRegenCap = 75;





    [DataField("brainslugTime")]
    public TimeSpan BrainSlugTime = TimeSpan.FromSeconds(2); // !!!ALL COOLDOWNS IS LOW FOR TESTS!!!

    [DataField("assumeControlTime")]
    public TimeSpan AssumeControlTime = TimeSpan.FromSeconds(2);

    [DataField("reproduceTime")]
    public TimeSpan ReproduceTime = TimeSpan.FromSeconds(2);

    [DataField("chansePounce"), ViewVariables(VVAccess.ReadWrite)]
    public static int ChansePounce = 33;

    [DataField("brainreleaseTime")]
    public float BrainRealeseTime = 3f;


    [DataField("paralyzeTime"), ViewVariables(VVAccess.ReadWrite)]
    public float ParalyzeTime = 3f;

    [DataField("ichorChemical", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string IchorChemical = "Ichor";

    [ViewVariables(VVAccess.ReadWrite), DataField("healRate")]
    public float HealRate = 15f;

    [ViewVariables(VVAccess.ReadWrite), DataField("soundBrainSlugJump")]
    public SoundSpecifier? SoundBrainSlugJump = new SoundPathSpecifier("/Audio/Animals/brainslug_scream.ogg");

    [DataField("actionBrainSlugJump")]
    public ProtoId<EntityPrototype> ActionBrainSlugJump = "ActionBrainSlugJump"; // jump

    [DataField("actionBrainSlug")]
    public ProtoId<EntityPrototype> BrainSlugAction = "ActionBrainSlug"; // infest

    [DataField("actionDominateVictim")]
    public ProtoId<EntityPrototype> DominateVictimAction = "ActionDominateVictim"; // stun

    [DataField("actionTormentHostSlug")]
    public ProtoId<EntityPrototype> TormentHostSlugAction = "ActionTormentHostSlug"; // torment

    [DataField("actionAssumeControlSlug")]
    public ProtoId<EntityPrototype> AssumeControlAction = "ActionAssumeControlSlug"; // assume control

    [DataField("actionReproduceSlug")]
    public ProtoId<EntityPrototype> ReproduceAction = "ActionReproduceSlug"; // reproduce

    [DataField("actionReleaseSlug")]
    public ProtoId<EntityPrototype> ReleaseSlugAction = "ActionReleaseSlug"; // release

    [DataField("actionStoreSlug")]
    public ProtoId<EntityPrototype> StoreSlugAction = "ActionStoreSlug"; // ui store



    [ViewVariables(VVAccess.ReadWrite), DataField("soundBrainHugging")]
    public SoundSpecifier? SoundBrainHugging = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };
}
