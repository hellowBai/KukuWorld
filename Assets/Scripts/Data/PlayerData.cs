using UnityEngine;
using System.Collections.Generic;

// Unity序列化的玩家数据类
[System.Serializable]
public class PlayerData
{
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public int Coins { get; set; }
    public int Gems { get; set; }
    public float Souls { get; set; }
    public int Experience { get; set; }
    public int TotalCaptures { get; set; }
    public int TotalDefeats { get; set; }
    public int HighestWave { get; set; }
    
    // 收集的KuKu列表
    public List<MythicalKukuData> CollectedKukus { get; set; }
    
    // 装备
    public List<WeaponData> EquippedWeapons { get; set; }
    public List<BuildingData> BuiltBuildings { get; set; }
    
    // 解锁内容
    public List<string> UnlockedAreas { get; set; }
    public List<string> CompletedAchievements { get; set; }
    
    public PlayerData()
    {
        PlayerName = "KuKu Hunter";
        Level = 1;
        Coins = 1000;
        Gems = 100;
        Souls = 0f;
        Experience = 0;
        TotalCaptures = 0;
        TotalDefeats = 0;
        HighestWave = 0;
        
        CollectedKukus = new List<MythicalKukuData>();
        EquippedWeapons = new List<WeaponData>();
        BuiltBuildings = new List<BuildingData>();
        UnlockedAreas = new List<string>();
        CompletedAchievements = new List<string>();
    }
    
    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            Coins += amount;
            Debug.Log($"获得 {amount} 金币，当前: {Coins}");
        }
    }
    
    public bool SpendCoins(int amount)
    {
        if (amount > 0 && Coins >= amount)
        {
            Coins -= amount;
            Debug.Log($"花费 {amount} 金币，剩余: {Coins}");
            return true;
        }
        return false;
    }
    
    public void AddGems(int amount)
    {
        if (amount > 0)
        {
            Gems += amount;
            Debug.Log($"获得 {amount} 神石，当前: {Gems}");
        }
    }
    
    public bool SpendGems(int amount)
    {
        if (amount > 0 && Gems >= amount)
        {
            Gems -= amount;
            Debug.Log($"花费 {amount} 神石，剩余: {Gems}");
            return true;
        }
        return false;
    }
    
    public void AddSouls(float amount)
    {
        if (amount > 0)
        {
            Souls += amount;
            Debug.Log($"获得 {amount:F1} 灵魂，当前: {Souls:F1}");
        }
    }
    
    public bool SpendSouls(float amount)
    {
        if (amount > 0 && Souls >= amount)
        {
            Souls -= amount;
            Debug.Log($"花费 {amount:F1} 灵魂，剩余: {Souls:F1}");
            return true;
        }
        return false;
    }
    
    public void AddExperience(int amount)
    {
        if (amount > 0)
        {
            Experience += amount;
            Debug.Log($"获得 {amount} 经验，当前: {Experience}");
            
            // 检查是否升级
            CheckLevelUp();
        }
    }
    
    void CheckLevelUp()
    {
        int expNeeded = Level * 1000; // 每级需要的经验
        if (Experience >= expNeeded)
        {
            Level++;
            Experience -= expNeeded;
            Debug.Log($"玩家升级到 {Level} 级！");
        }
    }
    
    public void AddKuku(MythicalKukuData kuku)
    {
        if (kuku != null)
        {
            // 检查是否已经拥有相同ID的KuKu
            if (!CollectedKukus.Exists(k => k.Id == kuku.Id))
            {
                CollectedKukus.Add(kuku);
                TotalCaptures++;
                Debug.Log($"新收集KuKu: {kuku.Name}，总共: {CollectedKukus.Count} 个");
            }
        }
    }
    
    public void RemoveKuku(int kukuId)
    {
        var kukuToRemove = CollectedKukus.Find(k => k.Id == kukuId);
        if (kukuToRemove != null)
        {
            CollectedKukus.Remove(kukuToRemove);
            Debug.Log($"移除KuKu: {kukuToRemove.Name}，剩余: {CollectedKukus.Count} 个");
        }
    }
    
    public MythicalKukuData GetKukuById(int id)
    {
        return CollectedKukus.Find(k => k.Id == id);
    }
    
    public List<MythicalKukuData> GetKukusByRarity(MythicalKukuData.MythicalRarity rarity)
    {
        return CollectedKukus.FindAll(k => k.Rarity == rarity);
    }
    
    public int GetKukuCountByRarity(MythicalKukuData.MythicalRarity rarity)
    {
        return CollectedKukus.FindAll(k => k.Rarity == rarity).Count;
    }
    
    public bool HasKuku(int kukuId)
    {
        return CollectedKukus.Exists(k => k.Id == kukuId);
    }
    
    public bool HasKukuOfType(MythicalKukuData.MythicalRarity rarity)
    {
        return CollectedKukus.Exists(k => k.Rarity == rarity);
    }
    
    public int GetTotalKukuPower()
    {
        int totalPower = 0;
        foreach (var kuku in CollectedKukus)
        {
            totalPower += Mathf.CeilToInt(kuku.GetTotalPower());
        }
        return totalPower;
    }
    
    public float GetAverageKukuLevel()
    {
        if (CollectedKukus.Count == 0) return 0f;
        
        float totalLevel = 0f;
        foreach (var kuku in CollectedKukus)
        {
            totalLevel += kuku.Level;
        }
        return totalLevel / CollectedKukus.Count;
    }
    
    public int GetTotalKukuCount()
    {
        return CollectedKukus.Count;
    }
    
    public int GetCommonKukuCount()
    {
        return GetKukuCountByRarity(MythicalKukuData.MythicalRarity.Celestial);
    }
    
    public int GetRareKukuCount()
    {
        return GetKukuCountByRarity(MythicalKukuData.MythicalRarity.Immortal);
    }
    
    public int GetEpicKukuCount()
    {
        return GetKukuCountByRarity(MythicalKukuData.MythicalRarity.DivineBeast);
    }
    
    public int GetLegendaryKukuCount()
    {
        return GetKukuCountByRarity(MythicalKukuData.MythicalRarity.Sacred);
    }
    
    public int GetMythicKukuCount()
    {
        return GetKukuCountByRarity(MythicalKukuData.MythicalRarity.Primordial);
    }
    
    public PlayerData Clone()
    {
        var clone = new PlayerData
        {
            PlayerName = this.PlayerName,
            Level = this.Level,
            Coins = this.Coins,
            Gems = this.Gems,
            Souls = this.Souls,
            Experience = this.Experience,
            TotalCaptures = this.TotalCaptures,
            TotalDefeats = this.TotalDefeats,
            HighestWave = this.HighestWave
        };
        
        // 深拷贝列表
        clone.CollectedKukus = new List<MythicalKukuData>();
        foreach (var kuku in this.CollectedKukus)
        {
            clone.CollectedKukus.Add(kuku.Clone());
        }
        
        clone.EquippedWeapons = new List<WeaponData>();
        foreach (var weapon in this.EquippedWeapons)
        {
            clone.EquippedWeapons.Add(weapon.Clone());
        }
        
        clone.BuiltBuildings = new List<BuildingData>();
        foreach (var building in this.BuiltBuildings)
        {
            clone.BuiltBuildings.Add(building.Clone());
        }
        
        clone.UnlockedAreas = new List<string>(this.UnlockedAreas);
        clone.CompletedAchievements = new List<string>(this.CompletedAchievements);
        
        return clone;
    }
}