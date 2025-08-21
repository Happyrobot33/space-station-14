using System.Numerics;
using Content.Shared._Starlight.Weapon.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Slippery;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;

//linq
using System.Linq;
using Robust.Shared.Containers;
using Content.Shared.Item;
using Content.Shared.Materials;

namespace Content.Shared.Starlight.Automation;

public sealed partial class SharedEntityMoverSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;
    public override void Initialize()
    {

    }

    //update
    public override void Update(float frameTime)
    {
        //query all entities with EntityMoverComponent
        var query = EntityQueryEnumerator<EntityMoverComponent>();
        while (query.MoveNext(out var ent, out var mover))
        {
            //Log.Info("Processing EntityMoverComponent for entity: " + ent);
            //get entity's transform
            var transform = Transform(ent);

            //get entities one tile above us
            var above = transform.Coordinates.Offset(mover.PickupLocation);
            var below = transform.Coordinates.Offset(mover.DropLocation);

            var ents = _lookupSystem.GetEntitiesInRange(above, mover.PickupRange, LookupFlags.Dynamic);

            var dropLocationEntities = _lookupSystem.GetEntitiesInRange(below, mover.DropSearchRange);

            //teleport the entities to the below
            foreach (var other in ents)
            {
                //Log.Info("Checking entity: " + other + " for EntityMoverComponent");
                if (other == ent || !HasComp<TransformComponent>(other))
                    continue;

                var otherTransform = Transform(other);

                //make sure its a item lol
                if (!HasComp<ItemComponent>(other))
                    continue;

                //make sure its on the same map
                if (otherTransform.MapID != transform.MapID)
                    continue;

                //make sure not anchored
                if (otherTransform.Anchored)
                {
                    //Log.Info("Entity: " + other + " is anchored, skipping.");
                    continue;
                }

                //now check if the drop off location has storage that will accept the entity
                var canDrop = true;
                foreach (var dropEntity in dropLocationEntities)
                {
                    //check if the material storage can accept the entity
                    if (_materialStorage.TryInsertMaterialEntity(ent, other, dropEntity))
                    {
                        canDrop = false;
                        break;
                    }

                    //generic container loading. Disabled for now, has a shitton of edge cases to be resolved
                    /* //we want to try to insert into containers, BUT we cant do that if there is material storage
                    if (!HasComp<MaterialStorageComponent>(dropEntity) && TryComp<ContainerManagerComponent>(dropEntity, out var containerManager))
                    {
                        foreach (var container in containerManager.Containers)
                        {
                            //check if the container can accept the entity
                            if (_containerSystem.InsertOrDrop(other, container.Value))
                            {
                                canDrop = false;
                                break;
                            }
                        } 
                    } */
                }

                //move the entity to below
                //Log.Info("Moving entity: " + other + " to below coordinates: " + below);
                if (canDrop && !mover.LeaveItemIfNoSpace)
                {
                    _transform.SetCoordinates(other, below);
                }
            }
        }
    }
}
