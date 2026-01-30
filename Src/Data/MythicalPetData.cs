using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 神话宠物数据结构 - 继承自PetData
    /// </summary>
    [System.Serializable]
    public class MythicalPetData : PetData
    {
        // 神话背景
        public string MythologicalBackground { get; set; }    // 神话背景
        public string Element { get; set; }                   // 元素属性 (金木水火土)

        // 神话技能系统
        public MythicalSkillType SkillType { get; set; }      // 技能类型
        public string SkillDescription { get; set; }          // 技能描述
        public float SkillRange { get; set; }                 // 技能范围
        public float SkillPower { get; set; }                 // 技能威力

        public enum MythicalSkillType 
        { 
            GuardianShield, HealingAura, ElementalBlast, 
            SummonAssist, Purification, DivineProtection, 
            LifeLink, BarrierCreation 
        }

        // 神话属性加成
        public float DivinePower { get; set; }                // 神力
        public float ProtectionPower { get; set; }            // 守护力
        public float PurificationPower { get; set; }          // 净化力

        // 神话稀有度
        public new MythicalRarity Rarity { get; set; }        // 神话稀有度
        public enum MythicalRarity 
        { 
            Celestial, Immortal, DivineBeast, Sacred, Primordial 
        }

        // 进化相关
        public int EvolutionLevel { get; set; }               // 进化等级
        public float EvolutionProgress { get; set; }          // 进化进度
        public int EvolutionStonesRequired { get; set; }      // 进化所需神石

        // 融合相关
        public bool CanFuseWithRobots { get; set; }           // 是否可与机器人融合
        public float FusionCompatibility { get; set; }        // 融合兼容性

        // 构造函数
        public MythicalPetData() : base()
        {
            MythologicalBackground = "A legendary creature from ancient myths";
            Element = "Fire"; // 默认元素
            SkillType = MythicalSkillType.GuardianShield;
            SkillDescription = "Creates a protective shield around allies";
            SkillRange = 3f;
            SkillPower = 10f;
            DivinePower = 10f;
            ProtectionPower = 5f;
            PurificationPower = 3f;
            Rarity = MythicalRarity.Celestial;
            EvolutionLevel = 1;
            EvolutionProgress = 0f;
            EvolutionStonesRequired = 10;
            CanFuseWithRobots = false;
            FusionCompatibility = 0.5f;
        }

        // 获取神话稀有度颜色
        public Color GetMythicalRarityColor()
        {
            switch (Rarity)
            {
                case MythicalRarity.Celestial: return Color.white;
                case MythicalRarity.Immortal: return new Color(0.8f, 0.6f, 1.0f); // 紫色
                case MythicalRarity.DivineBeast: return new Color(1.0f, 0.7f, 0.0f); // 橙色
                case MythicalRarity.Sacred: return new Color(1.0f, 0.9f, 0.0f); // 金色
                case MythicalRarity.Primordial: return new Color(0.0f, 1.0f, 1.0f); // 青色
                default: return Color.white;
            }
        }

        // 重写升级方法，增加神话属性
        public override void LevelUp()
        {
            base.LevelUp(); // 调用父类的升级方法
            // 增加神话属性
            DivinePower *= 1.05f;
            ProtectionPower *= 1.05f;
            PurificationPower *= 1.05f;
        }

        // 重写添加经验值方法
        public override bool AddExperience(float exp)
        {
            bool levelUp = base.AddExperience(exp);
            if (levelUp)
            {
                // 检查是否达到进化条件
                if (Level % 10 == 0) // 每10级进化一次
                {
                    EvolutionLevel++;
                    EvolutionProgress = 0;
                }
            }
            return levelUp;
        }

        // 重写吸收灵魂方法
        public override bool AbsorbSoul(float soulPower)
        {
            if (!CanAbsorbSoul) return false;

            // 计算进化成功率（基于灵魂强度和当前宠物稀有度）
            float successRate = soulPower * SoulAbsorptionRate / (int)Rarity;
            
            if (Random.value < successRate)
            {
                // 成功进化，提升稀有度
                if ((int)Rarity < System.Enum.GetValues(typeof(MythicalRarity)).Length - 1)
                {
                    Rarity = (MythicalRarity)((int)Rarity + 1);
                    EvolutionProgress += 0.2f; // 进化进度增加
                    return true;
                }
            }
            return false;
        }

        // 检查是否可以与机器人融合
        public bool CanFuseWithRobot(UnitData robot)
        {
            if (!CanFuseWithRobots) return false;
            if (EvolutionLevel < 5) return false; // 需要达到一定进化等级
            if (robot == null) return false;
            
            // 检查融合兼容性
            return FusionCompatibility > 0.3f;
        }

        // 执行与机器人的融合
        public object FuseWithRobot(UnitData robot)
        {
            if (!CanFuseWithRobot(robot)) return null;
            
            // 创建融合后的单位
            UnitData fusedUnit = new UnitData();
            fusedUnit.Type = UnitData.UnitType.PetHybrid;
            fusedUnit.Name = $"{this.Name}-{robot.Name} Hybrid";
            fusedUnit.Description = $"A fusion of {this.Name} and {robot.Name}";
            
            // 合并属性
            fusedUnit.AttackPower = (this.AttackPower + robot.AttackPower) * 1.2f; // 20%加成
            fusedUnit.DefensePower = (this.DefensePower + robot.DefensePower) * 1.2f;
            fusedUnit.Speed = (this.Speed + robot.Speed) / 2;
            fusedUnit.Health = (this.Health + robot.Health) * 1.1f;
            fusedUnit.Range = Mathf.Max(this.SkillRange, robot.Range);
            
            // 融合后的单位可装备6件装备
            fusedUnit.MaxEquipmentSlots = 6;
            fusedUnit.IsFinalForm = true;
            
            return fusedUnit;
        }
    }
}