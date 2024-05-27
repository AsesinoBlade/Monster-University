// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.MagicAndEffects;


namespace MonsterUniversity
{

    public class MonsterUniversityMod : MonoBehaviour
    {
        static Mod mod;

        static readonly HashSet<int> WeaponWielders = new HashSet<int>()
        {
            (int)MobileTypes.Orc, (int)MobileTypes.OrcSergeant, (int)MobileTypes.OrcShaman, (int)MobileTypes.OrcWarlord,
            (int)MobileTypes.Centaur, (int)MobileTypes.SkeletalWarrior
        };

        public static MonsterUniversityMod Instance;

        public Color PaperDollAverageColor { get; private set; } = Color.gray;

        Mod firstPersonLightingMod;



        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            Instance = go.AddComponent<MonsterUniversityMod>();

            mod.IsReady = true;
        }



        static int CalculateStealthChance(float distance, DaggerfallEntityBehaviour target)
        {
            int luck = target.Entity.Stats.GetLiveStatValue(DFCareer.Stats.Luck);

            //If target is player and is not trying to sneak, undo any skill tally from EnemySenses.StealthCheck() 
            if (target == GameManager.Instance.PlayerEntityBehaviour)
            {
                PlayerMotor playerMotor = GameManager.Instance.PlayerMotor;
                bool isSneaky = playerMotor.IsMovingLessThanHalfSpeed || playerMotor.IsCrouching;

                if (!isSneaky)
                {
                    PlayerEntity player = GameManager.Instance.PlayerEntity;
                    uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();

                    if (player.TimeOfLastStealthCheck == gameMinutes)
                    {
                        //Give a luck chance of keeping the skill point, as with Skulduggery mod.
                        if (Dice100.FailedRoll(luck / 2))
                            player.TallySkill(DFCareer.Skills.Stealth, -1);
                    }
                }
            }


            float noise = EnemySensesEnhancement.GetNoise(target);

            float stealth = target.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Stealth);

            float chance = (stealth + 10) * Mathf.Log10(distance / noise);

            //Luck determines the auto-success and auto-fail values.
            float min = luck / 10;
            float max = 90 + min;

            chance = Mathf.Clamp(chance, min, max);

            return (int)chance;
        }


        /// <summary>
        /// Attempts to get the lighting at the specified location by messaging the 'First-Person-Lighting' mod.
        /// </summary>
        public Color GetLightingAtLocation(Vector3 location)
        {
            Color tint = Color.gray;

            if (firstPersonLightingMod == null || !firstPersonLightingMod.IsReady)
                return tint;

            firstPersonLightingMod.MessageReceiver("locationLighting", location, (string message, object data) =>
            {
                tint = (Color)data;
            });

            return tint;
        }


        /// <summary>
        /// Attempts to get the lighting at the entity's location by messaging the 'First-Person-Lighting' mod.
        /// </summary>
        public Color GetLightingOnEntity(DaggerfallEntityBehaviour entity)
        {
            Color tint = Color.gray;

            if (firstPersonLightingMod == null || !firstPersonLightingMod.IsReady)
                return tint;

            firstPersonLightingMod.MessageReceiver("entityLighting", entity, (string message, object data) =>
            {
                tint = (Color)data;
            });

            return tint;
        }



        void Start()
        {
            Debug.Log("Start(): Monster-University");

            firstPersonLightingMod = ModManager.Instance.GetMod("First-Person-Lighting");

            //Register event handler on enemy spawn to handle changes for enemy.
            GameManager.OnEnemySpawn += GameManager_OnEnemySpawnHandler;

            //Register a custom area-effect healing spell for Healer class.
            Spells.HealHealthAreaMU healAreaTemplateEffect = new Spells.HealHealthAreaMU();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(healAreaTemplateEffect);

            //Register a custom enemy light spell.
            Spells.EnemyMageLightMU enemyMageLightTemplateEffect = new Spells.EnemyMageLightMU();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(enemyMageLightTemplateEffect);

            //Set new animations for bard and sorcerer, so they can have spellcasting animations.
            ModifyEnemyAnimations();

            FormulaHelper.RegisterOverride(mod, "CalculateStealthChance",
                (Func<float, DaggerfallEntityBehaviour, int>)CalculateStealthChance);


            Debug.Log("Finished Start(): Monster-University");
        }


        void Update()
        {
            if (GameManager.IsGamePaused)
                return;

            //Periodically sample the PC paper doll.
            if (Time.frameCount % 199 == 0)
                PaperDollAverageColor = GetPaperDollAverageColor();
        }


        /// <summary>
        /// Sample the image of the PC paper doll to get the average color.
        /// </summary>
        Color GetPaperDollAverageColor()
        {

            DaggerfallUI.Instance.PaperDollRenderer.Refresh();
            Texture2D paperdoll = DaggerfallUI.Instance.PaperDollRenderer.PaperDollTexture;

            try
            {
                Color background = paperdoll.GetPixel(0, 0);

                int sampleCount = 0;
                Color totalColor = Color.clear;

                for (int i = 0; i < 100; ++i)
                {
                    int x = UnityEngine.Random.Range(0, paperdoll.width);
                    int y = UnityEngine.Random.Range(0, paperdoll.height);

                    Color pixel = paperdoll.GetPixel(x, y);

                    if (pixel != background)
                    {
                        totalColor += pixel;
                        ++sampleCount;
                    }
                }

                if (sampleCount > 0)
                    return totalColor / sampleCount;
                else
                    return Color.gray * 0.5f;
            }
            catch
            {
                return Color.gray;
            }
        }


        /// <summary>
        /// Triggered when a new enemy entity is spawned.
        /// Makes adjustments to the entity.
        /// </summary>
        void GameManager_OnEnemySpawnHandler(GameObject enemy)
        {
            try
            {
                AdjustEnemy(enemy);
            }
            catch (Exception e)
            {
                Debug.LogError($"Monster-University OnEnemySpawn/AdjustEnemy: {e.Message}");
            }
        }


        /// <summary>
        /// Potentially alters the enemy's stats, skills, equipment, and spellbook.
        /// </summary>
        void AdjustEnemy(GameObject enemy)
        {
            DaggerfallEntityBehaviour behaviour = enemy.GetComponent<DaggerfallEntityBehaviour>();

            EnemyEntity entity = behaviour.Entity as EnemyEntity;

            //Seed random to keep values consistent on save/reload.
            DaggerfallEnemy dfEnemy = behaviour.GetComponent<DaggerfallEnemy>();

            if (dfEnemy.LoadID != 0)
                UnityEngine.Random.InitState((int)dfEnemy.LoadID);

            bool isCustom = entity.MobileEnemy.ID > 255;

            //Tweak stats to fit a bell curve.
            //Give stat level bonus to human classes.
            Stats.Adjust(entity);

            Skills.Adjust(entity, isCustom);

            if (entity.EntityType == EntityTypes.EnemyClass || WeaponWielders.Contains(entity.MobileEnemy.ID))
            {
                //Make sure they have an appropriate weapon equipped and equip any magic items.
                Equipment.Adjust(entity);
            }

            //Additional adjustments for standard human classes.
            if (entity.EntityType == EntityTypes.EnemyClass && !isCustom)
            {
                //Base game gives all enemies 100+level*10 magicka.
                //Give enemy classes the 'correct' amount of magicka based on class.
                int intelligence = entity.Stats.GetLiveStatValue(DFCareer.Stats.Intelligence);
                entity.MaxMagicka = (int)(entity.Career.SpellPointMultiplierValue * intelligence);

                if (!SaveLoadManager.Instance.LoadInProgress)
                    entity.CurrentMagicka = entity.MaxMagicka;
            }

            //Rebuild spellbook for human casters.  Possibly add spells for certain monsters.
            if (!isCustom)
                Spells.Adjust(entity);

            //Ensure appropriate resistances etc.
            AdjustCareer(entity);

            //Add incumbent effect to handle damage from sunlight/holy places etc.
            AddPassiveSpecials(behaviour);

            //Add new components for motor and senses enhancement.
            enemy.AddComponent<EnemySensesEnhancement>();
            enemy.AddComponent<EnemyMotorEnhancement>();

            //Replace EntityConcealmentBehaviour with a modified version.
            GameObject.Destroy(enemy.GetComponent<EntityConcealmentBehaviour>());
            enemy.AddComponent<EntityConcealmentBehaviourMU>();

        }


        /// <summary>
        /// Additional modifications to enemy resistances.
        /// </summary>
        void AdjustCareer(EnemyEntity entity)
        {
            if (entity.MobileEnemy.BloodIndex == 2) //bloodless enemies
            {
                entity.Career.Disease = DFCareer.Tolerance.Immune;
                entity.Career.Paralysis = DFCareer.Tolerance.Immune;
                entity.Career.Poison = DFCareer.Tolerance.Immune;
            }

            if (entity.MobileEnemy.Team == MobileTeams.Undead)
            {
                entity.Career.Disease = DFCareer.Tolerance.Immune;
                entity.Career.DamageFromHolyPlaces = true;
            }
        }


        /// <summary>
        /// Assigns PassiveSpecials imcumbent effect to handle damage from sunlight/holy etc.
        /// </summary>
        void AddPassiveSpecials(DaggerfallEntityBehaviour behaviour)
        {
            EntityEffectManager effectManager = behaviour.GetComponent<EntityEffectManager>();
           
            // If PassiveSpecialsEffect is already incumbent, skip
            if (effectManager.FindIncumbentEffect<PassiveSpecialsEffect>() != null)
                return;

            // Instantiate effect
            EffectBundleSettings settings = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.None,
                Effects = new EffectEntry[] { new EffectEntry(PassiveSpecialsEffect.EffectKey) },
            };
            effectManager.AssignBundle(new EntityEffectBundle(settings, behaviour), AssignBundleFlags.BypassSavingThrows);
        }



        /// <summary>
        /// Changes bard and sorcerer animations to allow spellcasting.
        /// </summary>
        void ModifyEnemyAnimations()
        {
            const int G = 85;   // Mob Array Gap from 42 .. 128 = 85

            //Using Sorcerer graphic same as in Roleplay and Realism mod
            int sorcererIndex = (int)MobileTypes.Sorcerer - G;
            EnemyBasics.Enemies[sorcererIndex].MaleTexture = 476;
            EnemyBasics.Enemies[sorcererIndex].FemaleTexture = 475;
            EnemyBasics.Enemies[sorcererIndex].HasRangedAttack1 = false;
            EnemyBasics.Enemies[sorcererIndex].CastsMagic = true;
            EnemyBasics.Enemies[sorcererIndex].PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, 5 };
            EnemyBasics.Enemies[sorcererIndex].ChanceForAttack2 = 33;
            EnemyBasics.Enemies[sorcererIndex].PrimaryAttackAnimFrames2 = new int[] { 5, 4, 3, -1, 2, 1, 0 };
            EnemyBasics.Enemies[sorcererIndex].ChanceForAttack3 = 33;
            EnemyBasics.Enemies[sorcererIndex].PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 2, 2, 1, 0 };
            EnemyBasics.Enemies[sorcererIndex].HasSpellAnimation = true;
            EnemyBasics.Enemies[sorcererIndex].SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 };


            int bardIndex = (int)MobileTypes.Bard - G;
            EnemyBasics.Enemies[bardIndex].MaleTexture = 490;
            EnemyBasics.Enemies[bardIndex].FemaleTexture = 489;
            EnemyBasics.Enemies[bardIndex].HasRangedAttack1 = true;
            EnemyBasics.Enemies[bardIndex].HasRangedAttack2 = true;
            EnemyBasics.Enemies[bardIndex].CastsMagic = true;
            EnemyBasics.Enemies[bardIndex].PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 };
            EnemyBasics.Enemies[bardIndex].ChanceForAttack2 = 50;
            EnemyBasics.Enemies[bardIndex].PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 };
            EnemyBasics.Enemies[bardIndex].ChanceForAttack3 = 0;
            EnemyBasics.Enemies[bardIndex].RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 };
            EnemyBasics.Enemies[bardIndex].HasSpellAnimation = true;
            EnemyBasics.Enemies[bardIndex].SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 };

        }





    } //class MonsterUniversityMod


} //namespace MonsterUniversity
