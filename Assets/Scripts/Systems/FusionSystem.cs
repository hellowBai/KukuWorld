using UnityEngine;
using System.Collections.Generic;

public class FusionSystem : MonoBehaviour
{
    [Header("融合设置")]
    public float baseSoulCost = 20f;
    public float rarityMultiplier = 1.5f;
    public float levelMultiplier = 1.2f;
    
    private GameManager gameManager;
    private PlayerData playerData;
    
    // 事件
    public System.Action<MythicalKukuData> OnFusionCompleted;
    public System.Action<string> OnFusionFailed;
    
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
        
        Debug.Log("融合系统初始化完成");
    }
    
    /// <summary>
    /// 尝试融合两个KuKu
    /// </summary>
    /// <param name="kukuId1">第一个KuKu ID</param>
    /// <param name="kukuId2">第二个KuKu ID</param>
    /// <returns>融合结果</returns>
    public FusionResult AttemptFusion(int kukuId1, int kukuId2)
    {
        if (playerData == null)
        {
            return new FusionResult(false, null, "玩家数据未加载");
        }
        
        if (kukuId1 == kukuId2)
        {
            return new FusionResult(false, null, "不能融合同一个KuKu");
        }
        
        // 查找KuKu
        MythicalKukuData kuku1 = playerData.GetKukuById(kukuId1);
        MythicalKukuData kuku2 = playerData.GetKukuById(kukuId2);
        
        if (kuku1 == null || kuku2 == null)
        {
            return new FusionResult(false, null, "未找到指定的KuKu");
        }
        
        // 计算融合成本
        float soulCost = CalculateFusionCost(kuku1, kuku2);
        if (playerData.Souls < soulCost)
        {
            return new FusionResult(false, null, $"灵魂不足，需要 {soulCost:F1} 灵魂，当前 {playerData.Souls:F1}");
        }
        
        // 执行融合
        MythicalKukuData fusedKuku = FuseKukus(kuku1, kuku2);
        
        // 消耗灵魂
        playerData.SpendSouls(soulCost);
        
        // 移除原KuKu
        playerData.RemoveKuku(kukuId1);
        playerData.RemoveKuku(kukuId2);
        
        // 添加融合后的KuKu
        playerData.AddKuku(fusedKuku);
        
        // 触发融合成功事件
        OnFusionCompleted?.Invoke(fusedKuku);
        
        return new FusionResult(true, fusedKuku, $"成功融合 {fusedKuku.Name}！");
    }
    
    /// <summary>
    /// 尝试将KuKu与机器人融合
    /// </summary>
    /// <param name="kukuId">KuKu ID</param>
    /// <param name="robotType">机器人类型</param>
    /// <returns>融合结果</returns>
    public FusionResult AttemptKukuRobotFusion(int kukuId, RobotType robotType)
    {
        if (playerData == null)
        {
            return new FusionResult(false, null, "玩家数据未加载");
        }
        
        // 查找KuKu
        MythicalKukuData kuku = playerData.GetKukuById(kukuId);
        if (kuku == null)
        {
            return new FusionResult(false, null, "未找到指定的KuKu");
        }
        
        // 计算融合成本
        float soulCost = CalculateKukuRobotFusionCost(kuku, robotType);
        if (playerData.Souls < soulCost)
        {
            return new FusionResult(false, null, $"灵魂不足，需要 {soulCost:F1} 灵魂，当前 {playerData.Souls:F1}");
        }
        
        // 执行KuKu-机器人融合
        MythicalKukuData fusedKuku = FuseKukuWithRobot(kuku, robotType);
        
        // 消耗灵魂
        playerData.SpendSouls(soulCost);
        
        // 移除原KuKu
        playerData.RemoveKuku(kukuId);
        
        // 添加融合后的KuKu
        playerData.AddKuku(fusedKuku);
        
        // 触发融合成功事件
        OnFusionCompleted?.Invoke(fusedKuku);
        
        return new FusionResult(true, fusedKuku, $"成功融合 {fusedKuku.Name}！");
    }
    
    /// <summary>
    /// 计算两个KuKu融合的成本
    /// </summary>
    public float CalculateFusionCost(MythicalKukuData kuku1, MythicalKukuData kuku2)
    {
        float cost = baseSoulCost;
        
        // 根据稀有度增加成本
        cost *= Mathf.Pow(rarityMultiplier, (int)kuku1.Rarity + (int)kuku2.Rarity);
        
        // 根据等级增加成本
        cost *= Mathf.Pow(levelMultiplier, (kuku1.Level + kuku2.Level) / 5f);
        
        return cost;
    }
    
    /// <summary>
    /// 计算KuKu与机器人融合的成本
    /// </summary>
    public float CalculateKukuRobotFusionCost(MythicalKukuData kuku, RobotType robotType)
    {
        float cost = baseSoulCost * 1.5f; // KuKu-机器人融合成本稍高
        
        // 根据稀有度增加成本
        cost *= Mathf.Pow(rarityMultiplier, (int)kuku.Rarity + (int)robotType);
        
        // 根据等级增加成本
        cost *= Mathf.Pow(levelMultiplier, kuku.Level / 5f);
        
        // 根据机器人类型增加成本
        cost *= GetRobotTypeMultiplier(robotType);
        
        return cost;
    }
    
    /// <summary>
    /// 执行两个KuKu的融合
    /// </summary>
    private MythicalKukuData FuseKukus(MythicalKukuData kuku1, MythicalKukuData kuku2)
    {
        MythicalKukuData fusedKuku = new MythicalKukuData();
        
        // 基本属性 - 取两个KuKu的平均值并略微提升
        fusedKuku.AttackPower = (kuku1.AttackPower + kuku2.AttackPower) * 0.52f;
        fusedKuku.DefensePower = (kuku1.DefensePower + kuku2.DefensePower) * 0.52f;
        fusedKuku.Health = (kuku1.Health + kuku2.Health) * 0.52f;
        fusedKuku.Speed = (kuku1.Speed + kuku2.Speed) * 0.5f;
        
        // 神话属性 - 取最大值并提升
        fusedKuku.DivinePower = Mathf.Max(kuku1.DivinePower, kuku2.DivinePower) * 1.1f;
        fusedKuku.ProtectionPower = Mathf.Max(kuku1.ProtectionPower, kuku2.ProtectionPower) * 1.1f;
        fusedKuku.PurificationPower = Mathf.Max(kuku1.PurificationPower, kuku2.PurificationPower) * 1.1f;
        
        // 技能继承 - 随机选择一个KuKu的技能，但威力提升
        if (Random.value > 0.5f)
        {
            fusedKuku.SkillName = kuku1.SkillName;
            fusedKuku.SkillDamage = kuku1.SkillDamage * 1.3f;
            fusedKuku.SkillCooldown = kuku1.SkillCooldown * 0.9f; // 冷却缩短
        }
        else
        {
            fusedKuku.SkillName = kuku2.SkillName;
            fusedKuku.SkillDamage = kuku2.SkillDamage * 1.3f;
            fusedKuku.SkillCooldown = kuku2.SkillCooldown * 0.9f; // 冷却缩短
        }
        
        // 稀有度 - 取较高的稀有度，有一定概率提升一级
        MythicalKukuData.MythicalRarity maxRarity = kuku1.Rarity >= kuku2.Rarity ? kuku1.Rarity : kuku2.Rarity;
        if (Random.value < 0.15f && (int)maxRarity < 4) // 15%概率提升稀有度
        {
            fusedKuku.Rarity = (MythicalKukuData.MythicalRarity)((int)maxRarity + 1);
        }
        else
        {
            fusedKuku.Rarity = maxRarity;
        }
        
        // 其他属性
        fusedKuku.Level = Mathf.Max(kuku1.Level, kuku2.Level);
        fusedKuku.Experience = (kuku1.Experience + kuku2.Experience) / 2;
        fusedKuku.CaptureDifficulty = (kuku1.CaptureDifficulty + kuku2.CaptureDifficulty) * 0.5f;
        fusedKuku.CanAbsorbSoul = kuku1.CanAbsorbSoul || kuku2.CanAbsorbSoul;
        fusedKuku.SoulAbsorptionRate = (kuku1.SoulAbsorptionRate + kuku2.SoulAbsorptionRate) * 0.55f;
        fusedKuku.EvolutionStonesRequired = (kuku1.EvolutionStonesRequired + kuku2.EvolutionStonesRequired) * 0.6f;
        
        // 名称
        fusedKuku.Name = $"{kuku1.Name}-{kuku2.Name}-融合体";
        fusedKuku.Description = $"由{kuku1.Name}和{kuku2.Name}融合而成的强大存在";
        
        // ID - 使用新的随机ID
        fusedKuku.Id = Random.Range(10000, 99999);
        
        return fusedKuku;
    }
    
    /// <summary>
    /// 执行KuKu与机器人的融合
    /// </summary>
    private MythicalKukuData FuseKukuWithRobot(MythicalKukuData kuku, RobotType robotType)
    {
        MythicalKukuData fusedKuku = kuku.Clone();
        
        // 根据机器人类型调整属性
        switch (robotType)
        {
            case RobotType.Warrior:
                fusedKuku.AttackPower *= 1.4f;
                fusedKuku.DefensePower *= 1.2f;
                fusedKuku.SkillDamage *= 1.3f;
                fusedKuku.Name = $"机械-{fusedKuku.Name}";
                break;
                
            case RobotType.Mage:
                fusedKuku.DivinePower *= 1.6f;
                fusedKuku.SkillDamage *= 1.5f;
                fusedKuku.SkillCooldown *= 0.7f;
                fusedKuku.Name = $"魔导-{fusedKuku.Name}";
                break;
                
            case RobotType.Archer:
                fusedKuku.Speed *= 1.3f;
                fusedKuku.AttackPower *= 1.2f;
                fusedKuku.SkillDamage *= 1.4f;
                fusedKuku.Name = $"机械-{fusedKuku.Name}";
                break;
                
            case RobotType.Tank:
                fusedKuku.Health *= 1.8f;
                fusedKuku.DefensePower *= 1.5f;
                fusedKuku.ProtectionPower *= 1.4f;
                fusedKuku.Name = $"装甲-{fusedKuku.Name}";
                break;
                
            case RobotType.Support:
                fusedKuku.PurificationPower *= 1.6f;
                fusedKuku.SkillCooldown *= 0.6f;
                fusedKuku.CanAbsorbSoul = true;
                fusedKuku.SoulAbsorptionRate *= 1.5f;
                fusedKuku.Name = $"支援-{fusedKuku.Name}";
                break;
        }
        
        // 融合特殊效果
        fusedKuku.SoulAbsorptionRate *= GetRobotTypeMultiplier(robotType);
        fusedKuku.EvolutionStonesRequired *= 0.8f; // 融合后更容易进化
        
        // 稀有度提升
        if ((int)fusedKuku.Rarity < 4 && Random.value < 0.25f) // 25%概率提升稀有度
        {
            fusedKuku.Rarity = (MythicalKukuData.MythicalRarity)((int)fusedKuku.Rarity + 1);
        }
        
        return fusedKuku;
    }
    
    /// <summary>
    /// 获取机器人类型的乘数
    /// </summary>
    private float GetRobotTypeMultiplier(RobotType robotType)
    {
        switch (robotType)
        {
            case RobotType.Warrior: return 1.2f;
            case RobotType.Mage: return 1.3f;
            case RobotType.Archer: return 1.1f;
            case RobotType.Tank: return 1.4f;
            case RobotType.Support: return 1.2f;
            default: return 1.0f;
        }
    }
    
    /// <summary>
    /// 获取融合信息
    /// </summary>
    public FusionInfo GetFusionInfo(int kukuId1, int kukuId2)
    {
        if (playerData == null)
        {
            return new FusionInfo(false, 0f, "玩家数据未加载");
        }
        
        MythicalKukuData kuku1 = playerData.GetKukuById(kukuId1);
        MythicalKukuData kuku2 = playerData.GetKukuById(kukuId2);
        
        if (kuku1 == null || kuku2 == null)
        {
            return new FusionInfo(false, 0f, "未找到指定的KuKu");
        }
        
        float soulCost = CalculateFusionCost(kuku1, kuku2);
        bool canAfford = playerData.Souls >= soulCost;
        
        return new FusionInfo(canAfford, soulCost, 
            $"融合成本: {soulCost:F1} 灵魂\n" +
            $"融合后稀有度: {(int)kuku1.Rarity >= (int)kuku2.Rarity ? kuku1.GetRarityName() : kuku2.GetRarityName()}或更高");
    }
    
    /// <summary>
    /// 获取KuKu-机器人融合信息
    /// </summary>
    public FusionInfo GetKukuRobotFusionInfo(int kukuId, RobotType robotType)
    {
        if (playerData == null)
        {
            return new FusionInfo(false, 0f, "玩家数据未加载");
        }
        
        MythicalKukuData kuku = playerData.GetKukuById(kukuId);
        if (kuku == null)
        {
            return new FusionInfo(false, 0f, "未找到指定的KuKu");
        }
        
        float soulCost = CalculateKukuRobotFusionCost(kuku, robotType);
        bool canAfford = playerData.Souls >= soulCost;
        
        return new FusionInfo(canAfford, soulCost, 
            $"KuKu-机器人融合成本: {soulCost:F1} 灵魂\n" +
            $"机器人类型: {robotType}\n" +
            $"融合后效果: {GetRobotTypeEffectDescription(robotType)}");
    }
    
    /// <summary>
    /// 获取机器人类型效果描述
    /// </summary>
    private string GetRobotTypeEffectDescription(RobotType robotType)
    {
        switch (robotType)
        {
            case RobotType.Warrior:
                return "大幅提升攻击力和防御力";
            case RobotType.Mage:
                return "大幅提升神力和技能伤害";
            case RobotType.Archer:
                return "大幅提升速度和远程攻击";
            case RobotType.Tank:
                return "大幅提升生命值和防御力";
            case RobotType.Support:
                return "大幅提升辅助能力和灵魂吸收";
            default:
                return "未知效果";
        }
    }
    
    public enum RobotType
    {
        Warrior = 0,    // 战士机器人
        Mage = 1,       // 法师机器人
        Archer = 2,     // 弓箭手机器人
        Tank = 3,       // 坦克机器人
        Support = 4     // 辅助机器人
    }
    
    public class FusionResult
    {
        public bool Success { get; set; }
        public MythicalKukuData FusedKuku { get; set; }
        public string Message { get; set; }
        
        public FusionResult(bool success, MythicalKukuData fusedKuku, string message)
        {
            Success = success;
            FusedKuku = fusedKuku;
            Message = message;
        }
    }
    
    public class FusionInfo
    {
        public bool CanAfford { get; set; }
        public float Cost { get; set; }
        public string InfoText { get; set; }
        
        public FusionInfo(bool canAfford, float cost, string infoText)
        {
            CanAfford = canAfford;
            Cost = cost;
            InfoText = infoText;
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("融合系统已销毁");
    }
}