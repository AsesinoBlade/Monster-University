// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using DaggerfallConnect;
using System.Collections.Generic;
using static DaggerfallWorkshop.MobileTypes;

namespace MonsterUniversity
{
    public static class Stats
    {
        static readonly Dictionary<MobileTypes, int[]> ClassStats = new Dictionary<MobileTypes, int[]>()
        {
            { Acrobat, new int[8]    {40, 39, 45, 65, 47, 50, 63, 51} },
            { Archer, new int[8]     {55, 43, 45, 60, 50, 44, 53, 50} },
            { Assassin, new int[8]   {55, 45, 45, 60, 52, 45, 50, 48} },
            { Barbarian, new int[8]  {65, 40, 45, 50, 58, 42, 50, 50} },
            { Bard, new int[8]       {45, 55, 50, 55, 40, 60, 50, 45} },
            { Battlemage, new int[8] {50, 60, 55, 50, 45, 40, 50, 50} },
            { Burglar, new int[8]    {44, 44, 46, 62, 46, 50, 58, 50} },
            { Healer, new int[8]     {42, 60, 62, 45, 45, 50, 46, 50} },
            { Knight, new int[8]     {60, 45, 48, 50, 49, 58, 45, 45} },
            { Mage, new int[8]       {42, 60, 65, 45, 45, 50, 43, 50} },
            { Monk, new int[8]       {50, 45, 45, 62, 48, 42, 58, 50} },
            { Nightblade, new int[8] {45, 55, 60, 65, 40, 40, 50, 45} },
            { Ranger, new int[8]     {60, 45, 48, 55, 50, 45, 47, 50} },
            { Rogue, new int[8]      {58, 40, 48, 62, 50, 48, 50, 44} },
            { Sorcerer, new int[8]   {45, 60, 65, 45, 40, 50, 45, 50} },
            { Spellsword, new int[8] {60, 50, 55, 50, 50, 40, 45, 50} },
            { Thief, new int[8]      {45, 47, 45, 58, 50, 47, 58, 50} },
            { Warrior, new int[8]    {60, 44, 47, 57, 50, 45, 47, 50} },
        };


        /// <summary>
        /// Add bonus stat points for enemy classes.
        /// Modify stats to fit a bell curve.
        /// </summary>
        public static void Adjust(EnemyEntity entity)
        {
            if (entity.EntityType == EntityTypes.EnemyClass)
                CalculateStats(entity);
            
            ApplyBellCurves(entity);
        }


        private static void CalculateStats(DaggerfallEntity entity)
        {
            CalculateStat(entity, DFCareer.Stats.Strength);
            CalculateStat(entity, DFCareer.Stats.Intelligence);
            CalculateStat(entity, DFCareer.Stats.Willpower);
            CalculateStat(entity, DFCareer.Stats.Agility);
            CalculateStat(entity, DFCareer.Stats.Endurance);
            CalculateStat(entity, DFCareer.Stats.Personality);
            CalculateStat(entity, DFCareer.Stats.Speed);
            CalculateStat(entity, DFCareer.Stats.Luck);
        }


        private static void CalculateStat(DaggerfallEntity entity, DFCareer.Stats stat)
        {
            EnemyEntity enemyEntity = entity as EnemyEntity;
            MobileTypes classType = (MobileTypes)enemyEntity.MobileEnemy.ID;

            if (!ClassStats.TryGetValue(classType, out int[] statValues))
                return;

            float statValue = statValues[(int)stat];
            float levelBoost = Mathf.Clamp((statValue - 36) / 10, 0.5f, 2f);
            levelBoost *= entity.Level;
            statValue += levelBoost;

            statValue = Mathf.Clamp(statValue, 10, 100);

            entity.Stats.SetPermanentStatValue(stat, (int)statValue);
        }


        private static void ApplyBellCurves(DaggerfallEntity entity)
        {
            Gaussian(entity, DFCareer.Stats.Strength);
            Gaussian(entity, DFCareer.Stats.Intelligence);
            Gaussian(entity, DFCareer.Stats.Willpower);
            Gaussian(entity, DFCareer.Stats.Agility);
            Gaussian(entity, DFCareer.Stats.Endurance);
            Gaussian(entity, DFCareer.Stats.Personality);
            Gaussian(entity, DFCareer.Stats.Speed);
            Gaussian(entity, DFCareer.Stats.Luck);
        }


        /// <summary>
        /// Modifies the stat to fit a bell curve.
        /// </summary>
        static void Gaussian(DaggerfallEntity entity, DFCareer.Stats stat)
        {
            int value = entity.Stats.GetPermanentStatValue(stat);

            const float range = 20;
            const int count = 7;

            float low = value - range;
            float high = value + range;

            float total = 0;

            for (int i = 0; i < count; ++i)
                total += Random.Range(low, high);

            total /= count;
            total = Mathf.Round(total);

            int maxValue = entity.EntityBehaviour.EntityType == EntityTypes.EnemyMonster ? 200 : 100;

            value = (int)Mathf.Clamp(total, 10, maxValue);

            entity.Stats.SetPermanentStatValue(stat, value);
        }


    } //class Stats



} //namespace
