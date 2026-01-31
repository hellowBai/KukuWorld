using UnityEngine;
using System.Collections.Generic;

public class EvolutionSystem : MonoBehaviour
{
    [Header("进化设置")]
    public float baseSoulRequirement = 10f;
    public float rarityMultiplier = 2f;
    public float levelMultiplier = 1.1f;
    
    private GameManager gameManager;
    private PlayerData playerData;
    
    // 事件
    public System.Action<MythicalKukuData> OnKukuEvolved;
    public System.Action<string> OnEvolutionFailed;
    
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
        
        Debug.Log("进化系统初始化完成");
    }
    
    /// <summary>
    /// 尝试进化指定的KuKu
    /// </summary>
    /// <param name="kukuId">要进化的KuKu ID</param>
    /// <returns>进化结果</returns>
    public EvolutionResult AttemptEvolution(int kukuId)
    {
        if (playerData == null)
        {
            return new EvolutionResult(false, null, "玩家数据未加载");
        }
        
        // 查找KuKu
        MythicalKukuData kuku = playerData.GetKukuById(kukuId);
        if (kuku == null)
        {
            return new EvolutionResult(false, null, "未找到指定的KuKu");
        }
        
        // 检查是否满足进化条件
        float soulRequirement = CalculateSoulRequirement(kuku);
        if (playerData.Souls < soulRequirement)
        {
            return new EvolutionResult(false, null, $"灵魂不足，需要 {soulRequirement:F1} 灵魂，当前 {playerData.Souls:F1}");
        }
        
        // 检查等级要求
        if (kuku.Level < 10) // 假设需要达到10级才能进化
        {
            return new EvolutionResult(false, null, $"KuKu等级不足，需要达到10级才能进化，当前等级: {kuku.Level}");
        }
        
        // 执行进化
        MythicalKukuData evolvedKuku = EvolveKuku(kuku);
        
        // 消耗灵魂
        playerData.SpendSouls(soulRequirement);
        
        // 替换原KuKu
        playerData.RemoveKuku(kukuId);
        playerData.AddKuku(evolvedKuku);
        
        // 触发进化成功事件
        OnKukuEvolved?.Invoke(evolvedKuku);
        
        return new EvolutionResult(true, evolvedKuku, $"成功进化 {evolvedKuku.Name}！");
    }
    
    /// <summary>
    /// 计算进化所需灵魂数量
    /// </summary>
    /// <param name="kuku">要进化的KuKu</param>
    /// <returns>所需灵魂数量</returns>
    public float CalculateSoulRequirement(MythicalKukuData kuku)
    {
        float requirement = baseSoulRequirement;
        
        // 根据稀有度增加需求
        requirement *= Mathf.Pow(rarityMultiplier, (int)kuku.Rarity);
        
        // 根据等级增加需求
        requirement *= Mathf.Pow(levelMultiplier, kuku.Level / 10f);
        
        return requirement;
    }
    
    /// <summary>
    /// 执行KuKu进化
    /// </summary>
    /// <param name="originalKuku">原始KuKu</param>
    /// <returns>进化后的KuKu</returns>
    MythicalKukuData EvolveKuku(MythicalKukuData originalKuku)
    {
        MythicalKukuData evolvedKuku = originalKuku.Clone();
        
        // 提升稀有度（如果还不是最高稀有度）
        if ((int)evolvedKuku.Rarity < 4) // 不是最高稀有度
        {
            evolvedKuku.Rarity = (MythicalKukuData.MythicalRarity)((int)evolvedKuku.Rarity + 1);
        }
        
        // 大幅提升所有属性
        evolvedKuku.AttackPower *= 2.0f;
        evolvedKuku.DefensePower *= 2.0f;
        evolvedKuku.Health *= 2.0f;
        evolvedKuku.DivinePower *= 2.0f;
        evolvedKuku.ProtectionPower *= 2.0f;
        evolvedKuku.PurificationPower *= 2.0f;
        
        // 提升技能伤害
        evolvedKuku.SkillDamage *= 1.8f;
        
        // 重置等级为1（进化后重新开始成长）
        evolvedKuku.Level = 1;
        evolvedKuku.Experience = 0;
        
        // 增加名字前缀表示进化
        evolvedKuku.Name = $"进化-{evolvedKuku.Name}";
        
        Debug.Log($"KuKu {originalKuku.Name} 进化为 {evolvedKuku.Name}");
        
        return evolvedKuku;
    }
    
    /// <summary>
    /// 吸收灵魂（非进化方式提升KuKu）
    /// </summary>
    /// <param name="kukuId">KuKu ID</param>
    /// <param name="soulAmount">吸收的灵魂数量</param>
    /// <returns>吸收结果</returns>
    public SoulAbsorptionResult AbsorbSouls(int kukuId, float soulAmount)
    {
        if (playerData == null)
        {
            return new SoulAbsorptionResult(false, 0f, "玩家数据未加载", 0f);
        }
        
        if (playerData.Souls < soulAmount)
        {
            return new SoulAbsorptionResult(false, 0f, $"灵魂不足，需要 {soulAmount:F1} 灵魂，当前 {playerData.Souls:F1}", 0f);
        }
        
        MythicalKukuData kuku = playerData.GetKukuById(kukuId);
        if (kuku == null)
        {
            return new SoulAbsorptionResult(false, 0f, "未找到指定的KuKu", 0f);
        }
        
        // 检查KuKu是否能吸收灵魂
        if (!kuku.CanAbsorbSoul)
        {
            return new SoulAbsorptionResult(false, 0f, "此KuKu无法吸收灵魂", 0f);
        }
        
        // 消耗灵魂
        playerData.SpendSouls(soulAmount);
        
        // 计算属性提升
        float powerIncrease = soulAmount * 0.1f; // 每点灵魂提供0.1的属性提升
        
        // 分配到各项属性
        kuku.AttackPower += powerIncrease * 0.3f;  // 30%给攻击力
        kuku.DefensePower += powerIncrease * 0.2f; // 20%给防御力
        kuku.Health += powerIncrease * 0.25f;      // 25%给生命值
        kuku.DivinePower += powerIncrease * 0.1f;  // 10%给神力
        kuku.ProtectionPower += powerIncrease * 0.08f; // 8%给护体力量
        kuku.PurificationPower += powerIncrease * 0.07f; // 7%给净化力量
        
        // 增加经验值
        int expGain = Mathf.CeilToInt(soulAmount * 5f); // 每点灵魂提供5点经验
        kuku.AddExperience(expGain);
        
        // 返回实际吸收的数量和效果
        return new SoulAbsorptionResult(true, soulAmount, $"成功吸收 {soulAmount:F1} 灵魂", powerIncrease);
    }
    
    /// <summary>
    /// 获取进化信息
    /// </summary>
    /// <param name="kukuId">KuKu ID</param>
    /// <returns>进化信息</returns>
    public EvolutionInfo GetEvolutionInfo(int kukuId)
    {
        if (playerData == null)
        {
            return new EvolutionInfo(false, 0f, 0, "玩家数据未加载");
        }
        
        MythicalKukuData kuku = playerData.GetKukuById(kukuId);
        if (kuku == null)
        {
            return new EvolutionInfo(false, 0f, 0, "未找到指定的KuKu");
        }
        
        float soulRequirement = CalculateSoulRequirement(kuku);
        int levelRequirement = 10;
        
        return new EvolutionInfo(
            kuku.Level >= levelRequirement && playerData.Souls >= soulRequirement,
            soulRequirement,
            levelRequirement,
            $"进化需要: {soulRequirement:F1} 灵魂, 等级 {levelRequirement}+");
    }
    
    public class EvolutionResult
    {
        public bool Success { get; set; }
        public MythicalKukuData EvolvedKuku { get; set; }
        public string Message { get; set; }
        
        public EvolutionResult(bool success, MythicalKukuData evolvedKuku, string message)
        {
            Success = success;
            EvolvedKuku = evolvedKuku;
            Message = message;
        }
    }
    
    public class SoulAbsorptionResult
    {
        public bool Success { get; set; }
        public float SoulUsed { get; set; }
        public string Message { get; set; }
        public float PowerIncrease { get; set; }
        
        public SoulAbsorptionResult(bool success, float soulUsed, string message, float powerIncrease)
        {
            Success = success;
            SoulUsed = soulUsed;
            Message = message;
            PowerIncrease = powerIncrease;
        }
    }
    
    public class EvolutionInfo
    {
        public bool CanEvolve { get; set; }
        public float SoulRequirement { get; set; }
        public int LevelRequirement { get; set; }
        public string InfoText { get; set; }
        
        public EvolutionInfo(bool canEvolve, float soulReq, int levelReq, string infoText)
        {
            CanEvolve = canEvolve;
            SoulRequirement = soulReq;
            LevelRequirement = levelReq;
            InfoText = infoText;
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("进化系统已销毁");
    }
}