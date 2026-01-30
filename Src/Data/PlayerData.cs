using System.Collections.Generic;
using UnityEngine;

namespace PetCollector.Data
{
    /// <summary>
    /// 玩家数据结构
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        // 基础信息
        public string PlayerName { get; set; }                   // 玩家姓名
        public int Level { get; set; } = 1;                     // 玩家等级
        public float Experience { get; set; } = 0;              // 经验值

        // 英雄系统
        public HeroData Hero { get; set; }                       // 英雄数据
        public List<HeroSkill> AvailableSkills { get; set; }     // 可用技能

        // 资源
        public int Coins { get; set; } = 1000;                  // 金币
        public int Gems { get; set; } = 100;                    // 神石
        public float Souls { get; set; } = 0f;                  // 灵魂值

        // 宠物收集（游戏内实际拥有的宠物）
        public Dictionary<int, PetData> CollectedPets { get; set; } // 收集的宠物
        public List<int> ActivePetTeam { get; set; }            // 当前宠物队伍

        // 建筑
        public List<BuildingData> BuiltBuildings { get; set; }   // 已建造建筑
        public List<UnitData> DeployedUnits { get; set; }       // 已部署单位

        // 游戏阶段状态
        public bool IsInCapturePhase { get; set; } = true;      // 是否在捕捉阶段
        public float TimeInCapturePhase { get; set; } = 0f;     // 捕捉阶段时间
        public int EnemiesDefeatedInDefense { get; set; } = 0;  // 防守阶段击败敌人数量
        public int CurrentWaveInDefense { get; set; } = 0;      // 防守阶段当前波次

        // 装备
        public List<WeaponData> OwnedEquipment { get; set; }    // 拥有装备
        public Dictionary<int, List<WeaponData>> UnitEquipment { get; set; } // 单位装备映射

        // 构造函数
        public PlayerData()
        {
            PlayerName = "Player";
            Level = 1;
            Experience = 0;
            Hero = new HeroData();
            AvailableSkills = new List<HeroSkill>(Hero.Skills); // 复制英雄技能
            Coins = 1000;
            Gems = 100;
            Souls = 0f;
            CollectedPets = new Dictionary<int, PetData>();
            ActivePetTeam = new List<int>();
            BuiltBuildings = new List<BuildingData>();
            DeployedUnits = new List<UnitData>();
            IsInCapturePhase = true;
            TimeInCapturePhase = 0f;
            EnemiesDefeatedInDefense = 0;
            CurrentWaveInDefense = 0;
            OwnedEquipment = new List<WeaponData>();
            UnitEquipment = new Dictionary<int, List<WeaponData>>();
        }

        // 添加金币
        public void AddCoins(int amount)
        {
            Coins = Mathf.Max(0, Coins + amount);
        }

        // 添加神石
        public void AddGems(int amount)
        {
            Gems = Mathf.Max(0, Gems + amount);
        }

        // 添加灵魂
        public void AddSouls(float amount)
        {
            Souls = Mathf.Max(0, Souls + amount);
        }

        // 添加经验值
        public bool AddExperience(float exp)
        {
            Experience += exp;
            float expForNextLevel = Level * 500f; // 每级需要的经验值
            
            if (Experience >= expForNextLevel)
            {
                LevelUp();
                return true; // 等级提升了
            }
            return false; // 等级未提升
        }

        // 添加宠物到收藏（游戏内）
        public void AddPet(PetData pet)
        {
            if (pet != null)
            {
                CollectedPets[pet.Id] = pet;
                pet.IsCollected = true;
                
                // 如果宠物是神话宠物，也要更新到图鉴系统
                if (pet is MythicalPetData mythicalPet)
                {
                    NotifyPetUnlocked(pet.Id);
                }
            }
        }

        // 通知收集系统解锁宠物（游戏外）
        public void NotifyPetUnlocked(int petId)
        {
            // 这里应该通知PetCollectionManager解锁宠物
            // 由于我们不直接引用，可以通过事件或其他方式通知
            Debug.Log($"通知收集系统：宠物 {petId} 已解锁");
        }

        // 移除宠物
        public bool RemovePet(int petId)
        {
            if (CollectedPets.ContainsKey(petId))
            {
                CollectedPets.Remove(petId);
                
                // 如果宠物在活跃队伍中，也从队伍中移除
                if (ActivePetTeam.Contains(petId))
                {
                    ActivePetTeam.Remove(petId);
                }
                
                return true;
            }
            return false;
        }

        // 升级玩家等级
        public void LevelUp()
        {
            Level++;
            Experience = 0;
            
            // 升级奖励
            AddCoins(100 * Level);
            AddGems(Level);
        }

        // 检查是否有足够资源
        public bool HasResources(int coins, int gems)
        {
            return Coins >= coins && Gems >= gems;
        }

        // 消耗资源
        public bool ConsumeResources(int coins, int gems)
        {
            if (!HasResources(coins, gems)) return false;
            
            Coins -= coins;
            Gems -= gems;
            return true;
        }

        // 检查是否可以进入防守阶段
        public bool CanStartDefensePhase()
        {
            // 至少需要有一定数量的宠物才能开始防守
            return CollectedPets.Count > 0;
        }

        // 切换到防守阶段
        public void EnterDefensePhase()
        {
            IsInCapturePhase = false;
            TimeInCapturePhase = 0f;
        }

        // 获取玩家战斗力评分
        public float GetPlayerCombatRating()
        {
            float rating = 0f;
            
            // 计算宠物总战斗力
            foreach (var pet in CollectedPets.Values)
            {
                rating += pet.AttackPower + pet.DefensePower + pet.Health / 10f;
            }
            
            // 计算建筑总战斗力
            foreach (var building in BuiltBuildings)
            {
                rating += building.AttackPower * 2 + building.Health / 10f;
            }
            
            // 计算单位总战斗力
            foreach (var unit in DeployedUnits)
            {
                rating += unit.AttackPower + unit.DefensePower + unit.Health / 10f;
            }
            
            // 加上玩家等级和资源的影响
            rating += Level * 50f + Coins / 10f + Gems * 20f;
            
            return rating;
        }

        // 添加已部署单位
        public void AddDeployedUnit(UnitData unit)
        {
            if (unit != null)
            {
                DeployedUnits.Add(unit);
                unit.IsDeployed = true;
            }
        }

        // 移除已部署单位
        public bool RemoveDeployedUnit(UnitData unit)
        {
            if (unit != null && DeployedUnits.Contains(unit))
            {
                DeployedUnits.Remove(unit);
                unit.IsDeployed = false;
                return true;
            }
            return false;
        }
    }
}