using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// KuKu收集系统 - 管理KuKu图鉴数据库（游戏外）
    /// </summary>
    public class KukuCollectionSystem
    {
        // 单例实例
        public static KukuCollectionSystem Instance { get; private set; }
        
        // KuKu数据库
        private Dictionary<int, KukuData> allKukuDatabase;
        private Dictionary<int, MythicalKukuData> allMythicalKukuDatabase;
        
        // 当前页面索引
        private int currentPageIndex;
        private const int KUKUS_PER_PAGE = 12;
        
        // 事件系统
        public event Action<KukuData> OnKukuUnlocked;                        // KuKu解锁事件
        public event Action<MythicalKukuData> OnMythicalKukuUnlocked;        // 神话KuKu解锁事件
        public event Action<int, float> OnPageCompleted;                     // 页面完成事件
        public event Action OnCollectionUpdated;                             // 收集更新事件
        
        // 构造函数
        public KukuCollectionSystem()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeKukuDatabase();
            }
        }
        
        /// <summary>
        /// 初始化KuKu数据库
        /// </summary>
        private void InitializeKukuDatabase()
        {
            allKukuDatabase = new Dictionary<int, KukuData>();
            allMythicalKukuDatabase = new Dictionary<int, MythicalKukuData>();
            
            // 生成基础KuKu数据
            GenerateBasicKukus();
            
            // 生成神话KuKu数据
            GenerateMythicalKukus();
            
            Debug.Log($"KuKu收集系统初始化完成 - 基础KuKu: {allKukuDatabase.Count}, 神话KuKu: {allMythicalKukuDatabase.Count}");
        }
        
        /// <summary>
        /// 生成基础KuKu数据
        /// </summary>
        private void GenerateBasicKukus()
        {
            string[] names = {
                "森林小精灵", "山川守护者", "湖泊仙子", "天空使者", "海洋歌者",
                "沙漠行者", "雪峰圣兽", "草原飞鹰", "洞穴智者", "遗迹守卫",
                "晨曦使者", "暮光精灵", "星辰骑士", "月影法师", "日耀战士"
            };
            
            string[] descriptions = {
                "栖息在森林深处的神秘生物", "守护山川河流的古老存在", "在湖泊中嬉戏的水之精灵",
                "翱翔于天空的自由使者", "在深海歌唱的神秘歌者", "穿越沙漠的坚韧旅者",
                "栖息雪峰的圣洁生物", "草原上飞翔的天空猎手", "洞穴中蕴含智慧的存在",
                "古老遗迹的忠诚守护者", "迎接晨曦的第一缕光芒", "在暮光中起舞的精灵",
                "星辰指引的勇敢骑士", "月影下的神秘法师", "太阳照耀的勇猛战士"
            };
            
            for (int i = 1; i <= 15; i++)
            {
                KukuData kuku = new KukuData
                {
                    Id = i,
                    Name = names[i - 1],
                    Description = descriptions[i - 1],
                    Rarity = (KukuData.RarityType)(i % 5), // 循环分配稀有度
                    AttackPower = 10f + i * 2f,
                    DefensePower = 5f + i * 1.5f,
                    Speed = 1f + i * 0.2f,
                    Health = 50f + i * 5f,
                    SkillName = $"技能{i}",
                    SkillDamage = 5f + i,
                    CaptureDifficulty = 1.0f - (i * 0.03f),
                    CanAbsorbSoul = i > 5, // 等级较高的KuKu可以吸收灵魂
                    SoulAbsorptionRate = i * 0.1f
                };
                
                allKukuDatabase[i] = kuku;
            }
        }
        
        /// <summary>
        /// 生成神话KuKu数据
        /// </summary>
        private void GenerateMythicalKukus()
        {
            string[] names = {
                "青龙", "白虎", "朱雀", "玄武", "麒麟", "凤凰", "饕餮", "貔貅",
                "应龙", "烛龙", "九尾狐", "毕方", "鲲鹏", "梼杌", "穷奇"
            };
            
            string[] elements = { "风", "火", "水", "土", "雷", "光", "暗", "木", "冰", "毒" };
            
            for (int i = 1; i <= 15; i++)
            {
                MythicalKukuData kuku = new MythicalKukuData
                {
                    Id = 100 + i,
                    Name = names[i - 1],
                    Description = $"传说中的神话生物 - {names[i - 1]}",
                    MythologicalBackground = "源自古代神话传说的神秘生物",
                    Element = elements[i % elements.Length],
                    SkillType = "Mythical",
                    SkillDescription = $"强大的神话技能 - {names[i - 1]}之怒",
                    SkillRange = 3f + i * 0.2f,
                    SkillPower = 50f + i * 5f,
                    AttackPower = 100f + i * 10f,
                    DefensePower = 80f + i * 8f,
                    Speed = 4f + i * 0.3f,
                    Health = 500f + i * 50f,
                    DivinePower = 50f + i * 5f,
                    ProtectionPower = 30f + i * 3f,
                    PurificationPower = 20f + i * 2f,
                    Rarity = (MythicalKukuData.MythicalRarity)(i % 5), // 循环分配稀有度
                    EvolutionLevel = 5, // 神话KuKu通常已达到进化顶峰
                    EvolutionProgress = 100f,
                    EvolutionStonesRequired = 50,
                    CanAbsorbSoul = true,
                    SoulAbsorptionRate = 1.0f + i * 0.1f,
                    CanFuseWithRobots = i > 8, // 部分神话KuKu可以融合
                    FusionCompatibility = 0.7f + (i % 3) * 0.1f,
                    Level = 20 + i * 2,
                    MaxEquipmentSlots = i > 10 ? 6 : 0, // 高级神话KuKu可装备
                    SpriteName = $"Mythical_{names[i - 1]}"
                };
                
                allMythicalKukuDatabase[100 + i] = kuku;
            }
        }
        
        /// <summary>
        /// 获取指定ID的基础KuKu数据
        /// </summary>
        public KukuData GetKukuTemplate(int kukuId)
        {
            if (allKukuDatabase.ContainsKey(kukuId))
            {
                return allKukuDatabase[kukuId].Clone();
            }
            return null;
        }
        
        /// <summary>
        /// 获取指定ID的神话KuKu数据
        /// </summary>
        public MythicalKukuData GetMythicalKukuTemplate(int kukuId)
        {
            if (allMythicalKukuDatabase.ContainsKey(kukuId))
            {
                return allMythicalKukuDatabase[kukuId].Clone();
            }
            return null;
        }
        
        /// <summary>
        /// 解锁KuKu（由游戏内系统调用）
        /// </summary>
        public void UnlockKuku(int kukuId)
        {
            if (allKukuDatabase.ContainsKey(kukuId))
            {
                KukuData kuku = allKukuDatabase[kukuId];
                OnKukuUnlocked?.Invoke(kuku);
                OnCollectionUpdated?.Invoke();
                
                Debug.Log($"解锁了KuKu: {kuku.Name}");
            }
            else if (allMythicalKukuDatabase.ContainsKey(kukuId))
            {
                MythicalKukuData kuku = allMythicalKukuDatabase[kukuId];
                OnMythicalKukuUnlocked?.Invoke(kuku);
                OnCollectionUpdated?.Invoke();
                
                Debug.Log($"解锁了神话KuKu: {kuku.Name}");
            }
        }
        
        /// <summary>
        /// 根据概率获取随机KuKu
        /// </summary>
        public KukuData GetRandomKukuByProbability()
        {
            // 根据稀有度权重随机选择
            List<KukuData> eligibleKukus = new List<KukuData>();
            
            foreach (var kvp in allKukuDatabase)
            {
                // 根据稀有度决定出现频率
                int weight = 10 - (int)kvp.Value.Rarity; // 稀有度越低权重越高
                for (int i = 0; i < weight; i++)
                {
                    eligibleKukus.Add(kvp.Value);
                }
            }
            
            if (eligibleKukus.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, eligibleKukus.Count);
                return eligibleKukus[randomIndex].Clone();
            }
            
            return null;
        }
        
        /// <summary>
        /// 根据概率获取随机神话KuKu
        /// </summary>
        public MythicalKukuData GetRandomMythicalKukuByProbability()
        {
            // 根据稀有度权重随机选择
            List<MythicalKukuData> eligibleKukus = new List<MythicalKukuData>();
            
            foreach (var kvp in allMythicalKukuDatabase)
            {
                // 根据稀有度决定出现频率
                int weight = 6 - (int)kvp.Value.Rarity; // 稀有度越低权重越高
                for (int i = 0; i < weight; i++)
                {
                    eligibleKukus.Add(kvp.Value);
                }
            }
            
            if (eligibleKukus.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, eligibleKukus.Count);
                return eligibleKukus[randomIndex].Clone();
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取当前页面的基础KuKu列表
        /// </summary>
        public List<KukuData> GetCurrentPageKukus()
        {
            return GetPageKukus(currentPageIndex);
        }
        
        /// <summary>
        /// 获取指定页面的基础KuKu列表
        /// </summary>
        public List<KukuData> GetPageKukus(int pageIndex)
        {
            var allKukus = allKukuDatabase.Values.ToList();
            int startIndex = pageIndex * KUKUS_PER_PAGE;
            
            if (startIndex >= allKukus.Count)
            {
                return new List<KukuData>(); // 返回空列表
            }
            
            var pageKukus = new List<KukuData>();
            
            for (int i = startIndex; i < startIndex + KUKUS_PER_PAGE && i < allKukus.Count; i++)
            {
                pageKukus.Add(allKukus[i].Clone());
            }
            
            return pageKukus;
        }
        
        /// <summary>
        /// 获取当前页面的神话KuKu列表
        /// </summary>
        public List<MythicalKukuData> GetCurrentPageMythicalKukus()
        {
            return GetPageMythicalKukus(currentPageIndex);
        }
        
        /// <summary>
        /// 获取指定页面的神话KuKu列表
        /// </summary>
        public List<MythicalKukuData> GetPageMythicalKukus(int pageIndex)
        {
            var allKukus = allMythicalKukuDatabase.Values.ToList();
            int startIndex = pageIndex * KUKUS_PER_PAGE;
            
            if (startIndex >= allKukus.Count)
            {
                return new List<MythicalKukuData>(); // 返回空列表
            }
            
            var pageKukus = new List<MythicalKukuData>();
            
            for (int i = startIndex; i < startIndex + KUKUS_PER_PAGE && i < allKukus.Count; i++)
            {
                pageKukus.Add(allKukus[i].Clone());
            }
            
            return pageKukus;
        }
        
        /// <summary>
        /// 获取基础KuKu总页数
        /// </summary>
        public int GetTotalPages()
        {
            int totalKukus = allKukuDatabase.Count;
            return Mathf.CeilToInt((float)totalKukus / KUKUS_PER_PAGE);
        }
        
        /// <summary>
        /// 获取神话KuKu总页数
        /// </summary>
        public int GetTotalMythicalPages()
        {
            int totalKukus = allMythicalKukuDatabase.Count;
            return Mathf.CeilToInt((float)totalKukus / KUKUS_PER_PAGE);
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
            }
        }
        
        /// <summary>
        /// 获取当前页面索引
        /// </summary>
        public int GetCurrentPageIndex()
        {
            return currentPageIndex;
        }
        
        /// <summary>
        /// 获取基础KuKu收集进度
        /// </summary>
        public float GetCollectionProgress()
        {
            // 这里应该连接到玩家数据，但暂时返回模拟值
            return (float)allKukuDatabase.Count / 100f; // 假设有100个KuKu
        }
        
        /// <summary>
        /// 获取指定稀有度的基础KuKu收集进度
        /// </summary>
        public float GetRarityCollectionProgress(KukuData.RarityType rarity)
        {
            int totalRarityKukus = allKukuDatabase.Values.Count(k => k.Rarity == rarity);
            int collectedRarityKukus = allKukuDatabase.Values.Count(k => k.Rarity == rarity); // 模拟全部收集
            
            return totalRarityKukus > 0 ? (float)collectedRarityKukus / totalRarityKukus : 0f;
        }
        
        /// <summary>
        /// 获取指定稀有度的神话KuKu收集进度
        /// </summary>
        public float GetMythicalRarityCollectionProgress(MythicalKukuData.MythicalRarity rarity)
        {
            int totalRarityKukus = allMythicalKukuDatabase.Values.Count(k => k.Rarity == rarity);
            int collectedRarityKukus = allMythicalKukuDatabase.Values.Count(k => k.Rarity == rarity); // 模拟全部收集
            
            return totalRarityKukus > 0 ? (float)collectedRarityKukus / totalRarityKukus : 0f;
        }
        
        /// <summary>
        /// 获取所有基础KuKu的数量
        /// </summary>
        public int GetTotalKukuCount()
        {
            return allKukuDatabase.Count;
        }
        
        /// <summary>
        /// 获取所有神话KuKu的数量
        /// </summary>
        public int GetTotalMythicalKukuCount()
        {
            return allMythicalKukuDatabase.Count;
        }
        
        /// <summary>
        /// 搜索KuKu
        /// </summary>
        public List<KukuData> SearchKukus(string searchTerm)
        {
            List<KukuData> results = new List<KukuData>();
            
            foreach (var kvp in allKukuDatabase)
            {
                if (kvp.Value.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    kvp.Value.Description.ToLower().Contains(searchTerm.ToLower()))
                {
                    results.Add(kvp.Value.Clone());
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 按稀有度筛选KuKu
        /// </summary>
        public List<KukuData> FilterKukusByRarity(KukuData.RarityType rarity)
        {
            List<KukuData> results = new List<KukuData>();
            
            foreach (var kvp in allKukuDatabase)
            {
                if (kvp.Value.Rarity == rarity)
                {
                    results.Add(kvp.Value.Clone());
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 按元素筛选神话KuKu
        /// </summary>
        public List<MythicalKukuData> FilterMythicalKukusByElement(string element)
        {
            List<MythicalKukuData> results = new List<MythicalKukuData>();
            
            foreach (var kvp in allMythicalKukuDatabase)
            {
                if (kvp.Value.Element.ToLower().Contains(element.ToLower()))
                {
                    results.Add(kvp.Value.Clone());
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 获取KuKu统计信息
        /// </summary>
        public Dictionary<KukuData.RarityType, int> GetKukuStatistics()
        {
            Dictionary<KukuData.RarityType, int> stats = new Dictionary<KukuData.RarityType, int>();
            
            foreach (KukuData.RarityType rarity in Enum.GetValues(typeof(KukuData.RarityType)))
            {
                stats[rarity] = allKukuDatabase.Values.Count(k => k.Rarity == rarity);
            }
            
            return stats;
        }
        
        /// <summary>
        /// 获取神话KuKu统计信息
        /// </summary>
        public Dictionary<MythicalKukuData.MythicalRarity, int> GetMythicalKukuStatistics()
        {
            Dictionary<MythicalKukuData.MythicalRarity, int> stats = new Dictionary<MythicalKukuData.MythicalRarity, int>();
            
            foreach (MythicalKukuData.MythicalRarity rarity in Enum.GetValues(typeof(MythicalKukuData.MythicalRarity)))
            {
                stats[rarity] = allMythicalKukuDatabase.Values.Count(k => k.Rarity == rarity);
            }
            
            return stats;
        }
        
        /// <summary>
        /// 重置系统
        /// </summary>
        public void Reset()
        {
            allKukuDatabase.Clear();
            allMythicalKukuDatabase.Clear();
            currentPageIndex = 0;
            
            InitializeKukuDatabase();
        }
    }
}