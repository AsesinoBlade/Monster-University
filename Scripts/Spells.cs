// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect;
using DaggerfallConnect.Save;

namespace MonsterUniversity
{


    public static class Spells
    {
        public static readonly List<DFCareer.Skills> MagicSkills = new List<DFCareer.Skills>()
        {
            DFCareer.Skills.Alteration, DFCareer.Skills.Destruction, DFCareer.Skills.Illusion,
            DFCareer.Skills.Mysticism, DFCareer.Skills.Restoration, DFCareer.Skills.Thaumaturgy
        };


        /// <summary>
        /// Rebuilds the spellbook of enemy classes, or possibly add spells to enemy monsters.
        /// </summary>
        public static void Adjust(EnemyEntity entity)
        {
            if (entity.EntityBehaviour.EntityType == EntityTypes.EnemyClass)
            {
                RebuildSpellbook(entity);
            }
            else if (entity.MobileEnemy.ID == (int)MobileTypes.Lich)
            {
                if (Dice100.SuccessRoll(70))
                    entity.AddSpell(GetClassicSpell(4)); //Levitate
            }
            else if (entity.MobileEnemy.ID == (int)MobileTypes.AncientLich)
            {
                if (Dice100.SuccessRoll(85))
                    entity.AddSpell(GetClassicSpell(4)); //Levitate
            }
            else if (entity.MobileEnemy.ID == (int)MobileTypes.VampireAncient)
            {
                if (Dice100.SuccessRoll(60))
                    entity.AddSpell(GetClassicSpell(4)); //Levitate
            }
        }



        /// <summary>
        /// Clears the existing spellbook, then adds appropriate spells based on skillset.
        /// </summary>
        static void RebuildSpellbook(EnemyEntity entity)
        {
            //Clear the spellbook.
            while (entity.SpellbookCount() > 0)
                entity.DeleteSpell(0);

            //Add appropriate spells for skillset.
            foreach (DFCareer.Skills skill in MagicSkills)
            {
                int skillValue = entity.Skills.GetPermanentSkillValue(skill);
                if (skillValue < 25)
                    continue;

                switch (skill)
                {
                    case DFCareer.Skills.Alteration:
                        AddAlterationSpells(entity, skillValue);
                        break;
                    case DFCareer.Skills.Destruction:
                        AddDestructionSpells(entity, skillValue);
                        break;
                    case DFCareer.Skills.Illusion:
                        AddIllusionSpells(entity, skillValue);
                        break;
                    case DFCareer.Skills.Mysticism:
                        AddMysticismSpells(entity, skillValue);
                        break;
                    case DFCareer.Skills.Restoration:
                        AddRestorationSpells(entity, skillValue);
                        break;
                    case DFCareer.Skills.Thaumaturgy:
                        AddThaumaturgySpells(entity, skillValue);
                        break;
                    default:
                        break;
                }
            }
        }


        static void AddAlterationSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 35 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(37)); //Slowfall

            if (skillValue > 75 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(50)); //Paralysis

            if (skillValue > 85 && entity.Skills.GetPermanentSkillValue(DFCareer.Skills.Destruction) > 85)
            {
                if (Dice100.SuccessRoll(50))
                    entity.AddSpell(GetClassicSpell(34)); //Wizard Rend
            }

        }


        static void AddDestructionSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 30 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(3)); //Frostbite

            if (skillValue > 37 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(8)); //Shock

            if (skillValue > 40 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(7)); //Wizard's Fire

            if (skillValue > 50 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(14)); //Fireball

            if (skillValue > 55 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(31)); //Lightning

            if (skillValue > 60 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(53)); //Hand Of Sleep

            if (skillValue > 65 && Dice100.SuccessRoll(45))
                entity.AddSpell(GetClassicSpell(25)); //Fire Storm

            if (skillValue > 70 && Dice100.SuccessRoll(45))
                entity.AddSpell(GetClassicSpell(16)); //Ice Bolt

            if (skillValue > 70 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(29)); //Toxic Cloud

            if (skillValue > 80 && Dice100.SuccessRoll(45))
                entity.AddSpell(GetClassicSpell(20)); //Ice Storm

        }


        static void AddIllusionSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 30 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(45)); //Shadow-Normal

            if (skillValue > 35 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(44)); //Chameleon-Normal

            if (skillValue > 40 && entity.MobileEnemy.Team == MobileTeams.KnightsAndMages)
            {
                if (Dice100.SuccessRoll(60))
                    entity.AddSpell(mageLightSpell);
            }

            if (skillValue > 45 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(6)); //Invisibility-Normal

            if (skillValue > 50 && Dice100.SuccessRoll(50))
                entity.AddSpell(shadowTrueSpell);

            if (skillValue > 55 && Dice100.SuccessRoll(50))
                entity.AddSpell(chameleonTrueSpell);

            if (skillValue > 70 && Dice100.SuccessRoll(40))
                entity.AddSpell(invisibilityTrueSpell);
        }


        static void AddMysticismSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 70 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(23)); //Silence
        }


        static void AddRestorationSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 30 && Dice100.SuccessRoll(skillValue + 20))
                entity.AddSpell(GetClassicSpell(10)); //Free Action

            if (skillValue > 35 && Dice100.SuccessRoll(skillValue + 30))
                entity.AddSpell(GetClassicSpell(97)); //Heal Self

            if (skillValue > 50 && entity.MobileEnemy.ID == (int)MobileTypes.Healer)
                entity.AddSpell(healAreaSpell);

            if (entity.Career.SpellAbsorption == DFCareer.SpellAbsorptionFlags.None)
            {
                if (skillValue > 85 && Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(47)); //Spell Absorption
            }

        }


        static void AddThaumaturgySpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 45 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(4)); //Levitate

            if (skillValue > 87 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(22)); //Spell Shield
            else if (skillValue > 75 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(39)); //Spell Resistance

            if (skillValue > 90 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(46)); //Spell Reflection
        }


        static EffectBundleSettings GetClassicSpell(int spellId)
        {
            if (GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(spellId, out SpellRecord.SpellRecordData spellData))
            {
                EffectBundleSettings bundle;
                GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spellData, BundleTypes.Spell, out bundle);
                return bundle;
            }
            else
            {
                Debug.LogWarning("Monster-University: unknown classic spell ID " + spellId);
                return new EffectBundleSettings();
            }

        }

        static EffectBundleSettings chameleonTrueSpell = new EffectBundleSettings()
        {
            Name = "True Chameleon",
            Version = EntityEffectBroker.CurrentSpellVersion,
            BundleType = BundleTypes.Spell,
            TargetType = TargetTypes.CasterOnly,
            ElementType = ElementTypes.Magic,
            Icon = new SpellIcon() { index = 0 },
            Effects = new EffectEntry[]
            {
                new EffectEntry(ChameleonTrue.EffectKey, new EffectSettings() {
                    DurationBase = 1, DurationPlus = 1, DurationPerLevel = 1
                })
            },
        };

        static EffectBundleSettings shadowTrueSpell = new EffectBundleSettings()
        {
            Name = "True Shadow",
            Version = EntityEffectBroker.CurrentSpellVersion,
            BundleType = BundleTypes.Spell,
            TargetType = TargetTypes.CasterOnly,
            ElementType = ElementTypes.Magic,
            Icon = new SpellIcon() { index = 0 },
            Effects = new EffectEntry[]
            {
                new EffectEntry(ShadowTrue.EffectKey, new EffectSettings() {
                    DurationBase = 1, DurationPlus = 1, DurationPerLevel = 1
                })
            },
        };

        static EffectBundleSettings invisibilityTrueSpell = new EffectBundleSettings()
        {
            Name = "True Invisibility",
            Version = EntityEffectBroker.CurrentSpellVersion,
            BundleType = BundleTypes.Spell,
            TargetType = TargetTypes.CasterOnly,
            ElementType = ElementTypes.Magic,
            Icon = new SpellIcon() { index = 0 },
            Effects = new EffectEntry[]
            {
                new EffectEntry(InvisibilityTrue.EffectKey, new EffectSettings() {
                    DurationBase = 1, DurationPlus = 1, DurationPerLevel = 1
                })
            },
        };

        static EffectBundleSettings healAreaSpell = new EffectBundleSettings()
        {
            Name = "Healing Aura",
            Version = EntityEffectBroker.CurrentSpellVersion,
            BundleType = BundleTypes.Spell,
            TargetType = TargetTypes.AreaAroundCaster,
            ElementType = ElementTypes.Magic,
            Icon = new SpellIcon() { index = 13 },
            Effects = new EffectEntry[]
            {
                new EffectEntry(HealHealthAreaMU.EffectKey, new EffectSettings() {
                    MagnitudeBaseMin = 2, MagnitudeBaseMax = 5,
                    MagnitudePlusMin = 1, MagnitudePlusMax = 5,
                    MagnitudePerLevel = 1
                })
            },
        };

        static EffectBundleSettings mageLightSpell = new EffectBundleSettings()
        {
            Name = "Mage Light",
            Version = EntityEffectBroker.CurrentSpellVersion,
            BundleType = BundleTypes.Spell,
            TargetType = TargetTypes.CasterOnly,
            ElementType = ElementTypes.Magic,
            Icon = new SpellIcon() { index = 0 },
            Effects = new EffectEntry[]
            {
                new EffectEntry(EnemyMageLightMU.EffectKey, new EffectSettings() {
                    DurationBase = 1, DurationPlus = 1, DurationPerLevel = 1
                })
            },
        };




        public class HealHealthAreaMU : BaseEntityEffect
        {
            public static readonly string EffectKey = "Heal-Health-Area-MU";

            public override void SetProperties()
            {
                properties.Key = EffectKey;
                properties.SupportMagnitude = true;
                properties.AllowedTargets = EntityEffectBroker.TargetFlags_Other;
                properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
                properties.AllowedCraftingStations = MagicCraftingStations.None;
                properties.MagicSkill = DFCareer.MagicSkills.Restoration;
                properties.MagnitudeCosts = MakeEffectCosts(20, 28);
                properties.DisableReflectiveEnumeration = true;
            }

            public override void MagicRound()
            {
                base.MagicRound();

                // Get peered entity gameobject
                DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
                if (!entityBehaviour)
                    return;

                // Skipping saving throw adjustments to magnitude
                int casterLevel = (caster) ? FormulaHelper.CalculateCasterLevel(caster.Entity, this) : 1;
                int baseMagnitude = Random.Range(settings.MagnitudeBaseMin, settings.MagnitudeBaseMax + 1);
                int plusMagnitude = Random.Range(settings.MagnitudePlusMin, settings.MagnitudePlusMax + 1);
                int multiplier = (int)Mathf.Floor(casterLevel / settings.MagnitudePerLevel);
                int magnitude = baseMagnitude + plusMagnitude * multiplier;

                entityBehaviour.Entity.IncreaseHealth(magnitude);
            }



        } //class HealHealthAreaMU



        public class EnemyMageLightMU : IncumbentEffect
        {
            public static readonly string EffectKey = "Enemy-MageLight-MU";
            Light myLight = null;

            public override void SetProperties()
            {
                properties.Key = EffectKey;
                properties.SupportDuration = true;
                properties.AllowedTargets = EntityEffectBroker.TargetFlags_Self;
                properties.AllowedCraftingStations = MagicCraftingStations.None;
                properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
                properties.MagicSkill = DFCareer.MagicSkills.Illusion;
                properties.DurationCosts = MakeEffectCosts(8, 40);
                properties.DisableReflectiveEnumeration = true;
            }

            public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
            {
                base.Start(manager, caster);
                StartLight();
            }

            public override void Resume(EntityEffectManager.EffectSaveData_v1 effectData, EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
            {
                base.Resume(effectData, manager, caster);
                StartLight();
            }

            public override void End()
            {
                base.End();
                EndLight();
            }

            protected override bool IsLikeKind(IncumbentEffect other)
            {
                return other is EnemyMageLightMU;
            }

            protected override void AddState(IncumbentEffect incumbent)
            {
                // Stack my rounds onto incumbent
                incumbent.RoundsRemaining += RoundsRemaining;
            }

            void StartLight()
            {
                // Do nothing if light already started
                if (myLight)
                    return;

                if (caster == null)
                {
                    RoundsRemaining = 0;
                    return;
                }

                // Create the light object
                GameObject go = new GameObject(EffectKey);
                go.transform.parent = caster.transform;
                go.transform.localPosition = Vector3.up;
                
                myLight = go.AddComponent<Light>();
                myLight.type = LightType.Point;
                Color color = Color.white;
                color.r = Random.Range(0.3f, 1f);
                color.g = Random.Range(0.3f, 1f);
                color.b = Random.Range(0.3f, 1f);
                myLight.color = color;
                myLight.range = 10;
                myLight.shadows = (DaggerfallUnity.Settings.EnableSpellShadows) ? LightShadows.Soft : LightShadows.None;
            }

            void EndLight()
            {
                // Destroy the light gameobject when done
                if (myLight)
                    GameObject.Destroy(myLight.gameObject);
            }


        } //class EnemyMageLightMU



    } //class Spells



} //namespace



