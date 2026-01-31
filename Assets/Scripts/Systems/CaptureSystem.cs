using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CaptureSystem : MonoBehaviour
{
    [Header("生成设置")]
    public float spawnInterval = 5f;
    public int maxWildKukus = 10;
    public Transform[] spawnPoints;
    
    [Header("捕捉设置")]
    public float baseCaptureSuccessRate = 0.7f;
    
    // 野生KuKu列表
    private List<WildKuku> wildKukus = new List<WildKuku>();
    private float lastSpawnTime = 0f;
    
    // 事件
    public System.Action<int> OnWildKukuSpawned;
    public System.Action<int> OnWildKukuCaptured;
    public System.Action<int> OnWildKukuDespawned;
    
    [System.Serializable]
    public struct WildKuku
    {
        public MythicalKukuData kukuData;
        public Vector3 position;
        public float remainingHP;
        public bool isCapturable;
        public float timeUntilDespawn;
        public bool isSoulAbsorbing;
        public float soulAccumulated;
        
        public WildKuku(MythicalKukuData kuku, Vector3 pos)
        {
            kukuData = kuku;
            position = pos;
            remainingHP = kuku.Health;
            isCapturable = false; // 初始不可捕捉
            timeUntilDespawn = 300f; // 5分钟内必须捕捉
            isSoulAbsorbing = false;
            soulAccumulated = 0f;
        }
    }
    
    [System.Serializable]
    public struct CaptureResult
    {
        public bool success;
        public MythicalKukuData capturedKuku;
        public string message;
        public List<ItemDrop> drops;
        
        public CaptureResult(bool isSuccess, MythicalKukuData kuku, string msg, List<ItemDrop> dropList = null)
        {
            success = isSuccess;
            capturedKuku = kuku;
            message = msg;
            drops = dropList ?? new List<ItemDrop>();
        }
    }
    
    [System.Serializable]
    public struct ItemDrop
    {
        public ItemType type;
        public float value;
        public int quantity;
        
        public enum ItemType { Soul, Coin, KukuEgg, Resource }
        
        public ItemDrop(ItemType t, float val, int qty)
        {
            type = t;
            value = val;
            quantity = qty;
        }
    }
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        Debug.Log("捕捉系统初始化");
    }
    
    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.CapturePhase)
        {
            UpdateCapturePhase();
        }
        
        // 更新野生KuKu
        UpdateWildKukus(Time.deltaTime);
    }
    
    void UpdateCapturePhase()
    {
        // 检查是否需要生成新的野生KuKu
        if (Time.time - lastSpawnTime >= spawnInterval && wildKukus.Count < maxWildKukus)
        {
            SpawnWildKuku();
            lastSpawnTime = Time.time;
        }
    }
    
    public void StartCapturePhase()
    {
        Debug.Log("捕捉阶段开始");
        
        // 清除之前的野生KuKu
        ClearAllWildKukus();
        
        // 开始生成野生KuKu
        lastSpawnTime = Time.time;
    }
    
    public void EndCapturePhase()
    {
        Debug.Log("捕捉阶段结束");
        
        // 处理剩余的野生KuKu
        foreach (var wildKuku in wildKukus.ToList())
        {
            // 任何未捕捉的KuKu都会消失
            RemoveWildKuku(wildKuku.kukuData.Id);
        }
    }
    
    void SpawnWildKuku()
    {
        if (spawnPoints.Length == 0) return;
        
        // 随机选择生成点
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // 生成野生KuKu
        MythicalKukuData wildKuku = GenerateWildKukuData();
        
        // 创建野生KuKu对象
        WildKuku wild = new WildKuku(wildKuku, spawnPoint.position);
        wildKukus.Add(wild);
        
        Debug.Log($"生成野生KuKu: {wildKuku.Name} at {spawnPoint.position}");
        
        OnWildKukuSpawned?.Invoke(wildKuku.Id);
    }
    
    MythicalKukuData GenerateWildKukuData()
    {
        MythicalKukuData wildKuku = new MythicalKukuData();
        wildKuku.Id = Random.Range(1000, 9999);
        wildKuku.Name = GenerateWildKukuName();
        wildKuku.Rarity = GetRandomRarity();
        wildKuku.Description = "在野外游荡的神秘生物";
        
        // 根据稀有度设置属性
        SetKukuAttributesByRarity(wildKuku, wildKuku.Rarity);
        
        return wildKuku;
    }
    
    string GenerateWildKukuName()
    {
        string[] prefixes = { "森林", "山川", "湖泊", "天空", "海洋", "沙漠", "雪山", "草原" };
        string[] middles = { "守护", "游荡", "潜伏", "嬉戏", "觅食", "修炼" };
        string[] suffixes = { "精灵", "仙子", "神兽", "灵童", "天将", "护法" };
        
        string prefix = prefixes[Random.Range(0, prefixes.Length)];
        string middle = middles[Random.Range(0, middles.Length)];
        string suffix = suffixes[Random.Range(0, suffixes.Length)];
        
        return prefix + middle + suffix;
    }
    
    MythicalKukuData.MythicalRarity GetRandomRarity()
    {
        // 根据权重随机选择稀有度
        float rand = Random.value;
        
        if (rand < 0.40f) return MythicalKukuData.MythicalRarity.Celestial;      // 40%
        else if (rand < 0.70f) return MythicalKukuData.MythicalRarity.Immortal;  // 30%
        else if (rand < 0.85f) return MythicalKukuData.MythicalRarity.DivineBeast; // 15%
        else if (rand < 0.95f) return MythicalKukuData.MythicalRarity.Sacred;    // 10%
        else return MythicalKukuData.MythicalRarity.Primordial;                 // 5%
    }
    
    void SetKukuAttributesByRarity(MythicalKukuData kuku, MythicalKukuData.MythicalRarity rarity)
    {
        // 基础属性
        kuku.AttackPower = 20f + (int)rarity * 15f;
        kuku.DefensePower = 15f + (int)rarity * 10f;
        kuku.Speed = 1.5f + (int)rarity * 0.3f;
        kuku.Health = 80f + (int)rarity * 40f;
        
        // 神话属性
        kuku.DivinePower = 15f + (int)rarity * 12f;
        kuku.ProtectionPower = 10f + (int)rarity * 8f;
        kuku.PurificationPower = 8f + (int)rarity * 6f;
        
        // 捕捉相关
        kuku.CaptureDifficulty = 1.2f - (float)rarity * 0.15f; // 稀有度越高越难捕捉
        kuku.CanAbsorbSoul = true;
        kuku.SoulAbsorptionRate = 0.3f + (int)rarity * 0.1f;
        
        // 进化相关
        kuku.EvolutionStonesRequired = 5f + (int)rarity * 3f;
    }
    
    public bool AttackWildKuku(int kukuId, float damage, out string message)
    {
        message = "";
        
        var wildKuku = wildKukus.Find(w => w.kukuData.Id == kukuId);
        if (wildKuku.kukuData.Id == 0) // 未找到
        {
            message = "未找到指定的野生KuKu";
            return false;
        }
        
        // 造成伤害
        wildKuku.remainingHP -= damage;
        
        // 更新列表中的数据
        for (int i = 0; i < wildKukus.Count; i++)
        {
            if (wildKukus[i].kukuData.Id == kukuId)
            {
                var updated = wildKukus[i];
                updated.remainingHP = wildKuku.remainingHP;
                
                // 如果生命值降到一定程度，变为可捕捉
                if (updated.remainingHP <= wildKukus[i].kukuData.Health * 0.3f)
                {
                    updated.isCapturable = true;
                }
                
                wildKukus[i] = updated;
                break;
            }
        }
        
        if (wildKuku.remainingHP <= 0)
        {
            // KuKu死亡，生成掉落物
            var drops = GenerateDeathDrops(wildKuku.kukuData);
            
            // 给玩家奖励
            foreach (var drop in drops)
            {
                ProcessDrop(drop);
            }
            
            // 移除野生KuKu
            RemoveWildKuku(kukuId);
            
            message = $"{wildKuku.kukuData.Name}被击败了！获得了奖励。";
        }
        else
        {
            message = $"{wildKuku.kukuData.Name}受到{damage}点伤害，剩余生命值: {Mathf.Max(0, wildKuku.remainingHP)}";
        }
        
        return wildKuku.remainingHP > 0;
    }
    
    public CaptureResult AttemptCapture(int kukuId, MythicalKukuData capturerKuku = null)
    {
        var wildKuku = wildKukus.Find(w => w.kukuData.Id == kukuId);
        if (wildKuku.kukuData.Id == 0) // 未找到
        {
            return new CaptureResult(false, null, "未找到指定的野生KuKu");
        }
        
        if (!wildKuku.isCapturable)
        {
            return new CaptureResult(false, null, "该KuKu目前不可捕捉，请先削弱其生命值！");
        }
        
        // 计算捕捉成功率
        float successRate = CalculateCaptureSuccessRate(wildKuku, capturerKuku);
        bool success = Random.value <= successRate;
        
        if (success)
        {
            // 捕捉成功
            MythicalKukuData capturedKuku = wildKuku.kukuData.Clone();
            capturedKuku.IsCollected = true;
            
            // 生成掉落物
            List<ItemDrop> drops = GenerateCaptureDrops(wildKuku.kukuData);
            
            // 给玩家奖励
            foreach (var drop in drops)
            {
                ProcessDrop(drop);
            }
            
            // 移除野生KuKu
            RemoveWildKuku(kukuId);
            
            string message = $"成功捕捉到{capturedKuku.Name}！";
            
            // 触发事件
            OnWildKukuCaptured?.Invoke(kukuId);
            
            return new CaptureResult(true, capturedKuku, message, drops);
        }
        else
        {
            // 捕捉失败，KuKu可能逃跑
            RemoveWildKuku(kukuId);
            
            string message = $"捕捉失败！{wildKuku.kukuData.Name}逃跑了...";
            
            return new CaptureResult(false, null, message);
        }
    }
    
    float CalculateCaptureSuccessRate(WildKuku wildKuku, MythicalKukuData capturerKuku)
    {
        float rate = baseCaptureSuccessRate;
        
        // 生命值比例修正（生命值越低成功率越高）
        float hpRatio = wildKuku.remainingHP / wildKuku.kukuData.Health;
        rate += (1 - hpRatio) * 0.4f; // 生命值越低，成功率提升越多
        
        // 稀有度修正（越稀有越难捕捉）
        rate -= (float)wildKuku.kukuData.Rarity * 0.1f;
        
        // 捕捉者KuKu修正（如果有的话）
        if (capturerKuku != null)
        {
            rate += capturerKuku.Level * 0.01f; // 等级越高成功率越高
        }
        
        // 确保成功率在合理范围内
        rate = Mathf.Clamp(rate, 0.05f, 0.95f);
        
        return rate;
    }
    
    List<ItemDrop> GenerateCaptureDrops(MythicalKukuData kuku)
    {
        List<ItemDrop> drops = new List<ItemDrop>();
        
        // 固定掉落：金币
        int coinAmount = Random.Range(8, 20);
        drops.Add(new ItemDrop(ItemDrop.ItemType.Coin, 0, coinAmount));
        
        // 概率掉落：灵魂
        if (Random.value < 0.8f)
        {
            float soulAmount = 1.0f + (int)kuku.Rarity * 0.5f;
            drops.Add(new ItemDrop(ItemDrop.ItemType.Soul, soulAmount, 1));
        }
        
        // 概率掉落：KuKu蛋
        if (Random.value < 0.15f)
        {
            drops.Add(new ItemDrop(ItemDrop.ItemType.KukuEgg, 0, 1));
        }
        
        // 概率掉落：资源
        if (Random.value < 0.1f)
        {
            int resourceAmount = Random.Range(1, 3);
            drops.Add(new ItemDrop(ItemDrop.ItemType.Resource, 0, resourceAmount));
        }
        
        return drops;
    }
    
    List<ItemDrop> GenerateDeathDrops(MythicalKukuData kuku)
    {
        List<ItemDrop> drops = new List<ItemDrop>();
        
        // 死亡固定掉落：较少金币
        int coinAmount = Random.Range(3, 8);
        drops.Add(new ItemDrop(ItemDrop.ItemType.Coin, 0, coinAmount));
        
        // 概率掉落：灵魂（比捕捉时少）
        if (Random.value < 0.5f)
        {
            float soulAmount = 0.5f + (int)kuku.Rarity * 0.3f;
            drops.Add(new ItemDrop(ItemDrop.ItemType.Soul, soulAmount, 1));
        }
        
        return drops;
    }
    
    void ProcessDrop(ItemDrop drop)
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        switch (drop.type)
        {
            case ItemDrop.ItemType.Coin:
                gameManager.AddPlayerCoins(drop.quantity);
                break;
            case ItemDrop.ItemType.Soul:
                gameManager.AddPlayerSouls(drop.value);
                break;
            case ItemDrop.ItemType.KukuEgg:
                // KuKu蛋可以用于孵化或其他用途
                Debug.Log($"获得KuKu蛋 x{drop.quantity}");
                break;
            case ItemDrop.ItemType.Resource:
                gameManager.AddPlayerGems(drop.quantity);
                break;
        }
    }
    
    void UpdateWildKukus(float deltaTime)
    {
        for (int i = wildKukus.Count - 1; i >= 0; i--)
        {
            var wildKuku = wildKukus[i];
            
            // 更新消失计时
            wildKuku.timeUntilDespawn -= deltaTime;
            
            // 如果时间到了，移除KuKu
            if (wildKuku.timeUntilDespawn <= 0)
            {
                Debug.Log($"{wildKuku.kukuData.Name}因超时而消失");
                RemoveWildKuku(wildKuku.kukuData.Id);
                continue;
            }
            
            // 更新灵魂吸收（如果适用）
            if (wildKuku.isSoulAbsorbing)
            {
                wildKuku.soulAccumulated += wildKuku.kukuData.SoulAbsorptionRate * deltaTime;
            }
            
            // 更新列表
            wildKukus[i] = wildKuku;
        }
    }
    
    void RemoveWildKuku(int kukuId)
    {
        wildKukus.RemoveAll(w => w.kukuData.Id == kukuId);
        OnWildKukuDespawned?.Invoke(kukuId);
    }
    
    public void ClearAllWildKukus()
    {
        wildKukus.Clear();
    }
    
    public List<WildKuku> GetWildKukus()
    {
        return new List<WildKuku>(wildKukus);
    }
    
    public (bool canCapture, float successRate, float hpRatio, string status) GetCaptureInfo(int kukuId)
    {
        var wildKuku = wildKukus.Find(w => w.kukuData.Id == kukuId);
        if (wildKuku.kukuData.Id == 0)
        {
            return (false, 0f, 0f, "未找到KuKu");
        }
        
        float hpRatio = wildKuku.remainingHP / wildKuku.kukuData.Health;
        bool canCapture = wildKuku.isCapturable;
        float successRate = CalculateCaptureSuccessRate(wildKuku, null);
        string status = wildKuku.isCapturable ? "可捕捉" : "生命值过高，无法捕捉";
        
        return (canCapture, successRate, hpRatio, status);
    }
    
    public bool HasWildKukuAtPosition(Vector3 position, float radius = 1.0f)
    {
        return wildKukus.Any(w => Vector3.Distance(w.position, position) <= radius);
    }
    
    public MythicalKukuData FindWildKukuById(int id)
    {
        var wildKuku = wildKukus.Find(w => w.kukuData.Id == id);
        return wildKuku.kukuData.Id != 0 ? wildKuku.kukuData : null;
    }
    
    public int GetWildKukuCount()
    {
        return wildKukus.Count;
    }
    
    public int GetMaxWildKukus()
    {
        return maxWildKukus;
    }
    
    public float GetSpawnInterval()
    {
        return spawnInterval;
    }
    
    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = Mathf.Max(1f, newInterval); // 至少1秒间隔
    }
}