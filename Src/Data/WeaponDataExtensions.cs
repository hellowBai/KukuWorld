using System;
using System.Collections.Generic;

namespace KukuWorld.Data
{
    /// <summary>
    /// 武器数据结构扩展
    /// </summary>
    public partial class WeaponData
    {
        // 添加缺失的属性
        public int MaxDurability { get; set; } = 100;
        public string SetName { get; set; } = "";
        public int SetPieceCount { get; set; } = 0;
        public string SpecialAbility { get; set; } = "";
        public bool IsUnique { get; set; } = false;
        public float EffectDuration { get; set; } = 0f;
        public float CooldownReduction { get; set; } = 0f;
        public float CriticalChance { get; set; } = 0f;
        public float CriticalDamage { get; set; } = 1.5f; // 倍数
        public string SpriteName { get; set; } = "";
        public Color Tint { get; set; } = Color.white;

        /// <summary>
        /// 获取武器类型名称
        /// </summary>
        public string GetTypeName()
        {
            switch (Type)
            {
                case WeaponType.Sword:
                    return "剑";
                case WeaponType.Shield:
                    return "盾";
                case WeaponType.Cannon:
                    return "炮";
                case WeaponType.Laser:
                    return "激光";
                case WeaponType.Missile:
                    return "导弹";
                case WeaponType.Armor:
                    return "护甲";
                case WeaponType.Engine:
                    return "引擎";
                case WeaponType.Processor:
                    return "处理器";
                case WeaponType.Crystal:
                    return "水晶";
                case WeaponType.Orb:
                    return "宝珠";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public string GetTierName()
        {
            switch (Tier)
            {
                case WeaponTier.Basic:
                    return "基础";
                case WeaponTier.Advanced:
                    return "高级";
                case WeaponTier.Expert:
                    return "专家";
                case WeaponTier.Master:
                    return "大师";
                case WeaponTier.Legendary:
                    return "传说";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 克隆武器数据
        /// </summary>
        public WeaponData Clone()
        {
            return new WeaponData
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Type = this.Type,
                Tier = this.Tier,
                AttackBonus = this.AttackBonus,
                DefenseBonus = this.DefenseBonus,
                SpeedBonus = this.SpeedBonus,
                HealthBonus = this.HealthBonus,
                RangeBonus = this.RangeBonus,
                SpecialEffect = this.SpecialEffect,
                Slot = this.Slot,
                IsCombinable = this.IsCombinable,
                CombineCost = this.CombineCost,
                RequiredTierToCombine = this.RequiredTierToCombine,
                ComponentsNeeded = this.ComponentsNeeded,
                EquipCost = this.EquipCost,
                RequiredLevel = this.RequiredLevel,
                IsEquipped = this.IsEquipped,
                Durability = this.Durability,
                Price = this.Price,
                ManufactureType = this.ManufactureType,
                RequiredMaterials = new List<CraftingMaterial>(this.RequiredMaterials),
                MaxDurability = this.MaxDurability,
                SetName = this.SetName,
                SetPieceCount = this.SetPieceCount,
                SpecialAbility = this.SpecialAbility,
                IsUnique = this.IsUnique,
                EffectDuration = this.EffectDuration,
                CooldownReduction = this.CooldownReduction,
                CriticalChance = this.CriticalChance,
                CriticalDamage = this.CriticalDamage,
                SpriteName = this.SpriteName,
                Tint = this.Tint
            };
        }
    }
}