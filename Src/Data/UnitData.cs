using System;
using System.Collections.Generic;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 单位数据结构 - 代表机器人、坦克等单位
    /// </summary>
    [Serializable]
    public class UnitData
    {
        // 基础信息
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // 单位类型
        public UnitType Type { get; set; }
        public enum UnitType { 
            Robot,      // 机器人
            Tank,       // 坦克
            Hybrid,     // 混合单位
            KukuHybrid  // KuKu融合单位
        }
        
        // 战斗属性
        public float AttackPower { get; set; }
        public float DefensePower { get; set; }
        public float Speed { get; set; }
        public float Health { get; set; }
        public float Range { get; set; }
        
        // 生产相关
        public int ProductionCost { get; set; }
        public int RequiredResearchLevel { get; set; }
        public float ProductionTime { get; set; }
        
        // 融合相关
        public bool CanFuse { get; set; }
        public int FusionCost { get; set; }
        public string FusionRecipe { get; set; }
        public bool IsFinalForm { get; set; }
        
        // 状态
        public bool IsDeployed { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        
        // 装备槽位
        public int MaxEquipmentSlots { get; set; }
        public List<WeaponData> EquippedItems { get; set; }
        
        // 其他属性
        public string SpriteName { get; set; }
        public Color Tint { get; set; }
        public bool IsOperational { get; set; }
        public float Energy { get; set; }
        public float MaxEnergy { get; set; }
        public int RequiredBuildingLevel { get; set; }
        public float UpgradeCostMultiplier { get; set; }
        public int MaxLevel { get; set; }

        public UnitData()
        {
            // 初始化默认值
            Name = "未知单位";
            Description = "一个未定义的单位";
            Type = UnitType.Robot;
            AttackPower = 10f;
            DefensePower = 5f;
            Speed = 1f;
            Health = 50f;
            Range = 3f;
            ProductionCost = 100;
            RequiredResearchLevel = 1;
            ProductionTime = 10f;
            CanFuse = true;
            FusionCost = 200;
            FusionRecipe = "";
            IsFinalForm = false;
            IsDeployed = false;
            Level = 1;
            Experience = 0f;
            MaxEquipmentSlots = 0;
            EquippedItems = new List<WeaponData>();
            SpriteName = "DefaultUnitSprite";
            Tint = Color.white;
            IsOperational = true;
            Energy = 100f;
            MaxEnergy = 100f;
            RequiredBuildingLevel = 1;
            UpgradeCostMultiplier = 1.5f;
            MaxLevel = 10;
        }
        
        /// <summary>
        /// 升级单位
        /// </summary>
        public void LevelUp()
        {
            if (Level < MaxLevel)
            {
                Level++;
                
                // 按比例提升属性
                AttackPower *= 1.15f;
                DefensePower *= 1.12f;
                Health *= 1.18f;
                Speed *= 1.05f;
                
                // 恢复满能量
                Energy = MaxEnergy;
            }
        }
        
        /// <summary>
        /// 添加经验值
        /// </summary>
        public bool AddExperience(float exp)
        {
            if (exp <= 0) return false;
            
            Experience += exp;
            
            // 检查是否可以升级
            float expRequired = Level * 100f; // 每级需要的经验值
            if (Experience >= expRequired && Level < MaxLevel)
            {
                Experience -= expRequired;
                LevelUp();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取生产花费
        /// </summary>
        public int GetProductionCost()
        {
            return (int)(ProductionCost * Math.Pow(UpgradeCostMultiplier, Level - 1));
        }
        
        /// <summary>
        /// 设置融合配方
        /// </summary>
        public void SetFusionRecipe(string recipe)
        {
            FusionRecipe = recipe;
        }
        
        /// <summary>
        /// 检查是否可以装备物品
        /// </summary>
        public bool CanEquipItem(WeaponData item)
        {
            if (item == null) return false;
            if (EquippedItems.Count >= MaxEquipmentSlots) return false;
            
            // 检查是否已有相同槽位的装备
            foreach (var equippedItem in EquippedItems)
            {
                if (equippedItem.Slot == item.Slot)
                {
                    return false; // 已有相同槽位的装备
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 装备物品
        /// </summary>
        public bool EquipItem(WeaponData item)
        {
            if (!CanEquipItem(item)) return false;
            
            EquippedItems.Add(item);
            item.IsEquipped = true;
            
            return true;
        }
        
        /// <summary>
        /// 卸下物品
        /// </summary>
        public bool UnequipItem(WeaponData item)
        {
            if (item == null) return false;
            
            bool removed = EquippedItems.Remove(item);
            if (removed)
            {
                item.IsEquipped = false;
            }
            
            return removed;
        }
        
        /// <summary>
        /// 获取总属性（包含装备加成）
        /// </summary>
        public (float atk, float def, float spd, float hp, float rng) GetTotalAttributes()
        {
            float totalAtk = AttackPower;
            float totalDef = DefensePower;
            float totalSpd = Speed;
            float totalHp = Health;
            float totalRng = Range;
            
            // 加上装备的属性加成
            foreach (var item in EquippedItems)
            {
                totalAtk += item.AttackBonus;
                totalDef += item.DefenseBonus;
                totalSpd += item.SpeedBonus;
                totalHp += item.HealthBonus;
                totalRng += item.RangeBonus;
            }
            
            return (totalAtk, totalDef, totalSpd, totalHp, totalRng);
        }
        
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [{GetTypeString()}] - Lv.{Level} ATK:{AttackPower:F1}";
        }
        
        /// <summary>
        /// 获取类型字符串
        /// </summary>
        private string GetTypeString()
        {
            switch (Type)
            {
                case UnitType.Robot:
                    return "机器人";
                case UnitType.Tank:
                    return "坦克";
                case UnitType.Hybrid:
                    return "混合单位";
                case UnitType.KukuHybrid:
                    return "KuKu融合体";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 复制当前单位数据
        /// </summary>
        public UnitData Clone()
        {
            var clonedUnit = new UnitData
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Type = this.Type,
                AttackPower = this.AttackPower,
                DefensePower = this.DefensePower,
                Speed = this.Speed,
                Health = this.Health,
                Range = this.Range,
                ProductionCost = this.ProductionCost,
                RequiredResearchLevel = this.RequiredResearchLevel,
                ProductionTime = this.ProductionTime,
                CanFuse = this.CanFuse,
                FusionCost = this.FusionCost,
                FusionRecipe = this.FusionRecipe,
                IsFinalForm = this.IsFinalForm,
                IsDeployed = this.IsDeployed,
                Level = this.Level,
                Experience = this.Experience,
                MaxEquipmentSlots = this.MaxEquipmentSlots,
                SpriteName = this.SpriteName,
                Tint = this.Tint,
                IsOperational = this.IsOperational,
                Energy = this.Energy,
                MaxEnergy = this.MaxEnergy,
                RequiredBuildingLevel = this.RequiredBuildingLevel,
                UpgradeCostMultiplier = this.UpgradeCostMultiplier,
                MaxLevel = this.MaxLevel
            };
            
            // 复制装备列表
            clonedUnit.EquippedItems = new List<WeaponData>();
            foreach (var item in this.EquippedItems)
            {
                clonedUnit.EquippedItems.Add(item.Clone());
            }
            
            return clonedUnit;
        }
        
        /// <summary>
        /// 融合两个单位
        /// </summary>
        public static UnitData FuseUnits(UnitData unit1, UnitData unit2, string fusionName = "")
        {
            if (unit1 == null || unit2 == null) return null;
            
            UnitData fusedUnit = new UnitData();
            
            // 设置基本信息
            fusedUnit.Name = string.IsNullOrEmpty(fusionName) ? 
                $"{unit1.Name}+{unit2.Name}" : fusionName;
            fusedUnit.Description = $"由{unit1.Name}和{unit2.Name}融合而成的强大单位";
            fusedUnit.Type = UnitType.Hybrid; // 融合后为混合单位
            
            // 合并属性（加权平均并有融合加成）
            fusedUnit.AttackPower = (unit1.AttackPower + unit2.AttackPower) * 1.25f; // 25%融合加成
            fusedUnit.DefensePower = (unit1.DefensePower + unit2.DefensePower) * 1.25f;
            fusedUnit.Speed = (unit1.Speed + unit2.Speed) / 2 * 1.1f; // 10%速度加成
            fusedUnit.Health = (unit1.Health + unit2.Health) * 1.3f; // 30%血量加成
            fusedUnit.Range = Mathf.Max(unit1.Range, unit2.Range) * 1.1f; // 射程提升
            
            // 生产相关属性
            fusedUnit.ProductionCost = (int)((unit1.ProductionCost + unit2.ProductionCost) * 1.5f);
            fusedUnit.RequiredResearchLevel = Mathf.Max(unit1.RequiredResearchLevel, unit2.RequiredResearchLevel);
            fusedUnit.ProductionTime = (unit1.ProductionTime + unit2.ProductionTime) / 2 * 1.3f;
            
            // 融合相关
            fusedUnit.CanFuse = true; // 融合后的单位仍可进一步融合
            fusedUnit.FusionCost = (int)((unit1.FusionCost + unit2.FusionCost) * 1.2f);
            fusedUnit.IsFinalForm = false; // 默认不是最终形态
            
            // 状态
            fusedUnit.Level = Mathf.Max(unit1.Level, unit2.Level); // 取较高等级
            fusedUnit.Experience = (unit1.Experience + unit2.Experience) / 2; // 平均经验
            
            // 装备槽位 - 两个单位的装备槽相加
            fusedUnit.MaxEquipmentSlots = unit1.MaxEquipmentSlots + unit2.MaxEquipmentSlots;
            fusedUnit.EquippedItems = new List<WeaponData>();
            
            // 复制两个单位的装备
            foreach (var item in unit1.EquippedItems)
            {
                fusedUnit.EquippedItems.Add(item.Clone());
            }
            foreach (var item in unit2.EquippedItems)
            {
                fusedUnit.EquippedItems.Add(item.Clone());
            }
            
            // 视觉和其他属性
            fusedUnit.SpriteName = $"Fused_{unit1.Id}_{unit2.Id}";
            fusedUnit.Tint = Color.Lerp(unit1.Tint, unit2.Tint, 0.5f);
            fusedUnit.IsOperational = true;
            fusedUnit.Energy = fusedUnit.MaxEnergy;
            fusedUnit.RequiredBuildingLevel = fusedUnit.RequiredResearchLevel;
            fusedUnit.UpgradeCostMultiplier = (unit1.UpgradeCostMultiplier + unit2.UpgradeCostMultiplier) / 2;
            fusedUnit.MaxLevel = Mathf.Max(unit1.MaxLevel, unit2.MaxLevel);
            
            return fusedUnit;
        }
    }
}