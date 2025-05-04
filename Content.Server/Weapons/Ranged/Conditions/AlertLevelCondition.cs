using Content.Server.AlertLevel;
using Content.Shared.Station.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Robust.Shared.Serialization;
using Robust.Shared.Map;

namespace Content.Server.Weapons.Ranged.Conditions;

public sealed partial class AlertLevelCondition : FireModeCondition
{
    [DataField(required: true)]
    public List<string> AlertLevels;

    public override bool Condition(FireModeConditionConditionArgs args)
    {
        var entityManager = args.EntityManager;

        var alertSystem = entityManager.System<AlertLevelSystem>();

        if (!entityManager.TryGetComponent<TransformComponent>(args.Shooter, out var transformComp))
            return false;

        IMapManager _mapManager = default!;
        IoCManager.Resolve<IMapManager>(ref _mapManager);

        //get all grids on the map the entity is on
        var grids = _mapManager.GetAllGrids(transformComp.MapID);

        foreach (var grid in grids)
        {
            if (entityManager.TryGetComponent<StationMemberComponent>(grid, out var stationMember) &&
                entityManager.TryGetComponent<AlertLevelComponent>(stationMember.Station, out var alertLevelComp))
            {
                var currentAlertLevel = alertSystem.GetLevel(stationMember.Station, alertLevelComp);
                if (AlertLevels.Contains(currentAlertLevel))
                {
                    return true; //return early
                }
                //return AlertLevels.Contains(currentAlertLevel);
            }
        }

        return false;
    }
}
