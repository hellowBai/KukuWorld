using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 宠物收集管理器 - 管理宠物图鉴数据库
    /// </summary>
    public class PetCollectionManager : MonoBehaviour
    {
        public static PetCollectionManager Instance { get; private set; } // 单例实例

        // 配置参数
        [Header("收集系统配置")]
        public int petsPerPage = 12;                    // 每页宠物数量
        public float unlockNotificationDuration = 2f;   // 解锁通知显示时间

        // 数据存储
        private Dictionary<int, PetCollectionEntry> allPetDatabase = new Dictionary<int, PetCollectionEntry>(); // 宠物数据库
        private int currentPageIndex = 0;                                      // 当前页面索引

        // 事件系统
        public System.Action<PetData> OnPetUnlocked;                        // 宠物解锁事件
        public System.Action<int, float> OnPageCompleted;                   // 页面完成事件
        public System.Action OnCollectionUpdated;                           // 收集更新事件
        public System.Action<int, int> OnCollectionProgressChanged;         // 收集进度变化事件

        void Awake()
        {
            // 确保只有一个PetCollectionManager实例
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePetDatabase();
        }

        /// <summary>
        /// 初始化宠物数据库
        /// </summary>
        private void InitializePetDatabase()
        {
            // 生成初始宠物数据库
            GenerateInitialPetDatabase();
        }

        /// <summary>
        /// 生成初始宠物数据库
        /// </summary>
        private void GenerateInitialPetDatabase()
        {
            // 创建常见稀有度宠物 (Common)
            for (int i = 1; i <= 20; i++)
            {
                PetCollectionEntry entry = new PetCollectionEntry(
                    i, 
                    GeneratePetName(PetData.RarityType.Common, i), 
                    PetData.RarityType.Common
                );
                allPetDatabase.Add(entry.petId, entry);
            }

            // 创建罕见稀有度宠物 (Rare)
            for (int i = 21; i <= 35; i++)
            {
                PetCollectionEntry entry = new PetCollectionEntry(
                    i, 
                    GeneratePetName(PetData.RarityType.Rare, i), 
                    PetData.RarityType.Rare
                );
                allPetDatabase.Add(entry.petId, entry);
            }

            // 创建史诗稀有度宠物 (Epic)
            for (int i = 36; i <= 45; i++)
            {
                PetCollectionEntry entry = new PetCollectionEntry(
                    i, 
                    GeneratePetName(PetData.RarityType.Epic, i), 
                    PetData.RarityType.Epic
                );
                allPetDatabase.Add(entry.petId, entry);
            }

            // 创建传说稀有度宠物 (Legendary)
            for (int i = 46; i <= 50; i++)
            {
                PetCollectionEntry entry = new PetCollectionEntry(
                    i, 
                    GeneratePetName(PetData.RarityType.Legendary, i), 
                    PetData.RarityType.Legendary
                );
                allPetDatabase.Add(entry.petId, entry);
            }

            // 创建神话稀有度宠物 (Mythic)
            for (int i = 51; i <= 55; i++)
            {
                PetCollectionEntry entry = new PetCollectionEntry(
                    i, 
                    GeneratePetName(PetData.RarityType.Mythic, i), 
                    PetData.RarityType.Mythic
                );
                allPetDatabase.Add(entry.petId, entry);
            }

            Debug.Log($"初始化了 {allPetDatabase.Count} 个宠物到图鉴数据库");
        }

        /// <summary>
        /// 生成宠物名字
        /// </summary>
        private string GeneratePetName(PetData.RarityType rarity, int id)
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
                case PetData.RarityType.Common:
                    prefixes = commonPrefixes;
                    break;
                case PetData.RarityType.Rare:
                    prefixes = rarePrefixes;
                    break;
                case PetData.RarityType.Epic:
                    prefixes = epicPrefixes;
                    break;
                case PetData.RarityType.Legendary:
                    prefixes = legendaryPrefixes;
                    break;
                case PetData.RarityType.Mythic:
                    prefixes = mythicPrefixes;
                    break;
            }

            string prefix = prefixes[id % prefixes.Length];
            string suffix = suffixes[id % suffixes.Length];

            return prefix + suffix;
        }

        /// <summary>
        /// 获取指定ID的宠物数据
        /// </summary>
        public PetData GetPetTemplate(int petId)
        {
            if (allPetDatabase.ContainsKey(petId))
            {
                // 创建一个PetData实例并填充基本信息
                PetCollectionEntry entry = allPetDatabase[petId];
                PetData pet = new PetData();
                
                pet.Id = entry.petId;
                pet.Name = entry.petName;
                pet.Rarity = entry.rarity;
                
                // 根据稀有度设置基础属性
                SetPetAttributesByRarity(pet, entry.rarity);
                
                return pet;
            }
            
            return null;
        }

        /// <summary>
        /// 获取神话宠物模板
        /// </summary>
        public MythicalPetData GetMythicalPetTemplate(int petId)
        {
            if (allPetDatabase.ContainsKey(petId))
            {
                PetCollectionEntry entry = allPetDatabase[petId];
                
                if (entry.rarity == PetData.RarityType.Mythic)
                {
                    MythicalPetData pet = new MythicalPetData();
                    
                    pet.Id = entry.petId;
                    pet.Name = entry.petName;
                    pet.Rarity = (MythicalPetData.MythicalRarity)System.Math.Min((int)MythicalPetData.MythicalRarity.Primordial, (int)entry.rarity);
                    
                    // 设置神话宠物属性
                    SetMythicalPetAttributes(pet, pet.Rarity);
                    
                    return pet;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 获取玩家拥有的宠物数据
        /// </summary>
        public PetData GetPlayerPet(int petId)
        {
            if (GameManager.Instance?.PlayerData.CollectedPets.ContainsKey(petId) == true)
            {
                return GameManager.Instance.PlayerData.CollectedPets[petId];
            }
            
            return null;
        }

        /// <summary>
        /// 获取玩家拥有的神话宠物数据
        /// </summary>
        public MythicalPetData GetPlayerMythicalPet(int petId)
        {
            if (GameManager.Instance?.PlayerData.CollectedPets.ContainsKey(petId) == true)
            {
                var pet = GameManager.Instance.PlayerData.CollectedPets[petId];
                if (pet is MythicalPetData)
                {
                    return (MythicalPetData)pet;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 解锁宠物（由游戏内系统调用）
        /// </summary>
        public void UnlockPet(int petId)
        {
            if (allPetDatabase.ContainsKey(petId))
            {
                PetCollectionEntry entry = allPetDatabase[petId];
                
                if (!entry.isUnlocked)
                {
                    // 标记为已解锁
                    entry.isUnlocked = true;
                    entry.timesCaught = 1;
                    entry.firstUnlockDate = DateTime.Now;
                    entry.lastCatchDate = DateTime.Now;
                    
                    // 更新数据库
                    allPetDatabase[petId] = entry;
                    
                    Debug.Log($"宠物 {entry.petName} 已解锁！");
                    
                    // 触发解锁事件
                    OnPetUnlocked?.Invoke(GetPetTemplate(petId));
                    
                    // 更新收集进度
                    OnCollectionUpdated?.Invoke();
                    
                    // 显示解锁通知
                    ShowUnlockNotification(entry.petName, entry.rarity);
                }
                else
                {
                    // 宠物已解锁，更新捕捉次数
                    entry.timesCaught++;
                    entry.lastCatchDate = DateTime.Now;
                    allPetDatabase[petId] = entry;
                }
            }
        }

        /// <summary>
        /// 显示解锁通知
        /// </summary>
        private void ShowUnlockNotification(string petName, PetData.RarityType rarity)
        {
            string rarityText = GetRarityDisplayName(rarity);
            string message = $"<color={GetRarityColorCode(rarity)}>解锁新宠物！</color>\n{petName} ({rarityText})";
            
            Debug.Log(message);
        }

        /// <summary>
        /// 根据概率获取随机宠物
        /// </summary>
        public PetData GetRandomPetByProbability()
        {
            // 计算总的权重
            float totalWeight = 0f;
            foreach (var entry in allPetDatabase.Values)
            {
                if (!entry.isUnlocked) // 只考虑未解锁的宠物
                {
                    float weight = GetRarityWeight(entry.rarity);
                    totalWeight += weight;
                }
            }
            
            if (totalWeight <= 0)
            {
                // 所有宠物都已解锁，返回已解锁宠物
                var unlockedPets = new List<PetCollectionEntry>();
                foreach (var entry in allPetDatabase.Values)
                {
                    if (entry.isUnlocked)
                    {
                        unlockedPets.Add(entry);
                    }
                }
                
                if (unlockedPets.Count > 0)
                {
                    var randomEntry = unlockedPets[UnityEngine.Random.Range(0, unlockedPets.Count)];
                    return GetPetTemplate(randomEntry.petId);
                }
                
                return null; // 没有任何宠物
            }
            
            // 随机选择一个宠物
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;
            
            foreach (var entry in allPetDatabase.Values)
            {
                if (!entry.isUnlocked)
                {
                    float weight = GetRarityWeight(entry.rarity);
                    accumulatedWeight += weight;
                    
                    if (randomValue <= accumulatedWeight)
                    {
                        return GetPetTemplate(entry.petId);
                    }
                }
            }
            
            // 如果没有找到合适的宠物，返回最后一个
            var lastEntry = allPetDatabase.Values.GetEnumerator();
            PetCollectionEntry last = new PetCollectionEntry(0, "", PetData.RarityType.Common);
            while (lastEntry.MoveNext())
            {
                last = lastEntry.Current;
            }
            
            return GetPetTemplate(last.petId);
        }

        /// <summary>
        /// 获取稀有度权重
        /// </summary>
        private float GetRarityWeight(PetData.RarityType rarity)
        {
            // 根据稀有度分配权重，越稀有的权重越低（更难遇到）
            switch (rarity)
            {
                case PetData.RarityType.Common: return 50f;    // 50% 权重
                case PetData.RarityType.Rare: return 25f;      // 25% 权重
                case PetData.RarityType.Epic: return 15f;      // 15% 权重
                case PetData.RarityType.Legendary: return 8f;  // 8% 权重
                case PetData.RarityType.Mythic: return 2f;     // 2% 权重
                default: return 10f;
            }
        }

        /// <summary>
        /// 获取当前页面的宠物列表
        /// </summary>
        public List<PetCollectionEntry> GetCurrentPagePets()
        {
            return GetPagePets(currentPageIndex);
        }

        /// <summary>
        /// 获取指定页面的宠物列表
        /// </summary>
        public List<PetCollectionEntry> GetPagePets(int pageIndex)
        {
            List<PetCollectionEntry> allPets = new List<PetCollectionEntry>(allPetDatabase.Values);
            
            int startIndex = pageIndex * petsPerPage;
            if (startIndex >= allPets.Count)
            {
                return new List<PetCollectionEntry>(); // 返回空列表
            }
            
            int endIndex = Math.Min(startIndex + petsPerPage, allPets.Count);
            List<PetCollectionEntry> pagePets = new List<PetCollectionEntry>();
            
            for (int i = startIndex; i < endIndex; i++)
            {
                pagePets.Add(allPets[i]);
            }
            
            return pagePets;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        public int GetTotalPages()
        {
            int totalCount = allPetDatabase.Count;
            return Mathf.CeilToInt((float)totalCount / petsPerPage);
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

        /// <summary>
        /// 获取总体收集进度
        /// </summary>
        public float GetCollectionProgress()
        {
            int totalPets = allPetDatabase.Count;
            if (totalPets <= 0) return 0f;
            
            int unlockedPets = 0;
            foreach (var entry in allPetDatabase.Values)
            {
                if (entry.isUnlocked)
                {
                    unlockedPets++;
                }
            }
            
            return (float)unlockedPets / totalPets;
        }

        /// <summary>
        /// 获取指定稀有度的收集进度
        /// </summary>
        public float GetRarityCollectionProgress(PetData.RarityType rarity)
        {
            int totalPetsOfRarity = 0;
            int unlockedPetsOfRarity = 0;
            
            foreach (var entry in allPetDatabase.Values)
            {
                if (entry.rarity == rarity)
                {
                    totalPetsOfRarity++;
                    if (entry.isUnlocked)
                    {
                        unlockedPetsOfRarity++;
                    }
                }
            }
            
            if (totalPetsOfRarity <= 0) return 0f;
            
            return (float)unlockedPetsOfRarity / totalPetsOfRarity;
        }

        /// <summary>
        /// 获取各稀有度统计
        /// </summary>
        public Dictionary<PetData.RarityType, (int total, int unlocked)> GetRarityStatistics()
        {
            Dictionary<PetData.RarityType, (int total, int unlocked)> stats = new Dictionary<PetData.RarityType, (int, int)>();
            
            // 初始化所有稀有度的统计
            foreach (PetData.RarityType rarity in Enum.GetValues(typeof(PetData.RarityType)))
            {
                stats[rarity] = (0, 0);
            }
            
            // 统计每个稀有度的数量
            foreach (var entry in allPetDatabase.Values)
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

        /// <summary>
        /// 设置宠物属性（根据稀有度）
        /// </summary>
        private void SetPetAttributesByRarity(PetData pet, PetData.RarityType rarity)
        {
            // 根据稀有度设置基础属性
            float rarityMultiplier = 1f;
            switch (rarity)
            {
                case PetData.RarityType.Common: rarityMultiplier = 1.0f; break;
                case PetData.RarityType.Rare: rarityMultiplier = 1.5f; break;
                case PetData.RarityType.Epic: rarityMultiplier = 2.0f; break;
                case PetData.RarityType.Legendary: rarityMultiplier = 3.0f; break;
                case PetData.RarityType.Mythic: rarityMultiplier = 5.0f; break;
            }
            
            pet.AttackPower = 10f * rarityMultiplier;
            pet.DefensePower = 5f * rarityMultiplier;
            pet.Speed = 1f * rarityMultiplier;
            pet.Health = 50f * rarityMultiplier;
            pet.Level = 1;
            pet.CaptureDifficulty = 1.0f - (int)rarity * 0.1f;
            pet.CanAbsorbSoul = true;
            pet.SoulAbsorptionRate = 0.1f + (int)rarity * 0.05f;
            
            // 设置稀有度颜色
            pet.Tint = pet.GetRarityColor();
        }

        /// <summary>
        /// 设置神话宠物属性
        /// </summary>
        private void SetMythicalPetAttributes(MythicalPetData pet, MythicalPetData.MythicalRarity rarity)
        {
            // 设置基础属性
            SetPetAttributesByRarity(pet, (PetData.RarityType)(int)rarity);
            
            // 设置神话特有属性
            float rarityMultiplier = 1f + (int)rarity * 0.5f;
            
            pet.DivinePower = 10f * rarityMultiplier;
            pet.ProtectionPower = 5f * rarityMultiplier;
            pet.PurificationPower = 3f * rarityMultiplier;
            
            // 设置元素属性
            string[] elements = { "Fire", "Water", "Wood", "Metal", "Earth", "Light", "Dark" };
            pet.Element = elements[(pet.Id - 51) % elements.Length]; // 神话宠物ID从51开始
            
            // 设置技能
            MythicalPetData.MythicalSkillType[] skills = Enum.GetValues(typeof(MythicalPetData.MythicalSkillType)) as MythicalPetData.MythicalSkillType[];
            pet.SkillType = skills[pet.Id % skills.Length];
            
            pet.SkillPower = 10f * rarityMultiplier;
            pet.SkillRange = 3f;
            
            // 设置进化相关属性
            pet.EvolutionLevel = 1;
            pet.EvolutionProgress = 0f;
            pet.EvolutionStonesRequired = 10 + (int)rarity * 5;
            
            // 设置融合属性
            pet.CanFuseWithRobots = (int)rarity >= 3; // Sacred及以上可融合
            pet.FusionCompatibility = 0.3f + (int)rarity * 0.1f;
        }

        /// <summary>
        /// 获取稀有度显示名称
        /// </summary>
        private string GetRarityDisplayName(PetData.RarityType rarity)
        {
            switch (rarity)
            {
                case PetData.RarityType.Common: return "普通";
                case PetData.RarityType.Rare: return "罕见";
                case PetData.RarityType.Epic: return "史诗";
                case PetData.RarityType.Legendary: return "传说";
                case PetData.RarityType.Mythic: return "神话";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取稀有度颜色代码
        /// </summary>
        private string GetRarityColorCode(PetData.RarityType rarity)
        {
            switch (rarity)
            {
                case PetData.RarityType.Common: return "#CCCCCC"; // 灰色
                case PetData.RarityType.Rare: return "#4169E1";   // 道奇蓝
                case PetData.RarityType.Epic: return "#DA70D6";   // 兰花紫
                case PetData.RarityType.Legendary: return "#FFD700"; // 金色
                case PetData.RarityType.Mythic: return "#00FFFF"; // 青色
                default: return "#FFFFFF"; // 白色
            }
        }

        /// <summary>
        /// 同步游戏内外收集状态
        /// </summary>
        public void SyncWithPlayerData()
        {
            if (GameManager.Instance?.PlayerData == null) return;
            
            // 遍历玩家已收集的宠物，确保图鉴中也标记为已解锁
            foreach (var collectedPet in GameManager.Instance.PlayerData.CollectedPets)
            {
                if (allPetDatabase.ContainsKey(collectedPet.Key))
                {
                    PetCollectionEntry entry = allPetDatabase[collectedPet.Key];
                    if (!entry.isUnlocked)
                    {
                        entry.isUnlocked = true;
                        entry.timesCaught++;
                        if (entry.firstUnlockDate == DateTime.MinValue)
                        {
                            entry.firstUnlockDate = DateTime.Now;
                        }
                        entry.lastCatchDate = DateTime.Now;
                        
                        allPetDatabase[collectedPet.Key] = entry;
                    }
                }
                else
                {
                    // 如果图鉴中没有这个宠物，可能是动态生成的，添加到图鉴
                    PetData pet = collectedPet.Value;
                    PetCollectionEntry newEntry = new PetCollectionEntry(pet.Id, pet.Name, pet.Rarity);
                    newEntry.isUnlocked = true;
                    newEntry.timesCaught = 1;
                    newEntry.firstUnlockDate = DateTime.Now;
                    newEntry.lastCatchDate = DateTime.Now;
                    
                    allPetDatabase[pet.Id] = newEntry;
                }
            }
        }

        // 内部类：宠物图鉴条目
        [System.Serializable]
        public class PetCollectionEntry
        {
            public int petId;                           // 宠物ID
            public string petName;                      // 宠物名称
            public PetData.RarityType rarity;           // 稀有度
            public bool isUnlocked;                     // 是否已解锁
            public int timesCaught;                     // 捕捉次数
            public DateTime firstUnlockDate;            // 首次解锁日期
            public DateTime lastCatchDate;              // 最后捕捉日期

            public PetCollectionEntry(int id, string name, PetData.RarityType r)
            {
                petId = id;
                petName = name;
                rarity = r;
                isUnlocked = false;
                timesCaught = 0;
                firstUnlockDate = DateTime.MinValue;
                lastCatchDate = DateTime.MinValue;
            }
        }
    }
}