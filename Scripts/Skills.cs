// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;


namespace MonsterUniversity
{

    public static class Skills
    {
        /// <summary>
        /// Sets enemy skill values based on their class primary, major, minor, and misc skills.
        /// </summary>
        public static void Adjust(EnemyEntity entity)
        {
            SetSkills(entity, entity.GetPrimarySkills(), 38, 3.4f);
            SetSkills(entity, entity.GetMajorSkills(), 31, 2.5f);
            SetSkills(entity, entity.GetMinorSkills(), 27, 2f);
            SetSkills(entity, entity.GetMiscSkills(), 7, 1f);
        }


        /// <summary>
        /// Calculates given skills given a base-value and increase-per-level value.
        /// </summary>
        static void SetSkills(EnemyEntity entity, List<DFCareer.Skills> skills, int baseValue, float perLevel)
        {
            foreach (DFCareer.Skills skill in skills)
                SetSkill(entity, skill, baseValue, perLevel);
        }


        /// <summary>
        /// Calculates specific skill given a base-value and increase-per-level value.
        /// </summary>
        static void SetSkill(EnemyEntity entity, DFCareer.Skills skill, int start, float perLevel)
        {
            if (Spells.MagicSkills.Contains(skill) && !entity.MobileEnemy.CastsMagic)
            {
                entity.Skills.SetPermanentSkillValue(skill, (short)Random.Range(2, 7));
                return;
            }

            float value = start + entity.Level * perLevel;

            //Gaussian distribution (bell-curve)
            value = Gaussian(value);

            value = Mathf.Clamp(value, 1, 100);

            entity.Skills.SetPermanentSkillValue(skill, (short)value);
        }


        /// <summary>
        /// Returns the value adjusted on a bell curve.
        /// </summary>
        static float Gaussian(float value)
        {
            const float range = 25;

            const int count = 7;

            float low = value - range;
            float high = value + range;

            float total = 0;

            for (int i = 0; i < count; ++i)
                total += Random.Range(low, high);

            total /= count;

            return Mathf.Round(total);
        }


    } //class Skills


} //namespace

