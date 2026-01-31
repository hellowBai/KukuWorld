using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class KukuCollectionSystem : MonoBehaviour
{
    [Header("收集系统设置")]
    public int maxCollectionSlots = 50;
    public int maxFavorites = 10;
    
    private GameManager gameManager;
    private PlayerData playerData;
    
    // 收藏夹
    private List<int> favoriteKukuIds = new List<int>();
    
    // 事件
    public System.Action<MythicalKukuData> OnKukuAddedToCollection;
    public System.Action<int> OnKukuRemovedFromCollection;
    public System.Action<int> OnKukuAddedToFavorites;
    public System.Action<int> OnKukuRemovedFromFavorites;
    public System.Action<string> OnCollectionError;
    
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
        
        Debug.Log("KuKu收集系统初始化完成");
    }
    
    /// <summary>
    /// 添加KuKu到收藏
    /// </summary>
    public bool AddKukuToCollection(MythicalKukuData kuku)
    {
        if (playerData == null)
        {
            Debug.LogError("玩家数据未加载");
            return false;
        }
        
        if (playerData.CollectedKukus.Count >= maxCollectionSlots)
        {
            Debug.LogError("收藏槽位已满");
            return false;
        }
        
        // 检查是否已存在
        if (playerData.HasKuku(kuku.Id))
        {
            Debug.LogWarning($"KuKu已存在于收藏中: {kuku.Name}");
            return false;
        }
        
        playerData.AddKuku(kuku);
        
        Debug.Log($"添加KuKu到收藏: {kuku.Name}");
        
        // 触发事件
        OnKukuAddedToCollection?.Invoke(kuku);
        
        return true;
    }
    
    /// <summary>
    /// 从收藏中移除KuKu
    /// </summary>
    public bool RemoveKukuFromCollection(int kukuId)
    {
        if (playerData == null)
        {
            Debug.LogError("玩家数据未加载");
            return false;
        }
        
        if (!playerData.HasKuku(kukuId))
        {
            Debug.LogError($"收藏中不存在该KuKu: {kukuId}");
            return false;
        }
        
        // 如果在收藏夹中，先移除
        if (favoriteKukuIds.Contains(kukuId))
        {
            RemoveFromFavorites(kukuId);
        }
        
        var kuku = playerData.GetKukuById(kukuId);
        playerData.RemoveKuku(kukuId);
        
        Debug.Log($"从收藏中移除KuKu: {kuku?.Name ?? "Unknown"} (ID: {kukuId})");
        
        // 触发事件
        OnKukuRemovedFromCollection?.Invoke(kukuId);
        
        return true;
    }
    
    /// <summary>
    /// 添加到收藏夹
    /// </summary>
    public bool AddToFavorites(int kukuId)
    {
        if (favoriteKukuIds.Contains(kukuId))
        {
            Debug.LogWarning($"KuKu已在收藏夹中: {kukuId}");
            return false;
        }
        
        if (favoriteKukuIds.Count >= maxFavorites)
        {
            Debug.LogError("收藏夹已满");
            return false;
        }
        
        if (!playerData.HasKuku(kukuId))
        {
            Debug.LogError($"收藏中不存在该KuKu: {kukuId}");
            return false;
        }
        
        favoriteKukuIds.Add(kukuId);
        
        Debug.Log($"添加KuKu到收藏夹: {kukuId}");
        
        // 触发事件
        OnKukuAddedToFavorites?.Invoke(kukuId);
        
        return true;
    }
    
    /// <summary>
    /// 从收藏夹移除
    /// </summary>
    public bool RemoveFromFavorites(int kukuId)
    {
        if (!favoriteKukuIds.Contains(kukuId))
        {
            Debug.LogWarning($"KuKu不在收藏夹中: {kukuId}");
            return false;
        }
        
        favoriteKukuIds.Remove(kukuId);
        
        Debug.Log($"从收藏夹移除KuKu: {kukuId}");
        
        // 触发事件
        OnKukuRemovedFromFavorites?.Invoke(kukuId);
        
        return true;
    }
    
    /// <summary>
    /// 获取收藏的KuKu
    /// </summary>
    public List<MythicalKukuData> GetCollection()
    {
        if (playerData == null) return new List<MythicalKukuData>();
        return new List<MythicalKukuData>(playerData.CollectedKukus);
    }
    
    /// <summary>
    /// 获取收藏夹中的KuKu
    /// </summary>
    public List<MythicalKukuData> GetFavorites()
    {
        if (playerData == null) return new List<MythicalKukuData>();
        
        List<MythicalKukuData> favorites = new List<MythicalKukuData>();
        
        foreach (int kukuId in favoriteKukuIds)
        {
            var kuku = playerData.GetKukuById(kukuId);
            if (kuku != null)
            {
                favorites.Add(kuku);
            }
        }
        
        return favorites;
    }
    
    /// <summary>
    /// 按稀有度筛选KuKu
    /// </summary>
    public List<MythicalKukuData> GetKukusByRarity(MythicalKukuData.MythicalRarity rarity)
    {
        if (playerData == null) return new List<MythicalKukuData>();
        return playerData.GetKukusByRarity(rarity);
    }
    
    /// <summary>
    /// 按类型筛选KuKu
    /// </summary>
    public List<MythicalKukuData> GetKukusByType(KukuType kukuType)
    {
        if (playerData == null) return new List<MythicalKukuData>();
        
        List<MythicalKukuData> filtered = new List<MythicalKukuData>();
        
        foreach (var kuku in playerData.CollectedKukus)
        {
            if (GetKukuType(kuku) == kukuType)
            {
                filtered.Add(kuku);
            }
        }
        
        return filtered;
    }
    
    /// <summary>
    /// 获取KuKu类型
    /// </summary>
    public KukuType GetKukuType(MythicalKukuData kuku)
    {
        // 根据属性判断KuKu类型
        if (kuku.DivinePower > kuku.AttackPower && kuku.DivinePower > kuku.DefensePower)
        {
            return KukuType.Magic;
        }
        else if (kuku.AttackPower > kuku.DefensePower)
        {
            return KukuType.Attack;
        }
        else if (kuku.DefensePower > kuku.AttackPower)
        {
            return KukuType.Defense;
        }
        else if (kuku.Speed > 2f)
        {
            return KukuType.Speed;
        }
        else
        {
            return KukuType.Balanced;
        }
    }
    
    /// <summary>
    /// 搜索KuKu
    /// </summary>
    public List<MythicalKukuData> SearchKukus(string searchTerm)
    {
        if (playerData == null) return new List<MythicalKukuData>();
        
        List<MythicalKukuData> results = new List<MythicalKukuData>();
        
        foreach (var kuku in playerData.CollectedKukus)
        {
            if (kuku.Name.ToLower().Contains(searchTerm.ToLower()) ||
                kuku.Description.ToLower().Contains(searchTerm.ToLower()))
            {
                results.Add(kuku);
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// 排序KuKu
    /// </summary>
    public List<MythicalKukuData> SortKukus(KukuSortCriteria criteria, bool ascending = true)
    {
        var collection = GetCollection();
        
        switch (criteria)
        {
            case KukuSortCriteria.Name:
                collection.Sort((x, y) => ascending ? 
                    string.Compare(x.Name, y.Name) : 
                    string.Compare(y.Name, x.Name));
                break;
                
            case KukuSortCriteria.Rarity:
                collection.Sort((x, y) => ascending ? 
                    ((int)x.Rarity).CompareTo((int)y.Rarity) : 
                    ((int)y.Rarity).CompareTo((int)x.Rarity));
                break;
                
            case KukuSortCriteria.Level:
                collection.Sort((x, y) => ascending ? 
                    x.Level.CompareTo(y.Level) : 
                    y.Level.CompareTo(x.Level));
                break;
                
            case KukuSortCriteria.Power:
                collection.Sort((x, y) => ascending ? 
                    x.GetTotalPower().CompareTo(y.GetTotalPower()) : 
                    y.GetTotalPower().CompareTo(x.GetTotalPower()));
                break;
                
            case KukuSortCriteria.CombatEffectiveness:
                collection.Sort((x, y) => ascending ? 
                    x.GetCombatEffectiveness().CompareTo(y.GetCombatEffectiveness()) : 
                    y.GetCombatEffectiveness().CompareTo(x.GetCombatEffectiveness()));
                break;
                
            case KukuSortCriteria.AcquisitionDate:
                // 如果有获取日期信息，可以按日期排序
                // 这里简化处理
                break;
        }
        
        return collection;
    }
    
    /// <summary>
    /// 获取收藏统计信息
    /// </summary>
    public CollectionStats GetCollectionStats()
    {
        if (playerData == null) return new CollectionStats();
        
        var stats = new CollectionStats();
        stats.TotalCount = playerData.GetTotalKukuCount();
        stats.MaxSlots = maxCollectionSlots;
        
        stats.CelestialCount = playerData.GetCommonKukuCount();
        stats.ImmortalCount = playerData.GetRareKukuCount();
        stats.DivineBeastCount = playerData.GetEpicKukuCount();
        stats.SacredCount = playerData.GetLegendaryKukuCount();
        stats.PrimordialCount = playerData.GetMythicKukuCount();
        
        stats.FavoriteCount = favoriteKukuIds.Count;
        stats.MaxFavorites = maxFavorites;
        
        stats.TotalPower = playerData.GetTotalKukuPower();
        stats.AverageLevel = playerData.GetAverageKukuLevel();
        
        return stats;
    }
    
    /// <summary>
    /// 获取最强的KuKu
    /// </summary>
    public MythicalKukuData GetStrongestKuku()
    {
        if (playerData == null || playerData.CollectedKukus.Count == 0) return null;
        
        MythicalKukuData strongest = playerData.CollectedKukus[0];
        foreach (var kuku in playerData.CollectedKukus)
        {
            if (kuku.GetTotalPower() > strongest.GetTotalPower())
            {
                strongest = kuku;
            }
        }
        
        return strongest;
    }
    
    /// <summary>
    /// 获取最高等级的KuKu
    /// </summary>
    public MythicalKukuData GetHighestLevelKuku()
    {
        if (playerData == null || playerData.CollectedKukus.Count == 0) return null;
        
        MythicalKukuData highestLevel = playerData.CollectedKukus[0];
        foreach (var kuku in playerData.CollectedKukus)
        {
            if (kuku.Level > highestLevel.Level)
            {
                highestLevel = kuku;
            }
        }
        
        return highestLevel;
    }
    
    /// <summary>
    /// 获取指定稀有度的最高级KuKu
    /// </summary>
    public MythicalKukuData GetHighestLevelKukuByRarity(MythicalKukuData.MythicalRarity rarity)
    {
        if (playerData == null) return null;
        
        var kukus = playerData.GetKukusByRarity(rarity);
        if (kukus.Count == 0) return null;
        
        MythicalKukuData highestLevel = kukus[0];
        foreach (var kuku in kukus)
        {
            if (kuku.Level > highestLevel.Level)
            {
                highestLevel = kuku;
            }
        }
        
        return highestLevel;
    }
    
    /// <summary>
    /// 获取收藏百分比
    /// </summary>
    public float GetCollectionPercentage()
    {
        if (playerData == null) return 0f;
        return (float)playerData.GetTotalKukuCount() / maxCollectionSlots * 100f;
    }
    
    /// <summary>
    /// 检查是否收集齐某稀有度的所有KuKu
    /// </summary>
    public bool HasCollectedAllOfRarity(MythicalKukuData.MythicalRarity rarity)
    {
        // 这需要知道每种稀有度应该有多少种KuKu
        // 简化实现，返回false
        return false;
    }
    
    /// <summary>
    /// 获取收藏成就
    /// </summary>
    public List<string> GetCollectionAchievements()
    {
        var achievements = new List<string>();
        
        if (playerData == null) return achievements;
        
        // 检查各种成就条件
        if (playerData.GetTotalKukuCount() >= 10)
            achievements.Add("新手收集家：收集10个KuKu");
            
        if (playerData.GetTotalKukuCount() >= 25)
            achievements.Add("资深收集家：收集25个KuKu");
            
        if (playerData.GetTotalKukuCount() >= 50)
            achievements.Add("大师收集家：收集全部KuKu");
            
        if (playerData.GetMythicKukuCount() > 0)
            achievements.Add("神话猎人：获得首个神话KuKu");
            
        if (playerData.GetTotalKukuPower() >= 1000)
            achievements.Add("强力战队：队伍总战力超过1000");
            
        return achievements;
    }
    
    public enum KukuType
    {
        Attack,      // 攻击型
        Defense,     // 防御型
        Magic,       // 魔法型
        Speed,       // 速度型
        Balanced     // 平衡型
    }
    
    public enum KukuSortCriteria
    {
        Name,                // 按名称
        Rarity,              // 按稀有度
        Level,               // 按等级
        Power,               // 按总战力
        CombatEffectiveness, // 按战斗效能
        AcquisitionDate      // 按获取日期
    }
    
    [System.Serializable]
    public class CollectionStats
    {
        public int TotalCount;
        public int MaxSlots;
        public int CelestialCount;
        public int ImmortalCount;
        public int DivineBeastCount;
        public int SacredCount;
        public int PrimordialCount;
        public int FavoriteCount;
        public int MaxFavorites;
        public int TotalPower;
        public float AverageLevel;
        
        public float GetRarityPercentage(MythicalKukuData.MythicalRarity rarity)
        {
            int count = 0;
            switch (rarity)
            {
                case MythicalKukuData.MythicalRarity.Celestial: count = CelestialCount; break;
                case MythicalKukuData.MythicalRarity.Immortal: count = ImmortalCount; break;
                case MythicalKukuData.MythicalRarity.DivineBeast: count = DivineBeastCount; break;
                case MythicalKukuData.MythicalRarity.Sacred: count = SacredCount; break;
                case MythicalKukuData.MythicalRarity.Primordial: count = PrimordialCount; break;
            }
            
            return TotalCount > 0 ? (float)count / TotalCount * 100f : 0f;
        }
        
        public float GetCollectionFillPercentage()
        {
            return MaxSlots > 0 ? (float)TotalCount / MaxSlots * 100f : 0f;
        }
        
        public float GetFavoriteFillPercentage()
        {
            return MaxFavorites > 0 ? (float)FavoriteCount / MaxFavorites * 100f : 0f;
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("KuKu收集系统已销毁");
    }
}