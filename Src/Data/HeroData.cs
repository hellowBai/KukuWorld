using System.Collections.Generic;

namespace KukuWorld.Data
{
    /// <summary>
    /// 英雄数据结构
    /// </summary>
    [System.Serializable]
    public class HeroData
    {
        public string HeroName { get; set; }                     // 英雄名称
        public HeroClass Class { get; set; }                     // 英雄职业
        public List<HeroSkill> Skills { get; set; }              // 技能列表

        public enum HeroClass 
        { 
            Warrior,     // 战士 - 擅长近战和防御
            Mage,        // 法师 - 擅长魔法攻击和辅助
            Ranger,      // 游侠 - 擅长远程攻击和捕捉
            Guardian     // 守护者 - 擅长守护和治疗
        }

        public enum SkillType 
        { 
            Active,      // 主动技能
            Passive      // 被动技能
        }

        // 构造函数
        public HeroData()
        {
            HeroName = "Unknown Hero";
            Class = HeroClass.Warrior;
            Skills = new List<HeroSkill>();
            
            // 根据职业初始化默认技能
            InitializeDefaultSkills();
        }

        // 根据职业初始化默认技能
        private void InitializeDefaultSkills()
        {
            switch (Class)
            {
                case HeroClass.Warrior:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "Shield Bash", 
                        Description = "Stun enemies with a shield strike", 
                        Type = SkillType.Active,
                        Cooldown = 5f,
                        Power = 10f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "Battle Cry", 
                        Description = "Increase defense of nearby units", 
                        Type = SkillType.Passive,
                        Power = 15f
                    });
                    break;
                    
                case HeroClass.Mage:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "Fireball", 
                        Description = "Launch a powerful fireball at enemies", 
                        Type = SkillType.Active,
                        Cooldown = 3f,
                        Power = 20f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "Mana Shield", 
                        Description = "Create a protective barrier", 
                        Type = SkillType.Passive,
                        Power = 25f
                    });
                    break;
                    
                case HeroClass.Ranger:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "Precision Shot", 
                        Description = "Deal critical damage to a target", 
                        Type = SkillType.Active,
                        Cooldown = 4f,
                        Power = 15f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "Trap Setting", 
                        Description = "Set traps to slow enemies", 
                        Type = SkillType.Passive,
                        Power = 10f
                    });
                    break;
                    
                case HeroClass.Guardian:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "Healing Light", 
                        Description = "Restore health to nearby allies", 
                        Type = SkillType.Active,
                        Cooldown = 6f,
                        Power = 25f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "Divine Protection", 
                        Description = "Grant temporary invulnerability", 
                        Type = SkillType.Passive,
                        Power = 30f
                    });
                    break;
            }
        }

        // 使用技能
        public bool UseSkill(int skillId)
        {
            HeroSkill skill = Skills.Find(s => s.Id == skillId);
            if (skill == null || skill.Type != HeroSkill.SkillType.Active) return false;
            
            // 这里可以添加技能使用的具体逻辑
            // 比如消耗资源、应用效果等
            return true;
        }

        // 升级技能
        public bool UpgradeSkill(int skillId)
        {
            HeroSkill skill = Skills.Find(s => s.Id == skillId);
            if (skill == null) return false;
            
            skill.Power *= 1.2f; // 技能威力提升20%
            skill.Cooldown *= 0.9f; // 冷却时间减少10%
            return true;
        }

        // 获取技能效果
        public float GetSkillEffect(int skillId)
        {
            HeroSkill skill = Skills.Find(s => s.Id == skillId);
            return skill != null ? skill.Power : 0f;
        }
    }

    /// <summary>
    /// 英雄技能数据结构
    /// </summary>
    [System.Serializable]
    public class HeroSkill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillType Type { get; set; }
        public float Cooldown { get; set; }
        public float Power { get; set; }

        public enum SkillType 
        { 
            Active,      // 主动技能
            Passive      // 被动技能
        }

        // 构造函数
        public HeroSkill()
        {
            Id = 0;
            Name = "Unknown Skill";
            Description = "An unknown skill";
            Type = SkillType.Active;
            Cooldown = 0f;
            Power = 0f;
        }
    }
}