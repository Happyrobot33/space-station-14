using Content.Shared.DoAfter;
using Content.Shared.Actions;
using Robust.Shared.Serialization;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Alien;

public abstract class SharedBrainHuggingSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;
}

public sealed partial class BrainSlugJumpActionEvent : WorldTargetActionEvent { }

public sealed partial class BrainSlugActionEvent : EntityTargetActionEvent { }

public sealed partial class DominateVictimActionEvent : EntityTargetActionEvent { }

public sealed partial class ReleaseSlugActionEvent : EntityTargetActionEvent { }

public sealed partial class TormentHostActionEvent : EntityTargetActionEvent { }

public sealed partial class AssumeControlActionEvent : EntityTargetActionEvent { }

public sealed partial class ReproduceActionEvent : EntityTargetActionEvent { }

public sealed partial class StoreActionEvent : EntityTargetActionEvent { }

public sealed partial class ReleaseControlActionEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class BrainHuggingDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class AssumeControlDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class ReproduceDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class ReleaseSlugDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class StoreDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class ReleaseDoAfterEvent : SimpleDoAfterEvent { }




