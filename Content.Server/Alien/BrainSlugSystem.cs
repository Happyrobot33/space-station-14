using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Medical;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Store.Systems;

using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Alien;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Revenant;
using Content.Shared.Revenant.Components;
using Content.Shared.Speech.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Store.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;

using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Alien
{
    public sealed class BrainSlugSystem : SharedBrainHuggingSystem
    {
        [Dependency] private SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly VomitSystem _vomit = default!;
        [Dependency] private readonly StoreSystem _store = default!;
        [Dependency] private readonly AlertsSystem _alerts = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;


        public override void Initialize()
        {
            SubscribeLocalEvent<BrainHuggingComponent, ComponentStartup>(OnStartup);


            SubscribeLocalEvent<BrainHuggingComponent, BrainSlugJumpActionEvent>(OnJumpBrainSlug);
            SubscribeLocalEvent<BrainHuggingComponent, ThrowDoHitEvent>(OnBrainSlugDoHit);

            SubscribeLocalEvent<BrainHuggingComponent, BrainSlugActionEvent>(OnBrainSlugAction);
            SubscribeLocalEvent<BrainHuggingComponent, BrainHuggingDoAfterEvent>(BrainHuggingOnDoAfter);

            SubscribeLocalEvent<BrainHuggingComponent, DominateVictimActionEvent>(OnDominateVictimAction);

            SubscribeLocalEvent<BrainHuggingComponent, TormentHostActionEvent>(OnTormentHostAction);

            SubscribeLocalEvent<BrainHuggingComponent, AssumeControlActionEvent>(OnAssumeControlAction);
            SubscribeLocalEvent<BrainHuggingComponent, AssumeControlDoAfterEvent>(AssumeControlDoAfter);

            SubscribeLocalEvent<BrainHuggingComponent, ReproduceActionEvent>(OnReproduceAction);
            SubscribeLocalEvent<BrainHuggingComponent, ReproduceDoAfterEvent>(ReproduceDoAfter);

            SubscribeLocalEvent<BrainHuggingComponent, StoreActionEvent>(OnShop);
            SubscribeLocalEvent<BrainHuggingComponent, ExaminedEvent>(OnExamine);

            SubscribeLocalEvent<SlugInsideComponent, ReleaseControlActionEvent>(OnReleaseControlAction);

            SubscribeLocalEvent<BrainHuggingComponent, ReleaseSlugActionEvent>(OnReleaseSlugAction);
            SubscribeLocalEvent<BrainSlugComponent, ReleaseSlugDoAfterEvent>(ReleaseSlugDoAfter);
        }


        protected void OnStartup(EntityUid uid, BrainHuggingComponent component, ComponentStartup args)
        {
            ChangeSlugGenesAmount(uid, 0, component);
            
            foreach (var action in component.BaseActions)
                UpdateAbilities(uid, component, action, true);
        }


        private bool ChangeSlugGenesAmount(EntityUid uid, FixedPoint2 amount, BrainHuggingComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return false;

            component.SlugGenes += amount;

            if (TryComp<StoreComponent>(uid, out var store))
                _store.UpdateUserInterface(uid, uid, store);

            //_alerts.ShowAlert(uid, AlertType.GenesPoint, (short) Math.Clamp(Math.Round(component.SlugGenes.Float() / 10f), 0, 16));

            return true;
        }

        private void OnExamine(EntityUid uid, BrainHuggingComponent component, ExaminedEvent args)
        {
            if (args.Examiner == args.Examined)
                args.PushMarkup(Loc.GetString("revenant-essence-amount",
                    ("current", component.SlugGenes.Int()), ("max", component.EssenceRegenCap.Int())));
        }

        private void OnShop(EntityUid uid, BrainHuggingComponent component, StoreActionEvent args)
        {
            if (!TryComp<StoreComponent>(uid, out var store))
                return;
            _store.ToggleUi(uid, uid, store);
        }

        private void OnBrainSlugDoHit(EntityUid uid, BrainHuggingComponent component, ThrowDoHitEvent args)
        {
            if (!TryComp(uid, out BrainSlugComponent? defcomp) || !HasComp<HumanoidAppearanceComponent>(args.Target))
                return;

            var host = args.Target;
            
            if (defcomp.GuardianContainer == _container.EnsureContainer<ContainerSlot>(host,"GuardianContainer"))
                return;
            
            defcomp.GuardianContainer = _container.EnsureContainer<ContainerSlot>(host,"GuardianContainer");

            _container.Insert(uid, defcomp.GuardianContainer);

            defcomp.EquipedOn = args.Target;

            _popup.PopupEntity(Loc.GetString("Something jumped on you!"), args.Target, args.Target, PopupType.LargeCaution);
        }



        private void OnJumpBrainSlug(EntityUid uid, BrainHuggingComponent component, BrainSlugJumpActionEvent args)
        {
            if (args.Handled)
                return;
            
            if (TryComp(uid, out BrainSlugComponent? defcomp) && defcomp.GuardianContainer != null)
                _container.Remove(uid, defcomp.GuardianContainer);

            args.Handled = true;
            var xform = Transform(uid);
            var mapCoords = args.Target.ToMap(EntityManager, _transform);
            Logger.Info(xform.MapPosition.ToString());
            Logger.Info(mapCoords.ToString());
            var direction = mapCoords.Position - xform.MapPosition.Position;
            Logger.Info(direction.ToString());

            _throwing.TryThrow(uid, direction, 7F, uid, 10F);
            if (component.SoundBrainSlugJump != null)
                _audioSystem.PlayPvs(component.SoundBrainSlugJump, uid, component.SoundBrainSlugJump.Params);
        }


        private void OnBrainSlugAction(EntityUid uid, BrainHuggingComponent component, BrainSlugActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            
            if (hugcomp == null)
                return;

            if (TryComp(target, out MobStateComponent? targetState))
            {

                switch (targetState.CurrentState)
                {
                    case MobState.Alive:
                    case MobState.Critical:
                        _popup.PopupEntity(Loc.GetString("Slug is sucking on your brain!"), uid, uid);
                        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, hugcomp.BrainSlugTime, new BrainHuggingDoAfterEvent(), uid, target: target, used: uid)
                        {
                            BreakOnMove = true,
                        });
                        break;
                    default:
                        _popup.PopupEntity(Loc.GetString("The target is dead!"), uid, uid);
                        break;
                }

                return;
            }
        }


        private void BrainHuggingOnDoAfter(EntityUid uid, BrainHuggingComponent component, BrainHuggingDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;

            else if (args.Args.Target != null)
            {
                var target = args.Target;
                if (target == null)
                    return;
                
                if (!TryComp(uid, out BrainSlugComponent? defcomp)  || !HasComp<HumanoidAppearanceComponent>(target) || defcomp.GuardianContainer != null && defcomp.GuardianContainer.Contains(uid))
                    return;
                
                defcomp.GuardianContainer = _container.EnsureContainer<ContainerSlot>(target.Value, "GuardianContainer");
                
                _container.Insert(uid, defcomp.GuardianContainer);
                
                defcomp.EquipedOn = target.Value;
                
                UpdateAbilities(uid, component, component.DominateVictimAction, true);
                
                UpdateAbilities(uid, component, component.ReleaseSlugAction, true);
                
                UpdateAbilities(uid, component, component.TormentHostSlugAction, true);
                
                UpdateAbilities(uid, component, component.AssumeControlAction, true);

                UpdateAbilities(uid, component, component.ReproduceAction, true);
                
                UpdateAbilities(uid, component, component.StoreSlugAction, true);
                
                
                foreach (var action in component.BaseActions)
                    UpdateAbilities(uid, component, action, false);

                if (TryComp(target, out MobStateComponent? mobState))
                {
                    if (mobState.CurrentState == MobState.Critical)
                    {
                        _popup.PopupEntity(Loc.GetString("Brain Slug is trying save your body!"), target.Value, target.Value);
                        var ichorInjection = new Solution(component.IchorChemical, component.HealRate);
                        ichorInjection.ScaleSolution(5.0f);
                        _bloodstreamSystem.TryAddToChemicals(target.Value, ichorInjection);
                    }
                }
            }



            _audioSystem.PlayPvs(component.SoundBrainHugging, uid);
        }

        private void OnDominateVictimAction(EntityUid uid, BrainHuggingComponent comp, DominateVictimActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            
            if (hugcomp == null)
                return;


            _popup.PopupEntity(Loc.GetString("Your limbs are stiff!"), uid, uid);
            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(hugcomp.ParalyzeTime), true);
        }


        private void OnTormentHostAction(EntityUid uid, BrainHuggingComponent comp, TormentHostActionEvent args)
        {
            var target = args.Target;
            if (TryComp(target, out VocalComponent? scream))
            {
                if (scream != null)
                {
                    _popup.PopupEntity(Loc.GetString("YOU FEEL HELLISH PAIN, YOU WILL BE TURNED INSIDE OUT AND ROLLED ON THE FLOOR!"), target, target, PopupType.LargeCaution);
                    _chat.TryEmoteWithChat(target, scream.ScreamId);
                }
            }
        }

        private void OnAssumeControlAction(EntityUid uid, BrainHuggingComponent component, AssumeControlActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            
            if (hugcomp == null)
                return;

            _popup.PopupEntity(Loc.GetString("You feel like a slug inside your head wants to take over your nervous system!"), target, target, PopupType.LargeCaution);
            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, hugcomp.AssumeControlTime, new AssumeControlDoAfterEvent(), uid, target: target, used: uid)
            {
                BreakOnMove = true,
            });

        }


        private void AssumeControlDoAfter(EntityUid uid, BrainHuggingComponent comp, AssumeControlDoAfterEvent args)
        {
            var target = args.Target;
            if (target == null)
                return;

            if (TryComp(target, out SlugInsideComponent? targetcomp))
            {
                targetcomp.Slug = uid;
                
                if (TryComp(target, out SlugInsideComponent? ttt))
                    if (_entityManager.TryGetComponent<ActorComponent?>(target, out var actorComponent))
                        targetcomp.NetParent = actorComponent.PlayerSession.UserId;
                    
                targetcomp.Parent = target.Value;
            }

            _mind.TryGetMind(uid, out var mindId, out var mind);

            if (mind != null)
                _mind.TransferTo(mindId, args.Target);

            if (targetcomp != null && targetcomp.ReleaseControlName != null)
                _actionsSystem.AddAction(target.Value, comp.ReleaseControlAction);
        }

        private void OnReproduceAction(EntityUid uid, BrainHuggingComponent comp, ReproduceActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            
            if (hugcomp == null)
                return;

            _popup.PopupEntity(Loc.GetString("You start to feel bad, as if something is about to come out of you!"), target, target, PopupType.LargeCaution);
            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, hugcomp.ReproduceTime, new ReproduceDoAfterEvent(), uid, target: target, used: uid)
            {
                BreakOnMove = true,
            });

        }

        private void ReproduceDoAfter(EntityUid uid, BrainHuggingComponent comp, ReproduceDoAfterEvent args)
        {
            var target = args.Target;
            
            if (target == null)
                return;

            _popup.PopupEntity(Loc.GetString("You start to feel bad, as if something is about to come out of you!"), target.Value, target.Value, PopupType.LargeCaution);


            if (TryComp<StatusEffectsComponent>(target.Value, out var status))
                _stunSystem.TrySlowdown(target.Value, TimeSpan.FromSeconds(22f), true, 0.3f, 0.3f, status);
            Spawn("MobBrainSlug", Transform(target.Value).Coordinates);
            _vomit.Vomit(uid, -80f, -80f);
        }

        private void OnReleaseControlAction(EntityUid uid, SlugInsideComponent comp, ReleaseControlActionEvent args)
        {
            if (TryComp(uid, out SlugInsideComponent? slug))
            {
                _mind.TryGetMind(uid, out var slugMindId, out var slugMind);
                if (slugMindId != null)
                    _mind.TransferTo(slugMindId, slug.Slug);
            }
            if (TryComp(uid, out SlugInsideComponent? parrent))
            {
                _mind.TryGetMind(parrent.NetParent, out var parrentMindId, out var parrentMind);
                if (parrentMindId != null)
                    _mind.TransferTo(parrentMindId.Value, parrent.Parent);
            }
            if (comp.ReleaseControlName != null)
            {
                //_actionsSystem.RemoveAction(uid, theAction);
            }

            _popup.PopupEntity(Loc.GetString("The slug got out of your nervous system."), uid, uid);

        }

        private void OnReleaseSlugAction(EntityUid uid, BrainHuggingComponent comp, ReleaseSlugActionEvent args)
        {
            TryComp(uid, out BrainSlugComponent? defcomp);
            
            if (defcomp == null)
                return;

            var target = defcomp.EquipedOn;

            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, comp.BrainSlugTime, new ReleaseSlugDoAfterEvent(), uid, target: target, used: uid)
            {
                BreakOnMove = false,
            });
        }


        private void ReleaseSlugDoAfter(EntityUid uid, BrainSlugComponent component, ReleaseSlugDoAfterEvent args)
        {
            TryComp(uid, out BrainHuggingComponent? hugcomp);
            if (hugcomp == null)
                return;

            _container.Remove(uid, component.GuardianContainer);

            if (TryComp(args.Target, out SlugInsideComponent? slugcomp))
                if (args.Target != null && slugcomp != null)
                    _entityManager.RemoveComponent<SlugInsideComponent>(args.Target.Value);
            
            UpdateAbilities(uid, hugcomp, hugcomp.ReleaseSlugAction, false);
            
            UpdateAbilities(uid, hugcomp, hugcomp.DominateVictimAction, false);
            
            UpdateAbilities(uid, hugcomp, hugcomp.TormentHostSlugAction, false);
            
            UpdateAbilities(uid, hugcomp, hugcomp.AssumeControlAction, false);
            
            UpdateAbilities(uid, hugcomp, hugcomp.ReproduceAction, false);
            
            UpdateAbilities(uid, hugcomp, hugcomp.StoreSlugAction, false);
            
            foreach (var action in hugcomp.BaseActions)
                UpdateAbilities(uid, hugcomp, action, true);
        }

        private void OnStoreAction(EntityUid uid, BrainHuggingComponent component, StoreActionEvent args)
        {
            _popup.PopupEntity(Loc.GetString("Slug don't have enough genes"), uid, uid);
        }
        
        private void UpdateAbilities(EntityUid uid, BrainHuggingComponent component, string actionId, bool addAction)
        {
            EntityUid? actionEntity = null;
            if (addAction)
            {
                if (!component.UnlockedAbilities.ContainsKey(actionId))
                {
                    _actionsSystem.AddAction(uid, ref actionEntity, actionId);
                    if (actionEntity != null)
                        component.UnlockedAbilities.Add(actionId, actionEntity.Value);
                }
            }
            else
            {
                if (component.UnlockedAbilities.TryGetValue(actionId, out var abilityUid))
                {
                    if (TryComp(uid, out ActionsComponent? comp))
                    {
                        _actionsSystem.RemoveAction(uid, abilityUid, comp);
                        if (abilityUid != null)
                            _actionContainer.RemoveAction(abilityUid.Value);
                        if (component.UnlockedAbilities.ContainsKey(actionId))
                            component.UnlockedAbilities.Remove(actionId);
                    }
                }
            }
            Dirty(uid, component);
        }


        //private void OnBlightAction(EntityUid uid, BrainHuggingComponent component, CoalInjectionActionEvent args)
        //{
        //    if (args.Handled)
        //        return;

        //    if (!TryUseAbility(uid, component, component.BlightCost, component.BlightDebuffs))
        //        return;

        //    args.Handled = true;
        //    // TODO: When disease refactor is in.
        //}


        //private bool TryUseAbility(EntityUid uid, BrainHuggingComponent component, FixedPoint2 abilityCost, Vector2 debuffs)
        //{
        //    if (component.Essence <= abilityCost)
        //    {
        //        _popup.PopupEntity(Loc.GetString("Slug don't have enough genes"), uid, uid);
        //        return false;
        //    }

        //    //var tileref = Transform(uid).Coordinates.GetTileRef();
        //    //if (tileref != null)
        //    //{
        //    //    if (_physics.GetEntitiesIntersectingBody(uid, (int) CollisionGroup.Impassable).Count > 0)
        //    //    {
        //    //        _popup.PopupEntity(Loc.GetString("revenant-in-solid"), uid, uid);
        //    //        return false;
        //    //    }
        //    //}

        //    ChangeGenesAmount(uid, abilityCost, component, false);

        //    //_statusEffects.TryAddStatusEffect<CorporealComponent>(uid, "Corporeal", TimeSpan.FromSeconds(debuffs.Y), false);
        //    //_stun.TryStun(uid, TimeSpan.FromSeconds(debuffs.X), false);

        //    return true;
        //}

        //public bool ChangeGenesAmount(EntityUid uid, FixedPoint2 amount, BrainHuggingComponent? component = null, bool allowDeath = true)
        //{
        //    if (!Resolve(uid, ref component))
        //        return false;

        //    if (!allowDeath && component.Essence + amount <= 0)
        //        return false;

        //    component.Essence += amount;

        //    //if (regenCap)
        //    //    FixedPoint2.Min(component.Essence, component.EssenceRegenCap);

        //    if (TryComp<StoreComponent>(uid, out var store))
        //        _store.UpdateUserInterface(uid, uid, store);

        //    //_alerts.ShowAlert(uid, AlertType.Essence, (short) Math.Clamp(Math.Round(component.Essence.Float() / 10f), 0, 16));


        //    return true;
        //}




        public override void Update(float frameTime)
        {
            base.Update(frameTime);



            foreach (var comp in EntityQuery<BrainSlugComponent>())
            {
                comp.Accumulator += frameTime;

                if (comp.Accumulator <= comp.DamageFrequency)
                    continue;

                comp.Accumulator = 0;

                if (comp.EquipedOn is not { Valid: true } targetId)
                    continue;
                if (TryComp(targetId, out MobStateComponent? mobState))
                {
                    if (mobState.CurrentState is MobState.Dead)
                    {
                        return;
                    }
                }

                foreach (var rev in EntityQuery<BrainHuggingComponent>())
                {
                    rev.Accumulator += frameTime;

                    if (rev.Accumulator <= 1)
                        continue;
                    rev.Accumulator -= 1;

                    if (rev.SlugGenes <= 40)
                    {
                        rev.AccumulatorStarveNotify += 1;
                        if (rev.AccumulatorStarveNotify > 30)
                        {
                            rev.AccumulatorStarveNotify = 0;
                            _popup.PopupEntity(Loc.GetString("flesh-cultist-hungry"),
                                rev.Owner, rev.Owner, PopupType.Large);
                        }
                    }

                    //if (rev.SlugGenes < 0)
                    //{
                    //    ParasiteComesOut(rev.Owner, rev);
                    //}

                    //ChangeSlugGenesAmount(rev.Owner, rev.HungerСonsumption, rev);
                }
            }

        }
    }
}
