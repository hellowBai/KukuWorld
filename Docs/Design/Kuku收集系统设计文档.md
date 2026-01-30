# Kuku收集塔防游戏《Kuku Collector》Kuku收集系统设计文档

## 1. 系统概述

Kuku收集系统是《Kuku Collector》游戏的核心玩法之一，负责管理游戏中所有Kuku的收集、存储、管理和展示。系统包含Kuku数据结构、收集逻辑、图鉴系统、队伍管理等功能，为玩家提供完整的Kuku收集体验。

## 2. 系统架构

### 2.1 KukuCollectionSystem (Kuku收集系统)
- **职责**: 管理所有Kuku相关数据，处理收集逻辑，管理图鉴系统
- **类型**: 静态系统类
- **依赖**: KukuData, PlayerData, GameManager

## 3. KukuCollectionSystem (Kuku收集系统)

### 3.1 类定义
```csharp
public static class KukuCollectionSystem
```

### 3.2 系统职责
- 管理Kuku数据的创建、存储和检索
- 处理Kuku收集逻辑
- 管理Kuku图鉴系统
- 提供Kuku队伍管理功能
- 处理Kuku进化、融合等高级功能

### 3.3 数据结构

#### 3.3.1 Kuku数据结构
```csharp
public class KukuData
{
    public int Id { get; set; }                                    // Kuku唯一ID
    public string Name { get; set; }                              // Kuku名称
    public string Description { get; set; }                       // 描述
    public string Element { get; set; }                           // 元素属性
    public string SkillType { get; set; }                         // 技能类型
    public string SkillDescription { get; set; }                  // 技能描述
    public float SkillRange { get; set; }                         // 技能范围
    public float SkillPower { get; set; }                         // 技能威力
    public float AttackPower { get; set; }                        // 攻击力
    public float DefensePower { get; set; }                       // 防御力
    public float Speed { get; set; }                              // 速度
    public float Health { get; set; }                             // 生命值
    public RarityType Rarity { get; set; }                        // 稀有度
    public int Level { get; set; }                                // 等级
    public float Experience { get; set; }                         // 经验值
    public int EvolutionLevel { get; set; }                       // 进化等级
    public float EvolutionProgress { get; set; }                  // 进化进度
    public int EvolutionStonesRequired { get; set; }              // 进化所需灵魂
    public bool CanAbsorbSoul { get; set; }                      // 是否可吸收灵魂
    public float SoulAbsorptionRate { get; set; }                // 灵魂吸收率
    public float FusionCompatibility { get; set; }               // 融合兼容性
    public bool CanFuseWithRobots { get; set; }                  // 是否可与机器人融合
    public string SpriteName { get; set; }                       // 精灵图名称
    public string MythologicalBackground { get; set; }           // 神话背景
    public float DivinePower { get; set; }                       // 神力
    public float ProtectionPower { get; set; }                   // 防护力
    public float PurificationPower { get; set; }                 // 净化力
    public int MaxEquipmentSlots { get; set; }                   // 最大装备槽位
    public List<WeaponData> EquippedItems { get; set; }          // 已装备物品
    public bool IsFavorite { get; set; }                         // 是否收藏
    public DateTime CaptureDate { get; set; }                    // 捕获日期
    public string CaptureLocation { get; set; }                  // 捕获地点
    public float CaptureDifficulty { get; set; }                 // 捕获难度

    public KukuData()
    {
        EquippedItems = new List<WeaponData>();
        CaptureDate = DateTime.Now;
        CaptureLocation = "Unknown";
        CaptureDifficulty = 1.0f;
        EvolutionStonesRequired = 10; // 默认需要10点灵魂进化
    }
    
    // 获取稀有度描述
    public string GetRarityDescription()
    {
        switch (Rarity)
        {
            case RarityType.Common: return "普通";
            case RarityType.Uncommon: return "罕见";
            case RarityType.Rare: return "稀有";
            case RarityType.Epic: return "史诗";
            case RarityType.Legendary: return "传说";
            case RarityType.Mythical: return "神话";
            default: return "未知";
        }
    }
    
    // 获取进化等级描述
    public string GetEvolutionLevelDescription()
    {
        switch (EvolutionLevel)
        {
            case 1: return "一阶";
            case 2: return "二阶";
            case 3: return "三阶";
            case 4: return "四阶";
            case 5: return "五阶(终阶)";
            default: return "未进化";
        }
    }
}

public enum RarityType
{
    Common = 0,      // 普通
    Uncommon = 1,    // 罕见
    Rare = 2,        // 稀有
    Epic = 3,        // 史诗
    Legendary = 4,   // 传说
    Mythical = 5     // 神话
}
```

#### 3.3.2 收集结果结构
```csharp
public struct CollectionResult
{
    public bool success;
    public KukuData kuku;
    public string message;
    public bool isNewDiscovery; // 是否为新发现

    public CollectionResult(bool isSuccess, KukuData k, string msg, bool isNew = false)
    {
        success = isSuccess;
        kuku = k;
        message = msg;
        isNewDiscovery = isNew;
    }
}
```

#### 3.3.3 图鉴条目结构
```csharp
public struct KukuPokedexEntry
{
    public int Id;
    public string Name;
    public RarityType Rarity;
    public string Element;
    public bool IsDiscovered; // 是否已被发现
    public bool IsCaptured;   // 是否已被捕获
    public int TimesEncountered; // 遭遇次数
    public int TimesCaptured;    // 捕获次数
    public DateTime FirstDiscoveryDate; // 首次发现日期

    public KukuPokedexEntry(int id, string name, RarityType rarity, string element)
    {
        Id = id;
        Name = name;
        Rarity = rarity;
        Element = element;
        IsDiscovered = false;
        IsCaptured = false;
        TimesEncountered = 0;
        TimesCaptured = 0;
        FirstDiscoveryDate = DateTime.MinValue;
    }
}
```

### 3.4 核心方法

#### 3.4.1 Kuku创建方法
```csharp
/// <summary>
/// 创建一个新的Kuku
/// </summary>
public static KukuData CreateNewKuku(RarityType rarity, string element = "None", string skillType = "None")
{
    KukuData kuku = new KukuData();
    
    // 生成唯一ID
    kuku.Id = GenerateUniqueId();
    
    // 设置基础属性
    kuku.Name = GenerateKukuName(rarity, element);
    kuku.Description = GenerateKukuDescription(kuku.Name, element);
    kuku.Element = element;
    kuku.SkillType = skillType;
    kuku.SkillDescription = GenerateSkillDescription(skillType);
    kuku.SkillRange = UnityEngine.Random.Range(2f, 8f);
    kuku.SkillPower = UnityEngine.Random.Range(10f, 50f);
    
    // 根据稀有度设置基础属性
    SetBaseAttributesByRarity(kuku, rarity);
    
    // 设置稀有度
    kuku.Rarity = rarity;
    
    // 设置等级和经验
    kuku.Level = 1;
    kuku.Experience = 0f;
    
    // 设置进化相关属性
    kuku.EvolutionLevel = 1;
    kuku.EvolutionProgress = 0f;
    kuku.EvolutionStonesRequired = 10; // 初始需要10点灵魂进化
    kuku.CanAbsorbSoul = true;
    kuku.SoulAbsorptionRate = 0.5f + (int)rarity * 0.1f;
    kuku.FusionCompatibility = 0.5f + (int)rarity * 0.1f;
    kuku.CanFuseWithRobots = false; // 初始不能融合，需进化到5级
    
    // 设置外观
    kuku.SpriteName = $"Kuku_{kuku.Id}";
    
    // 设置神话背景
    kuku.MythologicalBackground = GenerateMythologicalBackground(element);
    
    // 设置特殊属性
    kuku.DivinePower = 5f + (int)rarity * 3f;
    kuku.ProtectionPower = 3f + (int)rarity * 2f;
    kuku.PurificationPower = 2f + (int)rarity * 1.5f;
    
    // 设置装备槽位（仅在可融合后才有装备槽）
    kuku.MaxEquipmentSlots = 0;
    kuku.EquippedItems = new List<WeaponData>();
    
    // 设置收藏状态
    kuku.IsFavorite = false;
    
    // 设置捕获信息
    kuku.CaptureDate = DateTime.Now;
    kuku.CaptureLocation = "Mystery Zone";
    kuku.CaptureDifficulty = 1.0f - (int)rarity * 0.1f; // 稀有度越高越难捕获
    
    return kuku;
}

/// <summary>
/// 生成唯一的Kuku ID
/// </summary>
private static int GenerateUniqueId()
{
    // 生成一个唯一的ID，可以基于时间戳或其他唯一标识
    return (int)(DateTime.UtcNow.Ticks % 1000000);
}
```

#### 3.4.2 Kuku收集方法
```csharp
/// <summary>
/// 收集Kuku（添加到玩家收藏）
/// </summary>
public static CollectionResult CollectKuku(KukuData kuku)
{
    if (kuku == null)
    {
        return new CollectionResult(false, null, "无效的Kuku数据！");
    }

    // 检查玩家是否已经拥有这只Kuku
    bool alreadyOwned = false;
    if (GameManager.Instance?.PlayerData.CollectedKukus.ContainsKey(kuku.Id) == true)
    {
        alreadyOwned = true;
    }

    // 将Kuku添加到玩家收藏
    if (GameManager.Instance?.PlayerData != null)
    {
        GameManager.Instance.PlayerData.CollectedKukus[kuku.Id] = kuku;
        
        // 更新图鉴
        UpdatePokedex(kuku, !alreadyOwned);
        
        // 如果是新发现，给予额外奖励
        if (!alreadyOwned)
        {
            GameManager.Instance.PlayerData.AddCoins(50);
            GameManager.Instance.PlayerData.AddSouls(5f);
            
            string message = $"恭喜！首次捕获到 {kuku.Name}！获得了50金币和5点灵魂奖励！";
            return new CollectionResult(true, kuku, message, true);
        }
        else
        {
            string message = $"再次遇到 {kuku.Name}！已添加到收藏。";
            return new CollectionResult(true, kuku, message, false);
        }
    }
    else
    {
        return new CollectionResult(false, null, "游戏管理器未初始化！");
    }
}
```

#### 3.4.3 Kuku查询方法
```csharp
/// <summary>
/// 获取玩家拥有的Kuku列表
/// </summary>
public static List<KukuData> GetPlayerKukus()
{
    if (GameManager.Instance?.PlayerData == null)
    {
        return new List<KukuData>();
    }

    return new List<KukuData>(GameManager.Instance.PlayerData.CollectedKukus.Values);
}

/// <summary>
/// 根据ID获取Kuku
/// </summary>
public static KukuData GetKukuById(int id)
{
    if (GameManager.Instance?.PlayerData.CollectedKukus.ContainsKey(id) == true)
    {
        return GameManager.Instance.PlayerData.CollectedKukus[id];
    }

    return null;
}

/// <summary>
/// 根据稀有度获取Kuku列表
/// </summary>
public static List<KukuData> GetKukusByRarity(RarityType rarity)
{
    List<KukuData> result = new List<KukuData>();

    foreach (var kvp in GameManager.Instance?.PlayerData.CollectedKukus ?? new Dictionary<int, KukuData>())
    {
        if (kvp.Value.Rarity == rarity)
        {
            result.Add(kvp.Value);
        }
    }

    return result;
}

/// <summary>
/// 获取指定元素的Kuku列表
/// </summary>
public static List<KukuData> GetKukusByElement(string element)
{
    List<KukuData> result = new List<KukuData>();

    foreach (var kvp in GameManager.Instance?.PlayerData.CollectedKukus ?? new Dictionary<int, KukuData>())
    {
        if (kvp.Value.Element.Equals(element, StringComparison.OrdinalIgnoreCase))
        {
            result.Add(kvp.Value);
        }
    }

    return result;
}
```

#### 3.4.4 图鉴管理方法
```csharp
/// <summary>
/// 更新图鉴信息
/// </summary>
private static void UpdatePokedex(KukuData kuku, bool isNewDiscovery = false)
{
    if (GameManager.Instance?.PlayerData.KukuPokedex.ContainsKey(kuku.Id) == false)
    {
        // 如果图鉴中没有这个Kuku，添加新的条目
        KukuPokedexEntry entry = new KukuPokedexEntry(
            kuku.Id, 
            kuku.Name, 
            kuku.Rarity, 
            kuku.Element
        );
        
        GameManager.Instance.PlayerData.KukuPokedex[kuku.Id] = entry;
    }

    // 更新图鉴条目信息
    KukuPokedexEntry currentEntry = GameManager.Instance.PlayerData.KukuPokedex[kuku.Id];
    currentEntry.TimesEncountered++;
    
    if (isNewDiscovery)
    {
        currentEntry.IsDiscovered = true;
        currentEntry.FirstDiscoveryDate = DateTime.Now;
    }
    
    currentEntry.TimesCaptured++;
    currentEntry.IsCaptured = true;
    
    GameManager.Instance.PlayerData.KukuPokedex[kuku.Id] = currentEntry;
}

/// <summary>
/// 获取图鉴统计信息
/// </summary>
public static (int discovered, int total, float completionRate) GetPokedexStats()
{
    int totalEntries = 0;
    int discoveredEntries = 0;
    
    foreach (var entry in GameManager.Instance?.PlayerData.KukuPokedex.Values ?? new List<KukuPokedexEntry>())
    {
        totalEntries++;
        if (entry.IsDiscovered)
        {
            discoveredEntries++;
        }
    }
    
    float completionRate = totalEntries > 0 ? (float)discoveredEntries / totalEntries * 100f : 0f;
    
    return (discoveredEntries, totalEntries, completionRate);
}

/// <summary>
/// 获取图鉴条目
/// </summary>
public static KukuPokedexEntry GetPokedexEntry(int id)
{
    if (GameManager.Instance?.PlayerData.KukuPokedex.ContainsKey(id) == true)
    {
        return GameManager.Instance.PlayerData.KukuPokedex[id];
    }

    return new KukuPokedexEntry(); // 返回默认值
}
```

#### 3.4.5 队伍管理方法
```csharp
/// <summary>
/// 设置活跃Kuku队伍
/// </summary>
public static bool SetActiveKukuTeam(List<int> kukuIds)
{
    if (GameManager.Instance?.PlayerData == null)
    {
        return false;
    }

    // 验证所有ID都存在于玩家收藏中
    foreach (int id in kukuIds)
    {
        if (!GameManager.Instance.PlayerData.CollectedKukus.ContainsKey(id))
        {
            Debug.LogWarning($"Kuku ID {id} 不在玩家收藏中！");
            return false;
        }
    }

    // 设置活跃队伍
    GameManager.Instance.PlayerData.ActiveKukuTeam = new List<int>(kukuIds);
    
    Debug.Log($"成功设置活跃Kuku队伍，包含 {kukuIds.Count} 只Kuku");
    return true;
}

/// <summary>
/// 添加Kuku到活跃队伍
/// </summary>
public static bool AddKukuToActiveTeam(int kukuId)
{
    if (GameManager.Instance?.PlayerData == null)
    {
        return false;
    }

    // 检查Kuku是否存在
    if (!GameManager.Instance.PlayerData.CollectedKukus.ContainsKey(kukuId))
    {
        Debug.LogWarning($"Kuku ID {kukuId} 不在玩家收藏中！");
        return false;
    }

    // 检查是否已在队伍中
    if (GameManager.Instance.PlayerData.ActiveKukuTeam.Contains(kukuId))
    {
        Debug.LogWarning($"Kuku ID {kukuId} 已在活跃队伍中！");
        return false;
    }

    // 检查队伍是否已满（假设最多5只）
    if (GameManager.Instance.PlayerData.ActiveKukuTeam.Count >= 5)
    {
        Debug.LogWarning("活跃队伍已满（最多5只Kuku）！");
        return false;
    }

    // 添加到队伍
    GameManager.Instance.PlayerData.ActiveKukuTeam.Add(kukuId);
    
    Debug.Log($"Kuku ID {kukuId} 已添加到活跃队伍");
    return true;
}

/// <summary>
/// 从活跃队伍移除Kuku
/// </summary>
public static bool RemoveKukuFromActiveTeam(int kukuId)
{
    if (GameManager.Instance?.PlayerData == null)
    {
        return false;
    }

    bool removed = GameManager.Instance.PlayerData.ActiveKukuTeam.Remove(kukuId);
    
    if (removed)
    {
        Debug.Log($"Kuku ID {kukuId} 已从活跃队伍移除");
    }
    else
    {
        Debug.LogWarning($"Kuku ID {kukuId} 不在活跃队伍中！");
    }
    
    return removed;
}
```

#### 3.4.6 辅助方法
```csharp
/// <summary>
/// 根据稀有度设置基础属性
/// </summary>
private static void SetBaseAttributesByRarity(KukuData kuku, RarityType rarity)
{
    // 根据稀有度设置基础属性
    float multiplier = 1.0f + (int)rarity * 0.3f;
    
    kuku.AttackPower = 20f * multiplier;
    kuku.DefensePower = 15f * multiplier;
    kuku.Speed = 2f + (int)rarity * 0.2f;
    kuku.Health = 80f * multiplier;
}

/// <summary>
/// 生成Kuku名称
/// </summary>
private static string GenerateKukuName(RarityType rarity, string element)
{
    string[] commonPrefixes = { "小", "精", "巧", "灵", "妙" };
    string[] rarePrefixes = { "珍", "奇", "异", "宝", "神" };
    string[] epicPrefixes = { "圣", "仙", "魔", "龙", "凤" };
    string[] legendaryPrefixes = { "皇", "帝", "王", "尊", "霸" };
    string[] mythicalPrefixes = { "创世", "元初", "永恒", "无限", "混沌" };

    string[] suffixes = { "精灵", "守护", "使者", "战神", "智者", "勇士", "贤者", "圣者" };
    
    string[] elements = { "火", "水", "风", "土", "光", "暗", "雷", "冰", "木", "金" };
    
    string prefix = "";
    switch (rarity)
    {
        case RarityType.Common:
        case RarityType.Uncommon:
            prefix = commonPrefixes[UnityEngine.Random.Range(0, commonPrefixes.Length)];
            break;
        case RarityType.Rare:
            prefix = rarePrefixes[UnityEngine.Random.Range(0, rarePrefixes.Length)];
            break;
        case RarityType.Epic:
            prefix = epicPrefixes[UnityEngine.Random.Range(0, epicPrefixes.Length)];
            break;
        case RarityType.Legendary:
            prefix = legendaryPrefixes[UnityEngine.Random.Range(0, legendaryPrefixes.Length)];
            break;
        case RarityType.Mythical:
            prefix = mythicalPrefixes[UnityEngine.Random.Range(0, mythicalPrefixes.Length)];
            break;
    }
    
    string elementStr = element;
    if (string.IsNullOrEmpty(element) || element == "None")
    {
        elementStr = elements[UnityEngine.Random.Range(0, elements.Length)];
    }
    
    string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
    
    return $"{prefix}{elementStr}{suffix}";
}

/// <summary>
/// 生成Kuku描述
/// </summary>
private static string GenerateKukuDescription(string name, string element)
{
    string[] descriptions = {
        $"{name}是一只拥有{element}元素力量的神奇生物。",
        "据说{0}来自远古时代，拥有神秘的力量。",
        "{0}性格温和，但面对危险时会展现出惊人的勇气。",
        "{0}的{1}元素力量在战斗中非常强大。",
        "很少有人能真正了解{0}的全部能力。",
        "{0}被认为是{1}元素的完美化身。",
        "当{0}使用技能时，周围充满了{1}元素的气息。",
        "{0}的出现总是伴随着奇迹的发生。"
    };
    
    string template = descriptions[UnityEngine.Random.Range(0, descriptions.Length)];
    return string.Format(template, name, element);
}

/// <summary>
/// 生成技能描述
/// </summary>
private static string GenerateSkillDescription(string skillType)
{
    string[] fireSkills = { "释放炽热火焰攻击敌人", "召唤烈焰风暴", "喷射高温熔岩" };
    string[] waterSkills = { "释放水流冲击", "召唤海啸", "治愈友方单位" };
    string[] earthSkills = { "操控岩石攻击", "召唤大地之力", "制造防护屏障" };
    string[] windSkills = { "释放飓风", "快速移动", "削弱敌人速度" };
    string[] lightSkills = { "释放神圣光芒", "驱散黑暗", "增强友方力量" };
    string[] darkSkills = { "释放暗影攻击", "削弱敌人", "隐身能力" };
    string[] lightningSkills = { "释放雷电攻击", "麻痹敌人", "快速充能" };
    string[] iceSkills = { "冻结敌人", "冰霜冲击", "降低敌人温度" };
    string[] woodSkills = { "召唤藤蔓束缚", "自然治愈", "生长植物" };
    string[] metalSkills = { "金属硬化", "锐利攻击", "防御强化" };
    
    switch (skillType.ToLower())
    {
        case "fire":
        case "fireball":
        case "flame":
            return fireSkills[UnityEngine.Random.Range(0, fireSkills.Length)];
        case "water":
        case "heal":
        case "stream":
            return waterSkills[UnityEngine.Random.Range(0, waterSkills.Length)];
        case "earth":
        case "rock":
        case "stone":
            return earthSkills[UnityEngine.Random.Range(0, earthSkills.Length)];
        case "wind":
        case "gust":
        case "breeze":
            return windSkills[UnityEngine.Random.Range(0, windSkills.Length)];
        case "light":
        case "holy":
        case "radiance":
            return lightSkills[UnityEngine.Random.Range(0, lightSkills.Length)];
        case "dark":
        case "shadow":
        case "void":
            return darkSkills[UnityEngine.Random.Range(0, darkSkills.Length)];
        case "lightning":
        case "thunder":
        case "bolt":
            return lightningSkills[UnityEngine.Random.Range(0, lightningSkills.Length)];
        case "ice":
        case "freeze":
        case "frost":
            return iceSkills[UnityEngine.Random.Range(0, iceSkills.Length)];
        case "wood":
        case "nature":
        case "vine":
            return woodSkills[UnityEngine.Random.Range(0, woodSkills.Length)];
        case "metal":
        case "steel":
        case "armor":
            return metalSkills[UnityEngine.Random.Range(0, metalSkills.Length)];
        default:
            string[] generalSkills = { 
                "使用神秘力量攻击", 
                "施展独特技能", 
                "展现超凡能力", 
                "释放元素力量", 
                "使用古老魔法" 
            };
            return generalSkills[UnityEngine.Random.Range(0, generalSkills.Length)];
    }
}

/// <summary>
/// 生成神话背景
/// </summary>
private static string GenerateMythologicalBackground(string element)
{
    string[] backgrounds = {
        "来自古代神话传说的神秘生物",
        "元素力量的化身，拥有悠久的历史",
        "在远古时代就存在的传奇存在",
        "被神话故事传颂的神圣生物",
        "拥有古老智慧的元素守护者",
        "传说中由天地精华孕育而生",
        "承载着古老文明记忆的存在"
    };
    
    return backgrounds[UnityEngine.Random.Range(0, backgrounds.Length)];
}

/// <summary>
/// 获取稀有度权重列表
/// </summary>
public static List<(RarityType rarity, float weight)> GetRarityWeights()
{
    return new List<(RarityType, float)>
    {
        (RarityType.Common, 0.5f),     // 50% 概率
        (RarityType.Uncommon, 0.25f),  // 25% 概率
        (RarityType.Rare, 0.15f),      // 15% 概率
        (RarityType.Epic, 0.08f),      // 8% 概率
        (RarityType.Legendary, 0.02f), // 2% 概率
        (RarityType.Mythical, 0.005f)  // 0.5% 概率
    };
}

/// <summary>
/// 根据权重随机选择稀有度
/// </summary>
public static RarityType RandomRarityByWeight()
{
    List<(RarityType, float)> weights = GetRarityWeights();
    float totalWeight = weights.Sum(w => w.weight);
    float randomValue = UnityEngine.Random.Range(0f, totalWeight);
    
    float currentWeight = 0f;
    foreach (var (rarity, weight) in weights)
    {
        currentWeight += weight;
        if (randomValue <= currentWeight)
        {
            return rarity;
        }
    }
    
    return RarityType.Common; // 默认返回普通稀有度
}
```

## 4. 系统交互

### 4.1 数据流
```
CaptureSystem → KukuCollectionSystem → KukuData storage
UI → KukuCollectionSystem → KukuData retrieval
EvolutionSystem → KukuCollectionSystem → KukuData updates
FusionSystem → KukuCollectionSystem → KukuData updates
```

### 4.2 事件流
- 捕获Kuku → KukuCollectionSystem 添加到收藏 → 更新UI显示
- 进化Kuku → KukuCollectionSystem 更新数据 → UI显示新属性
- 融合Kuku → KukuCollectionSystem 更新数据 → 装备系统启用

### 4.3 依赖关系
- KukuCollectionSystem 依赖 PlayerData 存储Kuku数据
- UI系统依赖 KukuCollectionSystem 获取Kuku信息
- EvolutionSystem 依赖 KukuCollectionSystem 获取Kuku数据
- FusionSystem 依赖 KukuCollectionSystem 获取Kuku数据