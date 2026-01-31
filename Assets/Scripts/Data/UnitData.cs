using System;
using System.Collections.Generic;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 单位数据结构
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
        public enum UnitType { Robot, Tank, Hybrid, KukuHybrid }

        // 战斗属性
        public float AttackPower { get; set; }
        public float DefensePower { get; set; }
        public float Speed { get; set; }
        public float Health { get; set; }
        public float Range { get; set; }

        // 生产相关
        public int ProductionCost { get; set; } = 100;
        public int RequiredResearchLevel { get; set; } = 1;
        public float ProductionTime { get; set; } = 10f;

        // 融合相关
        public bool CanFuse { get; set; } = true;
        public int FusionCost { get; set; } = 200;
        public string FusionRecipe { get; set; } = "";
        public bool IsFinalForm { get; set; } = false; // 是否为最终形态（可装备6件装备）

        // 状态
        public bool IsDeployed { get; set; } = false;
        public int Level { get; set; } = 1;
        public float Experience { get; set; } = 0;

        // 装备槽位
        public int MaxEquipmentSlots { get; set; } = 0;
        public List<WeaponData> EquippedItems { get; set; }

        // 构造函数
        public UnitData()
        {
            Name = "默认单位";
            Description = "一个基础单位";
            Type = UnitType.Robot;
            AttackPower = 20f;
            DefensePower = 15f;
            Speed = 1.5f;
            Health = 100f;
            Range = 3f;
            ProductionCost = 100;
            RequiredResearchLevel = 1;
            ProductionTime = 10f;
            CanFuse = true;
            FusionCost = 200;
            IsDeployed = false;
            Level = 1;
            Experience = 0f;
            MaxEquipmentSlots = 0;
            EquippedItems = new List<WeaponData>();
        }

        /// <summary>
        /// 升级单位
        /// </summary>
        public void LevelUp()
        {
            Level++;
            // 提升基础属性
            AttackPower *= 1.15f;
            DefensePower *= 1.12f;
            Health *= 1.15f;
            if (Range > 0) Range *= 1.05f; // 如果有射程则提升
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public bool AddExperience(float exp)
        {
            Experience += exp;
            // 假设每150经验升一级
            while (Experience >= 150)
            {
                Experience -= 150;
                LevelUp();
            }
            return true;
        }

        /// <summary>
        /// 获取生产花费
        /// </summary>
        public int GetProductionCost()
        {
            // 根据等级调整生产成本
            return (int)(ProductionCost * Math.Pow(1.2, Level - 1));
        }

        /// <summary>
        /// 设置融合配方
        /// </summary>
        public void SetFusionRecipe(string recipe)
        {
            FusionRecipe = recipe;
        }

        /// <summary>
        /// 融合两个单位
        /// </summary>
        public static UnitData FuseUnits(UnitData unit1, UnitData unit2, string fusionName = "")
        {
            if (unit1 == null || unit2 == null)
                return null;

            UnitData fusedUnit = new UnitData
            {
                Name = string.IsNullOrEmpty(fusionName) ? $"{unit1.Name}+{unit2.Name}" : fusionName,
                Description = $"由{unit1.Name}和{unit2.Name}融合而成的强大单位",
                Type = UnitType.Hybrid, // 融合后的类型
                AttackPower = (unit1.AttackPower + unit2.AttackPower) * 1.2f, // 融合后攻击力提升20%
                DefensePower = (unit1.DefensePower + unit2.DefensePower) * 1.15f, // 防御力提升15%
                Speed = (unit1.Speed + unit2.Speed) * 0.9f, // 速度略微降低
                Health = (unit1.Health + unit2.Health) * 1.25f, // 血量提升25%
                Range = Math.Max(unit1.Range, unit2.Range) * 1.1f, // 射程取较大值并提升10%
                ProductionCost = (unit1.ProductionCost + unit2.ProductionCost) * 2, // 生产成本翻倍
                RequiredResearchLevel = Math.Max(unit1.RequiredResearchLevel, unit2.RequiredResearchLevel) + 2, // 研究等级要求提升
                ProductionTime = (unit1.ProductionTime + unit2.ProductionTime) * 1.5f, // 生产时间增加50%
                CanFuse = false, // 融合后的单位不能再融合
                FusionCost = 0, // 融合后的单位无需再融合
                IsFinalForm = true, // 标记为最终形态
                Level = Math.Max(unit1.Level, unit2.Level), // 等级取较高者
                MaxEquipmentSlots = 6, // 融合后的单位可装备6件装备
                EquippedItems = new List<WeaponData>() // 初始化装备列表
            };

            return fusedUnit;
        }

        /// <summary>
        /// KuKu与机器人融合
        /// </summary>
        public static UnitData FuseKukuWithRobot(MythicalKukuData kuku, UnitData robot, string fusionName = "")
        {
            if (kuku == null || robot == null)
                return null;

            // 检查KuKu是否可以融合
            if (!kuku.CanFuseWithRobots || kuku.EvolutionLevel < 5)
            {
                Debug.LogWarning("KuKu无法与机器人融合，可能是因为进化等级不足或不允许融合");
                return null;
            }

            // 检查机器人是否可以融合
            if (!robot.CanFuse || robot.Level < 10)
            {
                Debug.LogWarning("机器人无法与KuKu融合，可能是因为等级不足或不允许融合");
                return null;
            }

            UnitData fusedUnit = new UnitData
            {
                Name = string.IsNullOrEmpty(fusionName) ? $"{kuku.Name}融合体" : fusionName,
                Description = $"由{kuku.Name}和{robot.Name}融合而成的终极单位",
                Type = UnitType.KukuHybrid, // KuKu融合体类型
                AttackPower = (kuku.AttackPower + robot.AttackPower) * kuku.FusionCompatibility * 1.5f, // 融合兼容性影响最终属性
                DefensePower = (kuku.DefensePower + robot.DefensePower) * kuku.FusionCompatibility * 1.4f,
                Speed = (kuku.Speed + robot.Speed) * kuku.FusionCompatibility * 1.1f,
                Health = (kuku.Health + robot.Health) * kuku.FusionCompatibility * 1.6f,
                Range = Math.Max(kuku.SkillRange, robot.Range) * 1.2f, // 取较大射程并提升20%
                ProductionCost = (int)((kuku.DivinePower + robot.ProductionCost) * 3), // 非常高的生产成本
                RequiredResearchLevel = Math.Max(kuku.EvolutionLevel, robot.RequiredResearchLevel) + 5, // 很高的研究要求
                ProductionTime = (kuku.EvolutionLevel + robot.ProductionTime) * 3, // 很长的生产时间
                CanFuse = false, // 融合后的单位不能再融合
                FusionCost = 0, // 融合后的单位无需再融合
                IsFinalForm = true, // 标记为最终形态
                Level = Math.Max(kuku.EvolutionLevel, robot.Level), // 等级取较高者
                MaxEquipmentSlots = 6, // 终极单位可装备6件装备
                EquippedItems = new List<WeaponData>() // 初始化装备列表
            };

            return fusedUnit;
        }

        /// <summary>
        /// 检查是否可以装备物品
        /// </summary>
        public bool CanEquipItem(WeaponData item)
        {
            if (item == null || MaxEquipmentSlots <= 0)
                return false;

            // 检查是否有空余的装备槽位
            return EquippedItems.Count < MaxEquipmentSlots;
        }

        /// <summary>
        /// 装备物品
        /// </summary>
        public bool EquipItem(WeaponData item)
        {
            if (!CanEquipItem(item))
                return false;

            // 添加到装备列表
            EquippedItems.Add(item);

            // 应用装备属性
            ApplyEquipmentBonuses(item);

            return true;
        }

        /// <summary>
        /// 卸下物品
        /// </summary>
        public bool UnequipItem(WeaponData item)
        {
            if (item == null || !EquippedItems.Contains(item))
                return false;

            // 从装备列表移除
            EquippedItems.Remove(item);

            // 移除装备属性
            RemoveEquipmentBonuses(item);

            return true;
        }

        /// <summary>
        /// 应用装备属性加成
        /// </summary>
        private void ApplyEquipmentBonuses(WeaponData item)
        {
            if (item == null) return;

            AttackPower += item.AttackBonus;
            DefensePower += item.DefenseBonus;
            Speed += item.SpeedBonus;
            Health += item.HealthBonus;
            if (Range > 0) Range += item.RangeBonus; // 只有当单位原本有射程时才增加射程
        }

        /// <summary>
        /// 移除装备属性加成
        /// </summary>
        private void RemoveEquipmentBonuses(WeaponData item)
        {
            if (item == null) return;

            AttackPower -= item.AttackBonus;
            DefensePower -= item.DefenseBonus;
            Speed -= item.SpeedBonus;
            Health -= item.HealthBonus;
            if (Range > 0) Range -= item.RangeBonus; // 只有当单位原本有射程时才减少射程
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

            // 计算装备加成
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
        /// 复制当前单位数据
        /// </summary>
        public UnitData Clone()
        {
            UnitData clone = new UnitData
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
                EquippedItems = new List<WeaponData>()
            };

            // 复制装备列表
            foreach (var item in this.EquippedItems)
            {
                clone.EquippedItems.Add(item.Clone());
            }

            return clone;
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            var attrs = GetTotalAttributes();
            return $"{Name} [{Type}] - ATK:{attrs.atk:F0} DEF:{attrs.def:F0} HP:{attrs.hp:F0}";
        }
    }
}