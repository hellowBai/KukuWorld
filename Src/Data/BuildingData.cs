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
        public enum BuildingType { 
            Tower,          // 防御塔
            Research,       // 研究所
            Production,     // 生产工厂
            Special,        // 特殊建筑
            SoulCollector,  // 灵魂收集器
            Market,         // 市场
            Training,       // 训练场
            FusionCenter    // 融合中心
        }
        
        // 等级和需求
        public int Level { get; set; }
        public int RequiredResearchLevel { get; set; }
        public int RequiredCoins { get; set; }
        public int RequiredGems { get; set; }
        
        // 属性（根据类型不同）
        public float AttackPower { get; set; }      // 攻击力（塔类）
        public float Range { get; set; }            // 射程（塔类）
        public float Health { get; set; }           // 生命值
        public float BuildTime { get; set; }        // 建造时间
        
        // 研究属性
        public float ResearchSpeed { get; set; }    // 研究速度（研究类）
        public int MaxResearchLevel { get; set; }   // 最大研究等级
        
        // 生产属性
        public float ProductionSpeed { get; set; }  // 生产速度（生产类）
        public string ProducedItem { get; set; }    // 生产物品
        public UnitData.UnitType ProducesUnitType { get; set; } // 生产单位类型
        
        // 特殊属性
        public bool IsUpgradeable { get; set; }
        public int UpgradeCostMultiplier { get; set; }
        
        // 捕捉阶段特有属性
        public float SoulCollectionRate { get; set; }        // 灵魂收集速率
        public bool IsActiveDuringCapturePhase { get; set; }  // 捕捉阶段是否激活
        
        // 状态
        public bool IsActive { get; set; }
        public float CurrentHealth { get; set; }
        public int MaxLevel { get; set; }
        
        // 视觉相关
        public string SpriteName { get; set; }
        public Color Tint { get; set; }
        
        // 其他属性
        public float EnergyConsumption { get; set; }    // 能量消耗
        public float EnergyGeneration { get; set; }     // 能量生成
        public float MaintenanceCost { get; set; }      // 维护费用
        public float UpgradeTime { get; set; }          // 升级时间
        public int StorageCapacity { get; set; }        // 存储容量
        public int WorkerCapacity { get; set; }         // 工作人员容量
        public float Efficiency { get; set; }           // 效率
        public float DefenseBonus { get; set; }         // 防御加成
        public float ProductionBonus { get; set; }      // 生产加成
        public float ResearchBonus { get; set; }        // 研究加成
        public bool RequiresPower { get; set; }         // 是否需要电力
        public float PowerRequirement { get; set; }     // 电力需求
        public bool IsPowered { get; set; }             // 是否有电力供应
        public float ConstructionProgress { get; set; } // 建造进度
        public bool IsUnderConstruction { get; set; }   // 是否在建造中

        public BuildingData()
        {
            // 初始化默认值
            Name = "未知建筑";
            Description = "一个未定义的建筑";
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
            IsActiveDuringCapturePhase = false;
            IsActive = false;
            CurrentHealth = Health;
            MaxLevel = 10;
            SpriteName = "DefaultBuildingSprite";
            Tint = Color.white;
            EnergyConsumption = 0f;
            EnergyGeneration = 0f;
            MaintenanceCost = 0f;
            UpgradeTime = 5f;
            StorageCapacity = 0;
            WorkerCapacity = 0;
            Efficiency = 1.0f;
            DefenseBonus = 0f;
            ProductionBonus = 0f;
            ResearchBonus = 0f;
            RequiresPower = true;
            PowerRequirement = 10f;
            IsPowered = true;
            ConstructionProgress = 0f;
            IsUnderConstruction = false;
        }
        
        /// <summary>
        /// 升级建筑
        /// </summary>
        public void Upgrade()
        {
            if (Level < MaxLevel)
            {
                Level++;
                
                // 根据建筑类型提升属性
                switch (Type)
                {
                    case BuildingType.Tower:
                        AttackPower *= 1.3f;
                        Range *= 1.1f;
                        Health *= 1.2f;
                        break;
                    case BuildingType.Research:
                        ResearchSpeed *= 1.25f;
                        break;
                    case BuildingType.Production:
                        ProductionSpeed *= 1.2f;
                        break;
                    case BuildingType.SoulCollector:
                        SoulCollectionRate *= 1.3f;
                        break;
                    case BuildingType.Market:
                        StorageCapacity = (int)(StorageCapacity * 1.5f);
                        break;
                    case BuildingType.Training:
                        Efficiency *= 1.15f;
                        break;
                    case BuildingType.FusionCenter:
                        // 融合中心提升融合成功率
                        break;
                    case BuildingType.Special:
                        // 特殊建筑特殊处理
                        break;
                }
                
                // 恢复满血
                CurrentHealth = Health;
            }
        }
        
        /// <summary>
        /// 获取建造花费
        /// </summary>
        public (int coins, int gems) GetBuildCost()
        {
            int coins = (int)(RequiredCoins * Math.Pow(UpgradeCostMultiplier, Level - 1));
            int gems = (int)(RequiredGems * Math.Pow(UpgradeCostMultiplier, Level - 1) / 2);
            return (coins, gems);
        }
        
        /// <summary>
        /// 获取升级花费
        /// </summary>
        public (int coins, int gems) GetUpgradeCost()
        {
            int coins = (int)(RequiredCoins * UpgradeCostMultiplier);
            int gems = (int)(RequiredGems * UpgradeCostMultiplier / 2);
            return (coins, gems);
        }
        
        /// <summary>
        /// 检查是否可以建造（基于当前游戏阶段）
        /// </summary>
        public bool CanBuildInCurrentPhase(GameManager.GameState currentPhase)
        {
            switch (Type)
            {
                case BuildingType.SoulCollector:
                    // 灵魂收集器只能在捕捉阶段建造
                    return currentPhase == GameManager.GameState.CapturePhase;
                case BuildingType.Tower:
                case BuildingType.Research:
                case BuildingType.Production:
                case BuildingType.Special:
                case BuildingType.Market:
                case BuildingType.Training:
                case BuildingType.FusionCenter:
                    // 其他建筑可以在防守阶段建造
                    return currentPhase == GameManager.GameState.DefensePhase;
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 激活建筑（开始工作）
        /// </summary>
        public void Activate()
        {
            if (CurrentHealth > 0 && IsPowered)
            {
                IsActive = true;
            }
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
            CurrentHealth -= damage;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
        }
        
        /// <summary>
        /// 修理建筑
        /// </summary>
        public void Repair()
        {
            CurrentHealth = Health;
        }
        
        /// <summary>
        /// 获取类型名称
        /// </summary>
        public string GetTypeName()
        {
            switch (Type)
            {
                case BuildingType.Tower:
                    return "防御塔";
                case BuildingType.Research:
                    return "研究所";
                case BuildingType.Production:
                    return "生产工厂";
                case BuildingType.Special:
                    return "特殊建筑";
                case BuildingType.SoulCollector:
                    return "灵魂收集器";
                case BuildingType.Market:
                    return "市场";
                case BuildingType.Training:
                    return "训练场";
                case BuildingType.FusionCenter:
                    return "融合中心";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 检查建筑是否被摧毁
        /// </summary>
        public bool IsDestroyed()
        {
            return CurrentHealth <= 0;
        }
        
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [{GetTypeName()}] - Lv.{Level} HP:{CurrentHealth:F0}/{Health:F0}";
        }
        
        /// <summary>
        /// 复制当前建筑数据
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
                IsActive = this.IsActive,
                CurrentHealth = this.CurrentHealth,
                MaxLevel = this.MaxLevel,
                SpriteName = this.SpriteName,
                Tint = this.Tint,
                EnergyConsumption = this.EnergyConsumption,
                EnergyGeneration = this.EnergyGeneration,
                MaintenanceCost = this.MaintenanceCost,
                UpgradeTime = this.UpgradeTime,
                StorageCapacity = this.StorageCapacity,
                WorkerCapacity = this.WorkerCapacity,
                Efficiency = this.Efficiency,
                DefenseBonus = this.DefenseBonus,
                ProductionBonus = this.ProductionBonus,
                ResearchBonus = this.ResearchBonus,
                RequiresPower = this.RequiresPower,
                PowerRequirement = this.PowerRequirement,
                IsPowered = this.IsPowered,
                ConstructionProgress = this.ConstructionProgress,
                IsUnderConstruction = this.IsUnderConstruction
            };
        }
    }
}