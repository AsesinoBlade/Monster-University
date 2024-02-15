// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;


namespace MonsterUniversity
{

    public static class Equipment
    {

        /// <summary>
        /// Make sure entity has their best stuff equipped, including magic items.
        /// </summary>
        public static void Adjust(EnemyEntity entity)
        {
            //Make sure entity has at least one weapon appropriate to their skills.
            EnsureHasSkilledWeapon(entity);

            //Check if equipped items should be replaced with something better from inventory.
            for (int i = 0; i < entity.Items.Count; ++i)
            {
                DaggerfallUnityItem item = entity.Items.GetItem(i);

                if (ShouldEquip(entity, item))
                {
                    EquipSlots slot = GetSlot(entity.ItemEquipTable, item);
                    if (slot != EquipSlots.None)
                    {
                        entity.ItemEquipTable.UnequipItem(slot);
                        entity.ItemEquipTable.EquipItem(item, true, false);
                    }
                }
            }
        }



        static readonly List<DFCareer.Skills> meleeWeaponSkills = new List<DFCareer.Skills>()
        {
            DFCareer.Skills.Axe, DFCareer.Skills.BluntWeapon, DFCareer.Skills.HandToHand,
            DFCareer.Skills.LongBlade, DFCareer.Skills.ShortBlade
        };

        /// <summary>
        /// Make sure entity has an appropriate weapon in inventory for their skillset.
        /// If not, create one.
        /// </summary>
        /// <param name="entity"></param>
        static void EnsureHasSkilledWeapon(EnemyEntity entity)
        {
            //Determine best melee skill.
            DFCareer.Skills bestSkill = DFCareer.Skills.HandToHand;
            int best = 0;
            foreach (DFCareer.Skills skill in meleeWeaponSkills)
            {
                int value = entity.Skills.GetLiveSkillValue(skill);
                if (value > best)
                {
                    best = value;
                    bestSkill = skill;
                }
            }


            //If best melee weapon skill is hand-to-hand, then disarm.
            if (bestSkill == DFCareer.Skills.HandToHand)
            {
                entity.ItemEquipTable.UnequipItem(EquipSlots.RightHand);
                return;
            }


            //Check if entity already has appropriate weapon in their inventory.
            int requiredSkill = best - 10;
            bool hasAppropriateWeapon = false;
            for (int i = 0; i < entity.Items.Count; ++i)
            {
                DaggerfallUnityItem item = entity.Items.GetItem(i);
                if (meleeWeaponSkills.Contains(item.GetWeaponSkillID()))
                {
                    int skill = entity.Skills.GetLiveSkillValue(item.GetWeaponSkillID());
                    hasAppropriateWeapon |= (skill >= requiredSkill);
                }
            }


            //If no appropriate weapon is in their inventory, create one.
            if (!hasAppropriateWeapon)
            {
                int playerLevel = GameManager.Instance.PlayerEntity.Level;
                int itemLevel = (entity.MobileEnemy.ID == (int)MobileTypes.Knight_CityWatch) ? 1 : playerLevel;

                Weapons weaponType;
                switch (bestSkill)
                {
                    case DFCareer.Skills.Axe: weaponType = Weapons.Battle_Axe; break;
                    case DFCareer.Skills.BluntWeapon: weaponType = Weapons.Mace; break;
                    case DFCareer.Skills.LongBlade: weaponType = Weapons.Broadsword; break;
                    default: weaponType = Weapons.Dagger; break;
                }
                DaggerfallUnityItem weapon = ItemBuilder.CreateWeapon(weaponType, FormulaHelper.RandomMaterial(itemLevel));
                entity.Items.AddItem(weapon);
            }
        }


        /// <summary>
        /// Can item be equipped, and is it item better than what is already equipped?
        /// </summary>
        static bool ShouldEquip(EnemyEntity entity, DaggerfallUnityItem item)
        {
            if (item.IsEquipped)
                return false;

            if (CanEquip(entity, item) == false)
                return false;

            ItemEquipTable equipTable = entity.ItemEquipTable;

            DaggerfallUnityItem rightHand = equipTable.GetItem(EquipSlots.RightHand);
            DaggerfallUnityItem leftHand = equipTable.GetItem(EquipSlots.LeftHand);
            bool usingTwoHandedWeapon = rightHand != null && ItemEquipTable.GetItemHands(rightHand) == ItemHands.Both;

            if (item.ItemGroup == ItemGroups.Weapons)
            {
                int currentValue;
                if (item.GetWeaponSkillID() == DFCareer.Skills.Archery)
                {
                    if (usingTwoHandedWeapon)
                        return false;
                    else if (leftHand == null)
                        return true;

                    currentValue = GetWeaponValue(entity, leftHand);
                }
                else
                {
                    currentValue = GetWeaponValue(entity, rightHand);
                }

                int itemValue = GetWeaponValue(entity, item);

                return itemValue > currentValue;
            }
            else if (item.IsShield)
            {
                if (usingTwoHandedWeapon)
                    return false;
                else if (leftHand != null && leftHand.GetWeaponSkillID() == DFCareer.Skills.Archery)
                    return false;

                int currentValue = GetArmorValue(entity, leftHand);
                int itemValue = GetArmorValue(entity, item);

                return itemValue > currentValue;
            }
            else if (item.ItemGroup == ItemGroups.Armor)
            {
                EquipSlots slot = GetSlot(equipTable, item);
                DaggerfallUnityItem currentArmor = equipTable.GetItem(slot);

                int currentValue = GetArmorValue(entity, currentArmor);
                int itemValue = GetArmorValue(entity, item);

                return itemValue > currentValue;
            }
            else if (item.IsEnchanted)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Which equipment slot does the item belong in?
        /// </summary>
        static EquipSlots GetSlot(ItemEquipTable equipTable, DaggerfallUnityItem item)
        {
            if (item.GetWeaponSkillID() == DFCareer.Skills.Archery)
                return EquipSlots.LeftHand;
            else if (item.ItemGroup == ItemGroups.Weapons)
                return EquipSlots.RightHand;
            else
                return equipTable.GetEquipSlot(item);
        }


        /// <summary>
        /// Approximate how effective a weapon is, given the entity's skillset and weapon attributes.
        /// If item is null, assume hand-to-hand.
        /// </summary>
        static int GetWeaponValue(EnemyEntity entity, DaggerfallUnityItem item)
        {
            if (item == null)
                return entity.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand);

            if (item.ItemGroup != ItemGroups.Weapons)
                return 0;

            if (CanEquip(entity, item) == false)
                return 0;

            int value = entity.Skills.GetLiveSkillValue(item.GetWeaponSkillID());
            value += item.GetWeaponMaterialModifier() * 5;

            value += item.IsEnchanted ? 15 : 0;

            value += item.currentCondition < 10 ? -10 : 0;

            return value;
        }


        /// <summary>
        /// Guesses how good the armor is, based on protective value and enchantment.
        /// </summary>
        static int GetArmorValue(EnemyEntity entity, DaggerfallUnityItem item)
        {
            if (item == null || CanEquip(entity, item) == false)
                return 0;

            if (item.ItemGroup != ItemGroups.Armor)
                return 0;

            int value = item.GetMaterialArmorValue();

            value += item.IsEnchanted ? 15 : 0;

            return value;
        }


        /// <summary>
        /// Can the entity actually equip the item? Is it a forbidden type or material?
        /// Some logic gleaned from DaggerfallInventoryWindow.cs.
        /// </summary>
        static bool CanEquip(EnemyEntity entity, DaggerfallUnityItem item)
        {
            if (item.currentCondition < 1)
                return false;

            if (item.ItemGroup == ItemGroups.Weapons)
            {
                if ((item.GetWeaponSkillUsed() & (int)entity.Career.ForbiddenProficiencies) != 0)
                    return false;

                if (item.TemplateIndex == (int)Weapons.Arrow)
                    return false;

                if ((1 << item.NativeMaterialValue & (int)entity.Career.ForbiddenMaterials) != 0)
                    return false;
            }
            else if (item.ItemGroup == ItemGroups.Armor)
            {
                // Check for prohibited shield
                if (item.IsShield && ((1 << (item.TemplateIndex - (int)Armor.Buckler) & (int)entity.Career.ForbiddenShields) != 0))
                    return false;

                // Check for prohibited armor type (leather, chain or plate)
                if (!item.IsShield && (1 << (item.NativeMaterialValue >> 8) & (int)entity.Career.ForbiddenArmors) != 0)
                    return false;

                // Check for prohibited material
                if (((item.nativeMaterialValue >> 8) == 2)
                    && (1 << (item.NativeMaterialValue & 0xFF) & (int)entity.Career.ForbiddenMaterials) != 0)
                    return false;
            }

            return true;
        }



    } //class Equipment


} //namespace
