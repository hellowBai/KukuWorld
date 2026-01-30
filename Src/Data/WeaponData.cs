namespace KukuWorld.Data
{
    /// <summary>
    /// 武器数据结构
    /// </summary>
    [System.Serializable]
    public class WeaponData
    {
        // 基础信息
        public int Id { get; set; }                              // 武器ID
        public string Name { get; set; }                         // 武器名称
        public string Description { get; set; }                  // 武器描述

        // 武器类型
        public WeaponType Type { get; set; }                     // 武器类型
        public enum WeaponType 
        { 
            Sword, Shield, Cannon, Laser, Missile, 
            Armor, Engine, Processor, Crystal, Orb 
        }

        // 等级
        public WeaponTier Tier { get; set; }                     // 武器等级
        public enum WeaponTier { Basic, Advanced, Expert, Master, Legendary }

        // 战斗属性
        public float AttackBonus { get; set; } = 0f;            // 攻击加成
        public float DefenseBonus { get; set; } = 0f;           // 防御加成
        public float SpeedBonus { get; set; } = 0f;             // 速度加成
        public float HealthBonus { get; set; } = 0f;            // 生命加成
        public float RangeBonus { get; set; } = 0f;             // 射程加成
        public float SpecialEffect { get; set; } = 0f;          // 特殊效果

        // 装备槽位
        public EquipmentSlot Slot { get; set; }                  // 装备槽位
        public enum EquipmentSlot { Weapon, Shield, Head, Body, Legs, Accessory }

        // 合成相关
        public bool IsCombinable { get; set; } = true;          // 是否可合成
        public int CombineCost { get; set; } = 50;              // 合成花费
        public int RequiredTierToCombine { get; set; } = 0;     // 合成所需等级
        public int ComponentsNeeded { get; set; } = 2;          // 合成所需组件

        // 装备相关
        public int EquipCost { get; set; } = 0;                 // 装备花费
        public int RequiredLevel { get; set; } = 1;             // 装备所需等级

        // 状态
        public bool IsEquipped { get; set; } = false;           // 是否已装备
        public int Durability { get; set; } = 100;              // 耐久度

        // 构造函数
        public WeaponData()
        {
            Id = 0;
            Name = "Unknown Weapon";
            Description = "An unknown weapon";
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
        }
    }
}