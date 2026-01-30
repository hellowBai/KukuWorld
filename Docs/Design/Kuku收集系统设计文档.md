# Kuku收集塔防游戏《KukuWorld》Kuku收集系统设计文档

## 1. 系统概述

Kuku收集系统是《KukuWorld》游戏的核心组成部分，包含游戏内外两个层面的收集机制。游戏外的图鉴系统记录玩家已解锁的Kuku，游戏内的收集系统管理玩家实际拥有的Kuku。该系统与捕捉、进化、融合等系统紧密关联。

## 2. 系统架构

### 2.1 KukuCollectionManager (Kuku收集管理器)
- **职责**: 管理Kuku图鉴数据库，处理图鉴收集进度
- **类型**: 单例系统类
- **依赖**: KukuData, PlayerData, GameManager

## 3. KukuCollectionManager (Kuku收集管理器)

### 3.1 类定义
```csharp
public class KukuCollectionManager : MonoBehaviour
```

### 3.2 系统职责
- 管理Kuku图鉴数据库（游戏外）
- 处理图鉴收集进度
- 管理图鉴显示界面
- 处理页面切换
- 同步游戏内外收集状态

### 3.3 数据结构

#### 3.3.1 配置参数
```csharp
[Header("收集系统配置")]
public int kukusPerPage = 12;                    // 每页Kuku数量
public float unlockNotificationDuration = 2f;   // 解锁通知显示时间
```

#### 3.3.2 Kuku图鉴数据
```csharp
// Kuku图鉴条目
[System.Serializable]
public class KukuCollectionEntry
{
    public int kukuId;                           // KukuID
    public string kukuName;                      // Kuku名称
    public KukuData.RarityType rarity;           // 稀有度
    public bool isUnlocked;                     // 是否已解锁
    public int timesCaught;                     // 捕捉次数
    public System.DateTime firstUnlockDate;     // 首次解锁日期
    public System.DateTime lastCatchDate;       // 最后捕捉日期

    public KukuCollectionEntry(int id, string name, KukuData.RarityType r)
    {
        kukuId = id;
        kukuName = name;
        rarity = r;
        isUnlocked = false;
        timesCaught = 0;
        firstUnlockDate = System.DateTime.MinValue;
        lastCatchDate = System.DateTime.MinValue;
    }
}
```

#### 3.3.3 图鉴页面数据
```csharp
// 图鉴页面数据
public struct CollectionPageData
{
    public int pageIndex;                           // 页面索引
    public List<KukuCollectionEntry> kukus;         // 该页面的Kuku
    public int unlockedCount;                       // 已解锁数量
    public int totalCount;                          // 总数量
    public float completionRate;                    // 完成率

    public CollectionPageData(int index, List<KukuCollectionEntry> p)
    {
        pageIndex = index;
        kukus = p;
        unlockedCount = 0;
        totalCount = p.Count;
        completionRate = 0f;

        // 计算已解锁数量
        foreach (var kuku in p)
        {
            if (kuku.isUnlocked)
            {
                unlockedCount++;
            }
        }

        completionRate = totalCount > 0 ? (float)unlockedCount / totalCount : 0f;
    }
}
```

### 3.4 核心方法

#### 3.4.1 初始化方法
```csharp
/// <summary>
/// 初始化Kuku数据库
/// </summary>
private void InitializeKukuDatabase()
{
    // 这里可以从配置文件或资源中加载Kuku数据
    // 或者根据游戏进度动态生成
    
    // 示例：生成一些基础Kuku数据
    GenerateInitialKukuDatabase();
}

/// <summary>
/// 生成初始Kuku数据库
/// </summary>
private void GenerateInitialKukuDatabase()
{
    // 示例：创建一些基础Kuku
    // 在实际游戏中，这应该从配置文件或预制数据中加载
    
    // 创建常见稀有度Kuku (Common)
    for (int i = 1; i <= 20; i++)
    {
        KukuCollectionEntry entry = new KukuCollectionEntry(
            i, 
            GenerateKukuName(KukuData.RarityType.Common, i), 
            KukuData.RarityType.Common
        );
        allKukuDatabase.Add(entry.kukuId, entry);
    }

    // 创建罕见稀有度Kuku (Rare)
    for (int i = 21; i <= 35; i++)
    {
        KukuCollectionEntry entry = new KukuCollectionEntry(
            i, 
            GenerateKukuName(KukuData.RarityType.Rare, i), 
            KukuData.RarityType.Rare
        );
        allKukuDatabase.Add(entry.kukuId, entry);
    }

    // 创建史诗稀有度Kuku (Epic)
    for (int i = 36; i <= 45; i++)
    {
        KukuCollectionEntry entry = new KukuCollectionEntry(
            i, 
            GenerateKukuName(KukuData.RarityType.Epic, i), 
            KukuData.RarityType.Epic
        );
        allKukuDatabase.Add(entry.kukuId, entry);
    }

    // 创建传说稀有度Kuku (Legendary)
    for (int i = 46; i <= 50; i++)
    {
        KukuCollectionEntry entry = new KukuCollectionEntry(
            i, 
            GenerateKukuName(KukuData.RarityType.Legendary, i), 
            KukuData.RarityType.Legendary
        );
        allKukuDatabase.Add(entry.kukuId, entry);
    }

    // 创建神话稀有度Kuku (Mythic)
    for (int i = 51; i <= 55; i++)
    {
        KukuCollectionEntry entry = new KukuCollectionEntry(
            i, 
            GenerateKukuName(KukuData.RarityType.Mythic, i), 
            KukuData.RarityType.Mythic
        );
        allKukuDatabase.Add(entry.kukuId, entry);
    }

    Debug.Log($"初始化了 {allKukuDatabase.Count} 个Kuku到图鉴数据库");
}
```

#### 3.4.2 Kuku名称生成方法
```csharp
/// <summary>
/// 生成Kuku名字
/// </summary>
private string GenerateKukuName(KukuData.RarityType rarity, int id)
{
    string[] commonPrefixes = { "森林", "草原", "河流", "山峰", "湖泊", "沙漠", "海洋", "天空", "洞穴", "古树" };
    string[] rarePrefixes = { "神秘", "幻影", "幽灵", "暗夜", "星辰", "月光", "晨曦", "暮色", "彩虹", "极光" };
    string[] epicPrefixes = { "龙族", "凤凰", "麒麟", "玄武", "朱雀", "白虎", "青龙", "貔貅", "饕餮", "梼杌" };
    string[] legendaryPrefixes = { "创世", "混沌", "秩序", "永恒", "时光", "空间", "元素", "法则", "真理", "命运" };
    string[] mythicPrefixes = { "虚无", "创世神", "原初", "终结", "无限", "绝对", "超脱", "至高", "无上", "永恒" };

    string[] suffixes = { "精灵", "守护者", "使者", "战士", "法师", "弓手", "刺客", "骑士", "贤者", "智者" };

    string[] prefixes = new string[0];
    switch (rarity)
    {
        case KukuData.RarityType.Common:
            prefixes = commonPrefixes;
            break;
        case KukuData.RarityType.Rare:
            prefixes = rarePrefixes;
            break;
        case KukuData.RarityType.Epic:
            prefixes = epicPrefixes;
            break;
        case KukuData.RarityType.Legendary:
            prefixes = legendaryPrefixes;
            break;
        case KukuData.RarityType.Mythic:
            prefixes = mythicPrefixes;
            break;
    }

    string prefix = prefixes[id % prefixes.Length];
    string suffix = suffixes[id % suffixes.Length];

    return prefix + suffix;
}
```

#### 3.4.3 Kuku数据获取方法
```csharp
/// <summary>
/// 获取指定ID的Kuku数据
/// </summary>
public KukuData GetKukuTemplate(int kukuId)
{
    if (allKukuDatabase.ContainsKey(kukuId))
    {
        // 创建一个KukuData实例并填充基本信息
        KukuCollectionEntry entry = allKukuDatabase[kukuId];
        KukuData kuku = new KukuData();
        
        kuku.Id = entry.kukuId;
        kuku.Name = entry.kukuName;
        kuku.Rarity = entry.rarity;
        
        // 根据稀有度设置基础属性
        SetKukuAttributesByRarity(kuku, entry.rarity);
        
        return kuku;
    }
    
    return null;
}

/// <summary>
/// 获取神话Kuku模板
/// </summary>
public MythicalKukuData GetMythicalKukuTemplate(int kukuId)
{
    if (allKukuDatabase.ContainsKey(kukuId))
    {
        KukuCollectionEntry entry = allKukuDatabase[kukuId];
        
        if (entry.rarity == KukuData.RarityType.Mythic)
        {
            MythicalKukuData kuku = new MythicalKukuData();
            
            kuku.Id = entry.kukuId;
            kuku.Name = entry.kukuName;
            kuku.Rarity = (MythicalKukuData.MythicalRarity)System.Math.Min((int)MythicalKukuData.MythicalRarity.Primordial, (int)entry.rarity);
            
            // 设置神话Kuku属性
            SetMythicalKukuAttributes(kuku, kuku.Rarity);
            
            return kuku;
        }
    }
    
    return null;
}

/// <summary>
/// 获取玩家拥有的Kuku数据
/// </summary>
public KukuData GetPlayerKuku(int kukuId)
{
    if (GameManager.Instance?.PlayerData.CollectedKukus.ContainsKey(kukuId) == true)
    {
        return GameManager.Instance.PlayerData.CollectedKukus[kukuId];
    }
    
    return null;
}

/// <summary>
/// 获取玩家拥有的神话Kuku数据
/// </summary>
public MythicalKukuData GetPlayerMythicalKuku(int kukuId)
{
    if (GameManager.Instance?.PlayerData.CollectedKukus.ContainsKey(kukuId) == true)
    {
        var kuku = GameManager.Instance.PlayerData.CollectedKukus[kukuId];
        if (kuku is MythicalKukuData)
        {
            return (MythicalKukuData)kuku;
        }
    }
    
    return null;
}
```

#### 3.4.4 Kuku解锁方法
```csharp
/// <summary>
/// 解锁Kuku（由游戏内系统调用）
/// </summary>
public void UnlockKuku(int kukuId)
{
    if (allKukuDatabase.ContainsKey(kukuId))
    {
        KukuCollectionEntry entry = allKukuDatabase[kukuId];
        
        if (!entry.isUnlocked)
        {
            // 标记为已解锁
            entry.isUnlocked = true;
            entry.timesCaught = 1;
            entry.firstUnlockDate = System.DateTime.Now;
            entry.lastCatchDate = System.DateTime.Now;
            
            // 更新数据库
            allKukuDatabase[kukuId] = entry;
            
            Debug.Log($"Kuku {entry.kukuName} 已解锁！");
            
            // 触发解锁事件
            OnKukuUnlocked?.Invoke(GetKukuTemplate(kukuId));
            
            // 更新收集进度
            OnCollectionUpdated?.Invoke();
            
            // 显示解锁通知
            ShowUnlockNotification(entry.kukuName, entry.rarity);
        }
        else
        {
            // Kuku已解锁，更新捕捉次数
            entry.timesCaught++;
            entry.lastCatchDate = System.DateTime.Now;
            allKukuDatabase[kukuId] = entry;
        }
    }
}

/// <summary>
/// 显示解锁通知
/// </summary>
private void ShowUnlockNotification(string kukuName, KukuData.RarityType rarity)
{
    string rarityText = GetRarityDisplayName(rarity);
    string message = $"<color={GetRarityColorCode(rarity)}>解锁新Kuku！</color>\n{kukuName} ({rarityText})";
    
    Debug.Log(message);
    
    // 这里可以添加UI通知显示逻辑
    // 例如：弹出解锁提示框
}
```

#### 3.4.5 随机Kuku获取方法
```csharp
/// <summary>
/// 根据概率获取随机Kuku
/// </summary>
public KukuData GetRandomKukuByProbability()
{
    // 计算总的权重
    float totalWeight = 0f;
    foreach (var entry in allKukuDatabase.Values)
    {
        if (!entry.isUnlocked) // 只考虑未解锁的Kuku
        {
            float weight = GetRarityWeight(entry.rarity);
            totalWeight += weight;
        }
    }
    
    if (totalWeight <= 0)
    {
        // 所有Kuku都已解锁，返回已解锁Kuku
        var unlockedKukus = new List<KukuCollectionEntry>();
        foreach (var entry in allKukuDatabase.Values)
        {
            if (entry.isUnlocked)
            {
                unlockedKukus.Add(entry);
            }
        }
        
        if (unlockedKukus.Count > 0)
        {
            var randomEntry = unlockedKukus[UnityEngine.Random.Range(0, unlockedKukus.Count)];
            return GetKukuTemplate(randomEntry.kukuId);
        }
        
        return null; // 没有任何Kuku
    }
    
    // 随机选择一个Kuku
    float randomValue = UnityEngine.Random.Range(0f, totalWeight);
    float accumulatedWeight = 0f;
    
    foreach (var entry in allKukuDatabase.Values)
    {
        if (!entry.isUnlocked)
        {
            float weight = GetRarityWeight(entry.rarity);
            accumulatedWeight += weight;
            
            if (randomValue <= accumulatedWeight)
            {
                return GetKukuTemplate(entry.kukuId);
            }
        }
    }
    
    // 如果没有找到合适的Kuku，返回最后一个
    var lastEntry = allKukuDatabase.Values.GetEnumerator();
    KukuCollectionEntry last = new KukuCollectionEntry(0, "", KukuData.RarityType.Common);
    while (lastEntry.MoveNext())
    {
        last = lastEntry.Current;
    }
    
    return GetKukuTemplate(last.kukuId);
}

/// <summary>
/// 获取稀有度权重
/// </summary>
private float GetRarityWeight(KukuData.RarityType rarity)
{
    // 根据稀有度分配权重，越稀有的权重越低（更难遇到）
    switch (rarity)
    {
        case KukuData.RarityType.Common: return 50f;    // 50% 权重
        case KukuData.RarityType.Rare: return 25f;      // 25% 权重
        case KukuData.RarityType.Epic: return 15f;      // 15% 权重
        case KukuData.RarityType.Legendary: return 8f;  // 8% 权重
        case KukuData.RarityType.Mythic: return 2f;     // 2% 权重
        default: return 10f;
    }
}
```

#### 3.4.6 页面管理方法
```csharp
/// <summary>
/// 获取当前页面的Kuku列表
/// </summary>
public List<KukuCollectionEntry> GetCurrentPageKukus()
{
    return GetPageKukus(currentPageIndex);
}

/// <summary>
/// 获取指定页面的Kuku列表
/// </summary>
public List<KukuCollectionEntry> GetPageKukus(int pageIndex)
{
    List<KukuCollectionEntry> allKukus = new List<KukuCollectionEntry>(allKukuDatabase.Values);
    
    int startIndex = pageIndex * kukusPerPage;
    if (startIndex >= allKukus.Count)
    {
        return new List<KukuCollectionEntry>(); // 返回空列表
    }
    
    int endIndex = System.Math.Min(startIndex + kukusPerPage, allKukus.Count);
    List<KukuCollectionEntry> pageKukus = new List<KukuCollectionEntry>();
    
    for (int i = startIndex; i < endIndex; i++)
    {
        pageKukus.Add(allKukus[i]);
    }
    
    return pageKukus;
}

/// <summary>
/// 获取总页数
/// </summary>
public int GetTotalPages()
{
    int totalCount = allKukuDatabase.Count;
    return Mathf.CeilToInt((float)totalCount / kukusPerPage);
}

/// <summary>
/// 切换到指定页面
/// </summary>
public void SwitchToPage(int pageIndex)
{
    int totalPages = GetTotalPages();
    if (pageIndex >= 0 && pageIndex < totalPages)
    {
        currentPageIndex = pageIndex;
        Debug.Log($"切换到图鉴第 {pageIndex + 1} 页");
    }
    else
    {
        Debug.LogWarning($"无效的页面索引: {pageIndex}，总页数: {totalPages}");
    }
}

/// <summary>
/// 下一页
/// </summary>
public void NextPage()
{
    int totalPages = GetTotalPages();
    if (currentPageIndex < totalPages - 1)
    {
        currentPageIndex++;
        Debug.Log($"切换到图鉴第 {currentPageIndex + 1} 页");
    }
}

/// <summary>
/// 上一页
/// </summary>
public void PreviousPage()
{
    if (currentPageIndex > 0)
    {
        currentPageIndex--;
        Debug.Log($"切换到图鉴第 {currentPageIndex + 1} 页");
    }
}
```

#### 3.4.7 收集进度方法
```csharp
/// <summary>
/// 获取总体收集进度
/// </summary>
public float GetCollectionProgress()
{
    int totalKukus = allKukuDatabase.Count;
    if (totalKukus <= 0) return 0f;
    
    int unlockedKukus = 0;
    foreach (var entry in allKukuDatabase.Values)
    {
        if (entry.isUnlocked)
        {
            unlockedKukus++;
        }
    }
    
    return (float)unlockedKukus / totalKukus;
}

/// <summary>
/// 获取指定稀有度的收集进度
/// </summary>
public float GetRarityCollectionProgress(KukuData.RarityType rarity)
{
    int totalKukusOfRarity = 0;
    int unlockedKukusOfRarity = 0;
    
    foreach (var entry in allKukuDatabase.Values)
    {
        if (entry.rarity == rarity)
        {
            totalKukusOfRarity++;
            if (entry.isUnlocked)
            {
                unlockedKukusOfRarity++;
            }
        }
    }
    
    if (totalKukusOfRarity <= 0) return 0f;
    
    return (float)unlockedKukusOfRarity / totalKukusOfRarity;
}

/// <summary>
/// 获取各稀有度统计
/// </summary>
public Dictionary<KukuData.RarityType, (int total, int unlocked)> GetRarityStatistics()
{
    Dictionary<KukuData.RarityType, (int total, int unlocked)> stats = new Dictionary<KukuData.RarityType, (int, int)>();
    
    // 初始化所有稀有度的统计
    foreach (KukuData.RarityType rarity in System.Enum.GetValues(typeof(KukuData.RarityType)))
    {
        stats[rarity] = (0, 0);
    }
    
    // 统计每个稀有度的数量
    foreach (var entry in allKukuDatabase.Values)
    {
        var currentStats = stats[entry.rarity];
        currentStats.total++;
        
        if (entry.isUnlocked)
        {
            currentStats.unlocked++;
        }
        
        stats[entry.rarity] = currentStats;
    }
    
    return stats;
}
```

#### 3.4.8 辅助方法
```csharp
/// <summary>
/// 设置Kuku属性（根据稀有度）
/// </summary>
private void SetKukuAttributesByRarity(KukuData kuku, KukuData.RarityType rarity)
{
    // 根据稀有度设置基础属性
    float rarityMultiplier = 1f;
    switch (rarity)
    {
        case KukuData.RarityType.Common: rarityMultiplier = 1.0f; break;
        case KukuData.RarityType.Rare: rarityMultiplier = 1.5f; break;
        case KukuData.RarityType.Epic: rarityMultiplier = 2.0f; break;
        case KukuData.RarityType.Legendary: rarityMultiplier = 3.0f; break;
        case KukuData.RarityType.Mythic: rarityMultiplier = 5.0f; break;
    }
    
    kuku.AttackPower = 10f * rarityMultiplier;
    kuku.DefensePower = 5f * rarityMultiplier;
    kuku.Speed = 1f * rarityMultiplier;
    kuku.Health = 50f * rarityMultiplier;
    kuku.Level = 1;
    kuku.CaptureDifficulty = 1.0f - (int)rarity * 0.1f;
    kuku.CanAbsorbSoul = true;
    kuku.SoulAbsorptionRate = 0.1f + (int)rarity * 0.05f;
    
    // 设置稀有度颜色
    kuku.Tint = kuku.GetRarityColor();
}

/// <summary>
/// 设置神话Kuku属性
/// </summary>
private void SetMythicalKukuAttributes(MythicalKukuData kuku, MythicalKukuData.MythicalRarity rarity)
{
    // 设置基础属性
    SetKukuAttributesByRarity(kuku, (KukuData.RarityType)(int)rarity);
    
    // 设置神话特有属性
    float rarityMultiplier = 1f + (int)rarity * 0.5f;
    
    kuku.DivinePower = 10f * rarityMultiplier;
    kuku.ProtectionPower = 5f * rarityMultiplier;
    kuku.PurificationPower = 3f * rarityMultiplier;
    
    // 设置元素属性
    string[] elements = { "Fire", "Water", "Wood", "Metal", "Earth", "Light", "Dark" };
    kuku.Element = elements[(kuku.Id - 51) % elements.Length]; // 神话KukuID从51开始
    
    // 设置技能
    MythicalKukuData.MythicalSkillType[] skills = System.Enum.GetValues(typeof(MythicalKukuData.MythicalSkillType)) as MythicalKukuData.MythicalSkillType[];
    kuku.SkillType = skills[kuku.Id % skills.Length];
    
    kuku.SkillPower = 10f * rarityMultiplier;
    kuku.SkillRange = 3f;
    
    // 设置进化相关属性
    kuku.EvolutionLevel = 1;
    kuku.EvolutionProgress = 0f;
    kuku.EvolutionStonesRequired = 10 + (int)rarity * 5;
    
    // 设置融合属性
    kuku.CanFuseWithRobots = (int)rarity >= 3; // Sacred及以上可融合
    kuku.FusionCompatibility = 0.3f + (int)rarity * 0.1f;
}

/// <summary>
/// 获取稀有度显示名称
/// </summary>
private string GetRarityDisplayName(KukuData.RarityType rarity)
{
    switch (rarity)
    {
        case KukuData.RarityType.Common: return "普通";
        case KukuData.RarityType.Rare: return "罕见";
        case KukuData.RarityType.Epic: return "史诗";
        case KukuData.RarityType.Legendary: return "传说";
        case KukuData.RarityType.Mythic: return "神话";
        default: return "未知";
    }
}

/// <summary>
/// 获取稀有度颜色代码
/// </summary>
private string GetRarityColorCode(KukuData.RarityType rarity)
{
    switch (rarity)
    {
        case KukuData.RarityType.Common: return "#CCCCCC"; // 灰色
        case KukuData.RarityType.Rare: return "#4169E1";   // 道奇蓝
        case KukuData.RarityType.Epic: return "#DA70D6";   // 兰花紫
        case KukuData.RarityType.Legendary: return "#FFD700"; // 金色
        case KukuData.RarityType.Mythic: return "#00FFFF"; // 青色
        default: return "#FFFFFF"; // 白色
    }
}

/// <summary>
/// 同步游戏内外收集状态
/// </summary>
public void SyncWithPlayerData()
{
    if (GameManager.Instance?.PlayerData == null) return;
    
    // 遍历玩家已收集的Kuku，确保图鉴中也标记为已解锁
    foreach (var collectedKuku in GameManager.Instance.PlayerData.CollectedKukus)
    {
        if (allKukuDatabase.ContainsKey(collectedKuku.Key))
        {
            KukuCollectionEntry entry = allKukuDatabase[collectedKuku.Key];
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.timesCaught++;
                if (entry.firstUnlockDate == System.DateTime.MinValue)
                {
                    entry.firstUnlockDate = System.DateTime.Now;
                }
                entry.lastCatchDate = System.DateTime.Now;
                
                allKukuDatabase[collectedKuku.Key] = entry;
            }
        }
        else
        {
            // 如果图鉴中没有这个Kuku，可能是动态生成的，添加到图鉴
            KukuData kuku = collectedKuku.Value;
            KukuCollectionEntry newEntry = new KukuCollectionEntry(kuku.Id, kuku.Name, kuku.Rarity);
            newEntry.isUnlocked = true;
            newEntry.timesCaught = 1;
            newEntry.firstUnlockDate = System.DateTime.Now;
            newEntry.lastCatchDate = System.DateTime.Now;
            
            allKukuDatabase[kuku.Id] = newEntry;
        }
    }
}
```

### 3.5 事件系统
```csharp
// 事件声明
public System.Action<KukuData> OnKukuUnlocked;                        // Kuku解锁事件
public System.Action<int, float> OnPageCompleted;                   // 页面完成事件
public System.Action OnCollectionUpdated;                           // 收集更新事件
public System.Action<int, int> OnCollectionProgressChanged;         // 收集进度变化事件

// 事件触发示例
private void TriggerCollectionEvents()
{
    // 触发收集进度变化事件
    float overallProgress = GetCollectionProgress();
    int totalKukus = allKukuDatabase.Count;
    int unlockedKukus = (int)(overallProgress * totalKukus);
    
    OnCollectionProgressChanged?.Invoke(unlockedKukus, totalKukus);
    
    // 检查当前页面是否已完成
    var currentPageData = GetPageData(currentPageIndex);
    if (currentPageData.completionRate >= 1.0f)
    {
        OnPageCompleted?.Invoke(currentPageIndex, currentPageData.completionRate);
    }
}
```

## 4. 系统交互

### 4.1 数据流
```
CaptureSystem → KukuCollectionManager → KukuCollectionEntry
PlayerData → KukuCollectionManager → Sync Operation
UI → KukuCollectionManager → Display Data
GameManager → KukuCollectionManager → State Updates
```

### 4.2 事件流
- 捕捉成功 → KukuCollectionManager 解锁Kuku → 触发解锁事件 → UI更新显示
- 玩家查看图鉴 → KukuCollectionManager 提供数据 → UI展示图鉴
- 收集进度变化 → KukuCollectionManager 计算进度 → 触发进度事件

### 4.3 依赖关系
- KukuCollectionManager 依赖 GameManager 获取玩家数据
- CaptureSystem 依赖 KukuCollectionManager 记录解锁状态
- UI系统依赖 KukuCollectionManager 获取图鉴数据
- PlayerData 与 KukuCollectionManager 同步收集状态