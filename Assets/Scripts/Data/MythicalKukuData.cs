using System;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 神话KuKu数据结构 - 继承自KukuData
    /// </summary>
    [Serializable]
    public class MythicalKukuData
    {
        // 基础信息
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // 神话背景
        public string MythologicalBackground { get; set; }
        public string Element { get; set; }
        
        // 神话技能系统
        public string SkillType { get; set; }
        public string SkillDescription { get; set; }
        public float SkillRange { get; set; }
        public float SkillPower { get; set; }
        
        // 战斗属性
        public float AttackPower { get; set; }
        public float DefensePower { get; set; }
        public float Speed { get; set; }
        public float Health { get; set; }
        
        // 神话属性加成
        public float DivinePower { get; set; }
        public float ProtectionPower { get; set; }
        public float PurificationPower { get; set; }
        
        // 稀有度
        public MythicalRarity Rarity { get; set; }
        public enum MythicalRarity { 
            Celestial = 0,     // 天界
            Immortal = 1,      // 仙人
            DivineBeast = 2,   // 神兽
            Sacred = 3,        // 圣者
            Primordial = 4     // 元始
        }
        
        // 进化相关
        public int EvolutionLevel { get; set; }
        public float EvolutionProgress { get; set; }
        public int EvolutionStonesRequired { get; set; }
        public bool CanAbsorbSoul { get; set; }
        public float SoulAbsorptionRate { get; set; }
        
        // 融合相关
        public bool CanFuseWithRobots { get; set; }
        public float FusionCompatibility { get; set; }
        
        // 状态和等级
        public int Level { get; set; }
        public float Experience { get; set; }
        
        // 视觉相关
        public string SpriteName { get; set; }
        
        // 装备相关
        public int MaxEquipmentSlots { get; set; }
        
        // 其他属性
        public bool IsFavorite { get; set; }
        public DateTime CaptureDate { get; set; }
        public string CaptureLocation { get; set; }
        public float CaptureDifficulty { get; set; }

        public MythicalKukuData()
        {
            // 初始化默认值
            Name = "神秘KuKu";
            Description = "一只神秘的KuKu";
            MythologicalBackground = "来自远古神话的神秘生物";
            Element = "None";
            SkillType = "None";
            SkillDescription = "无技能";
            SkillRange = 2f;
            SkillPower = 10f;
            AttackPower = 20f;
            DefensePower = 15f;
            Speed = 2f;
            Health = 80f;
            DivinePower = 5f;
            ProtectionPower = 3f;
            PurificationPower = 2f;
            Rarity = MythicalRarity.Celestial;
            EvolutionLevel = 1;
            EvolutionProgress = 0f;
            EvolutionStonesRequired = 10;
            CanAbsorbSoul = true;
            SoulAbsorptionRate = 0.5f;
            CanFuseWithRobots = false;
            FusionCompatibility = 0.5f;
            Level = 1;
            Experience = 0f;
            SpriteName = "DefaultSprite";
            MaxEquipmentSlots = 0;
            IsFavorite = false;
            CaptureDate = DateTime.Now;
            CaptureLocation = "Unknown";
            CaptureDifficulty = 1.0f;
        }
        
        /// <summary>
        /// 获取神话稀有度颜色
        /// </summary>
        public Color GetMythicalRarityColor()
        {
            switch (Rarity)
            {
                case MythicalRarity.Celestial:
                    return Color.blue;
                case MythicalRarity.Immortal:
                    return Color.cyan;
                case MythicalRarity.DivineBeast:
                    return Color.magenta;
                case MythicalRarity.Sacred:
                    return Color.yellow;
                case MythicalRarity.Primordial:
                    return Color.red;
                default:
                    return Color.gray;
            }
        }
        
        /// <summary>
        /// 获取神话稀有度名称
        /// </summary>
        public string GetMythicalRarityName()
        {
            switch (Rarity)
            {
                case MythicalRarity.Celestial:
                    return "天界";
                case MythicalRarity.Immortal:
                    return "仙人";
                case MythicalRarity.DivineBeast:
                    return "神兽";
                case MythicalRarity.Sacred:
                    return "圣者";
                case MythicalRarity.Primordial:
                    return "元始";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 检查是否可以与机器人融合
        /// </summary>
        public bool CanFuseWithRobot(UnitData robot)
        {
            // 需要达到最高等级（5级）且机器人也需要达到一定等级
            return EvolutionLevel >= 5 && robot != null && robot.Level >= 10 && CanFuseWithRobots;
        }
        
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [{GetMythicalRarityName()}] - Lv.{Level} Evol.{EvolutionLevel}";
        }
        
        /// <summary>
        /// 复制当前KuKu数据
        /// </summary>
        public MythicalKukuData Clone()
        {
            return new MythicalKukuData
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                MythologicalBackground = this.MythologicalBackground,
                Element = this.Element,
                SkillType = this.SkillType,
                SkillDescription = this.SkillDescription,
                SkillRange = this.SkillRange,
                SkillPower = this.SkillPower,
                AttackPower = this.AttackPower,
                DefensePower = this.DefensePower,
                Speed = this.Speed,
                Health = this.Health,
                DivinePower = this.DivinePower,
                ProtectionPower = this.ProtectionPower,
                PurificationPower = this.PurificationPower,
                Rarity = this.Rarity,
                EvolutionLevel = this.EvolutionLevel,
                EvolutionProgress = this.EvolutionProgress,
                EvolutionStonesRequired = this.EvolutionStonesRequired,
                CanAbsorbSoul = this.CanAbsorbSoul,
                SoulAbsorptionRate = this.SoulAbsorptionRate,
                CanFuseWithRobots = this.CanFuseWithRobots,
                FusionCompatibility = this.FusionCompatibility,
                Level = this.Level,
                Experience = this.Experience,
                SpriteName = this.SpriteName,
                MaxEquipmentSlots = this.MaxEquipmentSlots,
                IsFavorite = this.IsFavorite,
                CaptureDate = this.CaptureDate,
                CaptureLocation = this.CaptureLocation,
                CaptureDifficulty = this.CaptureDifficulty
            };
        }
    }
}