using UnityEngine;

// Unity序列化的武器数据类
[System.Serializable]
public class WeaponData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public WeaponType WeaponType { get; set; }
    public float Power { get; set; }
    public int Price { get; set; }
    public RarityType Rarity { get; set; }
    public bool IsConsumable { get; set; }
    public int MaxStackSize { get; set; }
    public string SpriteName { get; set; }
    public Color Tint { get; set; }
    
    public WeaponData()
    {
        Tint = Color.white;
        IsConsumable = false;
        MaxStackSize = 1;
        Price = 100;
        Power = 10f;
        Rarity = RarityType.Common;
    }
    
    public WeaponData Clone()
    {
        return new WeaponData
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            WeaponType = this.WeaponType,
            Power = this.Power,
            Price = this.Price,
            Rarity = this.Rarity,
            IsConsumable = this.IsConsumable,
            MaxStackSize = this.MaxStackSize,
            SpriteName = this.SpriteName,
            Tint = this.Tint
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
            default: return Color.gray;
        }
    }
}

public enum WeaponType
{
    Sword,           // 剑
    Axe,             // 斧头
    Bow,             // 弓
    Staff,           // 法杖
    Shield,          // 盾
    HealingPotion,   // 治疗药水
    SoulStone,       // 灵石头
    ExpBoost,        // 经验药水
    CoinBag,         // 金币袋
    Special          // 特殊物品
}

public enum RarityType
{
    Common,      // 普通
    Rare,        // 稀有
    Epic,        // 史诗
    Legendary    // 传说
}