// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024


using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using System.Collections.Generic;
using UnityEngine;


namespace MonsterUniversity
{

    public class EnemyMotorEnhancement : MonoBehaviour
    {
        EnemyMotor enemyMotor;
        DaggerfallEntityBehaviour entityBehaviour;
        EnemyEntity entity;
        EnemySenses senses;
        EntityEffectManager entityEffectManager;
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

        float lastHealCheck;



        // Start is called before the first frame update
        void Start()
        {
            enemyMotor = GetComponent<EnemyMotor>();
            entityBehaviour = GetComponent<DaggerfallEntityBehaviour>();
            entity = entityBehaviour.Entity as EnemyEntity;
            senses = GetComponent<EnemySenses>();
            entityEffectManager = GetComponent<EntityEffectManager>();
            mobile = GetComponentInChildren<MobileUnit>();

            //Set delegate custom behaviour
            originalTakeActionCallback = enemyMotor.TakeActionHandler;
            enemyMotor.TakeActionHandler = TakeAction;
            enemyMotor.CanCastRangedSpellHandler = CanCastRangedSpell;
            enemyMotor.CanCastTouchSpellHandler = CanCastTouchSpell;

            //In vanilla daggerfall, player-character skill level is used to determine magic casting
            //costs for enemies (presumably a bug).  We will let enemy skills be used instead.
            EntityEffectManager effectManager = GetComponent<EntityEffectManager>();
            effectManager.UsePlayerCharacterSkillsForEnemyMagicCost = false;

            ClassifySpells();
        }


        // Update is called once per frame
        void Update()
        {

        }


        void TakeAction()
        {
            if (TryCastFallingSpell())
                return;

            if (TryCastFreeAction())
                return;

            if (Surrenders())
                return;

            //Call the original (default) TakeAction method
            originalTakeActionCallback();


            if (TryCastCombatPrepSpell())
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

            EffectBundleSettings spell = new EffectBundleSettings();

            //Check if an area effect spell is desired.
            if (rangedAreaAttackSpells.Count > 0)
            {
                int allyCount = 0;
                int enemyCount = 0;
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(senses.Target.transform.position, 4.3f);
                foreach (DaggerfallEntityBehaviour behaviour in entities)
                {
                    if (behaviour == entityBehaviour)
                        continue;
                    else if (entity.Team == MobileTeams.PlayerAlly && behaviour.EntityType == EntityTypes.Player)
                        ++allyCount;
                    else if (behaviour.Entity.Team == entity.Team)
                        ++allyCount;
                    else if (entity.Team == MobileTeams.PlayerAlly)
                    {
                        EnemyMotor motor = behaviour.GetComponent<EnemyMotor>();
                        if (motor && motor.IsHostile) //pacified enemies don't count as enemies to player allies
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
                enemyMotor.SelectedSpell = new EntityEffectBundle(spell, entityBehaviour);

                if (enemyMotor.EffectsAlreadyOnTarget(enemyMotor.SelectedSpell))
                    return false;

                // Check that there is a clear path to shoot a spell
                // All range spells are currently 25 speed and 0.45f radius
                return enemyMotor.HasClearPathToShootProjectile(25f, DaggerfallMissile.ArmLength, 0.45f);
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
                int allyCount = 0;
                int enemyCount = 0;
                List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(transform.position, 4.3f);
                foreach (DaggerfallEntityBehaviour behaviour in entities)
                {
                    if (behaviour == entityBehaviour)
                        continue;
                    else if (entity.Team == MobileTeams.PlayerAlly && behaviour.EntityType == EntityTypes.Player)
                        ++allyCount;
                    else if (behaviour.Entity.Team == entity.Team)
                        ++allyCount;
                    else if (entity.Team == MobileTeams.PlayerAlly)
                    {
                        EnemyMotor motor = behaviour.GetComponent<EnemyMotor>();
                        if (motor && motor.IsHostile) //pacified enemies don't count as enemies to player allies
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
                enemyMotor.SelectedSpell = new EntityEffectBundle(spell, entityBehaviour);

                return !enemyMotor.EffectsAlreadyOnTarget(enemyMotor.SelectedSpell);
            }
            else
            {
                return false;
            }

        }



        /// <summary>
        /// Check if a slowfall or levitate spell is needed and available, and cast it if so.
        /// </summary>
        bool TryCastFallingSpell()
        {
            if (enemyMotor.Falls && !entity.IsSlowFalling && !entity.IsParalyzed)
            {
                if (enemyMotor.LastGroundedY - transform.position.y < 5)
                    return false;  //We haven't fallen far enough yet.  Don't instant-cast...

                if (slowfallSpell.BundleType != BundleTypes.None)
                {
                    return TryCast(slowfallSpell);
                }
                else if (levitateSpell.BundleType != BundleTypes.None)
                {
                    return TryCast(levitateSpell);
                }
            }

            return false;
        }


        /// <summary>
        /// Check if a free-action spell is needed and available.  Cast it if so.
        /// </summary>
        bool TryCastFreeAction()
        {
            if (!entity.IsParalyzed)
                return false;

            if (mobile.IsPlayingOneShot())
                return false; //If paralyzed while playing a one-shot, they're just screwed.

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
            MobileTypes.OrcShaman, MobileTypes.OrcWarlord, MobileTypes.Spriggan
        };

        /// <summary>
        /// Certain creatures can potentially surrender (become pacified) if their health drops low enough.
        /// </summary>
        bool Surrenders()
        {
            if (!enemyMotor.CanAct)
                return false;

            if (mobile.IsPlayingOneShot())
                return false;

            //A lone healer will likely surrender/flee regardless of current health.
            //Others use health-based criteria.
            if (entity.CurrentHealth > entity.MaxHealth / 5)
                return false;

            //Don't surrender if a heal spell can be cast.
            if (healSpell.BundleType != BundleTypes.None && entity.CurrentMagicka > 10)
                return false;

            //Most monsters, and human knights/barbarians, never surrender.
            bool canSurrender = entity.EntityType == EntityTypes.EnemyClass;
            canSurrender ^= entity.MobileEnemy.ID == (int)MobileTypes.Knight;
            canSurrender ^= entity.MobileEnemy.ID == (int)MobileTypes.Barbarian;
            canSurrender |= monstersThatSurrender.Contains((MobileTypes)entity.MobileEnemy.ID);
            if (!canSurrender)
                return false;

            //Quest enemies don't surrender.
            if (senses.QuestBehaviour)
                return false;

            //Only half of enemies that can surrender/flee actually do.
            DaggerfallEnemy dfEnemy = entityBehaviour.GetComponent<DaggerfallEnemy>();
            if (dfEnemy && dfEnemy.LoadID % 2 == 0)
                return false;

            //Adds a random amount of delay before deciding...
            if (Random.Range(0f, 1.5f) > Time.smoothDeltaTime)
                return false;

            if (TryCastEscapeSpell())
                return true;

            //Making creature non-hostile.
            if (entity.Team != MobileTeams.PlayerAlly)
                enemyMotor.IsHostile = false;

            /*
            DaggerfallEntityBehaviour closest = GetClosestEnemy(7);
            if (closest == null)
                return true;

            destination = closest.transform.position;
            Vector3 direction = (closest.transform.position - transform.position).normalized;
            float moveSpeed = (entity.Stats.LiveSpeed + PlayerSpeedChanger.dfWalkBase) * MeshReader.GlobalScale;
            AttemptMove(direction, moveSpeed, true);
            */

            return true;
        }


        /// <summary>
        /// Check if a concealment spell is needed and available.  Cast it if so.
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
            if (!enemyMotor.CanAct || mobile.IsPlayingOneShot())
                return false;

            if (combatPrepSpell.BundleType == BundleTypes.None)
                return false;

            if (senses.Target == null)
                return false;

            if (entity.Team == MobileTeams.PlayerAlly)
                return false; //Player allies will conserve magicka for offense.

            //Add a random amount of delay...
            if (Random.Range(0f, 1.4f) > Time.smoothDeltaTime)
                return false;

            if (IsSpellActive(combatPrepSpell))
                return false;

            return TryCast(combatPrepSpell);
        }


        /// <summary>
        /// Check if a healing spell is available and needed by the entity or nearby allies.  Cast it if so.
        /// </summary>
        bool TryCastHealSpell()
        {
            if (Time.time < lastHealCheck + 2.5f)
                return false;

            lastHealCheck = Time.time;

            if (!enemyMotor.CanAct || mobile.IsPlayingOneShot())
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
            List<DaggerfallEntityBehaviour> entities = Utility.GetEntitiesInArea(transform.position, 4.3f);

            int allies = 0;
            int enemies = 0;

            foreach (DaggerfallEntityBehaviour behaviour in entities)
            {
                if (behaviour == entityBehaviour)
                    continue;
                else if (behaviour.Entity.CurrentHealth > 0.7f * behaviour.Entity.MaxHealth)
                    continue;
                else if (behaviour.Entity.Team == entity.Team)
                    ++allies;
                else if (entity.Team == MobileTeams.PlayerAlly && behaviour.EntityType == EntityTypes.Player)
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
            if (!enemyMotor.CanAct || mobile.IsPlayingOneShot())
                return false;

            if (!senses.Target || !senses.TargetInSight || senses.LastKnownTargetPos == EnemySenses.ResetPlayerPos)
                return false;

            if (enemyMotor.IsLevitating)
                return false;

            Vector3 targetVector = (senses.LastKnownTargetPos - entityBehaviour.transform.position);

            if (Mathf.Abs(targetVector.y) < 4 || !enemyMotor.ObstacleDetected)
                return false;

            //Our quarry is significantly above or below.  See if a levitate spell is available.
            if (levitateSpell.BundleType != BundleTypes.None)
                return TryCast(levitateSpell);
            else
                return false;
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

            EntityEffectBundle bundle = new EntityEffectBundle(spell, entityBehaviour);

            if (entityEffectManager.SetReadySpell(bundle))
            {
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
            LiveEffectBundle[] bundles = entityEffectManager.EffectBundles;

            foreach (LiveEffectBundle bundle in bundles)
            {
                if (bundle.liveEffects[0].Key == spell.Effects[0].Key)
                    return true;
            }

            return false;
        }



        /// <summary>
        /// Get the closest enemy near this entity, excluding player and player allies.
        /// </summary>
        DaggerfallEntityBehaviour GetClosestEnemy(float range)
        {
            float closestDistance = float.MaxValue;
            DaggerfallEntityBehaviour closestEnemy = null;

            foreach (DaggerfallEntityBehaviour behaviour in Utility.GetEntitiesInArea(transform.position, range))
            {
                if (behaviour.Entity.Team == entity.Team)
                    continue;
                else if (entity.Team == MobileTeams.PlayerAlly && behaviour.EntityType == EntityTypes.Player)
                    continue;

                float distance = Vector3.Distance(transform.position, behaviour.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = behaviour;
                }
            }

            return closestEnemy;
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
                else if (spell.Effects[0].Key == Spells.HealHealthAreaMU.EffectKey)
                    healAreaSpell = spell;
                else if (spell.Effects[0].Key == ChameleonNormal.EffectKey)
                    escapeSpells.Add(spell);
                else if (spell.Effects[0].Key == ShadowNormal.EffectKey && isInDungeon)
                    escapeSpells.Add(spell);
                else if (spell.Effects[0].Key == InvisibilityNormal.EffectKey)
                    escapeSpells.Add(spell);
                else if (spell.Effects[0].Key == Spells.EnemyMageLightMU.EffectKey && isInDungeon)
                    combatPrepSpells.Add(spell);
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

