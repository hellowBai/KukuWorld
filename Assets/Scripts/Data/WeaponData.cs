using System;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 武器/装备数据结构
    /// </summary>
    [Serializable]
    public class WeaponData
    {
        // 基础信息
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // 武器类型
        public WeaponType Type { get; set; }
        public enum WeaponType { 
            Sword, Shield, Cannon, Laser, Missile, 
            Armor, Engine, Processor, Crystal, Orb,
            Ring, Amulet, Gauntlet, Boots, Helm, 
            Cloak, Bracer, Belt
        }
        
        // 等级
        public WeaponTier Tier { get; set; }
        public enum WeaponTier { 
            Basic = 0,    // 基础
            Advanced = 1, // 高级
            Expert = 2,   // 专家
            Master = 3,   // 大师
            Legendary = 4 // 传说
        }
        
        // 战斗属性
        public float AttackBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float HealthBonus { get; set; }
        public float RangeBonus { get; set; }
        public float SpecialEffect { get; set; }
        
        // 装备槽位
        public EquipmentSlot Slot { get; set; }
        public enum EquipmentSlot { 
            Weapon, Shield, Head, Body, Legs, Accessory, 
            LeftHand, RightHand, Feet, Ring, Necklace 
        }
        
        // 合成相关
        public bool IsCombinable { get; set; }
        public int CombineCost { get; set; }
        public int RequiredTierToCombine { get; set; }
        public int ComponentsNeeded { get; set; }
        
        // 装备相关
        public int EquipCost { get; set; }
        public int RequiredLevel { get; set; }
        
        // 状态
        public bool IsEquipped { get; set; }
        public int Durability { get; set; }
        public int MaxDurability { get; set; }
        
        // 价格
        public int Price { get; set; }
        
        // 其他属性
        public string SpriteName { get; set; }
        public Color Tint { get; set; }
        public float EffectDuration { get; set; }
        public float CooldownReduction { get; set; }
        public float CriticalChance { get; set; }
        public float CriticalDamage { get; set; }
        public bool IsUnique { get; set; } // 是否为唯一装备
        public string SetName { get; set; } // 套装名称
        public int SetPieceCount { get; set; } // 套装件数
        public string SpecialAbility { get; set; } // 特殊能力描述

        public WeaponData()
        {
            // 初始化默认值
            Name = "未知装备";
            Description = "一个未定义的装备";
            Type = WeaponType.Sword;
            Tier = WeaponTier.Basic;
            AttackBonus = 0f;
            DefenseBonus = 0f;
            SpeedBonus = 0f;
            HealthBonus = 0f;
            RangeBonus = 0f;
            SpecialEffect = 0f;
            Slot = EquipmentSlot.Weapon;
            IsCombinable = true;
            CombineCost = 50;
            RequiredTierToCombine = 0;
            ComponentsNeeded = 2;
            EquipCost = 0;
            RequiredLevel = 1;
            IsEquipped = false;
            Durability = 100;
            MaxDurability = 100;
            Price = 100;
            SpriteName = "DefaultWeaponSprite";
            Tint = Color.white;
            EffectDuration = 0f;
            CooldownReduction = 0f;
            CriticalChance = 0f;
            CriticalDamage = 1.5f;
            IsUnique = false;
            SetName = "";
            SetPieceCount = 0;
            SpecialAbility = "";
        }
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public Color GetTierColor()
        {
            switch (Tier)
            {
                case WeaponTier.Basic:
                    return Color.white;
                case WeaponTier.Advanced:
                    return Color.green;
                case WeaponTier.Expert:
                    return Color.blue;
                case WeaponTier.Master:
                    return new Color(0.8f, 0.6f, 0f); // 金色
                case WeaponTier.Legendary:
                    return new Color(0.8f, 0.4f, 0.8f); // 紫色
                default:
                    return Color.gray;
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
        /// 获取类型名称
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
                case WeaponType.Ring:
                    return "戒指";
                case WeaponType.Amulet:
                    return "护符";
                case WeaponType.Gauntlet:
                    return "护手";
                case WeaponType.Boots:
                    return "靴子";
                case WeaponType.Helm:
                    return "头盔";
                case WeaponType.Cloak:
                    return "斗篷";
                case WeaponType.Bracer:
                    return "护腕";
                case WeaponType.Belt:
                    return "腰带";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 获取槽位名称
        /// </summary>
        public string GetSlotName()
        {
            switch (Slot)
            {
                case EquipmentSlot.Weapon:
                    return "武器";
                case EquipmentSlot.Shield:
                    return "副手/盾牌";
                case EquipmentSlot.Head:
                    return "头部";
                case EquipmentSlot.Body:
                    return "身体";
                case EquipmentSlot.Legs:
                    return "腿部";
                case EquipmentSlot.Accessory:
                    return "饰品";
                case EquipmentSlot.LeftHand:
                    return "左手";
                case EquipmentSlot.RightHand:
                    return "右手";
                case EquipmentSlot.Feet:
                    return "脚部";
                case EquipmentSlot.Ring:
                    return "戒指";
                case EquipmentSlot.Necklace:
                    return "项链";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 修复耐久度
        /// </summary>
        public void Repair()
        {
            Durability = MaxDurability;
        }
        
        /// <summary>
        /// 使用装备（减少耐久度）
        /// </summary>
        public void Use()
        {
            if (Durability > 0)
            {
                Durability--;
            }
        }
        
        /// <summary>
        /// 检查是否损坏
        /// </summary>
        public bool IsBroken()
        {
            return Durability <= 0;
        }
        
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"[{GetTierName()}]{Name} ({GetTypeName()}) - ATK+{AttackBonus:F1}, DEF+{DefenseBonus:F1}";
        }
        
        /// <summary>
        /// 复制当前武器数据
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
                MaxDurability = this.MaxDurability,
                Price = this.Price,
                SpriteName = this.SpriteName,
                Tint = this.Tint,
                EffectDuration = this.EffectDuration,
                CooldownReduction = this.CooldownReduction,
                CriticalChance = this.CriticalChance,
                CriticalDamage = this.CriticalDamage,
                IsUnique = this.IsUnique,
                SetName = this.SetName,
                SetPieceCount = this.SetPieceCount,
                SpecialAbility = this.SpecialAbility
            };
        }
    }
}