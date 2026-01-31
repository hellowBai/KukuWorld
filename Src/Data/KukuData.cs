using System;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 基础KuKu数据结构
    /// </summary>
    [Serializable]
    public class KukuData
    {
        // 基础信息
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // 稀有度
        public RarityType Rarity { get; set; }
        public enum RarityType { Common = 0, Rare = 1, Epic = 2, Legendary = 3, Mythic = 4 }

        // 战斗属性
        public float AttackPower { get; set; }
        public float DefensePower { get; set; }
        public float Speed { get; set; }
        public float Health { get; set; }

        // 视觉相关
        public string SpriteName { get; set; }
        public Color Tint { get; set; }

        // 状态
        public bool IsCollected { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }

        // 技能相关
        public string SkillName { get; set; }
        public float SkillDamage { get; set; }
        public float SkillCooldown { get; set; }

        // 捕捉相关
        public float CaptureDifficulty { get; set; }
        public bool CanAbsorbSoul { get; set; }
        public float SoulAbsorptionRate { get; set; }

        // 构造函数
        public KukuData()
        {
            Name = "KuKu";
            Description = "一只可爱的KuKu";
            Rarity = RarityType.Common;
            AttackPower = 10f;
            DefensePower = 5f;
            Speed = 1f;
            Health = 50f;
            Tint = Color.white;
            IsCollected = false;
            Level = 1;
            Experience = 0f;
            SkillName = "无技能";
            SkillDamage = 0f;
            SkillCooldown = 5f;
            CaptureDifficulty = 1.0f;
            CanAbsorbSoul = false;
            SoulAbsorptionRate = 0.1f;
            SpriteName = "DefaultKukuSprite";
        }

        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public Color GetRarityColor()
        {
            switch (Rarity)
            {
                case RarityType.Common:
                    return Color.white;
                case RarityType.Rare:
                    return Color.blue;
                case RarityType.Epic:
                    return Color.magenta;
                case RarityType.Legendary:
                    return Color.yellow;
                case RarityType.Mythic:
                    return Color.red;
                default:
                    return Color.gray;
            }
        }

        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public string GetRarityName()
        {
            switch (Rarity)
            {
                case RarityType.Common:
                    return "普通";
                case RarityType.Rare:
                    return "稀有";
                case RarityType.Epic:
                    return "史诗";
                case RarityType.Legendary:
                    return "传说";
                case RarityType.Mythic:
                    return "神话";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 升级KuKu
        /// </summary>
        public void LevelUp()
        {
            Level++;
            // 提升基础属性
            AttackPower *= 1.1f;
            DefensePower *= 1.1f;
            Health *= 1.1f;
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public bool AddExperience(float exp)
        {
            Experience += exp;
            // 假设每100经验升一级
            while (Experience >= 100)
            {
                Experience -= 100;
                LevelUp();
            }
            return true;
        }

        /// <summary>
        /// 吸收灵魂进行进化
        /// </summary>
        public bool AbsorbSoul(float soulPower)
        {
            if (!CanAbsorbSoul || soulPower <= 0)
                return false;

            // 根据灵魂吸收率计算实际吸收量
            float absorbed = soulPower * SoulAbsorptionRate;
            
            // 吸收的灵魂可能用于提升属性或进化
            Health += absorbed * 2f;
            AttackPower += absorbed * 0.5f;
            DefensePower += absorbed * 0.3f;

            return true;
        }

        /// <summary>
        /// 获取捕捉成功率
        /// </summary>
        public float GetCaptureSuccessRate(float playerPower)
        {
            // 根据捕捉难度和玩家实力计算成功率
            float baseRate = 1.0f - CaptureDifficulty;
            float playerAdvantage = Mathf.Clamp(playerPower / (playerPower + AttackPower), 0.1f, 1.0f);
            
            return Mathf.Clamp01(baseRate * playerAdvantage);
        }

        /// <summary>
        /// 复制当前KuKu数据
        /// </summary>
        public KukuData Clone()
        {
            return new KukuData
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Rarity = this.Rarity,
                AttackPower = this.AttackPower,
                DefensePower = this.DefensePower,
                Speed = this.Speed,
                Health = this.Health,
                SpriteName = this.SpriteName,
                Tint = this.Tint,
                IsCollected = this.IsCollected,
                Level = this.Level,
                Experience = this.Experience,
                SkillName = this.SkillName,
                SkillDamage = this.SkillDamage,
                SkillCooldown = this.SkillCooldown,
                CaptureDifficulty = this.CaptureDifficulty,
                CanAbsorbSoul = this.CanAbsorbSoul,
                SoulAbsorptionRate = this.SoulAbsorptionRate
            };
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [{GetRarityName()}] - Lv.{Level}";
        }
    }
}