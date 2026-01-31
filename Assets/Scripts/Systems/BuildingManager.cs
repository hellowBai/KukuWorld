using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [Header("建筑设置")]
    public int maxBuildings = 20;
    public Transform buildingParent;
    
    private GameManager gameManager;
    private PlayerData playerData;
    
    // 当前建筑列表
    private List<BuildingInstance> activeBuildings = new List<BuildingInstance>();
    
    // 生产队列
    private Queue<ProductionOrder> productionQueue = new Queue<ProductionOrder>();
    
    // 事件
    public System.Action<BuildingData> OnBuildingPlaced;
    public System.Action<BuildingData> OnBuildingUpgraded;
    public System.Action<BuildingData> OnBuildingDestroyed;
    public System.Action<ProductionOrder> OnProductionStarted;
    public System.Action<ProductionOrder> OnProductionCompleted;
    
    [System.Serializable]
    public class BuildingInstance
    {
        public BuildingData data;
        public Vector3 position;
        public int level;
        public float currentHealth;
        public float buildProgress; // 建造进度 (0-1)
        public float upgradeProgress; // 升级进度 (0-1)
        public bool isBeingBuilt;
        public bool isBeingUpgraded;
        public float lastProductionTime;
        public int productionQueueIndex;
        
        public BuildingInstance(BuildingData buildingData, Vector3 pos)
        {
            data = buildingData;
            position = pos;
            level = 1;
            currentHealth = buildingData.MaxHealth;
            buildProgress = 0f;
            upgradeProgress = 0f;
            isBeingBuilt = true;
            isBeingUpgraded = false;
            lastProductionTime = 0f;
            productionQueueIndex = -1;
        }
    }
    
    [System.Serializable]
    public class ProductionOrder
    {
        public BuildingInstance building;
        public BuildingUpgradeType upgradeType;
        public float startTime;
        public float totalTime;
        public System.Action onComplete;
        
        public ProductionOrder(BuildingInstance b, BuildingUpgradeType type, float duration, System.Action callback = null)
        {
            building = b;
            upgradeType = type;
            startTime = Time.time;
            totalTime = duration;
            onComplete = callback;
        }
        
        public float GetProgress()
        {
            return Mathf.Clamp01((Time.time - startTime) / totalTime);
        }
        
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, totalTime - (Time.time - startTime));
        }
    }
    
    public enum BuildingUpgradeType
    {
        LevelUpgrade,     // 等级提升
        AttributeBoost,   // 属性增强
        NewAbility,       // 新能力
        Efficiency,       // 效率提升
        Capacity          // 容量提升
    }
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            playerData = gameManager.GetPlayerData();
        }
        
        Debug.Log("建筑管理系统初始化完成");
    }
    
    /// <summary>
    /// 放置建筑
    /// </summary>
    public bool PlaceBuilding(BuildingType type, Vector3 position)
    {
        if (playerData == null)
        {
            Debug.LogError("玩家数据未加载");
            return false;
        }
        
        if (activeBuildings.Count >= maxBuildings)
        {
            Debug.LogError("已达建筑数量上限");
            return false;
        }
        
        // 检查是否有足够资源
        BuildingData buildingData = CreateBuildingData(type);
        if (playerData.Coins < buildingData.BuildCost)
        {
            Debug.LogError($"金币不足，需要 {buildingData.BuildCost} 金币，当前 {playerData.Coins}");
            return false;
        }
        
        // 扣除建造费用
        playerData.SpendCoins(buildingData.BuildCost);
        
        // 创建建筑实例
        BuildingInstance building = new BuildingInstance(buildingData, position);
        activeBuildings.Add(building);
        
        Debug.Log($"在 {position} 放置了建筑: {buildingData.Name}");
        
        // 触发放置事件
        OnBuildingPlaced?.Invoke(buildingData);
        
        return true;
    }
    
    /// <summary>
    /// 升级建筑
    /// </summary>
    public bool UpgradeBuilding(int buildingIndex, BuildingUpgradeType upgradeType)
    {
        if (buildingIndex < 0 || buildingIndex >= activeBuildings.Count)
        {
            Debug.LogError($"无效的建筑索引: {buildingIndex}");
            return false;
        }
        
        BuildingInstance building = activeBuildings[buildingIndex];
        
        if (building.isBeingUpgraded)
        {
            Debug.LogError($"建筑正在升级中: {building.data.Name}");
            return false;
        }
        
        // 计算升级成本
        int upgradeCost = CalculateUpgradeCost(building, upgradeType);
        if (playerData.Coins < upgradeCost)
        {
            Debug.LogError($"金币不足，需要 {upgradeCost} 金币，当前 {playerData.Coins}");
            return false;
        }
        
        // 检查升级前置条件
        if (!CanUpgradeBuilding(building, upgradeType))
        {
            Debug.LogError($"无法升级建筑: {building.data.Name}");
            return false;
        }
        
        // 扣除升级费用
        playerData.SpendCoins(upgradeCost);
        
        // 开始升级
        float upgradeTime = CalculateUpgradeTime(building, upgradeType);
        ProductionOrder order = new ProductionOrder(building, upgradeType, upgradeTime, () => OnUpgradeComplete(buildingIndex, upgradeType));
        
        building.isBeingUpgraded = true;
        building.upgradeProgress = 0f;
        productionQueue.Enqueue(order);
        
        Debug.Log($"开始升级建筑: {building.data.Name}, 类型: {upgradeType}, 时间: {upgradeTime:F1}s");
        
        // 触发生产开始事件
        OnProductionStarted?.Invoke(order);
        
        return true;
    }
    
    /// <summary>
    /// 销毁建筑
    /// </summary>
    public bool DestroyBuilding(int buildingIndex)
    {
        if (buildingIndex < 0 || buildingIndex >= activeBuildings.Count)
        {
            Debug.LogError($"无效的建筑索引: {buildingIndex}");
            return false;
        }
        
        BuildingInstance building = activeBuildings[buildingIndex];
        
        // 计算返还资源
        int refundAmount = Mathf.CeilToInt(building.data.BuildCost * 0.5f);
        playerData.AddCoins(refundAmount);
        
        // 移除建筑
        activeBuildings.RemoveAt(buildingIndex);
        
        Debug.Log($"销毁了建筑: {building.data.Name}，返还 {refundAmount} 金币");
        
        // 触发销毁事件
        OnBuildingDestroyed?.Invoke(building.data);
        
        return true;
    }
    
    /// <summary>
    /// 获取指定类型的建筑
    /// </summary>
    public List<BuildingInstance> GetBuildingsByType(BuildingType type)
    {
        List<BuildingInstance> buildings = new List<BuildingInstance>();
        
        foreach (var building in activeBuildings)
        {
            if (building.data.Type == type)
            {
                buildings.Add(building);
            }
        }
        
        return buildings;
    }
    
    /// <summary>
    /// 获取所有建筑
    /// </summary>
    public List<BuildingInstance> GetAllBuildings()
    {
        return new List<BuildingInstance>(activeBuildings);
    }
    
    /// <summary>
    /// 更新建筑系统
    /// </summary>
    void Update()
    {
        UpdateProductionQueue();
        UpdateBuildings();
    }
    
    /// <summary>
    /// 更新生产队列
    /// </summary>
    void UpdateProductionQueue()
    {
        if (productionQueue.Count > 0)
        {
            ProductionOrder currentOrder = productionQueue.Peek();
            
            if (currentOrder.GetProgress() >= 1f)
            {
                // 完成当前订单
                ProductionOrder completedOrder = productionQueue.Dequeue();
                completedOrder.onComplete?.Invoke();
                
                // 触发完成事件
                OnProductionCompleted?.Invoke(completedOrder);
            }
        }
    }
    
    /// <summary>
    /// 更新建筑
    /// </summary>
    void UpdateBuildings()
    {
        for (int i = 0; i < activeBuildings.Count; i++)
        {
            BuildingInstance building = activeBuildings[i];
            
            // 更新建造进度
            if (building.isBeingBuilt)
            {
                building.buildProgress += Time.deltaTime / building.data.BuildTime;
                
                if (building.buildProgress >= 1f)
                {
                    building.isBeingBuilt = false;
                    building.buildProgress = 1f;
                    building.currentHealth = building.data.MaxHealth;
                    
                    Debug.Log($"建筑建造完成: {building.data.Name}");
                }
            }
            
            // 更新升级进度
            if (building.isBeingUpgraded)
            {
                // 检查当前订单是否是此建筑
                if (productionQueue.Count > 0 && productionQueue.Peek().building == building)
                {
                    building.upgradeProgress = productionQueue.Peek().GetProgress();
                    
                    if (building.upgradeProgress >= 1f)
                    {
                        building.isBeingUpgraded = false;
                        building.upgradeProgress = 0f;
                    }
                }
            }
            
            // 更新生产功能（如果适用）
            UpdateBuildingProduction(building);
        }
    }
    
    /// <summary>
    /// 更新建筑生产功能
    /// </summary>
    void UpdateBuildingProduction(BuildingInstance building)
    {
        if (building.isBeingBuilt || building.currentHealth <= 0) return;
        
        // 检查生产间隔
        if (Time.time - building.lastProductionTime >= building.data.ProductionInterval)
        {
            // 执行生产
            ProduceResources(building);
            building.lastProductionTime = Time.time;
        }
    }
    
    /// <summary>
    /// 建筑生产资源
    /// </summary>
    void ProduceResources(BuildingInstance building)
    {
        switch (building.data.Type)
        {
            case BuildingType.ResourceGenerator:
                playerData.AddCoins(building.data.ProductionAmount);
                break;
                
            case BuildingType.SoulHarvester:
                playerData.AddSouls(building.data.ProductionAmount * 0.1f);
                break;
                
            case BuildingType.DefensiveTower:
                // 防御塔在战斗中发挥作用
                break;
                
            case BuildingType.TrainingCamp:
                // 训练营提升KuKu能力
                break;
                
            case BuildingType.EvolutionChamber:
                // 进化室加速进化
                break;
        }
        
        Debug.Log($"{building.data.Name} 产生了资源: +{building.data.ProductionAmount}");
    }
    
    /// <summary>
    /// 升级完成回调
    /// </summary>
    void OnUpgradeComplete(int buildingIndex, BuildingUpgradeType upgradeType)
    {
        if (buildingIndex < 0 || buildingIndex >= activeBuildings.Count) return;
        
        BuildingInstance building = activeBuildings[buildingIndex];
        
        // 应用升级效果
        ApplyUpgradeEffects(building, upgradeType);
        
        // 完成升级
        building.isBeingUpgraded = false;
        building.level++;
        
        Debug.Log($"建筑升级完成: {building.data.Name}, 等级: {building.level}");
        
        // 触发升级完成事件
        OnBuildingUpgraded?.Invoke(building.data);
    }
    
    /// <summary>
    /// 应用升级效果
    /// </summary>
    void ApplyUpgradeEffects(BuildingInstance building, BuildingUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case BuildingUpgradeType.LevelUpgrade:
                building.data.MaxHealth *= 1.2f;
                building.data.ProductionAmount *= 1.3f;
                building.currentHealth = building.data.MaxHealth;
                break;
                
            case BuildingUpgradeType.AttributeBoost:
                building.data.MaxHealth *= 1.1f;
                building.currentHealth = building.data.MaxHealth;
                break;
                
            case BuildingUpgradeType.NewAbility:
                // 解锁新能力
                break;
                
            case BuildingUpgradeType.Efficiency:
                building.data.ProductionInterval *= 0.8f; // 提高生产效率
                break;
                
            case BuildingUpgradeType.Capacity:
                building.data.ProductionAmount *= 1.5f; // 提高产量
                break;
        }
    }
    
    /// <summary>
    /// 计算升级成本
    /// </summary>
    int CalculateUpgradeCost(BuildingInstance building, BuildingUpgradeType upgradeType)
    {
        int baseCost = building.data.UpgradeCost;
        
        // 根据建筑等级增加成本
        baseCost = Mathf.CeilToInt(baseCost * Mathf.Pow(1.5f, building.level));
        
        // 根据升级类型调整
        switch (upgradeType)
        {
            case BuildingUpgradeType.LevelUpgrade: return Mathf.CeilToInt(baseCost * 1.2f);
            case BuildingUpgradeType.AttributeBoost: return Mathf.CeilToInt(baseCost * 0.8f);
            case BuildingUpgradeType.NewAbility: return Mathf.CeilToInt(baseCost * 2.0f);
            case BuildingUpgradeType.Efficiency: return Mathf.CeilToInt(baseCost * 1.0f);
            case BuildingUpgradeType.Capacity: return Mathf.CeilToInt(baseCost * 1.5f);
            default: return baseCost;
        }
    }
    
    /// <summary>
    /// 计算升级时间
    /// </summary>
    float CalculateUpgradeTime(BuildingInstance building, BuildingUpgradeType upgradeType)
    {
        float baseTime = building.data.UpgradeTime;
        
        // 根据建筑等级增加时间
        baseTime *= Mathf.Pow(1.3f, building.level);
        
        // 根据升级类型调整
        switch (upgradeType)
        {
            case BuildingUpgradeType.LevelUpgrade: return baseTime * 1.2f;
            case BuildingUpgradeType.AttributeBoost: return baseTime * 0.8f;
            case BuildingUpgradeType.NewAbility: return baseTime * 2.0f;
            case BuildingUpgradeType.Efficiency: return baseTime * 1.0f;
            case BuildingUpgradeType.Capacity: return baseTime * 1.5f;
            default: return baseTime;
        }
    }
    
    /// <summary>
    /// 检查是否可以升级建筑
    /// </summary>
    bool CanUpgradeBuilding(BuildingInstance building, BuildingUpgradeType upgradeType)
    {
        // 检查等级限制
        if (building.level >= building.data.MaxLevel)
        {
            return false;
        }
        
        // 检查前置条件（根据具体需求）
        switch (upgradeType)
        {
            case BuildingUpgradeType.NewAbility:
                // 某些能力可能需要特定等级
                return building.level >= 3;
                
            default:
                return true;
        }
    }
    
    /// <summary>
    /// 创建建筑数据
    /// </summary>
    BuildingData CreateBuildingData(BuildingType type)
    {
        BuildingData data = new BuildingData();
        data.Type = type;
        data.Id = Random.Range(10000, 99999);
        
        switch (type)
        {
            case BuildingType.ResourceGenerator:
                data.Name = "金币生成器";
                data.Description = "定期产生金币的建筑";
                data.BuildCost = 500;
                data.UpgradeCost = 300;
                data.BuildTime = 10f;
                data.UpgradeTime = 15f;
                data.MaxHealth = 100f;
                data.MaxLevel = 10;
                data.ProductionAmount = 10;
                data.ProductionInterval = 5f;
                break;
                
            case BuildingType.SoulHarvester:
                data.Name = "灵魂收割者";
                data.Description = "收集灵魂能量的建筑";
                data.BuildCost = 800;
                data.UpgradeCost = 500;
                data.BuildTime = 15f;
                data.UpgradeTime = 20f;
                data.MaxHealth = 80f;
                data.MaxLevel = 8;
                data.ProductionAmount = 5;
                data.ProductionInterval = 8f;
                break;
                
            case BuildingType.DefensiveTower:
                data.Name = "防御塔";
                data.Description = "自动攻击敌人的防御建筑";
                data.BuildCost = 1000;
                data.UpgradeCost = 600;
                data.BuildTime = 20f;
                data.UpgradeTime = 25f;
                data.MaxHealth = 200f;
                data.MaxLevel = 12;
                data.ProductionAmount = 0; // 不生产资源
                data.ProductionInterval = 0f;
                break;
                
            case BuildingType.TrainingCamp:
                data.Name = "训练营";
                data.Description = "提升KuKu能力的训练场所";
                data.BuildCost = 1200;
                data.UpgradeCost = 700;
                data.BuildTime = 25f;
                data.UpgradeTime = 30f;
                data.MaxHealth = 150f;
                data.MaxLevel = 10;
                data.ProductionAmount = 0;
                data.ProductionInterval = 0f;
                break;
                
            case BuildingType.EvolutionChamber:
                data.Name = "进化室";
                data.Description = "加速KuKu进化的神秘房间";
                data.BuildCost = 2000;
                data.UpgradeCost = 1000;
                data.BuildTime = 30f;
                data.UpgradeTime = 35f;
                data.MaxHealth = 120f;
                data.MaxLevel = 6;
                data.ProductionAmount = 0;
                data.ProductionInterval = 0f;
                break;
                
            default:
                data.Name = "未知建筑";
                data.Description = "未知类型的建筑";
                data.BuildCost = 100;
                data.UpgradeCost = 50;
                data.BuildTime = 5f;
                data.UpgradeTime = 10f;
                data.MaxHealth = 50f;
                data.MaxLevel = 5;
                data.ProductionAmount = 1;
                data.ProductionInterval = 10f;
                break;
        }
        
        return data;
    }
    
    /// <summary>
    /// 获取建筑槽位信息
    /// </summary>
    public (int usedSlots, int totalSlots) GetSlotInfo()
    {
        return (activeBuildings.Count, maxBuildings);
    }
    
    /// <summary>
    /// 获取生产队列信息
    /// </summary>
    public (int queueSize, float currentProgress, string currentItem) GetProductionInfo()
    {
        if (productionQueue.Count == 0)
        {
            return (0, 0f, "无生产任务");
        }
        
        var currentOrder = productionQueue.Peek();
        string itemName = currentOrder.upgradeType.ToString();
        
        return (productionQueue.Count, currentOrder.GetProgress(), itemName);
    }
    
    /// <summary>
    /// 取消当前生产任务
    /// </summary>
    public bool CancelCurrentProduction()
    {
        if (productionQueue.Count == 0) return false;
        
        var cancelledOrder = productionQueue.Dequeue();
        cancelledOrder.building.isBeingUpgraded = false;
        
        Debug.Log($"取消了生产任务: {cancelledOrder.upgradeType}");
        
        return true;
    }
    
    /// <summary>
    /// 快速建造（仅用于调试）
    /// </summary>
    public void FastBuild(int buildingIndex)
    {
        if (buildingIndex < 0 || buildingIndex >= activeBuildings.Count) return;
        
        var building = activeBuildings[buildingIndex];
        building.isBeingBuilt = false;
        building.buildProgress = 1f;
        building.currentHealth = building.data.MaxHealth;
        
        Debug.Log($"快速建造完成: {building.data.Name}");
    }
    
    void OnDestroy()
    {
        Debug.Log("建筑管理系统已销毁");
    }
}