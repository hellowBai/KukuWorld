using System;
using System.Collections.Generic;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 玩家数据结构
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        // 基础信息
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        public float ExperienceToNextLevel { get; set; }
        
        // 英雄系统
        public HeroData Hero { get; set; }
        public List<HeroSkill> AvailableSkills { get; set; }
        
        // 资源
        public int Coins { get; set; }
        public int Gems { get; set; }
        public float Souls { get; set; }
        
        // KuKu收集（游戏内实际拥有的KuKu）
        public Dictionary<int, MythicalKukuData> CollectedKukus { get; set; }
        public List<int> ActiveKukuTeam { get; set; }
        
        // 建筑
        public List<BuildingData> BuiltBuildings { get; set; }
        public List<UnitData> DeployedUnits { get; set; }
        
        // 游戏阶段状态
        public bool IsInCapturePhase { get; set; }
        public float TimeInCapturePhase { get; set; }
        public int EnemiesDefeatedInDefense { get; set; }
        public int CurrentWaveInDefense { get; set; }
        
        // 装备
        public List<WeaponData> OwnedEquipment { get; set; }
        public Dictionary<int, List<WeaponData>> UnitEquipment { get; set; }
        
        // 图鉴系统
        public Dictionary<int, KukuPokedexEntry> KukuPokedex { get; set; }
        
        // 游戏进度
        public int HighestWaveReached { get; set; }
        public int TotalPetsCaught { get; set; }
        public int TotalSoulsCollected { get; set; }
        public int TotalGemsEarned { get; set; }
        public int TotalCoinsEarned { get; set; }
        public DateTime LastPlayed { get; set; }
        public int PlayTimeMinutes { get; set; }
        
        // 设置
        public bool SoundEnabled { get; set; }
        public bool MusicEnabled { get; set; }
        public int GraphicsQuality { get; set; }
        public float Volume { get; set; }
        
        // 其他
        public List<string> Achievements { get; set; }
        public List<DateTime> LoginDates { get; set; }
        public int ConsecutiveLoginDays { get; set; }
        public int MaxConsecutiveLoginDays { get; set; }
        public bool HasClaimedDailyReward { get; set; }
        public DateTime LastDailyRewardClaim { get; set; }

        public PlayerData()
        {
            // 初始化默认值
            PlayerName = "KukuHunter";
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 100;
            
            Hero = new HeroData();
            AvailableSkills = new List<HeroSkill>();
            
            Coins = 1000;
            Gems = 100;
            Souls = 0;
            
            CollectedKukus = new Dictionary<int, MythicalKukuData>();
            ActiveKukuTeam = new List<int>();
            
            BuiltBuildings = new List<BuildingData>();
            DeployedUnits = new List<UnitData>();
            
            IsInCapturePhase = true;
            TimeInCapturePhase = 0;
            EnemiesDefeatedInDefense = 0;
            CurrentWaveInDefense = 0;
            
            OwnedEquipment = new List<WeaponData>();
            UnitEquipment = new Dictionary<int, List<WeaponData>>();
            
            KukuPokedex = new Dictionary<int, KukuPokedexEntry>();
            
            HighestWaveReached = 0;
            TotalPetsCaught = 0;
            TotalSoulsCollected = 0;
            TotalGemsEarned = 0;
            TotalCoinsEarned = 0;
            LastPlayed = DateTime.Now;
            PlayTimeMinutes = 0;
            
            SoundEnabled = true;
            MusicEnabled = true;
            GraphicsQuality = 2; // 中等
            Volume = 0.8f;
            
            Achievements = new List<string>();
            LoginDates = new List<DateTime>();
            ConsecutiveLoginDays = 0;
            MaxConsecutiveLoginDays = 0;
            HasClaimedDailyReward = false;
            LastDailyRewardClaim = DateTime.MinValue;
        }
        
        /// <summary>
        /// 添加金币
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount > 0)
            {
                Coins += amount;
                TotalCoinsEarned += amount;
            }
        }
        
        /// <summary>
        /// 添加神石
        /// </summary>
        public void AddGems(int amount)
        {
            if (amount > 0)
            {
                Gems += amount;
                TotalGemsEarned += amount;
            }
        }
        
        /// <summary>
        /// 添加灵魂
        /// </summary>
        public void AddSouls(float amount)
        {
            if (amount > 0)
            {
                Souls += amount;
                TotalSoulsCollected += (int)amount;
            }
        }
        
        /// <summary>
        /// 添加经验值
        /// </summary>
        public bool AddExperience(float exp)
        {
            if (exp <= 0) return false;
            
            Experience += exp;
            
            // 检查是否可以升级
            if (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                LevelUp();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 升级玩家等级
        /// </summary>
        public void LevelUp()
        {
            Level++;
            ExperienceToNextLevel = Level * 100f; // 每级需要的经验值
            
            // 可以在这里添加升级奖励
            Debug.Log($"玩家升级到 {Level} 级！");
        }
        
        /// <summary>
        /// 添加KuKu到收藏（游戏内）
        /// </summary>
        public void AddKuku(MythicalKukuData kuku)
        {
            if (kuku != null)
            {
                CollectedKukus[kuku.Id] = kuku;
                TotalPetsCaught++;
            }
        }
        
        /// <summary>
        /// 通知收集系统解锁KuKu（游戏外）
        /// </summary>
        public void NotifyKukuUnlocked(int kukuId)
        {
            // 这里可以触发成就、奖励等
        }
        
        /// <summary>
        /// 移除KuKu
        /// </summary>
        public bool RemoveKuku(int kukuId)
        {
            return CollectedKukus.Remove(kukuId);
        }
        
        /// <summary>
        /// 检查是否有足够资源
        /// </summary>
        public bool HasResources(int coins, int gems)
        {
            return Coins >= coins && Gems >= gems;
        }
        
        /// <summary>
        /// 消耗资源
        /// </summary>
        public bool ConsumeResources(int coins, int gems)
        {
            if (HasResources(coins, gems))
            {
                Coins -= coins;
                Gems -= gems;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 检查是否可以进入防守阶段
        /// </summary>
        public bool CanStartDefensePhase()
        {
            // 至少需要一只KuKu才能进入防守阶段
            return CollectedKukus.Count > 0;
        }
        
        /// <summary>
        /// 切换到防守阶段
        /// </summary>
        public void EnterDefensePhase()
        {
            IsInCapturePhase = false;
        }
        
        /// <summary>
        /// 添加已部署单位
        /// </summary>
        public void AddDeployedUnit(UnitData unit)
        {
            if (unit != null)
            {
                DeployedUnits.Add(unit);
            }
        }
        
        /// <summary>
        /// 获取玩家战斗力评分
        /// </summary>
        public float GetPlayerCombatRating()
        {
            float rating = 0f;
            
            // 计算KuKu的战斗力
            foreach (var kvp in CollectedKukus)
            {
                var kuku = kvp.Value;
                rating += kuku.AttackPower + kuku.DefensePower + kuku.Health * 0.1f;
            }
            
            // 计算已部署单位的战斗力
            foreach (var unit in DeployedUnits)
            {
                rating += unit.AttackPower + unit.DefensePower + unit.Health * 0.1f;
            }
            
            // 计算建筑的战斗力
            foreach (var building in BuiltBuildings)
            {
                if (building.Type == BuildingData.BuildingType.Tower)
                {
                    rating += building.AttackPower * 0.5f + building.Health * 0.05f;
                }
            }
            
            return rating;
        }
        
        /// <summary>
        /// 添加成就
        /// </summary>
        public void AddAchievement(string achievement)
        {
            if (!Achievements.Contains(achievement))
            {
                Achievements.Add(achievement);
            }
        }
        
        /// <summary>
        /// 检查是否拥有成就
        /// </summary>
        public bool HasAchievement(string achievement)
        {
            return Achievements.Contains(achievement);
        }
        
        /// <summary>
        /// 记录登录日期
        /// </summary>
        public void RecordLogin()
        {
            DateTime today = DateTime.Today;
            
            // 如果今天还没有记录过登录
            if (LoginDates.Count == 0 || LoginDates[LoginDates.Count - 1] != today)
            {
                LoginDates.Add(today);
                
                // 计算连续登录天数
                if (LoginDates.Count > 1)
                {
                    DateTime previousDay = LoginDates[LoginDates.Count - 2];
                    TimeSpan diff = today - previousDay;
                    
                    if (diff.Days == 1)
                    {
                        // 连续登录
                        ConsecutiveLoginDays++;
                    }
                    else
                    {
                        // 断开了连续登录
                        MaxConsecutiveLoginDays = Math.Max(MaxConsecutiveLoginDays, ConsecutiveLoginDays);
                        ConsecutiveLoginDays = 1;
                    }
                }
                else
                {
                    // 第一次登录
                    ConsecutiveLoginDays = 1;
                }
                
                MaxConsecutiveLoginDays = Math.Max(MaxConsecutiveLoginDays, ConsecutiveLoginDays);
            }
        }
        
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{PlayerName} - Lv.{Level} | 金币:{Coins} | 神石:{Gems} | 灵魂:{Souls:F0} | KuKu数量:{CollectedKukus.Count}";
        }
    }
    
    /// <summary>
    /// 英雄数据结构
    /// </summary>
    [Serializable]
    public class HeroData
    {
        public string HeroName { get; set; }
        public HeroClass Class { get; set; }
        public List<HeroSkill> Skills { get; set; }
        
        public enum HeroClass { 
            Warrior,     // 战士 - 擅长近战和防御
            Mage,        // 法师 - 擅长魔法攻击和辅助
            Ranger,      // 游侠 - 擅长远程攻击和捕捉
            Guardian     // 守护者 - 擅长守护和治疗
        }
        
        public HeroData()
        {
            HeroName = "无名英雄";
            Class = HeroClass.Ranger; // 默认选择游侠，适合捕捉
            Skills = new List<HeroSkill>();
        }
        
        /// <summary>
        /// 使用技能
        /// </summary>
        public bool UseSkill(int skillId)
        {
            foreach (var skill in Skills)
            {
                if (skill.Id == skillId && skill.IsAvailable())
                {
                    skill.Use();
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 升级技能
        /// </summary>
        public bool UpgradeSkill(int skillId)
        {
            foreach (var skill in Skills)
            {
                if (skill.Id == skillId)
                {
                    return skill.Upgrade();
                }
            }
            return false;
        }
    }
    
    /// <summary>
    /// 英雄技能结构
    /// </summary>
    [Serializable]
    public class HeroSkill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public HeroSkillType Type { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public float Cooldown { get; set; }
        public float CurrentCooldown { get; set; }
        public int Cost { get; set; } // 消耗资源
        public float EffectValue { get; set; } // 技能效果值
        public bool IsUnlocked { get; set; }
        
        public enum HeroSkillType { 
            Active,      // 主动技能
            Passive      // 被动技能
        }
        
        public HeroSkill()
        {
            Name = "未知技能";
            Description = "一个未定义的技能";
            Type = HeroSkillType.Passive;
            Level = 1;
            MaxLevel = 5;
            Cooldown = 10f;
            CurrentCooldown = 0f;
            Cost = 10;
            EffectValue = 1.0f;
            IsUnlocked = false;
        }
        
        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        public bool IsAvailable()
        {
            return IsUnlocked && CurrentCooldown <= 0;
        }
        
        /// <summary>
        /// 使用技能
        /// </summary>
        public void Use()
        {
            if (IsAvailable() && Type == HeroSkillType.Active)
            {
                CurrentCooldown = Cooldown;
            }
        }
        
        /// <summary>
        /// 升级技能
        /// </summary>
        public bool Upgrade()
        {
            if (Level < MaxLevel)
            {
                Level++;
                EffectValue *= 1.2f; // 每级提升20%
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 更新技能冷却
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown -= deltaTime;
                if (CurrentCooldown < 0)
                {
                    CurrentCooldown = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// KuKu图鉴条目结构
    /// </summary>
    [Serializable]
    public struct KukuPokedexEntry
    {
        public int Id;
        public string Name;
        public MythicalKukuData.MythicalRarity Rarity;
        public string Element;
        public bool IsDiscovered; // 是否已被发现
        public bool IsCaptured;   // 是否已被捕获
        public int TimesEncountered; // 遭遇次数
        public int TimesCaptured;    // 捕获次数
        public DateTime FirstDiscoveryDate; // 首次发现日期

        public KukuPokedexEntry(int id, string name, MythicalKukuData.MythicalRarity rarity, string element)
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
}