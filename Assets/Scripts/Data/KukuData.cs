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
        public enum RarityType { Common, Rare, Epic, Legendary, Mythic }

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

        public KukuData()
        {
            // 设置默认值
            Id = UnityEngine.Random.Range(1, 9999);
            Name = "KuKu";
            Description = "一个神秘的KuKu";
            Rarity = RarityType.Common;
            AttackPower = 10f;
            DefensePower = 5f;
            Speed = 1f;
            Health = 50f;
            SpriteName = "kuku_default";
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

        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public virtual Color GetRarityColor()
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
                    return Color.white;
            }
        }

        /// <summary>
        /// 升级KuKu
        /// </summary>
        public virtual void LevelUp()
        {
            Level++;
            // 属性随等级提升
            Health += 10f;
            AttackPower += 2f;
            DefensePower += 1f;
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public virtual bool AddExperience(float exp)
        {
            Experience += exp;
            // 每100经验升一级
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
        public virtual bool AbsorbSoul(float soulPower)
        {
            if (!CanAbsorbSoul) return false;
            
            // 灵魂吸收逻辑
            float absorption = soulPower * SoulAbsorptionRate;
            AddExperience(absorption * 10); // 转化为经验值
            return true;
        }

        /// <summary>
        /// 获取捕捉成功率
        /// </summary>
        public virtual float GetCaptureSuccessRate(float playerPower)
        {
            // 计算捕捉成功率，基于玩家实力和捕捉难度
            float baseRate = 0.5f; // 基础成功率
            float powerFactor = playerPower / (playerPower + AttackPower * CaptureDifficulty);
            return baseRate * powerFactor;
        }

        /// <summary>
        /// 克隆KuKu数据
        /// </summary>
        public virtual KukuData Clone()
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
    }
}