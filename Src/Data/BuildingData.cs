using System;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 建筑数据结构
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        // 基础信息
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // 建筑类型
        public BuildingType Type { get; set; }
        public enum BuildingType 
        { 
            Tower,           // 防御塔
            Research,        // 研究所
            Production,      // 生产工厂
            Special,         // 特殊建筑
            SoulCollector,   // 灵魂收集器（捕捉阶段特有）
            Temple           // 神殿（防守阶段核心）
        }

        // 等级和需求
        public int Level { get; set; } = 1;
        public int RequiredResearchLevel { get; set; } = 0;
        public int RequiredCoins { get; set; } = 100;
        public int RequiredGems { get; set; } = 0;

        // 属性（根据类型不同）
        public float AttackPower { get; set; } = 0f;  // 攻击力（塔类）
        public float Range { get; set; } = 0f;        // 射程（塔类）
        public float Health { get; set; } = 100f;     // 生命值
        public float BuildTime { get; set; } = 10f;   // 建造时间

        // 研究属性
        public float ResearchSpeed { get; set; } = 0f;        // 研究速度（研究类）
        public int MaxResearchLevel { get; set; } = 10;       // 最大研究等级

        // 生产属性
        public float ProductionSpeed { get; set; } = 0f;      // 生产速度（生产类）
        public string ProducedItem { get; set; } = "";        // 生产物品
        public UnitData.UnitType ProducesUnitType { get; set; } = UnitData.UnitType.Robot; // 生产单位类型

        // 特殊属性
        public bool IsUpgradeable { get; set; } = true;       // 是否可升级
        public int UpgradeCostMultiplier { get; set; } = 2;   // 升级花费倍数

        // 捕捉阶段特有属性
        public float SoulCollectionRate { get; set; } = 0f;   // 灵魂收集速率（灵魂收集器）
        public bool IsActiveDuringCapturePhase { get; set; } = true; // 捕捉阶段是否激活
        public bool IsActiveDuringDefensePhase { get; set; } = true; // 防守阶段是否激活

        // 视觉相关
        public string SpriteName { get; set; } = "";
        public Color Tint { get; set; } = Color.white;

        // 状态
        public bool IsBuilt { get; set; } = false;           // 是否已建造
        public bool IsActive { get; set; } = false;          // 是否激活
        public float CurrentHealth { get; set; } = 0f;       // 当前生命值
        public float ConstructionProgress { get; set; } = 0f; // 建造进度

        // 位置信息
        public Vector3 Position { get; set; } = Vector3.zero;

        // 构造函数
        public BuildingData()
        {
            Name = "未命名建筑";
            Description = "一个基础建筑";
            Type = BuildingType.Tower;
            Level = 1;
            RequiredCoins = 100;
            RequiredGems = 0;
            Health = 200f;
            CurrentHealth = Health;
            BuildTime = 10f;
            IsBuilt = false;
            IsActive = false;
            ConstructionProgress = 0f;
        }

        /// <summary>
        /// 升级建筑
        /// </summary>
        public void Upgrade()
        {
            if (!IsUpgradeable || Level >= MaxResearchLevel)
                return;

            Level++;

            // 根据建筑类型提升相应属性
            switch (Type)
            {
                case BuildingType.Tower:
                    AttackPower *= 1.3f; // 攻击力提升30%
                    Range *= 1.1f;       // 射程提升10%
                    Health *= 1.2f;      // 生命值提升20%
                    CurrentHealth = Health; // 恢复满血
                    break;
                case BuildingType.Research:
                    ResearchSpeed *= 1.25f; // 研究速度提升25%
                    MaxResearchLevel += 2;  // 最大研究等级提升
                    break;
                case BuildingType.Production:
                    ProductionSpeed *= 1.2f; // 生产速度提升20%
                    break;
                case BuildingType.SoulCollector:
                    SoulCollectionRate *= 1.4f; // 灵魂收集率提升40%
                    break;
                case BuildingType.Special:
                    // 特殊建筑根据具体情况提升属性
                    AttackPower *= 1.15f;
                    Health *= 1.15f;
                    CurrentHealth = Health;
                    break;
                case BuildingType.Temple:
                    // 神殿提升大量生命值
                    Health *= 1.5f;
                    CurrentHealth = Health;
                    break;
            }

            // 更新升级成本
            RequiredCoins = (int)(RequiredCoins * UpgradeCostMultiplier);
            RequiredGems = (int)(RequiredGems * UpgradeCostMultiplier);
        }

        /// <summary>
        /// 获取建造花费
        /// </summary>
        public (int coins, int gems) GetBuildCost()
        {
            return (RequiredCoins, RequiredGems);
        }

        /// <summary>
        /// 获取升级花费
        /// </summary>
        public (int coins, int gems) GetUpgradeCost()
        {
            int upgradeCoins = (int)(RequiredCoins * UpgradeCostMultiplier);
            int upgradeGems = (int)(RequiredGems * UpgradeCostMultiplier);
            
            // 花费随等级增加
            upgradeCoins = (int)(upgradeCoins * Math.Pow(1.5, Level - 1));
            upgradeGems = (int)(upgradeGems * Math.Pow(1.5, Level - 1));

            return (upgradeCoins, upgradeGems);
        }

        /// <summary>
        /// 检查是否可以建造（基于当前游戏阶段）
        /// </summary>
        public bool CanBuildInCurrentPhase(Systems.BattleSystem.BattleState currentPhase)
        {
            // 在捕捉阶段，只能建造捕捉相关的建筑
            if (currentPhase == Systems.BattleSystem.BattleState.CapturePhase)
            {
                return Type == BuildingType.SoulCollector || Type == BuildingType.Special;
            }

            // 在防守阶段，可以建造防守相关建筑
            if (currentPhase == Systems.BattleSystem.BattleState.DefensePhaseSetup ||
                currentPhase == Systems.BattleSystem.BattleState.WaveStart ||
                currentPhase == Systems.BattleSystem.BattleState.Fighting)
            {
                return Type == BuildingType.Tower || 
                       Type == BuildingType.Research || 
                       Type == BuildingType.Production || 
                       Type == BuildingType.Special ||
                       Type == BuildingType.Temple;
            }

            return true;
        }

        /// <summary>
        /// 检查是否可以收集灵魂（仅在捕捉阶段激活的建筑）
        /// </summary>
        public bool CanCollectSoulsInCurrentPhase(Systems.BattleSystem.BattleState currentPhase)
        {
            return Type == BuildingType.SoulCollector && 
                   currentPhase == Systems.BattleSystem.BattleState.CapturePhase &&
                   IsActiveDuringCapturePhase;
        }

        /// <summary>
        /// 激活建筑（开始工作）
        /// </summary>
        public void Activate()
        {
            if (!IsBuilt)
                return;

            IsActive = true;
        }

        /// <summary>
        /// 停止建筑
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsBuilt)
                return;

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                // 建筑被摧毁，可以在这里添加额外的逻辑
                Debug.Log($"{Name} 被摧毁了！");
            }
        }

        /// <summary>
        /// 修复建筑
        /// </summary>
        public void Repair(float repairAmount)
        {
            if (!IsBuilt)
                return;

            CurrentHealth = Mathf.Min(Health, CurrentHealth + repairAmount);
        }

        /// <summary>
        /// 开始建造
        /// </summary>
        public void StartConstruction()
        {
            IsBuilt = false;
            ConstructionProgress = 0f;
            CurrentHealth = 0f; // 建造过程中没有生命值
        }

        /// <summary>
        /// 更新建造进度
        /// </summary>
        public void UpdateConstruction(float deltaTime)
        {
            if (IsBuilt || ConstructionProgress >= 1f)
                return;

            // 根据建造时间和经过的时间更新进度
            float progressPerSecond = 1.0f / BuildTime;
            ConstructionProgress = Mathf.Min(1.0f, ConstructionProgress + progressPerSecond * deltaTime);

            if (ConstructionProgress >= 1.0f)
            {
                FinishConstruction();
            }
        }

        /// <summary>
        /// 完成建造
        /// </summary>
        private void FinishConstruction()
        {
            IsBuilt = true;
            CurrentHealth = Health; // 建造完成后恢复满血
            ConstructionProgress = 1.0f;
            Debug.Log($"{Name} 建造完成！");
        }

        /// <summary>
        /// 获取建造进度百分比
        /// </summary>
        public float GetConstructionPercentage()
        {
            return ConstructionProgress * 100f;
        }

        /// <summary>
        /// 获取建筑类型名称
        /// </summary>
        public string GetTypeName()
        {
            switch (Type)
            {
                case BuildingType.Tower: return "防御塔";
                case BuildingType.Research: return "研究所";
                case BuildingType.Production: return "生产工厂";
                case BuildingType.Special: return "特殊建筑";
                case BuildingType.SoulCollector: return "灵魂收集器";
                case BuildingType.Temple: return "神殿";
                default: return "未知建筑";
            }
        }

        /// <summary>
        /// 获取当前状态描述
        /// </summary>
        public string GetStatus()
        {
            if (!IsBuilt)
            {
                return $"建造中: {GetConstructionPercentage():F1}%";
            }
            else if (IsActive)
            {
                return "运行中";
            }
            else
            {
                return "待机";
            }
        }

        /// <summary>
        /// 复制建筑数据
        /// </summary>
        public BuildingData Clone()
        {
            return new BuildingData
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Type = this.Type,
                Level = this.Level,
                RequiredResearchLevel = this.RequiredResearchLevel,
                RequiredCoins = this.RequiredCoins,
                RequiredGems = this.RequiredGems,
                AttackPower = this.AttackPower,
                Range = this.Range,
                Health = this.Health,
                BuildTime = this.BuildTime,
                ResearchSpeed = this.ResearchSpeed,
                MaxResearchLevel = this.MaxResearchLevel,
                ProductionSpeed = this.ProductionSpeed,
                ProducedItem = this.ProducedItem,
                ProducesUnitType = this.ProducesUnitType,
                IsUpgradeable = this.IsUpgradeable,
                UpgradeCostMultiplier = this.UpgradeCostMultiplier,
                SoulCollectionRate = this.SoulCollectionRate,
                IsActiveDuringCapturePhase = this.IsActiveDuringCapturePhase,
                IsActiveDuringDefensePhase = this.IsActiveDuringDefensePhase,
                SpriteName = this.SpriteName,
                Tint = this.Tint,
                IsBuilt = this.IsBuilt,
                IsActive = this.IsActive,
                CurrentHealth = this.CurrentHealth,
                ConstructionProgress = this.ConstructionProgress,
                Position = this.Position
            };
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [{GetTypeName()}] - Lv.{Level} {GetStatus()}";
        }
    }
}