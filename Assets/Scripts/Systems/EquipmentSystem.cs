using UnityEngine;
using System.Collections.Generic;

public class EquipmentSystem : MonoBehaviour
{
    [Header("装备设置")]
    public int maxEquipmentSlots = 6; // 最多6件装备
    
    private GameManager gameManager;
    private PlayerData playerData;
    
    // 事件
    public System.Action<WeaponData> OnEquipmentEquipped;
    public System.Action<WeaponData> OnEquipmentUnequipped;
    public System.Action<string> OnEquipmentError;
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            playerData = gameManager.GetPlayerData();
        }
        
        Debug.Log("装备系统初始化完成");
    }
    
    /// <summary>
    /// 装备武器
    /// </summary>
    /// <param name="weaponId">武器ID</param>
    /// <param name="slotIndex">装备槽索引</param>
    /// <returns>是否成功装备</returns>
    public bool EquipWeapon(int weaponId, int slotIndex)
    {
        if (playerData == null)
        {
            Debug.LogError("玩家数据未加载");
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= maxEquipmentSlots)
        {
            Debug.LogError($"无效的装备槽索引: {slotIndex}");
            return false;
        }
        
        // 查找武器
        WeaponData weapon = FindWeaponInInventory(weaponId);
        if (weapon == null)
        {
            Debug.LogError($"未找到武器ID: {weaponId}");
            return false;
        }
        
        // 如果该槽位已有装备，先卸下
        if (slotIndex < playerData.EquippedWeapons.Count)
        {
            UnequipWeapon(slotIndex);
        }
        
        // 确保装备列表有足够的空间
        while (playerData.EquippedWeapons.Count <= slotIndex)
        {
            playerData.EquippedWeapons.Add(null);
        }
        
        // 装备武器
        playerData.EquippedWeapons[slotIndex] = weapon;
        
        // 从库存中移除（如果是消耗品）
        if (weapon.IsConsumable)
        {
            RemoveWeaponFromInventory(weaponId);
        }
        
        Debug.Log($"在槽位 {slotIndex} 装备了 {weapon.Name}");
        
        // 触发装备事件
        OnEquipmentEquipped?.Invoke(weapon);
        
        return true;
    }
    
    /// <summary>
    /// 卸下武器
    /// </summary>
    /// <param name="slotIndex">装备槽索引</param>
    /// <returns>被卸下的武器</returns>
    public WeaponData UnequipWeapon(int slotIndex)
    {
        if (playerData == null)
        {
            Debug.LogError("玩家数据未加载");
            return null;
        }
        
        if (slotIndex < 0 || slotIndex >= playerData.EquippedWeapons.Count)
        {
            Debug.LogError($"无效的装备槽索引: {slotIndex}");
            return null;
        }
        
        WeaponData unequippedWeapon = playerData.EquippedWeapons[slotIndex];
        
        if (unequippedWeapon != null)
        {
            // 将武器放回库存（如果不是消耗品）
            if (!unequippedWeapon.IsConsumable)
            {
                playerData.AddWeapon(unequippedWeapon);
            }
            
            // 清空槽位
            playerData.EquippedWeapons[slotIndex] = null;
            
            Debug.Log($"从槽位 {slotIndex} 卸下了 {unequippedWeapon.Name}");
            
            // 触发卸下事件
            OnEquipmentUnequipped?.Invoke(unequippedWeapon);
        }
        
        return unequippedWeapon;
    }
    
    /// <summary>
    /// 购买装备
    /// </summary>
    /// <param name="weaponType">武器类型</param>
    /// <returns>购买结果</returns>
    public PurchaseResult PurchaseEquipment(WeaponType weaponType)
    {
        if (playerData == null)
        {
            return new PurchaseResult(false, null, "玩家数据未加载");
        }
        
        // 创建武器
        WeaponData weapon = CreateWeapon(weaponType);
        
        // 检查价格
        if (weapon.Price > playerData.Coins)
        {
            return new PurchaseResult(false, null, $"金币不足，需要 {weapon.Price} 金币，当前 {playerData.Coins}");
        }
        
        // 扣除金币
        playerData.SpendCoins(weapon.Price);
        
        // 添加到库存
        playerData.AddWeapon(weapon);
        
        Debug.Log($"购买了装备: {weapon.Name}");
        
        return new PurchaseResult(true, weapon, $"成功购买 {weapon.Name}！");
    }
    
    /// <summary>
    /// 使用消耗品
    /// </summary>
    /// <param name="consumableId">消耗品ID</param>
    /// <returns>使用结果</returns>
    public bool UseConsumable(int consumableId)
    {
        if (playerData == null)
        {
            Debug.LogError("玩家数据未加载");
            return false;
        }
        
        WeaponData consumable = FindWeaponInInventory(consumableId);
        if (consumable == null || !consumable.IsConsumable)
        {
            Debug.LogError($"未找到消耗品ID: {consumableId}");
            return false;
        }
        
        // 执行消耗品效果
        ApplyConsumableEffect(consumable);
        
        // 从库存移除
        RemoveWeaponFromInventory(consumableId);
        
        Debug.Log($"使用了消耗品: {consumable.Name}");
        
        return true;
    }
    
    /// <summary>
    /// 获取可用的装备槽
    /// </summary>
    /// <returns>可用槽位索引列表</returns>
    public List<int> GetAvailableSlots()
    {
        List<int> availableSlots = new List<int>();
        
        for (int i = 0; i < maxEquipmentSlots; i++)
        {
            if (i >= playerData.EquippedWeapons.Count || playerData.EquippedWeapons[i] == null)
            {
                availableSlots.Add(i);
            }
        }
        
        return availableSlots;
    }
    
    /// <summary>
    /// 获取已装备的武器
    /// </summary>
    /// <returns>已装备的武器列表</returns>
    public List<WeaponData> GetEquippedWeapons()
    {
        if (playerData == null) return new List<WeaponData>();
        
        List<WeaponData> equipped = new List<WeaponData>();
        foreach (var weapon in playerData.EquippedWeapons)
        {
            if (weapon != null)
            {
                equipped.Add(weapon);
            }
        }
        
        return equipped;
    }
    
    /// <summary>
    /// 获取库存中的武器
    /// </summary>
    /// <returns>库存中的武器列表</returns>
    public List<WeaponData> GetInventoryWeapons()
    {
        if (playerData == null) return new List<WeaponData>();
        
        // 这里简单返回库存中的武器
        // 在实际实现中，可能需要单独的库存系统
        return new List<WeaponData>(playerData.EquippedWeapons.FindAll(w => w != null));
    }
    
    /// <summary>
    /// 应用消耗品效果
    /// </summary>
    private void ApplyConsumableEffect(WeaponData consumable)
    {
        switch (consumable.WeaponType)
        {
            case WeaponType.HealingPotion:
                // 恢复玩家生命值（在实际游戏中，这会影响战斗单位）
                if (gameManager != null)
                {
                    // 这里可以影响玩家的某种资源或单位
                    Debug.Log($"使用治疗药水，恢复生命值");
                }
                break;
                
            case WeaponType.SoulStone:
                // 增加灵魂
                playerData.AddSouls(consumable.Power * 10f);
                break;
                
            case WeaponType.ExpBoost:
                // 增加经验
                // 在实际游戏中，这可能影响KuKu的经验
                Debug.Log($"使用经验药水");
                break;
                
            case WeaponType.CoinBag:
                // 增加金币
                playerData.AddCoins(Mathf.CeilToInt(consumable.Power * 50f));
                break;
                
            default:
                Debug.Log($"使用了消耗品: {consumable.Name}");
                break;
        }
    }
    
    /// <summary>
    /// 查找库存中的武器
    /// </summary>
    private WeaponData FindWeaponInInventory(int weaponId)
    {
        // 在实际实现中，这里应该有一个独立的库存系统
        // 现在我们临时假设未装备的武器在库存中
        foreach (var weapon in GetInventoryWeapons())
        {
            if (weapon.Id == weaponId)
            {
                return weapon;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 从库存中移除武器
    /// </summary>
    private bool RemoveWeaponFromInventory(int weaponId)
    {
        // 在实际实现中，这里应该操作独立的库存系统
        // 现在我们只是简单地移除未装备的武器
        for (int i = 0; i < playerData.EquippedWeapons.Count; i++)
        {
            if (playerData.EquippedWeapons[i] != null && playerData.EquippedWeapons[i].Id == weaponId)
            {
                playerData.EquippedWeapons.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 创建武器
    /// </summary>
    private WeaponData CreateWeapon(WeaponType weaponType)
    {
        WeaponData weapon = new WeaponData();
        weapon.Id = Random.Range(100000, 999999);
        weapon.WeaponType = weaponType;
        
        switch (weaponType)
        {
            case WeaponType.Sword:
                weapon.Name = "钢铁剑";
                weapon.Description = "一把坚固的钢铁剑";
                weapon.Power = 50f;
                weapon.Price = 200;
                weapon.Rarity = RarityType.Common;
                break;
                
            case WeaponType.Axe:
                weapon.Name = "战斧";
                weapon.Description = "威力强大的战斧";
                weapon.Power = 70f;
                weapon.Price = 350;
                weapon.Rarity = RarityType.Rare;
                break;
                
            case WeaponType.Bow:
                weapon.Name = "精灵弓";
                weapon.Description = "精准的精灵长弓";
                weapon.Power = 45f;
                weapon.Price = 300;
                weapon.Rarity = RarityType.Rare;
                break;
                
            case WeaponType.Staff:
                weapon.Name = "法杖";
                weapon.Description = "蕴含魔法力量的法杖";
                weapon.Power = 60f;
                weapon.Price = 400;
                weapon.Rarity = RarityType.Epic;
                break;
                
            case WeaponType.Shield:
                weapon.Name = "钢铁盾";
                weapon.Description = "坚固的防护盾牌";
                weapon.Power = 30f; // 防御力
                weapon.Price = 250;
                weapon.Rarity = RarityType.Common;
                break;
                
            case WeaponType.HealingPotion:
                weapon.Name = "治疗药水";
                weapon.Description = "恢复生命值的药水";
                weapon.Power = 25f; // 治疗量
                weapon.Price = 50;
                weapon.Rarity = RarityType.Common;
                weapon.IsConsumable = true;
                break;
                
            case WeaponType.SoulStone:
                weapon.Name = "灵魂石";
                weapon.Description = "蕴含纯净灵魂的石头";
                weapon.Power = 5f; // 灵魂值
                weapon.Price = 150;
                weapon.Rarity = RarityType.Rare;
                weapon.IsConsumable = true;
                break;
                
            case WeaponType.ExpBoost:
                weapon.Name = "经验药水";
                weapon.Description = "快速提升经验的药水";
                weapon.Power = 10f; // 经验值
                weapon.Price = 100;
                weapon.Rarity = RarityType.Rare;
                weapon.IsConsumable = true;
                break;
                
            case WeaponType.CoinBag:
                weapon.Name = "金币袋";
                weapon.Description = "装满金币的钱袋";
                weapon.Power = 2f; // 金币倍数
                weapon.Price = 200;
                weapon.Rarity = RarityType.Epic;
                weapon.IsConsumable = true;
                break;
                
            default:
                weapon.Name = "未知武器";
                weapon.Description = "未知类型的武器";
                weapon.Power = 10f;
                weapon.Price = 100;
                weapon.Rarity = RarityType.Common;
                break;
        }
        
        return weapon;
    }
    
    /// <summary>
    /// 获取装备槽数量信息
    /// </summary>
    public (int usedSlots, int totalSlots) GetSlotInfo()
    {
        int used = 0;
        foreach (var weapon in playerData.EquippedWeapons)
        {
            if (weapon != null) used++;
        }
        return (used, maxEquipmentSlots);
    }
    
    /// <summary>
    /// 获取指定类型的装备
    /// </summary>
    public List<WeaponData> GetWeaponsByType(WeaponType weaponType)
    {
        List<WeaponData> weapons = new List<WeaponData>();
        
        foreach (var weapon in GetEquippedWeapons())
        {
            if (weapon.WeaponType == weaponType)
            {
                weapons.Add(weapon);
            }
        }
        
        return weapons;
    }
    
    /// <summary>
    /// 计算装备总加成
    /// </summary>
    public EquipmentBonus GetTotalEquipmentBonus()
    {
        float totalPower = 0f;
        float totalDefense = 0f;
        float totalSpeed = 0f;
        int rareItems = 0;
        
        foreach (var weapon in GetEquippedWeapons())
        {
            if (weapon != null)
            {
                switch (weapon.WeaponType)
                {
                    case WeaponType.Sword:
                    case WeaponType.Axe:
                    case WeaponType.Bow:
                    case WeaponType.Staff:
                        totalPower += weapon.Power;
                        break;
                        
                    case WeaponType.Shield:
                        totalDefense += weapon.Power;
                        break;
                }
                
                // 根据稀有度给予额外奖励
                switch (weapon.Rarity)
                {
                    case RarityType.Rare: rareItems += 1; break;
                    case RarityType.Epic: rareItems += 2; break;
                    case RarityType.Legendary: rareItems += 3; break;
                }
            }
        }
        
        // 稀有度套装奖励
        if (rareItems >= 3) totalPower += rareItems * 5f; // 套装奖励
        
        return new EquipmentBonus(totalPower, totalDefense, rareItems);
    }
    
    public struct EquipmentBonus
    {
        public float AttackBonus;
        public float DefenseBonus;
        public int RareItemCount;
        
        public EquipmentBonus(float attack, float defense, int rareCount)
        {
            AttackBonus = attack;
            DefenseBonus = defense;
            RareItemCount = rareCount;
        }
    }
    
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public WeaponData PurchasedItem { get; set; }
        public string Message { get; set; }
        
        public PurchaseResult(bool success, WeaponData item, string message)
        {
            Success = success;
            PurchasedItem = item;
            Message = message;
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("装备系统已销毁");
    }
}