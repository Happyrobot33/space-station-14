using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Emoting;
using Content.Shared.Speech;

namespace Content.Server.Starlight.Administration.Systems;
public sealed partial class AutoModerationSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<SpeakAttemptEvent>(OnSpeakAttempt);
        //SubscribeLocalEvent<EmoteAttemptEvent>(OnEmoteAttempt);
    }

    //watch for chat messages
    private async void OnSpeakAttempt(SpeakAttemptEvent args)
    {
        //make a generic testing rule
        var rule = new AutoModRule()
        {
            Message = "test",
        };
        await _db.AddAutoModRule(rule);
    }
}

//automod rule class

