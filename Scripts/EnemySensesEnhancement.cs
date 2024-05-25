// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace MonsterUniversity
{
    public class EnemySensesEnhancement : MonoBehaviour
    {
        EnemyEntity enemyEntity;
        EnemySenses enemySenses;
        bool isWary;

        EnemySenses.CanSeeTargetCallback originalCanSeeTargetCallback;
        EnemySenses.CanDetectOtherwiseCallback originalCanDetectOtherwiseCallback;

        float sightAcuity;
        float fieldOfViewStandard;
        float fieldOfViewWary;
        float hearingRadius;
        float sightRadius;

        static float lastMessageTime; //??????????????????????????????????????????

        readonly Dictionary<int, float[]> enemySenseValues = new Dictionary<int, float[]>()
        {
            //mobile type                                      sightAcuity       stdFOV   waryFOV   hearRadius sightRadius
            { (int)MobileTypes.Rat,                  new float[] { 0.80f,          270,     270,       24,        24 }},
            { (int)MobileTypes.Imp,                  new float[] { 2.70f,          190,     220,       20,        30 }},
            { (int)MobileTypes.Spriggan,             new float[] { 0.65f,          160,     160,       30,        25 }},
            { (int)MobileTypes.GiantBat,             new float[] { float.MaxValue, 100,     100,       19,        16 }},
            { (int)MobileTypes.GrizzlyBear,          new float[] { 1.10f,          190,     190,       26,        70 }},
            { (int)MobileTypes.SabertoothTiger,      new float[] { 1.45f,          180,     180,       23,        75 }},
            { (int)MobileTypes.Spider,               new float[] { 0.80f,          210,     210,       26,        24 }},
            { (int)MobileTypes.Orc,                  new float[] { 1.10f,          190,     220,       17,        50 }},
            { (int)MobileTypes.Centaur,              new float[] { 1.10f,          190,     220,       16,        65 }},
            { (int)MobileTypes.Werewolf,             new float[] { 1.20f,          190,     220,       25,        75 }},
            { (int)MobileTypes.Nymph,                new float[] { 1.00f,          190,     220,       16,        18 }},
            { (int)MobileTypes.Slaughterfish,        new float[] { 0.60f,          220,     220,       34,        20 }},
            { (int)MobileTypes.OrcSergeant,          new float[] { 1.10f,          190,     230,       17,        70 }},
            { (int)MobileTypes.Harpy,                new float[] { 1.00f,          190,     220,       16,        50 }},
            { (int)MobileTypes.Wereboar,             new float[] { 0.85f,          260,     300,       27,        45 }},
            { (int)MobileTypes.SkeletalWarrior,      new float[] { float.MaxValue, 360,     360,       12,        12 }},
            { (int)MobileTypes.Giant,                new float[] { 0.90f,          190,     220,       14,        75 }},
            { (int)MobileTypes.Zombie,               new float[] { 0.65f,          160,     260,       12,        25 }},
            { (int)MobileTypes.Ghost,                new float[] { float.MaxValue, 190,     190,       15,        28 }},
            { (int)MobileTypes.Mummy,                new float[] { float.MaxValue, 360,     360,       12,        14 }},
            { (int)MobileTypes.GiantScorpion,        new float[] { 0.80f,          220,     220,       27,        24 }},
            { (int)MobileTypes.OrcShaman,            new float[] { 1.10f,          190,     220,       19,        45 }},
            { (int)MobileTypes.Gargoyle,             new float[] { 1.80f,          190,     240,       25,        45 }},
            { (int)MobileTypes.Wraith,               new float[] { float.MaxValue, 170,     170,       18,        33 }},
            { (int)MobileTypes.OrcWarlord,           new float[] { 1.20f,          190,     230,       20,        70 }},
            { (int)MobileTypes.FrostDaedra,          new float[] { 3.80f,          200,     210,       34,        75 }},
            { (int)MobileTypes.FireDaedra,           new float[] { 4.10f,          200,     225,       32,        80 }},
            { (int)MobileTypes.Daedroth,             new float[] { 2.80f,          220,     240,       26,        60 }},
            { (int)MobileTypes.Vampire,              new float[] { 1.95f,          190,     220,       27,        65 }},
            { (int)MobileTypes.DaedraSeducer,        new float[] { 4.80f,          190,     220,       40,        70 }},
            { (int)MobileTypes.VampireAncient,       new float[] { 3.30f,          190,     220,       38,        80 }},
            { (int)MobileTypes.DaedraLord,           new float[] { 6.20f,          190,     220,       49,        90 }},
            { (int)MobileTypes.Lich,                 new float[] { float.MaxValue, 360,     360,       15,        19 }},
            { (int)MobileTypes.AncientLich,          new float[] { float.MaxValue, 360,     360,       23,        26 }},
            { (int)MobileTypes.Dragonling,           new float[] { 2.85f,          200,     240,       32,        80 }},
            { (int)MobileTypes.FireAtronach,         new float[] { 0.85f,          180,     200,       14,        35 }},
            { (int)MobileTypes.IronAtronach,         new float[] { 0.85f,          180,     200,       14,        35 }},
            { (int)MobileTypes.FleshAtronach,        new float[] { 0.85f,          180,     200,       15,        35 }},
            { (int)MobileTypes.IceAtronach,          new float[] { 0.85f,          180,     200,       14,        35 }},
            //Horse_Invalid
            { (int)MobileTypes.Dragonling_Alternate, new float[] { 3.15f,          200,     240,       36,        90 }},
            { (int)MobileTypes.Dreugh,               new float[] { 0.75f,          190,     220,       28,        35 }},
            { (int)MobileTypes.Lamia,                new float[] { 0.80f,          190,     220,       29,        30 }},

            //----Enemy Expansion mod
            //mobileID          sightAcuity      stdFOV   waryFOV   hearRadius sightRadius
            { 256,   new float[] { 1.20f,          190,     240,       18,        35 }}, //Goblin
            { 257,   new float[] { 1.70f,          190,     220,       20,        25 }}, //Homunuculus
            { 258,   new float[] { 0.90f,          220,     260,       17,        40 }}, //Lizardman
            { 259,   new float[] { 0.95f,          220,     260,       19,        60 }}, //Lizardman Warrior
            { 260,   new float[] { float.MaxValue, 100,     120,       20,        12 }}, //Bat
            { 261,   new float[] { 1.20f,          190,     220,       24,        70 }}, //Medusa
            { 262,   new float[] { 1.40f,          190,     190,       29,        70 }}, //Wolf
            { 263,   new float[] { 1.35f,          190,     210,       28,        70 }}, //Snow Wolf
            { 264,   new float[] { 1.65f,          180,     220,       28,        80 }}, //HellHound
            { 265,   new float[] { 1.65f,          190,     220,       25,        40 }}, //Grotesque
            { 266,   new float[] { float.MaxValue, 360,     360,       11,        13 }}, //Skeletal Soldier
            { 267,   new float[] { 1.20f,          190,     190,       25,        45 }}, //Dog
            { 268,   new float[] { 1.00f,          190,     220,       16,        18 }}, //Nymph
            { 269,   new float[] { 0.90f,          250,     280,       24,        50 }}, //Minotaur
            { 270,   new float[] { 0.70f,          180,     180,       14,        25 }}, //Iron Golem
            { 271,   new float[] { 1.30f,          240,     240,       27,        35 }}, //Blood Spider
            { 272,   new float[] { 1.15f,          180,     220,       23,        70 }}, //Troll
            { 273,   new float[] { float.MaxValue, 180,     180,       16,        25 }}, //Gloom Wraith
            { 274,   new float[] { float.MaxValue, 140,     160,       13,        16 }}, //Faded Ghost
            { 275,   new float[] { float.MaxValue, 190,     210,       16,        60 }}, //Vengeful King Lysandius
            { 276,   new float[] { 2.25f,          180,     220,       30,        80 }}, //Fire Daemon
            { 277,   new float[] { 1.50f,          190,     220,       22,        65 }}, //Ghoul
            { 278,   new float[] { 0.85f,          260,     260,       26,        30 }}, //Boar
            { 279,   new float[] { 0.90f,          180,     210,       25,        55 }}, //Land Dreugh
            { 280,   new float[] { 1.35f,          190,     190,       27,        70 }}, //Mountain Lion
            { 281,   new float[] { 0.70f,          300,     300,       13,        11 }}, //Mudcrab
            { 282,   new float[] { 1.15f,          180,     210,       18,        65 }}, //Ogre
            { 283,   new float[] { 2.80f,          360,     360,       12,        28 }}, //Wisp
            { 284,   new float[] { 0.75f,          170,     170,       14,        23 }}, //Ice Golem
            { 285,   new float[] { 1.50f,          190,     230,       21,        50 }}, //Dremora Churl
            { 286,   new float[] { 0.65f,          170,     170,       17,        25 }}, //Stone Golem
            { 287,   new float[] { 1.70f,          190,     230,       27,        70 }}, //Dire Ghoul
            { 288,   new float[] { 1.55f,          190,     240,       23,        14 }}, //Scamp
            { 289,   new float[] { 0.95f,          180,     180,       14,        15 }}, //Dwarven Sphere
            { 290,   new float[] { 0.95f,          180,     180,       14,        35 }}, //Dwarven Steam
        };


        static readonly EquipSlots[] armorSlots =
{
            EquipSlots.ChestArmor, EquipSlots.Feet, EquipSlots.Gloves, EquipSlots.Head,
            EquipSlots.LeftArm, EquipSlots.LegsArmor, EquipSlots.RightArm
        };



        /// <summary>
        /// Calculate how much noise the target is making, based on armor worn, movement, stealth, etc.
        /// </summary>
        public static float GetNoise(DaggerfallEntityBehaviour target)
        {
            float loudness = 1f;

            bool grounded = target.GetComponent<CharacterController>().isGrounded;

            ItemEquipTable equipTable = target.Entity.ItemEquipTable;
            foreach (EquipSlots slot in armorSlots)
            {
                DaggerfallUnityItem item = equipTable.GetItem(slot);
                if (item == null || item.ItemGroup != ItemGroups.Armor)
                    continue;

                float armorLoudness = (item.NativeMaterialValue == (int)ArmorMaterialTypes.Leather) ? 0.3f : 0.9f;

                if (slot == EquipSlots.Feet)
                    armorLoudness *= grounded ? 4 : 1;
                else if (slot == EquipSlots.LegsArmor)
                    armorLoudness *= grounded ? 2 : 1;
                else if (slot == EquipSlots.Head)
                    armorLoudness *= 0.5f;

                loudness += armorLoudness;
            }

            //Movement adjustment
            float movement = 1 + target.GetComponent<CharacterController>().velocity.magnitude;
            float noise = loudness * movement / 2;

            //Stealth adjustment
            float stealth = target.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Stealth);
            noise *= (120.0f - stealth) / 100.0f;

            if (stealth == 100)
                noise /= 2; //mastery bonus

            //Always some effective noise equivalent, i.e. air currents, body heat, odor, etc.
            noise = Mathf.Clamp(noise, 1, 20);

            return noise;
        }


        void Start()
        {
            DaggerfallEntityBehaviour behavior = GetComponent<DaggerfallEntityBehaviour>();
            enemyEntity = behavior.Entity as EnemyEntity;
            enemySenses = GetComponent<EnemySenses>();

            enemySenses.BlockedByIllusionEffectHandler = BlockedByIllusionEffect;

            originalCanSeeTargetCallback = enemySenses.CanSeeTargetHandler;
            enemySenses.CanSeeTargetHandler = CanSeeTarget;

            enemySenses.CanHearTargetHandler = CanHearTarget;


            originalCanDetectOtherwiseCallback = enemySenses.CanDetectOtherwiseHandler;
            enemySenses.CanDetectOtherwiseHandler = CanDetectOtherwise;

            SetSenseValues();
        }


        void Update()
        {
            if (GameManager.IsGamePaused)
                return;

            isWary = enemySenses.DetectedTarget && !enemySenses.TargetInSight;

            enemySenses.FieldOfView = isWary ? fieldOfViewWary : fieldOfViewStandard;

            //Periodically check if player's light source gets them noticed.
            if (Time.frameCount % 40 == 0)
            {
                if (enemySenses.Target == GameManager.Instance.PlayerEntityBehaviour)
                {
                    if (CanSeePlayerLight())
                    {
                        enemySenses.DetectedTarget = true;
                        enemySenses.LastKnownTargetPos = GameManager.Instance.PlayerEntityBehaviour.transform.position;
                        enemySenses.PredictedTargetPos = enemySenses.LastKnownTargetPos;
                    }
                }
            }
        }


        void SetSenseValues()
        {
            //Default to typical human class values
            sightAcuity = 1f;
            sightRadius = enemySenses.SightRadius;
            hearingRadius = 16;
            fieldOfViewStandard = 180;
            fieldOfViewWary = 220;

            //Check if the entity exists in the senses table.
            if (enemySenseValues.TryGetValue(enemyEntity.MobileEnemy.ID, out float[] values))
            {
                sightAcuity = values[0];
                fieldOfViewStandard = values[1];
                fieldOfViewWary = values[2];
                hearingRadius = values[3];
                sightRadius = values[4];
            }

            enemySenses.SightRadius = sightRadius;
            enemySenses.HearingRadius = hearingRadius;
            enemySenses.FieldOfView = fieldOfViewStandard;
        }




        /// <summary>
        /// Modified version of BlockedByIllusion.
        /// Concealment spells are handled differently in Monster University.
        /// </summary>
        bool BlockedByIllusionEffect()
        {
            return false;
        }


        /// <summary>
        /// Modified version of CanSeeTarget() to take lighting and other factors into account.
        /// </summary>
        bool CanSeeTarget(DaggerfallEntityBehaviour target)
        {
            Vector3 v = transform.position - target.transform.position;
            Vector3 direction = v.normalized;
            float distance = v.magnitude;

            if (distance < 1)
                return true;

            //Check basic line-of-sight first
            if (!originalCanSeeTargetCallback(target))
                return false;

            DaggerfallEntityBehaviour playerBehaviour = GameManager.Instance.PlayerEntityBehaviour;


            //Holy candles can block the vision of undead enemies if the target is within its radius.
            if (enemyEntity.Team == MobileTeams.Undead)
            {
                PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                if (playerEntity.LightSource != null && playerEntity.LightSource.TemplateIndex == (int)ReligiousItems.Holy_candle)
                    if (Vector3.Distance(target.transform.position, playerBehaviour.transform.position) < 5)
                        return false;
            }

            //Certain enemies, like skeletal warriors, automatically see everything in their sight range.
            if (sightAcuity == float.MaxValue)
                return true;

            //Automatically spotted if close enough and in field-of-view.
            if (distance < 2.1f)
                return true;

            //Moving across field of vision? How much?
            Vector3 movement = target.GetComponent<CharacterController>().velocity;
            float angle = Mathf.Max(Vector3.Angle(direction, movement.normalized), 10);
            float movementModifier = 1 + Mathf.Sin(angle * Mathf.Deg2Rad) * movement.magnitude / 2.5f;

            float warinessModifier = isWary ? 1.5f : 1;

            float threshold = Mathf.Log10(distance) / 35 / warinessModifier / sightAcuity;

            //A target with high stealth is somewhat less visible.
            float stealthSkill = target.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Stealth);
            float stealthModifier = 1.5f / Mathf.Log10(stealthSkill);

            float crouchingModifier = 1;
            if (target.EntityType == EntityTypes.Player && GameManager.Instance.PlayerMotor.IsCrouching)
                crouchingModifier = 0.8f;

            Color tColor;
            if (target.Entity.IsAShade)
                tColor = new Color(0, 0, 0, 0.08f);
            else if (target.EntityType == EntityTypes.Player)
                tColor = MonsterUniversityMod.Instance.PaperDollAverageColor;
            else
                tColor = new Color(0.3f, 0.3f, 0.3f, 1f);

            Color tLighting = MonsterUniversityMod.Instance.GetLightingOnEntity(target);

            if (target.Entity.IsInvisible)
                tColor.a = (1 - tLighting.grayscale) / 25; //invisibility works better in lit environments
            else if (target.Entity.IsBlending)
                tColor.a = (1 - tLighting.grayscale) / 10; //chameleon works better in lit environments
            else if (!target.Entity.IsAShade)
                tColor *= tLighting;

            //The player is considered well-lit while casting spells.
            if (target == playerBehaviour && GameManager.Instance.PlayerSpellCasting.IsPlayingAnim)
                tColor = Color.gray;

            //If target is readily visible, return true.
            float visibility = tColor.grayscale * tColor.a * stealthModifier * movementModifier * crouchingModifier;

            //????????????????????????????????????????????????????????
            if (Time.time > lastMessageTime + 3)
            {
                if (visibility > threshold && target == GameManager.Instance.PlayerEntityBehaviour)
                {
                    lastMessageTime = Time.time;
                    DaggerfallUI.AddHUDText("seen (" + visibility + " > " + threshold + ")", 2);
                }
            }

            if (visibility > threshold)
            {
                return true;
            }


            //===========Basic vision check failed, now compare target to the background.
            Vector3 background = GetTargetBackground(target);

            Color bgLighting;
            if (background == Vector3.zero)
            {
                //If background not hit, we are probably outside looking at the sky.
                SunlightManager sunLight = GameManager.Instance.SunlightManager;
                bgLighting = Color.white * (sunLight.DaylightScale * sunLight.ScaleFactor);
            }
            else
            {
                //Get the light behind the target.
                bgLighting = MonsterUniversityMod.Instance.GetLightingAtLocation(background);
            }

            Color bgColor = bgLighting * 0.2f; //assuming background color is dark gray

            //Calculate the effective color of the target...
            float tr = tColor.r * tColor.a + bgColor.r * (1 - tColor.a);
            float tg = tColor.g * tColor.a + bgColor.g * (1 - tColor.a);
            float tb = tColor.b * tColor.a + bgColor.b * (1 - tColor.a);

            //...and contrast it with the background
            float redDiff = Mathf.Abs(tr - bgColor.r);
            float greenDiff = Mathf.Abs(tg - bgColor.g);
            float blueDiff = Mathf.Abs(tb - bgColor.b);

            float contrast = redDiff + greenDiff + blueDiff;

            visibility = contrast * stealthModifier * movementModifier / 4f;

            //????????????????????
            if (Time.time > lastMessageTime + 2)
            {
                if (target == GameManager.Instance.PlayerEntityBehaviour)
                {
                    if (visibility > threshold)
                    {
                        lastMessageTime = Time.time;
                        DaggerfallUI.AddHUDText("silhouette seen (" + visibility + " > " + threshold + ")", 2);
                    }
                }
            }

            return visibility > threshold;
        }


        /// <summary>
        /// Modified version of CanHearTarget() to take armor and movement speed into account.
        /// </summary>
        bool CanHearTarget()
        {
            if (enemySenses.DistanceToTarget > hearingRadius)
                return false;

            // If something is between enemy and target then return false, to minimize
            // enemies walking against walls.
            Ray ray = new Ray(transform.position, enemySenses.DirectionToTarget);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (GameObjectHelper.IsStaticGeometry(hit.transform.gameObject))
                    return false;
            }

            float noise = GetNoise(enemySenses.Target);

            if (enemyEntity.Career.AcuteHearing)
                noise *= enemyEntity.ImprovedAcuteHearing ? 1.5f : 1.3f;

            float result = Mathf.Sqrt(hearingRadius - enemySenses.DistanceToTarget + 1) * noise;

            //???????????????????????????????????????????????????
            if (Time.time > lastMessageTime + 3)
            {
                if (enemySenses.Target == GameManager.Instance.PlayerEntityBehaviour)
                {
                    if (result > 7f)
                    {
                        lastMessageTime = Time.time;
                        DaggerfallUI.AddHUDText("heard (" + result + " > 6)", 2);
                    }
                }
            }


            return result > 7f;
        }


        /// <summary>
        /// Get's location of background terrain in line-of-sight behind target.
        /// </summary>
        Vector3 GetTargetBackground(DaggerfallEntityBehaviour target)
        {
            // Set origin of ray to approximate eye position
            CharacterController controller = GetComponent<CharacterController>();
            Vector3 eyePos = transform.position + controller.center;
            eyePos.y += controller.height / 3;

            // Set destination to the target's upper body
            controller = target.transform.GetComponent<CharacterController>();
            Vector3 targetTorsoPos = target.transform.position + controller.center;
            targetTorsoPos.y += controller.height / 3.5f;

            Vector3 eyeDirectionToTarget = (targetTorsoPos - eyePos).normalized;

            int targetMask = ~(1 << target.gameObject.layer);

            //Looking past target toward background.
            Ray ray = new Ray(targetTorsoPos, eyeDirectionToTarget);
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, targetMask))
                return hit.point - (eyeDirectionToTarget * 0.01f);
            else
                return Vector3.zero; //This might happen if outside looking at sky.
        }


        /// <summary>
        ///  Checks if this entity detects the target via non-standard means.
        /// </summary>
        bool CanDetectOtherwise(DaggerfallEntityBehaviour target)
        {
            //In case another mod is also employing logic, give them a chance to detect.
            if (originalCanDetectOtherwiseCallback(target))
                return true;

            if (target != GameManager.Instance.PlayerEntityBehaviour)
                return false;

            return CanSeePlayerLight();
        }


        /// <summary>
        ///  Checks if this entity notices the light carried/used by the player, if any.
        ///  The player should be automatically detected if using a light and the entity is within the light radius.
        /// </summary>
        bool CanSeePlayerLight()
        {
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            DaggerfallEntityBehaviour player = GameManager.Instance.PlayerEntityBehaviour;

            if (Vector3.Distance(enemySenses.transform.position, player.transform.position) > 20)
                return false;

            float lightRange = 10;

            if (!DaggerfallUnity.Settings.PlayerTorchFromItems)
            {
                EnablePlayerTorch enablePlayertorch = GameManager.Instance.PlayerObject.GetComponent<EnablePlayerTorch>();
                if (enablePlayertorch.PlayerTorch.activeSelf)
                {
                    Light light = enablePlayertorch.PlayerTorch.GetComponent<Light>();
                    lightRange = light ? light.range : 2;
                }
                else
                {
                    return false;
                }
            }
            else if (playerEntity.LightSource == null)
            {
                EntityEffectManager effectManager = player.GetComponent<EntityEffectManager>();
                if (effectManager.FindIncumbentEffect<LightNormal>() == null && effectManager.FindIncumbentEffect<MageLight>() == null)
                    return false;
            }
            else
            {
                lightRange = playerEntity.LightSource.ItemTemplate.capacityOrTarget;
            }

            lightRange -= 2;

            Vector3 direction = (player.transform.position - transform.position).normalized;

            Ray ray = new Ray(transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Min(sightRadius, lightRange)))
                return hit.collider == GameManager.Instance.PlayerController;
            else
                return false;
        }



    } //class EnemySensesEnhancement




} //namespace
