// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;

namespace MonsterUniversity
{

    public static class Skills
    {

        /// <summary>
        /// Sets enemy skill values based on their class primary, major, minor, and misc skills.
        /// </summary>
        public static void Adjust(EnemyEntity entity, bool isCustom)
        {
            if (entity.EntityType == EntityTypes.EnemyClass && !isCustom)
            {
                //Human class enemy skills should follow their class template
                SetSkills(entity, entity.GetPrimarySkills(), 38, 3.8f);
                SetSkills(entity, entity.GetMajorSkills(), 31, 2.5f);
                SetSkills(entity, entity.GetMinorSkills(), 27, 2f);
                SetSkills(entity, entity.GetMiscSkills(), 7, 1f);
            }
            else
            {
                for (int i = 0; i < (int)DFCareer.Skills.Count; ++i)
                {
                    //Keeping magic skills at their default values.
                    if (Spells.MagicSkills.Contains((DFCareer.Skills)i))
                        continue;

                    short value = (short)(30 + Random.Range(3f, 5f) * entity.Level);
                    if (entity.GetLanguageSkill() == (DFCareer.Skills)i && value < 50)
                        value = (short)Random.Range(50, 101);

                    if (value > 100)
                        value = 100;

                    entity.Skills.SetPermanentSkillValue(i, value);
                }
            }
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
            float value = start + entity.Level * perLevel;

            //Gaussian distribution (bell-curve)
            value = Gaussian(value);

            value = Mathf.Clamp(value, 1, 100);

            if (Spells.MagicSkills.Contains(skill) && !entity.MobileEnemy.CastsMagic)
                entity.Skills.SetPermanentSkillValue(skill, (short)Random.Range(1, 9));
            else
                entity.Skills.SetPermanentSkillValue(skill, (short)value);
        }


        /// <summary>
        /// Returns the value adjusted on a bell curve.
        /// </summary>
        static float Gaussian(float value)
        {
            const float range = 25;

            const int count = 5;

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

