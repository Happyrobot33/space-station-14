using Content.Server.Database;
using Content.Shared.Emoting;
using Content.Shared.Speech;

namespace Content.Server.Starlight.Administration.Systems;
public sealed partial class AdminVerbSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<SpeakAttemptEvent>(OnSpeakAttempt);
        //SubscribeLocalEvent<EmoteAttemptEvent>(OnEmoteAttempt);
    }

    //watch for chat messages
    private void OnSpeakAttempt(SpeakAttemptEvent args)
    {
        var task = _db.
    }
}
