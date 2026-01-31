using UnityEngine;

// Unity序列化的建筑数据类
[System.Serializable]
public class BuildingData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public BuildingType Type { get; set; }
    public int BuildCost { get; set; }
    public int UpgradeCost { get; set; }
    public float BuildTime { get; set; }
    public float UpgradeTime { get; set; }
    public float MaxHealth { get; set; }
    public int MaxLevel { get; set; }
    public int ProductionAmount { get; set; }
    public float ProductionInterval { get; set; }
    public string SpriteName { get; set; }
    public Color Tint { get; set; }
    
    public BuildingData()
    {
        Tint = Color.white;
        MaxHealth = 100f;
        MaxLevel = 10;
        BuildTime = 10f;
        UpgradeTime = 15f;
        ProductionInterval = 5f;
    }
    
    public BuildingData Clone()
    {
        return new BuildingData
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            Type = this.Type,
            BuildCost = this.BuildCost,
            UpgradeCost = this.UpgradeCost,
            BuildTime = this.BuildTime,
            UpgradeTime = this.UpgradeTime,
            MaxHealth = this.MaxHealth,
            MaxLevel = this.MaxLevel,
            ProductionAmount = this.ProductionAmount,
            ProductionInterval = this.ProductionInterval,
            SpriteName = this.SpriteName,
            Tint = this.Tint
        };
    }
}

public enum BuildingType
{
    ResourceGenerator,    // 资源生成器
    SoulHarvester,        // 灵魂收割者
    DefensiveTower,       // 防御塔
    TrainingCamp,         // 训练营
    EvolutionChamber      // 进化室
}