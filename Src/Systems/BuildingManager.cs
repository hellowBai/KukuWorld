using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 建筑管理器 - 管理所有建筑实例，处理建筑建造和升级
    /// </summary>
    public class BuildingManager
    {
        // 建筑列表
        private List<BuildingData> allBuildings;
        private List<BuildingData> activeBuildings; // 当前活跃的建筑
        
        // 建筑限制
        private int maxBuildings = 50; // 最大建筑数量
        private Dictionary<BuildingData.BuildingType, int> buildingLimits; // 每种类型建筑的最大数量
        
        // 事件
        public event Action<BuildingData> OnBuildingConstructed;
        public event Action<BuildingData> OnBuildingUpgraded;
        public event Action<BuildingData> OnBuildingDestroyed;
        public event Action<BuildingData, float> OnBuildingHealthChanged;
        public event Action<string> OnBuildingError;
        
        // 构造函数
        public BuildingManager()
        {
            allBuildings = new List<BuildingData>();
            activeBuildings = new List<BuildingData>();
            
            InitializeBuildingLimits();
        }
        
        /// <summary>
        /// 初始化建筑限制
        /// </summary>
        private void InitializeBuildingLimits()
        {
            buildingLimits = new Dictionary<BuildingData.BuildingType, int>
            {
                { BuildingData.BuildingType.Tower, 20 },        // 最多20座防御塔
                { BuildingData.BuildingType.Research, 5 },      // 最多5个研究所
                { BuildingData.BuildingType.Production, 10 },   // 最多10个生产工厂
                { BuildingData.BuildingType.Special, 8 },       // 最多8个特殊建筑
                { BuildingData.BuildingType.SoulCollector, 15 }, // 最多15个灵魂收集器
                { BuildingData.BuildingType.Temple, 1 }         // 只能有一个神殿
            };
        }
        
        /// <summary>
        /// 建造建筑
        /// </summary>
        public bool BuildBuilding(BuildingData building, Vector3 position)
        {
            if (building == null)
            {
                OnBuildingError?.Invoke("建筑数据无效");
                return false;
            }
            
            // 检查是否超过建筑总数限制
            if (allBuildings.Count >= maxBuildings)
            {
                OnBuildingError?.Invoke("已达到建筑总数上限");
                return false;
            }
            
            // 检查是否超过该类型建筑的限制
            int currentTypeCount = GetBuildingsOfType(building.Type).Count;
            if (currentTypeCount >= buildingLimits[building.Type])
            {
                OnBuildingError?.Invoke($"已达到{building.GetTypeName()}数量上限");
                return false;
            }
            
            // 检查资源是否足够
            var cost = building.GetBuildCost();
            // 这里需要连接到玩家数据来检查资源，暂时跳过检查
            
            // 创建建筑实例
            BuildingData newBuilding = building.Clone();
            newBuilding.Id = GenerateBuildingId();
            newBuilding.Position = position;
            newBuilding.StartConstruction();
            
            // 添加到列表
            allBuildings.Add(newBuilding);
            
            // 如果建筑已完成建造（例如某些特殊建筑），则激活
            if (newBuilding.IsBuilt)
            {
                activeBuildings.Add(newBuilding);
            }
            
            OnBuildingConstructed?.Invoke(newBuilding);
            
            Debug.Log($"建筑 {newBuilding.Name} 开始建造");
            return true;
        }
        
        /// <summary>
        /// 升级建筑
        /// </summary>
        public bool UpgradeBuilding(int buildingId)
        {
            BuildingData building = GetBuildingById(buildingId);
            
            if (building == null)
            {
                OnBuildingError?.Invoke("找不到指定的建筑");
                return false;
            }
            
            if (!building.IsUpgradeable)
            {
                OnBuildingError?.Invoke("该建筑无法升级");
                return false;
            }
            
            if (building.Level >= building.MaxResearchLevel)
            {
                OnBuildingError?.Invoke("建筑已达到最高等级");
                return false;
            }
            
            // 检查资源是否足够
            var upgradeCost = building.GetUpgradeCost();
            // 这里需要连接到玩家数据来检查资源，暂时跳过检查
            
            // 执行升级
            building.Upgrade();
            
            OnBuildingUpgraded?.Invoke(building);
            
            Debug.Log($"建筑 {building.Name} 升级到 {building.Level} 级");
            return true;
        }
        
        /// <summary>
        /// 销毁建筑
        /// </summary>
        public bool DestroyBuilding(int buildingId)
        {
            BuildingData building = GetBuildingById(buildingId);
            
            if (building == null)
            {
                OnBuildingError?.Invoke("找不到指定的建筑");
                return false;
            }
            
            // 从所有列表中移除
            allBuildings.Remove(building);
            activeBuildings.Remove(building);
            
            OnBuildingDestroyed?.Invoke(building);
            
            Debug.Log($"建筑 {building.Name} 已被销毁");
            return true;
        }
        
        /// <summary>
        /// 激活所有建筑（根据当前阶段）
        /// </summary>
        public void ActivateBuildingsForPhase(BattleSystem.BattleState phase)
        {
            foreach (var building in allBuildings)
            {
                // 检查建筑是否可以在当前阶段激活
                if (building.CanBuildInCurrentPhase(phase))
                {
                    building.Activate();
                    if (!activeBuildings.Contains(building))
                    {
                        activeBuildings.Add(building);
                    }
                }
                else
                {
                    building.Deactivate();
                    activeBuildings.Remove(building);
                }
            }
            
            Debug.Log($"为阶段 {phase} 激活了 {activeBuildings.Count} 个建筑");
        }
        
        /// <summary>
        /// 更新建筑逻辑
        /// </summary>
        public void UpdateBuildings(float deltaTime)
        {
            // 更新所有建筑的建造进度
            foreach (var building in allBuildings)
            {
                if (!building.IsBuilt)
                {
                    building.UpdateConstruction(deltaTime);
                    
                    // 如果建筑刚完成建造，将其添加到活跃列表
                    if (building.IsBuilt && !activeBuildings.Contains(building))
                    {
                        activeBuildings.Add(building);
                    }
                }
            }
            
            // 根据不同建筑类型执行特定逻辑
            ExecuteBuildingOperations(deltaTime);
        }
        
        /// <summary>
        /// 执行建筑操作
        /// </summary>
        private void ExecuteBuildingOperations(float deltaTime)
        {
            foreach (var building in activeBuildings)
            {
                if (!building.IsActive)
                    continue;
                
                switch (building.Type)
                {
                    case BuildingData.BuildingType.SoulCollector:
                        // 灵魂收集器产生灵魂
                        CollectSouls(building, deltaTime);
                        break;
                    case BuildingData.BuildingType.Production:
                        // 生产工厂生产单位
                        ProduceUnits(building, deltaTime);
                        break;
                    case BuildingData.BuildingType.Research:
                        // 研究所进行研究
                        ConductResearch(building, deltaTime);
                        break;
                    case BuildingData.BuildingType.Tower:
                        // 防御塔执行防御逻辑（在战斗系统中处理）
                        break;
                    case BuildingData.BuildingType.Special:
                        // 特殊建筑的特殊逻辑
                        ExecuteSpecialBuildingLogic(building, deltaTime);
                        break;
                    case BuildingData.BuildingType.Temple:
                        // 神殿的特殊逻辑
                        ExecuteTempleLogic(building, deltaTime);
                        break;
                }
            }
        }
        
        /// <summary>
        /// 灵魂收集
        /// </summary>
        private void CollectSouls(BuildingData building, float deltaTime)
        {
            // 灵魂收集器按照速率收集灵魂
            float soulsCollected = building.SoulCollectionRate * deltaTime;
            
            // 这里需要连接到玩家数据来添加灵魂，暂时只记录
            Debug.Log($"灵魂收集器 {building.Name} 收集了 {soulsCollected:F2} 点灵魂");
        }
        
        /// <summary>
        /// 生产单位
        /// </summary>
        private void ProduceUnits(BuildingData building, float deltaTime)
        {
            // 生产工厂按照生产速度生产单位
            float productionProgress = building.ProductionSpeed * deltaTime;
            
            // 实际游戏中会根据进度生成单位
            Debug.Log($"生产工厂 {building.Name} 生产进度: {productionProgress:F2}");
        }
        
        /// <summary>
        /// 进行研究
        /// </summary>
        private void ConductResearch(BuildingData building, float deltaTime)
        {
            // 研究所按照研究速度进行研究
            float researchProgress = building.ResearchSpeed * deltaTime;
            
            // 实际游戏中会根据进度解锁技术
            Debug.Log($"研究所 {building.Name} 研究进度: {researchProgress:F2}");
        }
        
        /// <summary>
        /// 执行特殊建筑逻辑
        /// </summary>
        private void ExecuteSpecialBuildingLogic(BuildingData building, float deltaTime)
        {
            // 特殊建筑的具体逻辑
            Debug.Log($"特殊建筑 {building.Name} 执行特殊逻辑");
        }
        
        /// <summary>
        /// 执行神殿逻辑
        /// </summary>
        private void ExecuteTempleLogic(BuildingData building, float deltaTime)
        {
            // 神殿的特殊逻辑
            Debug.Log($"神殿 {building.Name} 执行神殿逻辑");
        }
        
        /// <summary>
        /// 获取指定类型的建筑
        /// </summary>
        public List<BuildingData> GetBuildingsOfType(BuildingData.BuildingType type)
        {
            return allBuildings.Where(b => b.Type == type).ToList();
        }
        
        /// <summary>
        /// 获取可生产指定单位的建筑
        /// </summary>
        public List<BuildingData> GetProductionBuildingsForUnit(UnitData.UnitType unitType)
        {
            return allBuildings.Where(b => 
                b.Type == BuildingData.BuildingType.Production && 
                b.ProducesUnitType == unitType).ToList();
        }
        
        /// <summary>
        /// 获取建筑通过ID
        /// </summary>
        public BuildingData GetBuildingById(int id)
        {
            return allBuildings.FirstOrDefault(b => b.Id == id);
        }
        
        /// <summary>
        /// 获取所有建筑
        /// </summary>
        public List<BuildingData> GetAllBuildings()
        {
            return new List<BuildingData>(allBuildings);
        }
        
        /// <summary>
        /// 获取活跃建筑
        /// </summary>
        public List<BuildingData> GetActiveBuildings()
        {
            return new List<BuildingData>(activeBuildings);
        }
        
        /// <summary>
        /// 生成建筑ID
        /// </summary>
        private int GenerateBuildingId()
        {
            // 简单的ID生成方法，实际中可能需要更复杂的逻辑
            int newId = 1;
            while (allBuildings.Exists(b => b.Id == newId))
            {
                newId++;
            }
            return newId;
        }
        
        /// <summary>
        /// 检查位置是否可以建造
        /// </summary>
        public bool CanBuildAtPosition(Vector3 position, float minDistance = 2f)
        {
            // 检查与其他建筑的距离
            foreach (var building in allBuildings)
            {
                float distance = Vector3.Distance(building.Position, position);
                if (distance < minDistance)
                {
                    return false;
                }
            }
            
            // 可以添加更多检查，如地形、障碍物等
            return true;
        }
        
        /// <summary>
        /// 获取指定位置附近的建筑
        /// </summary>
        public List<BuildingData> GetBuildingsNearPosition(Vector3 position, float radius)
        {
            List<BuildingData> nearbyBuildings = new List<BuildingData>();
            
            foreach (var building in allBuildings)
            {
                float distance = Vector3.Distance(building.Position, position);
                if (distance <= radius)
                {
                    nearbyBuildings.Add(building);
                }
            }
            
            return nearbyBuildings;
        }
        
        /// <summary>
        /// 获取建筑统计信息
        /// </summary>
        public Dictionary<BuildingData.BuildingType, int> GetBuildingStatistics()
        {
            var stats = new Dictionary<BuildingData.BuildingType, int>();
            
            foreach (BuildingData.BuildingType type in Enum.GetValues(typeof(BuildingData.BuildingType)))
            {
                stats[type] = GetBuildingsOfType(type).Count;
            }
            
            return stats;
        }
        
        /// <summary>
        /// 获取所有建筑的总价值
        /// </summary>
        public int GetTotalBuildingValue()
        {
            int totalValue = 0;
            
            foreach (var building in allBuildings)
            {
                totalValue += building.RequiredCoins; // 简单地使用建造费用作为价值
            }
            
            return totalValue;
        }
        
        /// <summary>
        /// 修复建筑
        /// </summary>
        public bool RepairBuilding(int buildingId, float repairAmount)
        {
            BuildingData building = GetBuildingById(buildingId);
            
            if (building == null)
            {
                OnBuildingError?.Invoke("找不到指定的建筑");
                return false;
            }
            
            building.Repair(repairAmount);
            
            OnBuildingHealthChanged?.Invoke(building, building.CurrentHealth);
            
            Debug.Log($"建筑 {building.Name} 修复了 {repairAmount:F2} 点生命值");
            return true;
        }
        
        /// <summary>
        /// 对建筑造成伤害
        /// </summary>
        public bool DamageBuilding(int buildingId, float damage)
        {
            BuildingData building = GetBuildingById(buildingId);
            
            if (building == null)
            {
                OnBuildingError?.Invoke("找不到指定的建筑");
                return false;
            }
            
            building.TakeDamage(damage);
            
            OnBuildingHealthChanged?.Invoke(building, building.CurrentHealth);
            
            if (building.CurrentHealth <= 0)
            {
                // 建筑被摧毁
                DestroyBuilding(buildingId);
            }
            
            Debug.Log($"建筑 {building.Name} 受到 {damage:F2} 点伤害，剩余生命值: {building.CurrentHealth:F2}");
            return true;
        }
        
        /// <summary>
        /// 获取建筑建造队列
        /// </summary>
        public List<BuildingData> GetConstructionQueue()
        {
            return allBuildings.Where(b => !b.IsBuilt).ToList();
        }
        
        /// <summary>
        /// 重置建筑管理器
        /// </summary>
        public void Reset()
        {
            allBuildings.Clear();
            activeBuildings.Clear();
            
            Debug.Log("建筑管理器已重置");
        }
        
        /// <summary>
        /// 获取当前建筑数量
        /// </summary>
        public int GetBuildingCount()
        {
            return allBuildings.Count;
        }
        
        /// <summary>
        /// 获取指定类型建筑的当前数量
        /// </summary>
        public int GetBuildingCount(BuildingData.BuildingType type)
        {
            return GetBuildingsOfType(type).Count;
        }
        
        /// <summary>
        /// 检查是否可以建造指定类型的建筑
        /// </summary>
        public bool CanBuildType(BuildingData.BuildingType type)
        {
            return GetBuildingCount(type) < buildingLimits[type];
        }
        
        /// <summary>
        /// 获取建造某类型建筑还需要多少数量
        /// </summary>
        public int GetRemainingBuildableCount(BuildingData.BuildingType type)
        {
            return buildingLimits[type] - GetBuildingCount(type);
        }
        
        /// <summary>
        /// 获取所有建筑的建造进度
        /// </summary>
        public float GetOverallConstructionProgress()
        {
            if (allBuildings.Count == 0) return 100f;
            
            float totalProgress = 0f;
            foreach (var building in allBuildings)
            {
                totalProgress += building.GetConstructionPercentage();
            }
            
            return totalProgress / allBuildings.Count;
        }
    }
}