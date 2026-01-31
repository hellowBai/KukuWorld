using UnityEngine;
using System;

// Unity序列化的KuKu数据类
[System.Serializable]
public class KukuData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RarityType Rarity { get; set; }
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
    
    public enum RarityType
    {
        Common,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
    
    public KukuData()
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
    }
    
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
    
    public string GetRarityName()
    {
        switch (Rarity)
        {
            case RarityType.Common: return "普通";
            case RarityType.Rare: return "稀有";
            case RarityType.Epic: return "史诗";
            case RarityType.Legendary: return "传说";
            case RarityType.Mythic: return "神话";
            default: return "未知";
        }
    }
    
    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case RarityType.Common: return Color.white;
            case RarityType.Rare: return Color.blue;
            case RarityType.Epic: return Color.magenta;
            case RarityType.Legendary: return Color.yellow;
            case RarityType.Mythic: return Color.red;
            default: return Color.gray;
        }
    }
    
    public int GetExpForNextLevel()
    {
        return Level * 100; // 简化经验计算
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
            AttackPower *= 1.1f;
            DefensePower *= 1.1f;
            Health *= 1.1f;
        }
    }
}