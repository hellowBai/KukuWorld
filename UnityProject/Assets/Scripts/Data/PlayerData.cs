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
    
    public void AddWeapon(WeaponData weapon)
    {
        if (weapon != null)
        {
            EquippedWeapons.Add(weapon);
            Debug.Log($"获得武器: {weapon.Name}");
        }
    }
    
    public void AddBuilding(BuildingData building)
    {
        if (building != null)
        {
            BuiltBuildings.Add(building);
            Debug.Log($"建造建筑: {building.Name}");
        }
    }
    
    public void UnlockArea(string areaName)
    {
        if (!string.IsNullOrEmpty(areaName) && !UnlockedAreas.Contains(areaName))
        {
            UnlockedAreas.Add(areaName);
            Debug.Log($"解锁区域: {areaName}");
        }
    }
    
    public void CompleteAchievement(string achievementName)
    {
        if (!string.IsNullOrEmpty(achievementName) && !CompletedAchievements.Contains(achievementName))
        {
            CompletedAchievements.Add(achievementName);
            Debug.Log($"完成成就: {achievementName}");
        }
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
    
    public int GetTotalCapturesToday()
    {
        // 简化实现：返回今日捕获数
        // 实际应用中需要存储每日数据
        return TotalCaptures;
    }
    
    public int GetTotalSoulsCollected()
    {
        // 简化实现：返回总收集的灵魂数
        // 实际应用中需要跟踪收集历史
        return Mathf.CeilToInt(Souls);
    }
    
    public int GetTotalCoinsSpent()
    {
        // 简化实现：返回总花费的金币数
        // 实际应用中需要跟踪消费历史
        return 1000 - Coins; // 假设初始值为1000
    }
    
    public float GetCaptureSuccessRate()
    {
        // 简化实现：返回捕捉成功率
        // 实际应用中需要跟踪尝试次数
        return TotalCaptures > 0 ? 100f : 0f;
    }
    
    public void ResetDailyProgress()
    {
        // 重置每日进度
        Debug.Log("重置每日进度");
    }
    
    public void SaveProgress()
    {
        // 保存进度到PlayerPrefs或其他存储
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetInt("PlayerLevel", Level);
        PlayerPrefs.SetInt("PlayerCoins", Coins);
        PlayerPrefs.SetInt("PlayerGems", Gems);
        PlayerPrefs.SetFloat("PlayerSouls", Souls);
        PlayerPrefs.SetInt("PlayerExperience", Experience);
        PlayerPrefs.SetInt("TotalCaptures", TotalCaptures);
        PlayerPrefs.SetInt("TotalDefeats", TotalDefeats);
        PlayerPrefs.SetInt("HighestWave", HighestWave);
        
        PlayerPrefs.Save();
        
        Debug.Log("玩家进度已保存");
    }
    
    public void LoadProgress()
    {
        // 从PlayerPrefs或其他存储加载进度
        PlayerName = PlayerPrefs.GetString("PlayerName", "KuKu Hunter");
        Level = PlayerPrefs.GetInt("PlayerLevel", 1);
        Coins = PlayerPrefs.GetInt("PlayerCoins", 1000);
        Gems = PlayerPrefs.GetInt("PlayerGems", 100);
        Souls = PlayerPrefs.GetFloat("PlayerSouls", 0f);
        Experience = PlayerPrefs.GetInt("PlayerExperience", 0);
        TotalCaptures = PlayerPrefs.GetInt("TotalCaptures", 0);
        TotalDefeats = PlayerPrefs.GetInt("TotalDefeats", 0);
        HighestWave = PlayerPrefs.GetInt("HighestWave", 0);
        
        Debug.Log("玩家进度已加载");
    }
    
    public void ResetProgress()
    {
        // 重置所有进度
        PlayerName = "KuKu Hunter";
        Level = 1;
        Coins = 1000;
        Gems = 100;
        Souls = 0f;
        Experience = 0;
        TotalCaptures = 0;
        TotalDefeats = 0;
        HighestWave = 0;
        
        CollectedKukus.Clear();
        EquippedWeapons.Clear();
        BuiltBuildings.Clear();
        UnlockedAreas.Clear();
        CompletedAchievements.Clear();
        
        Debug.Log("玩家进度已重置");
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