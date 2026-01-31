using System;
using System.Collections.Generic;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 建筑数据结构扩展
    /// </summary>
    public partial class BuildingData
    {
        // 添加缺少的位置属性
        public Vector3 Position { get; set; } = Vector3.zero;
        
        // 添加建造相关属性
        public bool IsBuilt { get; set; } = false;
        public float ConstructionProgress { get; set; } = 0f;
        public float MaxConstructionTime { get; set; } = 10f; // 总建造时间
        
        /// <summary>
        /// 开始建造
        /// </summary>
        public void StartConstruction()
        {
            ConstructionProgress = 0f;
            IsBuilt = false;
        }
        
        /// <summary>
        /// 更新建造进度
        /// </summary>
        public void UpdateConstruction(float deltaTime)
        {
            if (!IsBuilt)
            {
                ConstructionProgress += deltaTime;
                if (ConstructionProgress >= MaxConstructionTime)
                {
                    ConstructionProgress = MaxConstructionTime;
                    IsBuilt = true;
                }
            }
        }
        
        /// <summary>
        /// 获取建造进度百分比
        /// </summary>
        public float GetConstructionPercentage()
        {
            if (MaxConstructionTime <= 0) return 1f;
            return ConstructionProgress / MaxConstructionTime;
        }
        
        /// <summary>
        /// 获取建筑类型名称
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
                case BuildingType.Temple:
                    return "神殿";
                default:
                    return "未知建筑";
            }
        }
        
        /// <summary>
        /// 克隆建筑数据
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
                SpriteName = this.SpriteName,
                Tint = this.Tint,
                IsActive = this.IsActive,
                CurrentHealth = this.CurrentHealth,
                Position = this.Position,
                IsBuilt = this.IsBuilt,
                ConstructionProgress = this.ConstructionProgress,
                MaxConstructionTime = this.MaxConstructionTime
            };
        }
    }
}