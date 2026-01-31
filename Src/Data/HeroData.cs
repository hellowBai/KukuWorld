using System;
using System.Collections.Generic;
using UnityEngine;

namespace KukuWorld.Data
{
    /// <summary>
    /// 英雄数据结构
    /// </summary>
    [Serializable]
    public class HeroData
    {
        // 基础信息
        public string HeroName { get; set; }
        public HeroClass Class { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Mana { get; set; }
        public float MaxMana { get; set; }

        // 属性
        public float AttackPower { get; set; }
        public float DefensePower { get; set; }
        public float Speed { get; set; }
        public float CriticalChance { get; set; }

        // 技能
        public List<HeroSkill> Skills { get; set; }

        // 职业枚举
        public enum HeroClass 
        { 
            Warrior,     // 战士 - 擅长近战和防御
            Mage,        // 法师 - 擅长魔法攻击和辅助
            Ranger,      // 游侠 - 擅长远程攻击和捕捉
            Guardian     // 守护者 - 擅长守护和治疗
        }

        // 构造函数
        public HeroData()
        {
            HeroName = "英雄";
            Class = HeroClass.Warrior;
            Level = 1;
            Experience = 0;
            MaxHealth = 100;
            Health = MaxHealth;
            MaxMana = 50;
            Mana = MaxMana;
            AttackPower = 20;
            DefensePower = 15;
            Speed = 3;
            CriticalChance = 0.05f; // 5%暴击率
            Skills = new List<HeroSkill>();
            
            // 根据职业初始化基础属性
            InitializeStatsByClass();
            
            // 添加基础技能
            InitializeBasicSkills();
        }

        /// <summary>
        /// 根据职业初始化基础属性
        /// </summary>
        private void InitializeStatsByClass()
        {
            switch (Class)
            {
                case HeroClass.Warrior:
                    MaxHealth = 150;
                    Health = MaxHealth;
                    AttackPower = 25;
                    DefensePower = 20;
                    Speed = 2.5f;
                    break;
                case HeroClass.Mage:
                    MaxHealth = 80;
                    Health = MaxHealth;
                    MaxMana = 100;
                    Mana = MaxMana;
                    AttackPower = 30;
                    DefensePower = 10;
                    Speed = 3.0f;
                    break;
                case HeroClass.Ranger:
                    MaxHealth = 100;
                    Health = MaxHealth;
                    AttackPower = 22;
                    DefensePower = 12;
                    Speed = 3.5f;
                    CriticalChance = 0.15f; // 游侠有更高暴击率
                    break;
                case HeroClass.Guardian:
                    MaxHealth = 200;
                    Health = MaxHealth;
                    AttackPower = 15;
                    DefensePower = 30;
                    Speed = 2.0f;
                    break;
            }
        }

        /// <summary>
        /// 初始化基础技能
        /// </summary>
        private void InitializeBasicSkills()
        {
            switch (Class)
            {
                case HeroClass.Warrior:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "英勇打击", 
                        Description = "对单个敌人造成150%攻击力的伤害", 
                        SkillType = HeroSkill.SkillTypeEnum.Active,
                        Cooldown = 2f,
                        Cost = 10,
                        Power = 1.5f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "坚韧", 
                        Description = "提升自身防御力50%，持续5秒", 
                        SkillType = HeroSkill.SkillTypeEnum.Passive,
                        Cooldown = 0,
                        Cost = 0,
                        Power = 0.5f
                    });
                    break;
                case HeroClass.Mage:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "火焰冲击", 
                        Description = "对范围内敌人造成120%攻击力的魔法伤害", 
                        SkillType = HeroSkill.SkillTypeEnum.Active,
                        Cooldown = 3f,
                        Cost = 20,
                        Power = 1.2f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "智慧", 
                        Description = "提升自身法术强度30%", 
                        SkillType = HeroSkill.SkillTypeEnum.Passive,
                        Cooldown = 0,
                        Cost = 0,
                        Power = 0.3f
                    });
                    break;
                case HeroClass.Ranger:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "精准射击", 
                        Description = "对单个敌人造成200%攻击力的伤害，必定暴击", 
                        SkillType = HeroSkill.SkillTypeEnum.Active,
                        Cooldown = 4f,
                        Cost = 15,
                        Power = 2.0f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "狩猎本能", 
                        Description = "提升自身暴击率20%", 
                        SkillType = HeroSkill.SkillTypeEnum.Passive,
                        Cooldown = 0,
                        Cost = 0,
                        Power = 0.2f
                    });
                    break;
                case HeroClass.Guardian:
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 1, 
                        Name = "守护之盾", 
                        Description = "为自己和附近友军施加护盾，吸收伤害", 
                        SkillType = HeroSkill.SkillTypeEnum.Active,
                        Cooldown = 5f,
                        Cost = 25,
                        Power = 50f
                    });
                    Skills.Add(new HeroSkill 
                    { 
                        Id = 2, 
                        Name = "坚韧意志", 
                        Description = "提升自身和附近友军防御力40%", 
                        SkillType = HeroSkill.SkillTypeEnum.Passive,
                        Cooldown = 0,
                        Cost = 0,
                        Power = 0.4f
                    });
                    break;
            }
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        public bool UseSkill(int skillId)
        {
            HeroSkill skill = Skills.Find(s => s.Id == skillId);
            
            if (skill == null || !skill.IsUsable())
                return false;

            // 消耗法力值
            Mana -= skill.Cost;
            
            // 更新冷却时间
            skill.StartCooldown();

            return true;
        }

        /// <summary>
        /// 升级技能
        /// </summary>
        public bool UpgradeSkill(int skillId)
        {
            HeroSkill skill = Skills.Find(s => s.Id == skillId);
            
            if (skill == null)
                return false;

            return skill.Upgrade();
        }

        /// <summary>
        /// 获取技能效果
        /// </summary>
        public float GetSkillEffect(int skillId)
        {
            HeroSkill skill = Skills.Find(s => s.Id == skillId);
            
            if (skill == null)
                return 0f;

            return skill.GetEffectivePower();
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public bool AddExperience(float exp)
        {
            Experience += exp;
            
            // 检查是否升级
            while (Experience >= GetExpForNextLevel())
            {
                Experience -= GetExpForNextLevel();
                LevelUp();
            }
            
            return true;
        }

        /// <summary>
        /// 升级英雄
        /// </summary>
        public void LevelUp()
        {
            Level++;
            
            // 提升基础属性
            MaxHealth += 15;
            Health = MaxHealth;
            MaxMana += 5;
            Mana = MaxMana;
            AttackPower += 3;
            DefensePower += 2;
            Speed += 0.1f;
            
            Debug.Log($"{HeroName} 升级到 {Level} 级!");
        }

        /// <summary>
        /// 获取下一级所需经验值
        /// </summary>
        public int GetExpForNextLevel()
        {
            // 经验值需求随等级增长
            return 100 + (Level - 1) * 50;
        }

        /// <summary>
        /// 恢复生命值
        /// </summary>
        public void RestoreHealth(float amount)
        {
            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        /// <summary>
        /// 恢复法力值
        /// </summary>
        public void RestoreMana(float amount)
        {
            Mana = Mathf.Min(MaxMana, Mana + amount);
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(1, damage - DefensePower * 0.5f); // 防御减免
            Health = Mathf.Max(0, Health - actualDamage);
        }

        /// <summary>
        /// 检查是否存活
        /// </summary>
        public bool IsAlive()
        {
            return Health > 0;
        }

        /// <summary>
        /// 重置冷却时间
        /// </summary>
        public void ResetSkillCooldowns()
        {
            foreach (var skill in Skills)
            {
                skill.ResetCooldown();
            }
        }

        /// <summary>
        /// 获取职业名称
        /// </summary>
        public string GetClassName()
        {
            switch (Class)
            {
                case HeroClass.Warrior: return "战士";
                case HeroClass.Mage: return "法师";
                case HeroClass.Ranger: return "游侠";
                case HeroClass.Guardian: return "守护者";
                default: return "未知";
            }
        }

        /// <summary>
        /// 复制英雄数据
        /// </summary>
        public HeroData Clone()
        {
            HeroData clone = new HeroData
            {
                HeroName = this.HeroName,
                Class = this.Class,
                Level = this.Level,
                Experience = this.Experience,
                Health = this.Health,
                MaxHealth = this.MaxHealth,
                Mana = this.Mana,
                MaxMana = this.MaxMana,
                AttackPower = this.AttackPower,
                DefensePower = this.DefensePower,
                Speed = this.Speed,
                CriticalChance = this.CriticalChance,
                Skills = new List<HeroSkill>()
            };

            // 复制技能列表
            foreach (var skill in this.Skills)
            {
                clone.Skills.Add(skill.Clone());
            }

            return clone;
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        public override string ToString()
        {
            return $"{HeroName} [{GetClassName()}] - Lv.{Level} HP:{Health:F0}/{MaxHealth:F0}";
        }
    }

    /// <summary>
    /// 英雄技能数据结构
    /// </summary>
    [Serializable]
    public class HeroSkill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillTypeEnum SkillType { get; set; }
        public float Cooldown { get; set; }
        public float CurrentCooldown { get; set; }
        public int Cost { get; set; } // 法力值消耗
        public float Power { get; set; } // 技能威力/效果
        public int MaxLevel { get; set; } = 5; // 最大等级
        public int CurrentLevel { get; set; } = 1; // 当前等级

        public enum SkillTypeEnum { Active, Passive }

        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        public bool IsUsable()
        {
            if (SkillType == SkillTypeEnum.Passive)
                return true; // 被动技能始终可用

            return CurrentCooldown <= 0;
        }

        /// <summary>
        /// 开始冷却
        /// </summary>
        public void StartCooldown()
        {
            if (SkillType == SkillTypeEnum.Active)
            {
                CurrentCooldown = Cooldown / GetCooldownReduction(); // 考虑冷却缩减
            }
        }

        /// <summary>
        /// 重置冷却
        /// </summary>
        public void ResetCooldown()
        {
            CurrentCooldown = 0;
        }

        /// <summary>
        /// 更新冷却时间
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown = Mathf.Max(0, CurrentCooldown - deltaTime);
            }
        }

        /// <summary>
        /// 获取冷却缩减（随等级提升）
        /// </summary>
        private float GetCooldownReduction()
        {
            // 每级减少5%冷却时间
            return 1.0f - (CurrentLevel - 1) * 0.05f;
        }

        /// <summary>
        /// 获取有效威力（随等级提升）
        /// </summary>
        public float GetEffectivePower()
        {
            // 每级提升10%威力
            return Power * (1.0f + (CurrentLevel - 1) * 0.1f);
        }

        /// <summary>
        /// 升级技能
        /// </summary>
        public bool Upgrade()
        {
            if (CurrentLevel >= MaxLevel)
                return false;

            CurrentLevel++;
            return true;
        }

        /// <summary>
        /// 获取升级所需资源（示例）
        /// </summary>
        public int GetUpgradeCost()
        {
            return 50 * CurrentLevel; // 每级递增
        }

        /// <summary>
        /// 复制技能数据
        /// </summary>
        public HeroSkill Clone()
        {
            return new HeroSkill
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                SkillType = this.SkillType,
                Cooldown = this.Cooldown,
                CurrentCooldown = this.CurrentCooldown,
                Cost = this.Cost,
                Power = this.Power,
                MaxLevel = this.MaxLevel,
                CurrentLevel = this.CurrentLevel
            };
        }
    }
}