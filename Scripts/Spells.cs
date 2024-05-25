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
using DaggerfallWorkshop.Game.Utility.ModSupport;

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
            else if (entity.MobileEnemy.ID == (int)MobileTypes.Lich && Dice100.SuccessRoll(50))
            {
                entity.AddSpell(GetClassicSpell(4)); //Levitate
            }
            else if (entity.MobileEnemy.ID == (int)MobileTypes.AncientLich && Dice100.SuccessRoll(75))
            {
                entity.AddSpell(GetClassicSpell(4)); //Levitate
            }
        }



        /// <summary>
        /// Clears the existing spellbook, then adds appropriate spells based on skillset.
        /// </summary>
        static void RebuildSpellbook(EnemyEntity entity)
        {
            //Check if 'Kab's Unleveled Spells' mod is installed
            bool usingUnleveledSpells = (ModManager.Instance.GetModFromGUID("eb8c0317-b8ab-4679-bf61-1eaaff77a1f3") != null);

            //Clear the spellbook.
            while (entity.SpellbookCount() > 0)
                entity.DeleteSpell(0);

            //Add appropriate spells for skillset.
            foreach (DFCareer.Skills skill in MagicSkills)
            {
                int skillValue = entity.Skills.GetPermanentSkillValue(skill);
                if (skillValue < 25)
                    continue;

                if (!usingUnleveledSpells)
                {
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
                else
                {
                    switch (skill)
                    {
                        case DFCareer.Skills.Alteration:
                            AddUnleveledAlterationSpells(entity, skillValue);
                            break;
                        case DFCareer.Skills.Destruction:
                            AddUnleveledDestructionSpells(entity, skillValue);
                            break;
                        case DFCareer.Skills.Illusion:
                            AddIllusionSpells(entity, skillValue);
                            break;
                        case DFCareer.Skills.Mysticism:
                            AddMysticismSpells(entity, skillValue);
                            break;
                        case DFCareer.Skills.Restoration:
                            AddUnleveledRestorationSpells(entity, skillValue);
                            break;
                        case DFCareer.Skills.Thaumaturgy:
                            AddUnleveledThaumaturgySpells(entity, skillValue);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        static void AddAlterationSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 35 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(37)); //Slowfall

            if (skillValue > 74 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(50)); //Paralysis

            if (skillValue > 82 && entity.Skills.GetPermanentSkillValue(DFCareer.Skills.Destruction) > 82)
            {
                if (Dice100.SuccessRoll(50))
                    entity.AddSpell(GetClassicSpell(34)); //Wizard Rend
            }

        }


        static void AddUnleveledAlterationSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 33 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(37)); //Slowfall

            if (skillValue > 50 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(50)); //Paralysis

            if (skillValue > 62 && entity.Skills.GetPermanentSkillValue(DFCareer.Skills.Destruction) > 62)
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


        static void AddUnleveledDestructionSpells(EnemyEntity entity, int skillValue)
        {

            if (skillValue < 43 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(102)); //Apprentice Fire
            else if (skillValue < 43 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(3)); //Minor Frostbite

            if (skillValue > 34 && skillValue < 60 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(8)); //Shock

            if (skillValue > 35 && skillValue < 49 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(101)); //Stinking Cloud

            if (skillValue > 45 && skillValue < 56 && Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(33)); //Wildfire

            if (skillValue > 45 && skillValue < 75 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(53)); //Hand of Sleep

            if (skillValue > 48 && skillValue < 73 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(25)); //Lesser Fire Storm

            if (skillValue > 49 && skillValue < 84 && Dice100.SuccessRoll(60))
                    entity.AddSpell(GetClassicSpell(16)); //Ice Bolt

            if (skillValue > 49 && skillValue < 80 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(121)); //Minor Fireball

            if (skillValue > 51 && skillValue < 86 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(29)); //Toxic Cloud

            if (skillValue > 57 && skillValue < 76 && Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(7)); //Wizard's Fire

            if (skillValue > 58 && skillValue < 77 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(31)); //Lightning

            if (skillValue > 62 && Dice100.SuccessRoll(70))
                entity.AddSpell(GetClassicSpell(118)); //Greater Frostbite

            if (skillValue > 65 && skillValue < 88 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(107)); //Greater Icebolt

            if (skillValue > 71 && skillValue < 90 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(52)); //Vampiric Touch

            if (skillValue > 75 && Dice100.SuccessRoll(65))
                entity.AddSpell(GetClassicSpell(14)); //Fireball

            if (skillValue > 75 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(104)); //Greater Flamestorm

            if (skillValue > 85)
            {
                if (Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(113)); //Acid Cloud
                if (Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(111)); //Greater Lightning
                if (Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(20)); //Ice Storm
                if (Dice100.SuccessRoll(65))
                    entity.AddSpell(GetClassicSpell(110)); //Major Firestorm
            }

            if (skillValue == 100 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(32)); //Gods' Fire
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

            if (skillValue > 45 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(6)); //Invisibility-Normal

            if (skillValue > 50 && Dice100.SuccessRoll(40))
                entity.AddSpell(shadowTrueSpell);

            if (skillValue > 55 && Dice100.SuccessRoll(40))
                entity.AddSpell(chameleonTrueSpell);

            if (skillValue > 70 && Dice100.SuccessRoll(40))
                entity.AddSpell(invisibilityTrueSpell);
        }


        static void AddMysticismSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 60 && Dice100.SuccessRoll(40) && entity.MobileEnemy.ID != (int)MobileTypes.Healer)
                entity.AddSpell(GetClassicSpell(23)); //Silence
        }


        static void AddRestorationSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 30 && Dice100.SuccessRoll(skillValue + 10))
                entity.AddSpell(GetClassicSpell(10)); //Free Action

            if (skillValue > 47 && entity.MobileEnemy.ID == (int)MobileTypes.Healer)
                entity.AddSpell(healAreaSpell);

            if (skillValue > 70 && Dice100.SuccessRoll(skillValue - 20))
                entity.AddSpell(GetClassicSpell(24)); //Troll's Blood - regeneration
            else if (skillValue > 35 && Dice100.SuccessRoll(skillValue))
                entity.AddSpell(GetClassicSpell(97)); //Balyna's Balm - heal self

            if (entity.Career.SpellAbsorption == DFCareer.SpellAbsorptionFlags.None)
            {
                if (skillValue > 81 && Dice100.SuccessRoll(40))
                    entity.AddSpell(GetClassicSpell(47)); //Spell Absorption
            }

        }


        static void AddUnleveledRestorationSpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 35 && Dice100.SuccessRoll(skillValue))
                entity.AddSpell(GetClassicSpell(10)); //Free Action

            if (entity.MobileEnemy.ID == (int)MobileTypes.Healer)
            {
                if (skillValue < 55)
                    entity.AddSpell(GetClassicSpell(97)); //Balyna's Balm
                else if (skillValue < 80 && Dice100.SuccessRoll(50))
                    entity.AddSpell(GetClassicSpell(24)); //Troll's Blood
                else if (skillValue < 80)
                    entity.AddSpell(GetClassicSpell(103)); //Balyna's Salve
                else
                    entity.AddSpell(GetClassicSpell(64)); //Heal

                if (skillValue > 50 && Dice100.SuccessRoll(skillValue))
                    entity.AddSpell(healAreaSpell);
            }
            else
            {
                if (skillValue > 35 && skillValue < 55 && Dice100.SuccessRoll(skillValue))
                    entity.AddSpell(GetClassicSpell(97)); //Balyna's Balm
                else if (skillValue > 54 && skillValue < 80 && Dice100.SuccessRoll(skillValue))
                    entity.AddSpell(GetClassicSpell(103)); //Balyna's Salve
                else if (skillValue > 79 && Dice100.SuccessRoll(40))
                    entity.AddSpell(GetClassicSpell(64)); //Heal
            }

            if (entity.Career.SpellAbsorption == DFCareer.SpellAbsorptionFlags.None)
            {
                if (skillValue > 81 && Dice100.SuccessRoll(40))
                    entity.AddSpell(GetClassicSpell(47)); //Spell Absorption
            }

        }


        static void AddThaumaturgySpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 50 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(4)); //Levitate

            if (skillValue > 87 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(22)); //Spell Shield
            else if (skillValue > 75 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(39)); //Spell Resistance

            if (skillValue > 93 && Dice100.SuccessRoll(50))
                entity.AddSpell(GetClassicSpell(46)); //Spell Reflection
        }


        static void AddUnleveledThaumaturgySpells(EnemyEntity entity, int skillValue)
        {
            if (skillValue > 55 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(4)); //Levitate

            if (skillValue > 87 && Dice100.SuccessRoll(60))
                entity.AddSpell(GetClassicSpell(22)); //Spell Shield
            else if (skillValue > 75 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(39)); //Spell Resistance
            else if (skillValue > 60 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(30)); //Shalidor's Mirror (brief spell reflection)

            if (skillValue > 93 && Dice100.SuccessRoll(40))
                entity.AddSpell(GetClassicSpell(46)); //Spell Reflection
        }


        static EffectBundleSettings GetClassicSpell(int spellId)
        {
            if (GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(spellId, out SpellRecord.SpellRecordData spellData))
            {
                GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spellData, BundleTypes.Spell, out EffectBundleSettings bundle);
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
            Name = "Chameleon True",
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
            Name = "Shadow True",
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
            Name = "Invisibility True",
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
            Name = "Magical Light",
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
                properties.SupportDuration = false;
                properties.SupportChance = false;
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



