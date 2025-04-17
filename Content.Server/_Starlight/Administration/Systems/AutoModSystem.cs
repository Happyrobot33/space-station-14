using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Chat.V2.Repository;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Content.Shared.Chat.V2;
using Content.Shared.Chat.V2.Repository;
using Content.Shared.Emoting;
using Content.Shared.Speech;
using Robust.Server.Player;

namespace Content.Server.Starlight.Chat.Systems;
public sealed partial class AutoModSystem : SharedChatSystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IEntitySystemManager _manager = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    private readonly ISawmill _automodLog = Logger.GetSawmill("automod");

    //cache the rules list
    private List<AutoModRule> _rules = new();
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChatAttemptEvent>(OnChatAttempt);

        //TODO: Make our cache update automatically somehow. For now this works
        //but this will need to be fixed for runtime changes
        _automodLog.Info($"AutoModSystem initialized. Updating cache.");
        UpdateCache();
    }

    //task to update cache
    public async Task UpdateCache()
    {
        //get the rules from the database
        _rules = await _db.GetAutoModRules();
    }

    //watch for chat messages
    private void OnChatAttempt(ChatAttemptEvent args)
    {
        //set the message to nothing
        string message = args.Message;

        //_automodLog.Info($"Checking message: {message} against {_rules.Count} rules.");
        //check if the message contains any of the rules
        foreach (var rule in _rules)
        {
            //check if the rule is even enabled
            if (!rule.Enabled)
                continue;

            //convert the rule to a regex
            var regex = new Regex(rule.Regex);

            //_automodLog.Info($"Checking against rule: {rule.Regex}");

            //check for match
            if (regex.IsMatch(message))
            {
                //_automodLog.Info($"Rule matched: {rule.Regex}");
                if (rule.CancelSpeech)
                {
                    //_automodLog.Info($"Rule cancelled speech: {rule.Regex}");
                    //cancel the speech if the rule is set to do so
                    args.Cancel();
                }

                //if there is a message defined, show it in a popup
                if (!string.IsNullOrEmpty(rule.Message))
                {
                    //send the message to the user
                    var chatSystem = _manager.GetEntitySystem<ChatSystem>();

                    //check if they have a entity
                    _chat.ChatMessageToOne(ChatChannel.Server,
                        rule.Message,
                        rule.Message,
                        EntityUid.Invalid,
                        false,
                        args.Sender.Channel);
                }
            }
        }
    }
}

//automod rule class

