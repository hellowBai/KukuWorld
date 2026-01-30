using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 基础宠物数据结构
    /// </summary>
    [System.Serializable]
    public class PetData
    {
        // 基础信息
        public int Id { get; set; }                           // 宠物唯一标识
        public string Name { get; set; }                      // 宠物名称
        public string Description { get; set; }               // 宠物描述

        // 稀有度
        public virtual RarityType Rarity { get; set; }        // 稀有度枚举
        public enum RarityType { Common, Rare, Epic, Legendary, Mythic }

        // 战斗属性
        public float AttackPower { get; set; }                // 攻击力
        public float DefensePower { get; set; }               // 防御力
        public float Speed { get; set; }                      // 速度
        public float Health { get; set; }                     // 生命值

        // 视觉相关
        public string SpriteName { get; set; }                // 精灵名称
        public Color Tint { get; set; }                       // 着色

        // 状态
        public bool IsCollected { get; set; }                 // 是否已收集（游戏内）
        public int Level { get; set; }                        // 等级
        public float Experience { get; set; }                 // 经验值

        // 技能相关
        public string SkillName { get; set; }                 // 技能名称
        public float SkillDamage { get; set; }                // 技能伤害
        public float SkillCooldown { get; set; }              // 技能冷却

        // 捕捉相关
        public float CaptureDifficulty { get; set; }          // 捕捉难度系数
        public bool CanAbsorbSoul { get; set; }              // 是否能吸收灵魂
        public float SoulAbsorptionRate { get; set; }         // 灵魂吸收率

        // 构造函数
        public PetData()
        {
            Id = 0;
            Name = "Unknown Pet";
            Description = "A mysterious pet";
            Rarity = RarityType.Common;
            AttackPower = 10f;
            DefensePower = 5f;
            Speed = 1f;
            Health = 50f;
            SpriteName = "default_pet";
            Tint = Color.white;
            IsCollected = false;
            Level = 1;
            Experience = 0f;
            SkillName = "Basic Attack";
            SkillDamage = 5f;
            SkillCooldown = 2f;
            CaptureDifficulty = 1.0f;
            CanAbsorbSoul = false;
            SoulAbsorptionRate = 0.1f;
        }

        // 获取稀有度颜色
        public virtual Color GetRarityColor()
        {
            switch (Rarity)
            {
                case RarityType.Common: return Color.gray;
                case RarityType.Rare: return Color.blue;
                case RarityType.Epic: return Color.magenta;
                case RarityType.Legendary: return Color.yellow;
                case RarityType.Mythic: return Color.cyan;
                default: return Color.white;
            }
        }

        // 升级宠物
        public virtual void LevelUp()
        {
            Level++;
            Experience = 0;
            // 增加属性
            AttackPower *= 1.1f;
            DefensePower *= 1.1f;
            Health *= 1.1f;
        }

        // 添加经验值
        public virtual bool AddExperience(float exp)
        {
            Experience += exp;
            float expForNextLevel = Level * 100f; // 每级需要的经验值
            
            if (Experience >= expForNextLevel)
            {
                LevelUp();
                return true; // 等级提升了
            }
            return false; // 等级未提升
        }

        // 吸收灵魂进行进化
        public virtual bool AbsorbSoul(float soulPower)
        {
            if (!CanAbsorbSoul) return false;
            
            // 计算进化成功率（基于灵魂强度和当前宠物稀有度）
            float successRate = soulPower * SoulAbsorptionRate / (int)Rarity;
            
            if (Random.value < successRate)
            {
                // 成功进化，提升稀有度
                if ((int)Rarity < System.Enum.GetValues(typeof(RarityType)).Length - 1)
                {
                    Rarity = (RarityType)((int)Rarity + 1);
                    return true;
                }
            }
            return false;
        }

        // 获取捕捉成功率
        public virtual float GetCaptureSuccessRate(float playerPower)
        {
            // 基础成功率受宠物稀有度和生命值影响
            float baseRate = 1.0f - CaptureDifficulty;
            float healthFactor = 1.0f - (Health / 100f); // 生命值越低成功率越高
            float playerFactor = playerPower * 0.1f; // 玩家实力影响
            
            float finalRate = Mathf.Clamp01(baseRate + healthFactor + playerFactor);
            return finalRate;
        }
    }
}