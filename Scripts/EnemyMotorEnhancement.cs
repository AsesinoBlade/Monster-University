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

        float lastHealCheck;



        // Start is called before the first frame update
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

            if (Time.frameCount % 50 == 0)
            {
                //Check if falling and cast slowfall/levitate if necessary.
                TryCastFallingSpell();

                //This likely won't get triggered as enemy can't act while paralyzed.
                //This might be modified later.
                if (TryCastFreeAction())
                    return;
            }
        }


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



        bool CanCastRangedSpell()
        {
            if (entity.CurrentMagicka <= 0)
                return false;

            if (senses.Target == null)
                return false;

            EffectBundleSettings spell = new EffectBundleSettings();

            //Check if an area effect spell is desired.
            if (rangedAreaAttackSpells.Count > 0)
            {
                int allyCount = 0;
                int enemyCount = 0;
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(senses.Target.transform.position, 4.3f);
                foreach (DaggerfallEntityBehaviour targetBehaviour in entities)
                {
                    if (targetBehaviour == behaviour)
                        continue;
                    else if (entity.Team == MobileTeams.PlayerAlly && targetBehaviour.EntityType == EntityTypes.Player)
                        ++allyCount;
                    else if (targetBehaviour.Entity.Team == entity.Team)
                        ++allyCount;
                    else if (entity.Team == MobileTeams.PlayerAlly)
                    {
                        EnemyMotor targetMotor = targetBehaviour.GetComponent<EnemyMotor>();
                        if (targetMotor && targetMotor.IsHostile) //pacified enemies don't count as enemies to player allies
                            ++enemyCount;
                    }
                    else
                        ++enemyCount;
                }

                if ((enemyCount > 1 || rangedAttackSpells.Count == 0) && allyCount == 0)
                    spell = rangedAreaAttackSpells[Random.Range(0, rangedAreaAttackSpells.Count)];
            }

            //If not using an area effect spell, select a single target spell
            if (spell.BundleType == BundleTypes.None && rangedAttackSpells.Count > 0)
                spell = rangedAttackSpells[Random.Range(0, rangedAttackSpells.Count)];

            if (spell.BundleType != BundleTypes.None)
            {
                motor.SelectedSpell = new EntityEffectBundle(spell, behaviour);

                if (motor.EffectsAlreadyOnTarget(motor.SelectedSpell))
                    return false;

                // Check that there is a clear path to shoot a spell
                // All range spells are currently 25 speed and 0.45f radius
                return motor.HasClearPathToShootProjectile(25f, DaggerfallMissile.ArmLength, 0.45f);
            }
            else
            {
                return false;
            }

        }


        bool CanCastTouchSpell()
        {
            if (entity.CurrentMagicka <= 0)
                return false;

            EffectBundleSettings spell = new EffectBundleSettings();

            //Check if an caster-area effect spell is desired.
            if (areaAttackSpells.Count > 0)
            {
                //EffectBundleSettings offering = areaAttackSpells[Random.Range(0, areaAttackSpells.Count)];

                int allyCount = 0;
                int enemyCount = 0;
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(transform.position, 4.3f);
                foreach (DaggerfallEntityBehaviour targetBehaviour in entities)
                {
                    if (targetBehaviour == behaviour)
                        continue;
                    else if (entity.Team == MobileTeams.PlayerAlly && targetBehaviour.EntityType == EntityTypes.Player)
                        ++allyCount;
                    else if (targetBehaviour.Entity.Team == entity.Team)
                        ++allyCount;
                    else if (entity.Team == MobileTeams.PlayerAlly)
                    {
                        EnemyMotor targetMotor = targetBehaviour.GetComponent<EnemyMotor>();
                        if (targetMotor && targetMotor.IsHostile) //pacified enemies don't count as enemies to player allies
                            ++enemyCount;
                    }
                    else
                        ++enemyCount;
                }

                if ((enemyCount > 1 || touchAttackSpells.Count == 0) && allyCount == 0)
                    spell = areaAttackSpells[Random.Range(0, areaAttackSpells.Count)];
            }

            //If not using an area effect spell, select a single target spell
            if (spell.BundleType == BundleTypes.None && touchAttackSpells.Count > 0)
                spell = touchAttackSpells[Random.Range(0, touchAttackSpells.Count)];

            if (spell.BundleType != BundleTypes.None)
            {
                motor.SelectedSpell = new EntityEffectBundle(spell, behaviour);

                return !motor.EffectsAlreadyOnTarget(motor.SelectedSpell);
            }
            else
            {
                return false;
            }

        }


        int AnalyzeTargetEffect(DaggerfallEntityBehaviour target, )

        /// <summary>
        /// Check if a slowfall or levitate spell is needed and available.
        /// </summary>
        bool TryCastFallingSpell()
        {
            if (motor.Falls && !entity.IsSlowFalling && !entity.IsParalyzed)
            {
                if (motor.LastGroundedY - transform.position.y < 5)
                    return false;  //We haven't fallen far enough yet.  Don't cast...

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

            if (senses.Target == null)
                return false;

            if (entity.Team == MobileTeams.PlayerAlly)
                return false; //Player allies will conserve magicka for offense.

            //Add a random amount of delay...
            if (Random.Range(0f, 1.6f) > Time.smoothDeltaTime)
                return false;

            if (IsSpellActive(combatPrepSpell))
                return false;

            return TryCast(combatPrepSpell);
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

            if (senses.Target == null)
                return false;

            if (!motor.CanAct || mobile.IsPlayingOneShot())
                return false;

            if (IsSpellActive(lightSpell))
                return false;

            if (MonsterUniversityMod.Instance.GetLightingOnEntity(senses.Target).grayscale > 0.15f)
                return false; //enough light

            return TryCast(lightSpell);
        }


        /// <summary>
        /// Check if a healing spell is available and needed by the entity or nearby allies.  Cast it if so.
        /// </summary>
        bool TryCastHealSpell()
        {
            if (Time.time < lastHealCheck + 2.5f)
                return false;

            lastHealCheck = Time.time;

            if (!motor.CanAct || mobile.IsPlayingOneShot())
                return false;

            return TryCastHealArea() || TryCastHealSelf();
        }


        /// <summary>
        /// Check if a healing spell is needed and available.  Cast it if so.
        /// </summary>
        bool TryCastHealSelf()
        {
            if (healSpell.BundleType == BundleTypes.None)
                return false;

            if (entity.CurrentHealth > entity.MaxHealth / 3)
                return false;

            if (IsSpellActive(healSpell))
                return false;

            return TryCast(healSpell);
        }


        /// <summary>
        /// Check if an area-effect healing spell is needed and available.  Cast it if so.
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
                bool needsHealing = targetBehaviour.Entity.CurrentHealth > 0.6f * targetBehaviour.Entity.MaxHealth;

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
                return TryCast(healAreaSpell);
            else
                return false;
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

            //checks how close the path is to the floor at various points
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
                //???????????????????????????????
                DaggerfallUI.AddHUDText("enemy casting " + spell.Name);
                mobile.ChangeEnemyState(MobileStates.Spell);

                return true;
            }

            return false;
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

