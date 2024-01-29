// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

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
using DaggerfallWorkshop.Game.Utility.ModSupport;

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

        bool isDaggerfallEnemyExpansionInstalled;

        static float lastMessageTime; //??????????????????????????????????????????

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
            enemySenses.CanDetectOtherwiseHandler = CanSeePlayerLight;


            isDaggerfallEnemyExpansionInstalled = ModManager.Instance.GetMod("Daggerfall Enemy Expansion") != null;


            SetSenseValues();

        }


        void FixedUpdate()
        {
            isWary = enemySenses.DetectedTarget && !enemySenses.TargetInSight;

            enemySenses.FieldOfView = isWary ? fieldOfViewWary : fieldOfViewStandard;
        }


        void SetSenseValues()
        {
            sightRadius = enemySenses.SightRadius;
            hearingRadius = 16;

            switch (enemyEntity.MobileEnemy.ID)
            {
                case (int)MobileTypes.Rat:
                    sightAcuity = 0.80f;
                    fieldOfViewStandard = 260;
                    fieldOfViewWary = 260;
                    hearingRadius = 24;
                    sightRadius = 15;
                    break;
                case (int)MobileTypes.Imp:
                    sightAcuity = 2.5f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 18;
                    break;
                case (int)MobileTypes.Spriggan:
                    sightAcuity = 0.8f;
                    fieldOfViewStandard = 160;
                    fieldOfViewWary = 190;
                    hearingRadius = 30;
                    sightRadius = 35;
                    break;
                case (int)MobileTypes.GiantBat:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 100;
                    fieldOfViewWary = 100;
                    hearingRadius = 19;
                    sightRadius = 14;
                    break;
                case (int)MobileTypes.GrizzlyBear:
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 190;
                    hearingRadius = 24;
                    break;
                case (int)MobileTypes.SabertoothTiger:
                    sightAcuity = 1.25f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 190;
                    hearingRadius = 25;
                    break;
                case (int)MobileTypes.Spider:
                    sightAcuity = 1.2f;
                    fieldOfViewStandard = 220;
                    fieldOfViewWary = 220;
                    hearingRadius = 20;
                    sightRadius = 14;
                    break;
                case (int)MobileTypes.Orc:
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 17;
                    break;
                case (int)MobileTypes.Centaur:
                    sightAcuity = 1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 16;
                    break;
                case (int)MobileTypes.Werewolf:
                    sightAcuity = 1.2f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 210;
                    hearingRadius = 22;
                    break;
                case (int)MobileTypes.Nymph:
                    sightAcuity = 1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 16;
                    break;
                case (int)MobileTypes.Slaughterfish:
                    sightAcuity = 0.7f;
                    fieldOfViewStandard = 220;
                    fieldOfViewWary = 220;
                    hearingRadius = 29;
                    sightRadius = 15;
                    break;
                case (int)MobileTypes.OrcSergeant:
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 17;
                    break;
                case (int)MobileTypes.Harpy:
                    sightAcuity = 1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 18;
                    break;
                case (int)MobileTypes.Wereboar:
                    sightAcuity = 0.8f;
                    fieldOfViewStandard = 220;
                    fieldOfViewWary = 250;
                    hearingRadius = 26;
                    sightRadius = 24;
                    break;
                case (int)MobileTypes.SkeletalWarrior:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 11;
                    sightRadius = 11;
                    break;
                case (int)MobileTypes.Giant:
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 14;
                    break;
                case (int)MobileTypes.Zombie:
                    sightAcuity = 0.7f;
                    fieldOfViewStandard = 160;
                    fieldOfViewWary = 160;
                    hearingRadius = 12;
                    sightRadius = 25;
                    break;
                case (int)MobileTypes.Ghost:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 190;
                    hearingRadius = 16;
                    break;
                case (int)MobileTypes.Mummy:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 12;
                    sightRadius = 15;
                    break;
                case (int)MobileTypes.GiantScorpion:
                    sightAcuity = 0.85f;
                    fieldOfViewStandard = 250;
                    fieldOfViewWary = 250;
                    hearingRadius = 30;
                    sightRadius = 10;
                    break;
                case (int)MobileTypes.OrcShaman:
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 17;
                    break;
                case (int)MobileTypes.Gargoyle:
                    sightAcuity = 1.35f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 240;
                    hearingRadius = 19;
                    break;
                case (int)MobileTypes.Wraith:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 170;
                    fieldOfViewWary = 170;
                    hearingRadius = 16;
                    break;
                case (int)MobileTypes.OrcWarlord:
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 17;
                    break;
                case (int)MobileTypes.FrostDaedra:
                    sightAcuity = 3f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 230;
                    hearingRadius = 26;
                    break;
                case (int)MobileTypes.FireDaedra:
                    sightAcuity = 3f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 230;
                    hearingRadius = 26;
                    break;
                case (int)MobileTypes.Daedroth:
                    sightAcuity = 3f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 230;
                    hearingRadius = 26;
                    break;
                case (int)MobileTypes.Vampire:
                    sightAcuity = 1.55f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 22;
                    break;
                case (int)MobileTypes.DaedraSeducer:
                    sightAcuity = 3.5f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 230;
                    hearingRadius = 30;
                    break;
                case (int)MobileTypes.VampireAncient:
                    sightAcuity = 2f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 22;
                    break;
                case (int)MobileTypes.DaedraLord:
                    sightAcuity = 4f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 230;
                    hearingRadius = 32;
                    break;
                case (int)MobileTypes.Lich:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 15;
                    sightRadius = 18;
                    break;
                case (int)MobileTypes.AncientLich:
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 15;
                    sightRadius = 25;
                    break;
                case (int)MobileTypes.Dragonling:
                    sightAcuity = 1.7f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 240;
                    hearingRadius = 30;
                    break;
                case (int)MobileTypes.Dragonling_Alternate:
                    sightAcuity = 2.0f;
                    fieldOfViewStandard = 200;
                    fieldOfViewWary = 240;
                    hearingRadius = 32;
                    break;
                case (int)MobileTypes.FireAtronach:
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 180;
                    hearingRadius = 14;
                    break;
                case (int)MobileTypes.IronAtronach:
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 180;
                    hearingRadius = 14;
                    break;
                case (int)MobileTypes.FleshAtronach:
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 180;
                    hearingRadius = 15;
                    break;
                case (int)MobileTypes.IceAtronach:
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 180;
                    hearingRadius = 14;
                    break;
                case (int)MobileTypes.Dreugh:
                    sightAcuity = 0.8f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 29;
                    sightRadius = 15;
                    break;
                case (int)MobileTypes.Lamia:
                    sightAcuity = 0.8f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 29;
                    sightRadius = 15;
                    break;
                default:
                    sightAcuity = 1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 230;
                    hearingRadius = 16;
                    sightRadius = enemySenses.SightRadius;

                    if (isDaggerfallEnemyExpansionInstalled)
                        SetExpandedEnemySenseValues();

                    break;
            }

            enemySenses.SightRadius = sightRadius;
            enemySenses.HearingRadius = hearingRadius;
            enemySenses.FieldOfView = fieldOfViewStandard;
        }


        /// <summary>
        /// Check for other entity types if Daggerfall Enemy Expansion is installed.
        /// </summary>
        void SetExpandedEnemySenseValues()
        {
            switch (enemyEntity.MobileEnemy.ID)
            {

                case 256: //Goblin - Daggerfall Enemy Expansion
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 230;
                    hearingRadius = 18;
                    break;
                case 257: //Homunuculus - Daggerfall Enemy Expansion
                    sightAcuity = 2f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 18;
                    break;
                case 258: //Lizardman - Daggerfall Enemy Expansion
                    sightAcuity = 0.85f;
                    fieldOfViewStandard = 220;
                    fieldOfViewWary = 260;
                    hearingRadius = 15;
                    break;
                case 259: //Lizardman Warrior - Daggerfall Enemy Expansion
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 220;
                    fieldOfViewWary = 260;
                    hearingRadius = 17;
                    break;
                case 260: //Bat - Daggerfall Enemy Expansion
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 100;
                    fieldOfViewWary = 100;
                    hearingRadius = 16;
                    sightRadius = 12;
                    break;
                case 261: //Medusa - Daggerfall Enemy Expansion
                    sightAcuity = 1.2f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 20;
                    break;
                case 262: //Wolf - Daggerfall Enemy Expansion
                    sightAcuity = 1.3f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 210;
                    hearingRadius = 30;
                    break;
                case 263: //Snow Wolf - Daggerfall Enemy Expansion
                    sightAcuity = 1.3f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 210;
                    hearingRadius = 30;
                    break;
                case 264: //HellHound - Daggerfall Enemy Expansion
                    sightAcuity = 1.45f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 28;
                    break;
                case 265: //Grotesque - Daggerfall Enemy Expansion
                    sightAcuity = 1.25f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 23;
                    break;
                case 266: //Skeletal Soldier - Daggerfall Enemy Expansion
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 11;
                    sightRadius = 11;
                    break;
                case 267: //Dog - Daggerfall Enemy Expansion
                    sightAcuity = 1.2f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 210;
                    hearingRadius = 28;
                    break;
                case 268: //Nymph - Daggerfall Enemy Expansion
                    sightAcuity = 1f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 16;
                    break;
                case 269: //Minotaur - Daggerfall Enemy Expansion
                    sightAcuity = 1.15f;
                    fieldOfViewStandard = 210;
                    fieldOfViewWary = 240;
                    hearingRadius = 19;
                    break;
                case 270: //Iron Golem - Daggerfall Enemy Expansion
                    sightAcuity = 0.75f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 180;
                    hearingRadius = 13;
                    sightRadius = 20;
                    break;
                case 271: //Blood Spider - Daggerfall Enemy Expansion
                    sightAcuity = 1.6f;
                    fieldOfViewStandard = 300;
                    fieldOfViewWary = 300;
                    hearingRadius = 13;
                    sightRadius = 19;
                    break;
                case 272: //Troll - Daggerfall Enemy Expansion
                    sightAcuity = 1.15f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 220;
                    hearingRadius = 24;
                    break;
                case 273: //Gloom Wraith - Daggerfall Enemy Expansion
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 180;
                    hearingRadius = 18;
                    break;
                case 274: //Faded Ghost - Daggerfall Enemy Expansion
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 160;
                    fieldOfViewWary = 160;
                    hearingRadius = 15;
                    sightRadius = 25;
                    break;
                case 275: //Vengeful King Lysandius - Daggerfall Enemy Expansion
                    sightAcuity = float.MaxValue;
                    fieldOfViewStandard = 210;
                    fieldOfViewWary = 210;
                    hearingRadius = 25;
                    break;
                case 276: //Fire Daemon - Daggerfall Enemy Expansion
                    sightAcuity = 2.5f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 220;
                    hearingRadius = 29;
                    break;
                case 277: //Ghoul - Daggerfall Enemy Expansion
                    sightAcuity = 1.25f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 220;
                    hearingRadius = 20;
                    break;
                case 278: //Boar - Daggerfall Enemy Expansion
                    sightAcuity = 0.8f;
                    fieldOfViewStandard = 240;
                    fieldOfViewWary = 240;
                    hearingRadius = 28;
                    break;
                case 279:  //Land Dreugh - Daggerfall Enemy Expansion
                    sightAcuity = 0.85f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 210;
                    hearingRadius = 27;
                    break;
                case 280:  //Mountain Lion - Daggerfall Enemy Expansion
                    sightAcuity = 1.35f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 190;
                    hearingRadius = 28;
                    break;
                case 281:  //Mudcrab - Daggerfall Enemy Expansion
                    sightAcuity = 0.9f;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 14;
                    sightRadius = 13;
                    break;
                case 282:  //Ogre - Daggerfall Enemy Expansion
                    sightAcuity = 1.1f;
                    fieldOfViewStandard = 180;
                    fieldOfViewWary = 220;
                    hearingRadius = 15;
                    break;
                case 283:  //Wisp - Daggerfall Enemy Expansion
                    sightAcuity = 2.5f;
                    fieldOfViewStandard = 360;
                    fieldOfViewWary = 360;
                    hearingRadius = 5;
                    sightRadius = 26;
                    break;
                case 284: //Ice Golem - Daggerfall Enemy Expansion
                    sightAcuity = 0.75f;
                    fieldOfViewStandard = 170;
                    fieldOfViewWary = 170;
                    hearingRadius = 14;
                    sightRadius = 25;
                    break;
                case 285: //Dremora Churl - Daggerfall Enemy Expansion
                    sightAcuity = 2f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 230;
                    hearingRadius = 25;
                    break;
                case 286: //Stone Golem - Daggerfall Enemy Expansion
                    sightAcuity = 0.75f;
                    fieldOfViewStandard = 170;
                    fieldOfViewWary = 170;
                    hearingRadius = 14;
                    sightRadius = 20;
                    break;
                case 287: //Dire Ghoul - Daggerfall Enemy Expansion
                    sightAcuity = 1.5f;
                    fieldOfViewStandard = 190;
                    fieldOfViewWary = 240;
                    hearingRadius = 22;
                    break;
            }
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

            //Certain enemies, like skeletal warriors, automatically see everything in their sight range.
            if (sightAcuity == float.MaxValue)
                return true;

            //Chance of automatically spotting, if close enough and in field-of-view.
            if (distance < 2f && Dice100.SuccessRoll(30))
                return true;

            bool khajiitSuit = false;
            DaggerfallUnityItem legs = target.Entity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);
            if (legs != null && (legs.TemplateIndex == (int)MensClothing.Khajiit_suit || legs.TemplateIndex == (int)WomensClothing.Khajiit_suit))
            {
                ItemEquipTable equipTable = target.Entity.ItemEquipTable;
                if (equipTable.GetItem(EquipSlots.ChestArmor) == null && equipTable.GetItem(EquipSlots.ChestClothes) == null)
                    khajiitSuit = true;
            }

            //Moving across field of vision? How much?
            Vector3 movement = target.GetComponent<CharacterController>().velocity;
            float angle = Mathf.Max(Vector3.Angle(direction, movement.normalized), 10);
            float movementModifier = 1 + Mathf.Sin(angle * Mathf.Deg2Rad) * movement.magnitude / 2.5f;

            float warinessModifier = isWary ? 1.5f : 1;

            float threshold = Mathf.Log10(distance) / 20 / warinessModifier / sightAcuity;

            //A target with high stealth is somewhat less visible.
            float stealthSkill = target.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Stealth);
            float stealthModifier = 1.5f / Mathf.Log10(stealthSkill);

            float crouchingModifier = 1;
            if (target.EntityType == EntityTypes.Player && GameManager.Instance.PlayerMotor.IsCrouching)
                crouchingModifier = 0.85f;

            Color tColor;
            if (target.Entity.IsAShade)
                tColor = new Color(0, 0, 0, 0.2f);
            else if (khajiitSuit)
                tColor = new Color(0.25f, 0.25f, 0.25f, 1);
            else
                tColor = Color.gray;

            if (target.Entity.IsInvisible)
                tColor.a = 0.01f;
            else if (target.Entity.IsBlending)
                tColor.a = 0.03f;
            else if (!target.Entity.IsAShade)
                tColor *= MonsterUniversityMod.Instance.GetLightingOnEntity(target);

            //The player is considered well-lit while casting spells.
            if (target == GameManager.Instance.PlayerEntityBehaviour && GameManager.Instance.PlayerSpellCasting.IsPlayingAnim)
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


            //Basic vision check failed, now compare target to the background.
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

            Color bgColor = bgLighting * 0.5f;

            //Calculate the effective color of the target...
            float tr = tColor.r * tColor.a + bgColor.r * (1 - tColor.a);
            float tg = tColor.g * tColor.a + bgColor.g * (1 - tColor.a);
            float tb = tColor.b * tColor.a + bgColor.b * (1 - tColor.a);

            //...and contrast it with the background
            float redDiff = Mathf.Abs(tr - bgColor.r);
            float greenDiff = Mathf.Abs(tg - bgColor.g);
            float blueDiff = Mathf.Abs(tb - bgColor.b);

            float contrast = redDiff + greenDiff + blueDiff;

            visibility = contrast * stealthModifier * movementModifier;

            //????????????????????
            if (false && Time.time > lastMessageTime + 3)
            {
                if (target == GameManager.Instance.PlayerEntityBehaviour)
                {
                    lastMessageTime = Time.time;
                    if (visibility > threshold)
                        DaggerfallUI.AddHUDText("silhouette seen (" + visibility + " > " + threshold + ")", 2);
                    else
                        DaggerfallUI.AddHUDText("silhouette NOT seen (" + visibility + " > " + threshold + ")", 2);
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
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (GameObjectHelper.IsStaticGeometry(hit.transform.gameObject))
                    return false;
            }

            float noise = GetNoise();

            if (enemyEntity.Career.AcuteHearing)
                noise *= enemyEntity.ImprovedAcuteHearing ? 1.5f : 1.3f;

            float result = Mathf.Sqrt(hearingRadius - enemySenses.DistanceToTarget + 1) * noise;

            //???????????????????????????????????????????????????
            if (Time.time > lastMessageTime + 3)
            {
                if (enemySenses.Target == GameManager.Instance.PlayerEntityBehaviour)
                {
                    lastMessageTime = Time.time;
                    if (result > 6f)
                        DaggerfallUI.AddHUDText("heard (" + result + " > 6)", 2);
                    else
                        DaggerfallUI.AddHUDText("NOT heard (" + result + " <= 6)", 2);
                }
            }


            return result > 6f;
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
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, targetMask))
                return hit.point - (eyeDirectionToTarget * 0.01f);
            else
                return Vector3.zero; //This might happen if outside looking at sky.
        }


        static readonly EquipSlots[] armorSlots =
{
            EquipSlots.ChestArmor, EquipSlots.Feet, EquipSlots.Gloves, EquipSlots.Head,
            EquipSlots.LeftArm, EquipSlots.LegsArmor, EquipSlots.RightArm
        };

        /// <summary>
        /// Calculate how much noise the target is making, based on armor worn, movement, stealth, etc.
        /// </summary>
        float GetNoise()
        {
            float loudness = 1f;

            bool grounded = enemySenses.Target.GetComponent<CharacterController>().isGrounded;

            ItemEquipTable equipTable = enemySenses.Target.Entity.ItemEquipTable;
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
            float movement = 1 + enemySenses.Target.GetComponent<CharacterController>().velocity.magnitude;
            float noise = loudness * movement / 2;

            //Stealth adjustment
            float stealth = enemySenses.Target.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Stealth);
            noise *= (120.0f - stealth) / 100.0f;

            //Always some effective noise equivalent, i.e. air currents, body heat, odor, etc.
            noise = Mathf.Clamp(noise, 1, 20);

            return noise;
        }


        /// <summary>
        ///  Checks if this entity notices the light carried/used by the player, if any.
        ///  The player should be automatically detected if using a light and the entity is within the light radius.
        /// </summary>
        bool CanSeePlayerLight(DaggerfallEntityBehaviour target)
        {
            //In case another mod is also employing logic, give them a chance to detect.
            if (originalCanDetectOtherwiseCallback(target) == true)
                return true;

            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            DaggerfallEntityBehaviour player = GameManager.Instance.PlayerEntityBehaviour;

            if (target != player)
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
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Min(sightRadius, lightRange)))
                return hit.collider == GameManager.Instance.PlayerController;
            else
                return false;

        }


    } //class EnemySensesEnhancement




} //namespace
