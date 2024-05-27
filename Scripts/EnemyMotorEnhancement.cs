// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024


using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility;

namespace MonsterUniversity
{

    public class EnemyMotorEnhancement : MonoBehaviour
    {
        EnemyMotor motor;
        DaggerfallEntityBehaviour behaviour;
        EnemyEntity entity;
        EnemySenses senses;
        EntityEffectManager effectManager;
        MobileUnit mobile;
        EnemyMotor.TakeActionCallback originalTakeActionCallback;

        readonly List<EffectBundleSettings> rangedAttackSpells = new List<EffectBundleSettings>();
        readonly List<EffectBundleSettings> rangedAreaAttackSpells = new List<EffectBundleSettings>();
        readonly List<EffectBundleSettings> touchAttackSpells = new List<EffectBundleSettings>();
        readonly List<EffectBundleSettings> areaAttackSpells = new List<EffectBundleSettings>();
        readonly List<EffectBundleSettings> escapeSpells = new List<EffectBundleSettings>();
        EffectBundleSettings combatPrepSpell = new EffectBundleSettings();
        EffectBundleSettings healSpell = new EffectBundleSettings();
        EffectBundleSettings healAreaSpell = new EffectBundleSettings();
        EffectBundleSettings slowfallSpell = new EffectBundleSettings();
        EffectBundleSettings levitateSpell = new EffectBundleSettings();
        EffectBundleSettings freeActionSpell = new EffectBundleSettings();
        EffectBundleSettings lightSpell = new EffectBundleSettings();

        float nextHealCheck;



        void Start()
        {
            motor = GetComponent<EnemyMotor>();
            behaviour = GetComponent<DaggerfallEntityBehaviour>();
            entity = behaviour.Entity as EnemyEntity;
            senses = GetComponent<EnemySenses>();
            effectManager = GetComponent<EntityEffectManager>();
            mobile = GetComponentInChildren<MobileUnit>();

            //Set delegate custom behaviour
            originalTakeActionCallback = motor.TakeActionHandler;
            motor.TakeActionHandler = TakeAction;
            motor.CanCastRangedSpellHandler = CanCastRangedSpell;
            motor.CanCastTouchSpellHandler = CanCastTouchSpell;

            //In vanilla daggerfall, player-character skill level is used to determine magic casting
            //costs for enemies.  We will let enemy skills be used instead.
            effectManager.UsePlayerCharacterSkillsForEnemyMagicCost = false;

            ClassifySpells();
        }


        void Update()
        {
            if (GameManager.IsGamePaused)
                return;

            if (Time.frameCount % 47 == 0)
            {
                //Check if falling and cast slowfall/levitate if necessary.
                TryCastFallingSpell();

                //Should enemies be able to cast free action?  Maybe I'll add a mod setting later...
                TryCastFreeAction();
            }

            //If enemy detected, warn allies in the area.
            if (Time.frameCount % 99 == 0 && senses.Target != null && senses.DetectedTarget)
            {
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(transform.position, 18f);
                foreach (DaggerfallEntityBehaviour behaviour in entities)
                {
                    if (behaviour.Entity.Team == entity.Team)
                    {
                        EnemySenses allySenses = behaviour.GetComponent<EnemySenses>();
                        if (allySenses.Target == null || allySenses.DetectedTarget == false)
                        {
                            if (Utility.HasPath(transform.position, allySenses.transform.position))
                            {
                                allySenses.Target = senses.Target;
                                allySenses.DetectedTarget = true;
                                allySenses.LastKnownTargetPos = senses.LastKnownTargetPos;
                            }
                        }
                    }
                }
            }


        }


        /// <summary>
        /// Our version of TakeAction, replacing the one in EnemyMotor.
        /// </summary>
        void TakeAction()
        {
            //Check if enemy might surrender.
            if (Surrenders())
                return;

            //Call the original (default) TakeAction method
            originalTakeActionCallback();

            if (TryCastCombatPrepSpell())
                return;

            if (TryCastLightSpell())
                return;

            if (TryCastHealSpell())
                return;

            if (TryCastLevitateSpell())
                return;
        }



        /// <summary>
        /// Our version of CanCastRangedSpell, replacing the one in EnemyMotor.
        /// </summary>
        bool CanCastRangedSpell()
        {
            if (entity.CurrentMagicka <= 0)
                return false;

            EffectBundleSettings spell = new EffectBundleSettings();

            //Check if an area effect spell is desired.
            if (rangedAreaAttackSpells.Count > 0)
            {
                EffectBundleSettings choice = rangedAreaAttackSpells[Random.Range(0, rangedAreaAttackSpells.Count)];

                int tally = 0;
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(senses.Target.transform.position, 4.3f);

                foreach (DaggerfallEntityBehaviour targetBehaviour in entities)
                {
                    tally += AnalyzeTargetEffect(targetBehaviour, choice);
                }

                if (tally >= 2 || (tally >= 1 && rangedAttackSpells.Count == 0))
                    spell = choice;
            }

            //If not using an area effect spell, select a single target spell
            if (spell.BundleType == BundleTypes.None && rangedAttackSpells.Count > 0)
            {
                EffectBundleSettings choice = rangedAttackSpells[Random.Range(0, rangedAttackSpells.Count)];
                if (AnalyzeTargetEffect(senses.Target, choice) == 1)
                    spell = choice;
            }

            if (spell.BundleType != BundleTypes.None)
            {
                EntityEffectBundle bundle = new EntityEffectBundle(spell, behaviour);

                bool alreadyAffected = motor.EffectsAlreadyOnTarget(bundle);
                bool hasClearPath = motor.HasClearPathToShootProjectile(25f, DaggerfallMissile.ArmLength, 0.45f);

                if (!alreadyAffected && hasClearPath)
                {
                    motor.SelectedSpell = bundle;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Our version of CanCastTouchSpell, replacing the one in EnemyMotor.
        /// </summary>
        bool CanCastTouchSpell()
        {
            if (entity.CurrentMagicka <= 0)
                return false;

            EffectBundleSettings spell = new EffectBundleSettings();

            //Check if an caster-area effect spell is desired.
            if (areaAttackSpells.Count > 0)
            {
                EffectBundleSettings choice = areaAttackSpells[Random.Range(0, areaAttackSpells.Count)];

                int tally = 0;
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(transform.position, 4.3f);

                foreach (DaggerfallEntityBehaviour targetBehaviour in entities)
                {
                    if (targetBehaviour == behaviour)
                        continue;

                    tally += AnalyzeTargetEffect(targetBehaviour, choice);
                }

                if (tally >= 2 || (tally >= 1 && touchAttackSpells.Count == 0))
                    spell = choice;
            }

            //If not using an area effect spell, select a single target touch spell
            if (spell.BundleType == BundleTypes.None && touchAttackSpells.Count > 0)
            {
                EffectBundleSettings choice = touchAttackSpells[Random.Range(0, touchAttackSpells.Count)];
                if (AnalyzeTargetEffect(senses.Target, choice) == 1)
                    spell = choice;
            }

            if (spell.BundleType != BundleTypes.None)
            {
                EntityEffectBundle bundle = new EntityEffectBundle(spell, behaviour);
                if (!motor.EffectsAlreadyOnTarget(bundle))
                {
                    motor.SelectedSpell = bundle;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns 1 if spell likely damages enemy, -1 if it likely damages ally, 0 otherwise
        /// </summary>
        int AnalyzeTargetEffect(DaggerfallEntityBehaviour target, EffectBundleSettings spell)
        {
            if (entity.Team == MobileTeams.PlayerAlly && target.EntityType == EntityTypes.Player)
                return GuessImmunity(target, spell) ? 0 : -1;
            else if (target.Entity.Team == entity.Team)
                return GuessImmunity(target, spell) ? 0 : -1;
            else if (entity.Team == MobileTeams.PlayerAlly)
            {
                EnemyMotor targetMotor = target.GetComponent<EnemyMotor>();
                if (targetMotor && targetMotor.IsHostile)
                    return GuessImmunity(target, spell) ? 0 : 1;
                else
                    return GuessImmunity(target, spell) ? 0 : -1;
            }
            else
                return GuessImmunity(target, spell) ? 0 : 1;
        }


        /// <summary>
        /// Tries to determine if target is immune to the spell, depending on intelligence check.
        /// </summary>
        bool GuessImmunity(DaggerfallEntityBehaviour target, EffectBundleSettings spell)
        {
            if (Dice100.FailedRoll(entity.Stats.LiveIntelligence))
                return Dice100.SuccessRoll(50); //Ummm, duh, flip a coin.

            switch (spell.ElementType)
            {
                case ElementTypes.Cold:
                    return target.Entity.Career.Frost == DaggerfallConnect.DFCareer.Tolerance.Immune;
                case ElementTypes.Fire:
                    return target.Entity.Career.Fire == DaggerfallConnect.DFCareer.Tolerance.Immune;
                case ElementTypes.Magic:
                    return target.Entity.Career.Magic == DaggerfallConnect.DFCareer.Tolerance.Immune;
                case ElementTypes.Poison:
                    return target.Entity.Career.Poison == DaggerfallConnect.DFCareer.Tolerance.Immune;
                case ElementTypes.Shock:
                    return target.Entity.Career.Shock == DaggerfallConnect.DFCareer.Tolerance.Immune;
                default:
                    return true;
            }

        }


        /// <summary>
        /// Check if a slowfall or levitate spell is needed and available.
        /// </summary>
        bool TryCastFallingSpell()
        {
            if (motor.Falls && !entity.IsSlowFalling && !entity.IsParalyzed)
            {
                if (motor.LastGroundedY - transform.position.y < 5)
                    return false;  //We haven't fallen far enough yet.

                if (levitateSpell.BundleType != BundleTypes.None)
                {
                    return TryCast(levitateSpell);
                }
                else if (slowfallSpell.BundleType != BundleTypes.None)
                {
                    return TryCast(slowfallSpell);
                }
            }

            return false;
        }


        /// <summary>
        /// Check if a free-action spell is needed and available.
        /// </summary>
        bool TryCastFreeAction()
        {
            if (!entity.IsParalyzed)
                return false;

            if (mobile.IsPlayingOneShot())
                return false;

            if (freeActionSpell.BundleType == BundleTypes.None)
                return false;

            //Adds a delay before attempting cast.
            if (Random.Range(0f, 1.5f) > Time.smoothDeltaTime)
                return false;

            return TryCast(freeActionSpell);
        }




        static readonly HashSet<MobileTypes> monstersThatSurrender = new HashSet<MobileTypes>()
        {
            MobileTypes.Centaur, MobileTypes.Dragonling, MobileTypes.Dreugh, MobileTypes.Gargoyle, MobileTypes.GrizzlyBear,
            MobileTypes.Harpy, MobileTypes.Imp, MobileTypes.Lamia, MobileTypes.Nymph, MobileTypes.Orc, MobileTypes.OrcSergeant,
            MobileTypes.OrcShaman, MobileTypes.Spriggan
        };

        /// <summary>
        /// Certain creatures can potentially surrender (become pacified) if their health drops low enough.
        /// </summary>
        bool Surrenders()
        {
            if (!motor.CanAct)
                return false;

            if (mobile.IsPlayingOneShot())
                return false;

            if (entity.CurrentHealth > entity.MaxHealth / 5)
                return false;

            //Don't surrender if a heal spell can be cast.
            if (healSpell.BundleType != BundleTypes.None && entity.CurrentMagicka > 15)
                return false;

            //Quest enemies don't surrender.
            if (senses.QuestBehaviour)
                return false;

            //Only half of enemies that can surrender/flee actually do.
            DaggerfallEnemy dfEnemy = behaviour.GetComponent<DaggerfallEnemy>();
            if (dfEnemy && dfEnemy.LoadID % 2 == 0)
                return false;

            //Adds a random amount of delay before deciding...
            if (Random.Range(0f, 1.5f) > Time.smoothDeltaTime)
                return false;

            //Most monsters, and human knights/barbarians, never surrender.
            bool canSurrender = entity.EntityType == EntityTypes.EnemyClass;
            canSurrender ^= entity.MobileEnemy.ID == (int)MobileTypes.Knight;
            canSurrender ^= entity.MobileEnemy.ID == (int)MobileTypes.Barbarian;
            canSurrender |= monstersThatSurrender.Contains((MobileTypes)entity.MobileEnemy.ID);
            canSurrender |= entity.MobileEnemy.ID == 256; //Goblin, expanded enemies
            if (!canSurrender)
                return false;

            //Making creature non-hostile.
            if (entity.Team != MobileTeams.PlayerAlly)
                motor.IsHostile = false;

            TryCastEscapeSpell();

            return true;
        }


        /// <summary>
        /// Check if a concealment spell is needed and available.
        /// </summary>
        bool TryCastEscapeSpell()
        {
            if (entity.IsMagicallyConcealed)
                return false;

            if (escapeSpells.Count == 0)
                return false;

            EffectBundleSettings escapeSpell = new EffectBundleSettings();

            foreach (EffectBundleSettings spell in escapeSpells)
            {
                escapeSpell = spell;
                if (spell.Effects[0].Key.StartsWith("Shadow") && GameManager.Instance.PlayerEnterExit.IsPlayerInDarkness)
                    break;
            }

            return TryCast(escapeSpell);
        }


        /// <summary>
        /// At the start of combat, check if a combat prep spell is available and cast it.
        /// </summary>
        bool TryCastCombatPrepSpell()
        {
            if (!motor.CanAct || mobile.IsPlayingOneShot())
                return false;

            if (combatPrepSpell.BundleType == BundleTypes.None)
                return false;

            if (senses.Target == null || senses.DetectedTarget == false)
                return false;

            if (entity.Team == MobileTeams.PlayerAlly)
                return false; //Player allies will conserve magicka for offense.

            //Add a random amount of delay...
            if (Random.Range(0f, 1.6f) > Time.smoothDeltaTime)
                return false;

            if (IsSpellActive(combatPrepSpell))
                return false;

            //Check path to target.  Don't cast if target is in another room.
            if (Utility.HasPath(transform.position, senses.Target.transform.position))
                return TryCast(combatPrepSpell);
            else
                return false;
        }


        /// <summary>
        /// Check if a light spell is available and needed.
        /// </summary>
        bool TryCastLightSpell()
        {
            if (lightSpell.BundleType == BundleTypes.None)
                return false;

            if (Time.frameCount % 20 != 0)
                return false; //only checking occasionally

            if (senses.Target == null || senses.DetectedTarget == false)
                return false;

            if (!motor.CanAct || mobile.IsPlayingOneShot())
                return false;

            if (IsSpellActive(lightSpell))
                return false;

            if (MonsterUniversityMod.Instance.GetLightingOnEntity(senses.Target).grayscale > 0.2f)
                return false; //enough light

            //Check path to target.  Don't cast if target is in another room.
            if (Utility.HasPath(transform.position, senses.Target.transform.position))
                return TryCast(lightSpell);
            else
                return false;
        }


        /// <summary>
        /// Check if a healing spell is available and needed by the entity or nearby allies.
        /// </summary>
        bool TryCastHealSpell()
        {
            if (Time.time < nextHealCheck)
                return false;

            if (!motor.CanAct || mobile.IsPlayingOneShot())
                return false;

            return TryCastHealArea() || TryCastHealSelf();
        }


        /// <summary>
        /// Check if a healing spell is needed and available.
        /// </summary>
        bool TryCastHealSelf()
        {
            if (healSpell.BundleType == BundleTypes.None)
                return false;

            if (entity.CurrentHealth > entity.MaxHealth / 3)
                return false;

            if (IsSpellActive(healSpell))
                return false;

            nextHealCheck = Time.time + Random.Range(2.1f, 4.8f);

            return TryCast(healSpell);
        }


        /// <summary>
        /// Check if an area-effect healing spell is needed and available.
        /// </summary>
        bool TryCastHealArea()
        {
            if (healAreaSpell.BundleType == BundleTypes.None)
                return false;

            //The area of effect should be 4, as per DaggerfallMissile.cs ExplosionRadius
            List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(transform.position, 4.2f);

            int allies = 0;
            int enemies = 0;

            foreach (DaggerfallEntityBehaviour targetBehaviour in entities)
            {
                bool needsHealing = targetBehaviour.Entity.CurrentHealth < 0.6f * targetBehaviour.Entity.MaxHealth;

                if (targetBehaviour == behaviour)
                    continue;
                else if (targetBehaviour.Entity.Team == entity.Team && needsHealing)
                    ++allies;
                else if (entity.Team == MobileTeams.PlayerAlly && targetBehaviour.EntityType == EntityTypes.Player && needsHealing)
                    ++allies;
                else
                    ++enemies;
            }

            if (allies > 0 && enemies == 0)
            {
                nextHealCheck = Time.time + Random.Range(2.1f, 5.2f);
                return TryCast(healAreaSpell);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Check if a levitation spell is needed and available.  Cast it if so.
        /// </summary>
        bool TryCastLevitateSpell()
        {
            if (!motor.CanAct || mobile.IsPlayingOneShot())
                return false;

            if (!senses.Target || !senses.TargetInSight || senses.LastKnownTargetPos == EnemySenses.ResetPlayerPos)
                return false;

            if (motor.IsLevitating)
                return false;

            Vector3 targetVector = (senses.LastKnownTargetPos - behaviour.transform.position);

            //If on similar elevation, then skip.
            if (Mathf.Abs(targetVector.y) < 5)
                return false;

            if (HasPossibleWalkingPathToTarget())
                return false;

            //Our quarry is significantly above or below.  See if a levitate spell is available.
            if (levitateSpell.BundleType != BundleTypes.None)
                return TryCast(levitateSpell);
            else
                return false;
        }


        /// <summary>
        /// If target is at a different elevation, check if it is possible to reach on foot.
        /// </summary>
        bool HasPossibleWalkingPathToTarget()
        {
            if (GameManager.Instance.PlayerEnterExit.IsPlayerSubmerged)
                return false;

            Vector3 startLocation = transform.position;
            Vector3 destination = senses.Target.transform.position;

            if (GetAltitude(destination) > 3f)
                return false;

            //shift upward closer to eye-height to reduce clipping through stairs and what-not
            startLocation += Vector3.up;

            //Checks how close each point in the path is to the floor at various points.
            //A big step up/down means levitation is required.
            float distance = Vector3.Distance(startLocation, destination);
            Vector3 offsetDirection = (destination - startLocation).normalized;
            float lastAltitude = GetAltitude(startLocation);
            for (float offset = 0f; offset <= distance; offset += 1f)
            {
                Vector3 position = startLocation + offsetDirection * offset;
                float altitude = GetAltitude(position);
                if (Mathf.Abs(altitude - lastAltitude) > 1.5f)
                    return false;
                lastAltitude = altitude;
            }

            return true;

        }


        /// <summary>
        /// Returns distance above ground of provided position, maximum of 20.
        /// </summary>
        /// <returns>Altitude, maximum of 20</returns>
        public static float GetAltitude(Vector3 position)
        {
            const float maxDistance = 20f;

            if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, maxDistance))
                return hit.distance;

            return maxDistance;
        }


        /// <summary>
        /// Ready spell, assuming the entity has magicka available and is not silenced.
        /// </summary>
        bool TryCast(EffectBundleSettings spell)
        {
            if (entity.CurrentMagicka <= 0)
                return false;

            if (mobile.IsPlayingOneShot())
                return false;

            if (spell.BundleType == BundleTypes.None)
                return false;

            if (entity.IsSilenced)
                return false;

            EntityEffectBundle bundle = new EntityEffectBundle(spell, behaviour);

            if (effectManager.SetReadySpell(bundle))
            {
                mobile.ChangeEnemyState(MobileStates.Spell);
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// Check if the spell is already active on the entity.
        /// </summary>
        bool IsSpellActive(EffectBundleSettings spell)
        {
            LiveEffectBundle[] bundles = effectManager.EffectBundles;

            foreach (LiveEffectBundle bundle in bundles)
            {
                if (bundle.liveEffects[0].Key == spell.Effects[0].Key)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Classify spells in spellbook to use for various circumstances.
        /// </summary>
        void ClassifySpells()
        {
            EffectBundleSettings[] spells = entity.GetSpells();

            List<EffectBundleSettings> combatPrepSpells = new List<EffectBundleSettings>();

            bool isInDungeon = GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon;

            foreach (EffectBundleSettings spell in spells)
            {
                if (spell.Effects[0].Key == HealHealth.EffectKey)
                    healSpell = spell;
                else if (spell.Effects[0].Key == Regenerate.EffectKey)
                    healSpell = spell;
                else if (spell.Effects[0].Key == Spells.HealHealthAreaMU.EffectKey)
                    healAreaSpell = spell;
                else if (spell.Effects[0].Key == ChameleonNormal.EffectKey)
                    escapeSpells.Add(spell);
                else if (spell.Effects[0].Key == ShadowNormal.EffectKey && isInDungeon)
                    escapeSpells.Add(spell);
                else if (spell.Effects[0].Key == InvisibilityNormal.EffectKey)
                    escapeSpells.Add(spell);
                else if (spell.Effects[0].Key == Spells.EnemyMageLightMU.EffectKey)
                    lightSpell = spell;
                else if (spell.Effects[0].Key == ChameleonTrue.EffectKey)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == ShadowTrue.EffectKey && isInDungeon)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == InvisibilityTrue.EffectKey)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == SpellAbsorption.EffectKey)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == SpellReflection.EffectKey)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == SpellResistance.EffectKey)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == Shield.EffectKey)
                    combatPrepSpells.Add(spell);
                else if (spell.Effects[0].Key == Slowfall.EffectKey)
                    slowfallSpell = spell;
                else if (spell.Effects[0].Key == Levitate.EffectKey)
                    levitateSpell = spell;
                else if (spell.Effects[0].Key == FreeAction.EffectKey)
                    freeActionSpell = spell;
                else if (spell.TargetType == TargetTypes.SingleTargetAtRange)
                    rangedAttackSpells.Add(spell);
                else if (spell.TargetType == TargetTypes.AreaAtRange)
                    rangedAreaAttackSpells.Add(spell);
                else if (spell.TargetType == TargetTypes.ByTouch)
                    touchAttackSpells.Add(spell);
                else if (spell.TargetType == TargetTypes.AreaAroundCaster)
                    areaAttackSpells.Add(spell);
            }

            if (combatPrepSpells.Count > 0)
            {
                int pick = Random.Range(0, combatPrepSpells.Count);
                combatPrepSpell = combatPrepSpells[pick];
            }
        }



    } //class EnemyMotorEnhancement



} //namespace

