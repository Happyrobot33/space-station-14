
using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Server;

namespace Content.Server.Starlight.Watchdogs;
public sealed class EventSchedulerWatchdog : EntitySystem, IPostInjectInit
{
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IBaseServer _server = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    private ISawmill _sawmill = default!;

    [ViewVariables]
    private bool _restartOnRoundEnd = false;

    Dictionary<EntityUid, float> timers = new();

    public void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    public void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        if (_restartOnRoundEnd)
        {
            _sawmill.Info($"Shutting down via {nameof(EventSchedulerWatchdog)}!");
            var reason = $"Server is restarting due to {nameof(EventSchedulerWatchdog)}.";
            _server.Shutdown(reason);
        }
    }

    private void OnRestartNeedDetected()
    {
        _chatManager.DispatchServerAnnouncement("An issue has been detected with the event scheduler. The server will restart automatically at the end of this round.");
        _restartOnRoundEnd = true;
        _sawmill.Info($"{nameof(EventSchedulerWatchdog)} detected a restart need. The server will restart at the end of this round.");
    }

    public override void Update(float frametime)
    {
        if (_restartOnRoundEnd) return;
        
        //everytime we check this, we want to compare with the previous update tick

        //create a fresh list of timers
        Dictionary<EntityUid, float> timersNew = new();

        //first see if we even have any game rules active
        var query = EntityQueryEnumerator<BasicStationEventSchedulerComponent>();
        while (query.MoveNext(out var uid, out var rule))
        {
            //add all the timers
            timersNew.Add(uid, rule.TimeUntilNextEvent);
        }

        var query2 = EntityQueryEnumerator<RampingStationEventSchedulerComponent>();
        while (query2.MoveNext(out var uid, out var rule))
        {
            //add all the timers
            timersNew.Add(uid, rule.TimeUntilNextEvent);
        }

        //if the two dictionarys match EXACTLY, then we know that the timers are not changing
        if (timersNew.Count > 0)
        {
            if (timers.SequenceEqual(timersNew))
            {
                OnRestartNeedDetected();
            }
            else
            {
                //if they are not equal, then we know that the timers are changing
                //so we can just set the timers to the new ones
                timers = timersNew;
            }
        }
    }

    public void PostInject()
    {
        _sawmill = _logManager.GetSawmill(nameof(EventSchedulerWatchdog));
    }
}
