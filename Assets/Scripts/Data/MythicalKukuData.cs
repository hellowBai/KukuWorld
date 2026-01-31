using UnityEngine;
using System;

// Unity序列化的神话KuKu数据类
[System.Serializable]
public class MythicalKukuData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public MythicalRarity Rarity { get; set; }
    public float AttackPower { get; set; }
    public float DefensePower { get; set; }
    public float Speed { get; set; }
    public float Health { get; set; }
    public string SpriteName { get; set; }
    public Color Tint { get; set; }
    public bool IsCollected { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string SkillName { get; set; }
    public float SkillDamage { get; set; }
    public float SkillCooldown { get; set; }
    public float CaptureDifficulty { get; set; }
    public bool CanAbsorbSoul { get; set; }
    public float SoulAbsorptionRate { get; set; }
    public float EvolutionStonesRequired { get; set; }
    
    // 神话属性
    public float DivinePower { get; set; }     // 神力
    public float ProtectionPower { get; set; } // 护体力量
    public float PurificationPower { get; set; } // 净化力量
    
    public enum MythicalRarity
    {
        Celestial,      // 天界
        Immortal,       // 仙人
        DivineBeast,    // 神兽
        Sacred,         // 圣者
        Primordial      // 元始
    }
    
    public MythicalKukuData()
    {
        Tint = Color.white;
        IsCollected = false;
        Level = 1;
        Experience = 0;
        AttackPower = 10f;
        DefensePower = 5f;
        Speed = 1f;
        Health = 50f;
        CaptureDifficulty = 1.0f;
        CanAbsorbSoul = false;
        SoulAbsorptionRate = 0.1f;
        EvolutionStonesRequired = 10f;
        
        // 神话属性
        DivinePower = 5f;
        ProtectionPower = 3f;
        PurificationPower = 2f;
    }
    
    public MythicalKukuData Clone()
    {
        return new MythicalKukuData
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
            SoulAbsorptionRate = this.SoulAbsorptionRate,
            EvolutionStonesRequired = this.EvolutionStonesRequired,
            DivinePower = this.DivinePower,
            ProtectionPower = this.ProtectionPower,
            PurificationPower = this.PurificationPower
        };
    }
    
    public string GetRarityName()
    {
        switch (Rarity)
        {
            case MythicalRarity.Celestial: return "天界";
            case MythicalRarity.Immortal: return "仙人";
            case MythicalRarity.DivineBeast: return "神兽";
            case MythicalRarity.Sacred: return "圣者";
            case MythicalRarity.Primordial: return "元始";
            default: return "未知";
        }
    }
    
    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case MythicalRarity.Celestial: return Color.blue;      // 天蓝色
            case MythicalRarity.Immortal: return Color.cyan;       // 青色
            case MythicalRarity.DivineBeast: return Color.magenta; // 洋红色
            case MythicalRarity.Sacred: return Color.yellow;      // 黄色
            case MythicalRarity.Primordial: return Color.red;     // 红色
            default: return Color.gray;
        }
    }
    
    public int GetExpForNextLevel()
    {
        return Level * 150; // 神话KuKu需要更多经验
    }
    
    public bool CanEvolve()
    {
        return Experience >= GetExpForNextLevel();
    }
    
    public void AddExperience(int exp)
    {
        Experience += exp;
        
        // 检查升级
        while (Experience >= GetExpForNextLevel() && GetExpForNextLevel() > 0)
        {
            LevelUp();
        }
    }
    
    void LevelUp()
    {
        if (Experience >= GetExpForNextLevel())
        {
            Experience -= GetExpForNextLevel();
            Level++;
            
            // 提升基础属性
            AttackPower *= 1.12f;
            DefensePower *= 1.12f;
            Health *= 1.12f;
            DivinePower *= 1.1f;
            ProtectionPower *= 1.1f;
            PurificationPower *= 1.1f;
        }
    }
    
    public static string GetRarityDescription(MythicalRarity rarity)
    {
        switch (rarity)
        {
            case MythicalRarity.Celestial:
                return "天界级别的存在，拥有神圣的力量";
            case MythicalRarity.Immortal:
                return "仙人级别的存在，具有不朽的能力";
            case MythicalRarity.DivineBeast:
                return "神兽级别的存在，拥有强大的自然力量";
            case MythicalRarity.Sacred:
                return "圣者级别的存在，具有净化一切邪恶的力量";
            case MythicalRarity.Primordial:
                return "元始级别的存在，创世之初的古老力量";
            default:
                return "未知级别的存在";
        }
    }
    
    public float GetTotalPower()
    {
        return AttackPower + DefensePower + DivinePower + ProtectionPower + PurificationPower;
    }
    
    public float GetCombatEffectiveness()
    {
        return (AttackPower * 0.3f) + (DefensePower * 0.2f) + (DivinePower * 0.2f) + 
               (ProtectionPower * 0.15f) + (PurificationPower * 0.15f);
    }
    
    public bool IsMythical()
    {
        return true; // 所有此类都是神话级别
    }
    
    public MythicalKukuData UpgradeRarity()
    {
        if ((int)Rarity < 4) // 不是最高稀有度
        {
            var upgraded = Clone();
            upgraded.Rarity = (MythicalRarity)((int)Rarity + 1);
            upgraded.AttackPower *= 1.5f;
            upgraded.DefensePower *= 1.5f;
            upgraded.Health *= 1.5f;
            upgraded.DivinePower *= 1.5f;
            upgraded.ProtectionPower *= 1.5f;
            upgraded.PurificationPower *= 1.5f;
            return upgraded;
        }
        return this; // 已经是最高稀有度
    }
}