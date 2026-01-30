using UnityEngine;

namespace PetCollector.Data
{
    /// <summary>
    /// 建筑数据结构
    /// </summary>
    [System.Serializable]
    public class BuildingData
    {
        // 基础信息
        public int Id { get; set; }                              // 建筑ID
        public string Name { get; set; }                         // 建筑名称
        public string Description { get; set; }                  // 建筑描述

        // 建筑类型
        public BuildingType Type { get; set; }                   // 建筑类型
        public enum BuildingType 
        { 
            Tower,                                              // 防御塔
            Research,                                           // 研究所
            Production,                                         // 生产工厂
            Special,                                            // 特殊建筑
            SoulCollector                                        // 灵魂收集器（捕捉阶段特有）
        }

        // 等级和需求
        public int Level { get; set; } = 1;                     // 等级
        public int RequiredResearchLevel { get; set; } = 0;     // 需要研究等级
        public int RequiredCoins { get; set; } = 100;           // 需要金币
        public int RequiredGems { get; set; } = 0;              // 需要神石

        // 属性（根据类型不同）
        public float AttackPower { get; set; } = 0f;            // 攻击力（塔类）
        public float Range { get; set; } = 0f;                  // 射程（塔类）
        public float Health { get; set; } = 100f;               // 生命值
        public float BuildTime { get; set; } = 10f;             // 建造时间

        // 研究属性
        public float ResearchSpeed { get; set; } = 0f;          // 研究速度（研究类）
        public int MaxResearchLevel { get; set; } = 10;         // 最大研究等级

        // 生产属性
        public float ProductionSpeed { get; set; } = 0f;        // 生产速度（生产类）
        public string ProducedItem { get; set; } = "";          // 生产物品
        public UnitData.UnitType ProducesUnitType { get; set; } = UnitData.UnitType.Robot; // 生产单位类型

        // 特殊属性
        public bool IsUpgradeable { get; set; } = true;         // 是否可升级
        public int UpgradeCostMultiplier { get; set; } = 2;     // 升级花费倍数

        // 捕捉阶段特有属性
        public float SoulCollectionRate { get; set; } = 0f;     // 灵魂收集速率（灵魂收集器）
        public bool IsActiveDuringCapturePhase { get; set; } = true; // 捕捉阶段是否激活

        // 视觉相关
        public string SpriteName { get; set; }                   // 精灵名称
        public Color Tint { get; set; } = Color.white;          // 着色

        // 状态
        public bool IsActive { get; set; } = false;              // 是否激活
        public float CurrentHealth { get; set; } = 100f;        // 当前生命值

        // 构造函数
        public BuildingData()
        {
            Id = 0;
            Name = "Unknown Building";
            Description = "An unknown building";
            Type = BuildingType.Tower;
            Level = 1;
            RequiredResearchLevel = 0;
            RequiredCoins = 100;
            RequiredGems = 0;
            AttackPower = 0f;
            Range = 0f;
            Health = 100f;
            BuildTime = 10f;
            ResearchSpeed = 0f;
            MaxResearchLevel = 10;
            ProductionSpeed = 0f;
            ProducedItem = "";
            ProducesUnitType = UnitData.UnitType.Robot;
            IsUpgradeable = true;
            UpgradeCostMultiplier = 2;
            SoulCollectionRate = 0f;
            IsActiveDuringCapturePhase = true;
            SpriteName = "default_building";
            Tint = Color.white;
            IsActive = false;
            CurrentHealth = 100f;
        }

        // 升级建筑
        public void Upgrade()
        {
            if (!IsUpgradeable || Level >= MaxResearchLevel) return;
            
            Level++;
            // 升级后增强属性
            Health *= 1.2f;
            CurrentHealth = Health; // 恢复满血
            if (Type == BuildingType.Tower)
            {
                AttackPower *= 1.15f;
                Range *= 1.1f;
            }
            else if (Type == BuildingType.Research)
            {
                ResearchSpeed *= 1.2f;
            }
            else if (Type == BuildingType.Production)
            {
                ProductionSpeed *= 1.15f;
            }
        }

        // 获取建造花费
        public (int coins, int gems) GetBuildCost()
        {
            int coins = RequiredCoins * Level;
            int gems = RequiredGems * Level;
            return (coins, gems);
        }

        // 获取升级花费
        public (int coins, int gems) GetUpgradeCost()
        {
            var buildCost = GetBuildCost();
            int upgradeCoins = buildCost.coins * UpgradeCostMultiplier;
            int upgradeGems = buildCost.gems * UpgradeCostMultiplier;
            return (upgradeCoins, upgradeGems);
        }

        // 检查是否可以建造（基于当前游戏阶段）
        public bool CanBuildInCurrentPhase(BattleSystem.BattleState currentPhase)
        {
            if (currentPhase == BattleSystem.BattleState.CapturePhase)
            {
                // 在捕捉阶段，只有特定建筑可以建造
                return Type == BuildingType.SoulCollector || Type == BuildingType.Special;
            }
            else if (currentPhase == BattleSystem.BattleState.Fighting || 
                     currentPhase == BattleSystem.BattleState.DefensePhaseSetup)
            {
                // 在防守阶段，可以建造所有建筑
                return true;
            }
            return false;
        }

        // 激活建筑（开始工作）
        public void Activate()
        {
            IsActive = true;
        }

        // 停止建筑
        public void Deactivate()
        {
            IsActive = false;
        }

        // 受到伤害
        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Deactivate();
            }
        }

        // 修复建筑
        public void Repair(float repairAmount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + repairAmount, Health);
            if (CurrentHealth > 0 && !IsActive)
            {
                Activate();
            }
        }
    }
}