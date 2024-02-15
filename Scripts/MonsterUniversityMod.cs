// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;


namespace MonsterUniversity
{

    public class MonsterUniversityMod : MonoBehaviour
    {
        private static Mod mod;

        public static MonsterUniversityMod Instance;

        public bool UseCustomStealthChance;

        Mod firstPersonLightingMod;


        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            Instance = go.AddComponent<MonsterUniversityMod>();

            mod.IsReady = true;
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

            Debug.Log("Finished Start(): Monster-University");

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


        /// <summary>
        /// Triggered when a new enemy entity is spawned.
        /// Make adjustments to the entity.
        /// </summary>
        void GameManager_OnEnemySpawnHandler(GameObject enemy)
        {
            try
            {
                AdjustEnemy(enemy);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in Monster-University OnEnemySpawn/AdjustEnemy: " + e.ToString());
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

            //Tweak stats to fit a bell curve.
            //Give stat level bonus to human classes.
            Stats.Adjust(entity);

            bool isCustom = entity.MobileEnemy.ID > 255;

            //Additional adjustments for standard human classes.
            if (entity.EntityType == EntityTypes.EnemyClass && !isCustom)
            {
                //Skill values should match class.
                Skills.Adjust(entity);

                //Make sure they have an appropriate weapon equipped and equip any magic items.
                if (!SaveLoadManager.Instance.LoadInProgress)
                    Equipment.Adjust(entity);

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

            //Add new components for motor and senses enhancement.
            enemy.AddComponent<EnemySensesEnhancement>();
            enemy.AddComponent<EnemyMotorEnhancement>();

            //Swap out EntityConcealmentBehaviour with a modified version.
            GameObject.Destroy(enemy.GetComponent<EntityConcealmentBehaviour>());
            enemy.AddComponent<EntityConcealmentBehaviourMU>();
        }



        /// <summary>
        /// Changes bard and sorcerer animations to allow spellcasting.
        /// </summary>
        void ModifyEnemyAnimations()
        {
            const int G = 85;   // Mob Array Gap from 42 .. 128 = 85

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
