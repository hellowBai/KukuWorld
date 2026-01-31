using System;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 进化系统 - 处理KuKu进化逻辑
    /// </summary>
    public static class EvolutionSystem
    {
        /// <summary>
        /// 进化结果结构
        /// </summary>
        public struct EvolutionResult
        {
            public bool success;
            public MythicalKukuData evolvedKuku;
            public string message;
            public bool isFinalEvolution;     // 是否达到最终进化等级（可融合）

            public EvolutionResult(bool isSuccess, MythicalKukuData kuku, string msg, bool isFinal = false)
            {
                success = isSuccess;
                evolvedKuku = kuku;
                message = msg;
                isFinalEvolution = isFinal;
            }
        }

        /// <summary>
        /// 灵魂类型枚举
        /// </summary>
        public enum SoulType 
        { 
            Common,           // 普通灵魂
            Rare,             // 稀有灵魂
            Epic,             // 史诗灵魂
            Legendary,        // 传说灵魂
            Mythic,           // 神话灵魂
            Divine,           // 神圣灵魂
            SoulShard,        // 灵魂碎片（合成材料）
            DivineEssence     // 神圣精华（高级合成材料）
        }

        /// <summary>
        /// 尝试进化KuKu
        /// </summary>
        public static EvolutionResult EvolveKuku(MythicalKukuData kuku)
        {
            if (kuku == null)
            {
                return new EvolutionResult(false, null, "KuKu数据无效");
            }

            // 检查是否可以进化
            if (kuku.EvolutionLevel >= 5) // 假设最高进化等级为5
            {
                return new EvolutionResult(false, kuku, "KuKu已达到最高进化等级", true);
            }

            // 检查是否有足够的进化石
            if (kuku.EvolutionStonesRequired > 0)
            {
                // 这里需要连接到玩家数据来检查是否拥有足够的进化石
                // 暂时假设玩家有足够的进化石
            }

            // 计算进化成功率
            float successRate = GetEvolutionSuccessRate(kuku);

            if (UnityEngine.Random.value <= successRate)
            {
                // 进化成功
                MythicalKukuData evolvedKuku = PerformEvolution(kuku);

                string message = $"进化成功！{kuku.Name} 进化为 {evolvedKuku.Name}！";
                bool isFinal = evolvedKuku.EvolutionLevel >= 5;

                return new EvolutionResult(true, evolvedKuku, message, isFinal);
            }
            else
            {
                // 进化失败，但不会失去KuKu，只会消耗进化材料
                return new EvolutionResult(false, kuku, "进化失败，进化材料被消耗");
            }
        }

        /// <summary>
        /// 通过吸收灵魂进化KuKu
        /// </summary>
        public static EvolutionResult EvolveKukuWithSoul(MythicalKukuData kuku, float soulPower, SoulType soulType)
        {
            if (kuku == null)
            {
                return new EvolutionResult(false, null, "KuKu数据无效");
            }

            if (!kuku.CanAbsorbSoul)
            {
                return new EvolutionResult(false, kuku, "该KuKu无法吸收灵魂");
            }

            // 计算灵魂的有效性
            float effectiveSoulPower = CalculateSoulEffectiveness(kuku, soulPower, soulType);

            // 检查是否积累了足够的灵魂来进化
            if (effectiveSoulPower < GetSoulRequirementForNextEvolution(kuku))
            {
                // 灵魂不足，但可以增加进化进度
                float progress = effectiveSoulPower / GetSoulRequirementForNextEvolution(kuku);
                kuku.EvolutionProgress += progress;

                if (kuku.EvolutionProgress >= 1.0f)
                {
                    // 进化进度满了，执行进化
                    kuku.EvolutionProgress = 0f;
                    MythicalKukuData evolvedKuku = PerformEvolution(kuku);
                    
                    string message = $"灵魂积累完成！{kuku.Name} 进化为 {evolvedKuku.Name}！";
                    bool isFinal = evolvedKuku.EvolutionLevel >= 5;

                    return new EvolutionResult(true, evolvedKuku, message, isFinal);
                }
                else
                {
                    string message = $"灵魂吸收成功！进化进度: {(kuku.EvolutionProgress * 100):F1}%";
                    return new EvolutionResult(true, kuku, message);
                }
            }
            else
            {
                // 灵魂充足，直接进化
                MythicalKukuData evolvedKuku = PerformEvolution(kuku);
                
                string message = $"灵魂进化成功！{kuku.Name} 进化为 {evolvedKuku.Name}！";
                bool isFinal = evolvedKuku.EvolutionLevel >= 5;

                return new EvolutionResult(true, evolvedKuku, message, isFinal);
            }
        }

        /// <summary>
        /// 获取进化成功率
        /// </summary>
        public static float GetEvolutionSuccessRate(MythicalKukuData kuku)
        {
            if (kuku == null) return 0f;

            // 基础成功率
            float baseRate = 0.8f; // 80%基础成功率

            // 根据进化等级调整成功率（等级越高成功率越低）
            float levelFactor = 1.0f - (kuku.EvolutionLevel - 1) * 0.15f; // 每级降低15%

            // 根据稀有度调整成功率
            float rarityFactor = 1.0f - ((int)kuku.Rarity * 0.05f); // 稀有度越高成功率越低

            // 计算最终成功率
            float finalRate = baseRate * levelFactor * rarityFactor;

            return Mathf.Clamp01(finalRate);
        }

        /// <summary>
        /// 获取进化预览
        /// </summary>
        public static MythicalKukuData GetEvolutionPreview(MythicalKukuData kuku)
        {
            if (kuku == null) return null;

            // 创建当前KuKu的副本并模拟进化
            MythicalKukuData preview = kuku.Clone();
            
            // 应用进化属性提升
            preview.EvolutionLevel++;
            preview.AttackPower *= 1.3f;
            preview.DefensePower *= 1.25f;
            preview.Health *= 1.3f;
            preview.Speed *= 1.1f;
            preview.DivinePower *= 1.2f;
            preview.ProtectionPower *= 1.2f;
            preview.PurificationPower *= 1.2f;

            return preview;
        }

        /// <summary>
        /// 计算灵魂吸收效果
        /// </summary>
        public static float CalculateSoulEffectiveness(MythicalKukuData kuku, float soulPower, SoulType soulType)
        {
            if (kuku == null) return 0f;

            // 基础效率
            float effectiveness = soulPower;

            // 根据灵魂类型调整效率
            switch (soulType)
            {
                case SoulType.Common:
                    effectiveness *= 1.0f;
                    break;
                case SoulType.Rare:
                    effectiveness *= 1.5f;
                    break;
                case SoulType.Epic:
                    effectiveness *= 2.0f;
                    break;
                case SoulType.Legendary:
                    effectiveness *= 3.0f;
                    break;
                case SoulType.Mythic:
                    effectiveness *= 4.0f;
                    break;
                case SoulType.Divine:
                    effectiveness *= 5.0f;
                    break;
                case SoulType.SoulShard:
                    effectiveness *= 0.3f; // 碎片效率较低
                    break;
                case SoulType.DivineEssence:
                    effectiveness *= 4.5f; // 精华效率很高
                    break;
            }

            // 根据KuKu的吸收率调整
            effectiveness *= kuku.SoulAbsorptionRate;

            return effectiveness;
        }

        /// <summary>
        /// 灵魂合成系统
        /// </summary>
        public static float SynthesizeSouls(SoulType inputType, int quantity)
        {
            // 定义合成比率
            float synthesisRate = 0f;

            switch (inputType)
            {
                case SoulType.Common:
                    synthesisRate = 0.5f; // 2个普通灵魂合成1个有效灵魂
                    break;
                case SoulType.SoulShard:
                    synthesisRate = 0.3f; // 3个灵魂碎片合成1个普通灵魂
                    break;
                case SoulType.DivineEssence:
                    synthesisRate = 3.0f; // 1个神圣精华等于3个神话灵魂
                    break;
                default:
                    synthesisRate = 1.0f; // 其他类型按1:1计算
                    break;
            }

            return quantity * synthesisRate;
        }

        /// <summary>
        /// 转换灵魂类型
        /// </summary>
        public static float ConvertSouls(SoulType fromType, SoulType toType, float amount)
        {
            // 简化的转换逻辑
            float conversionRate = GetConversionRate(fromType, toType);
            return amount * conversionRate;
        }

        /// <summary>
        /// 获取转换比率
        /// </summary>
        private static float GetConversionRate(SoulType fromType, SoulType toType)
        {
            if (fromType == toType) return 1.0f;

            // 定义转换表
            var conversionMatrix = new System.Collections.Generic.Dictionary<(SoulType, SoulType), float>
            {
                // 从低级到高级的转换（需要更多材料）
                { (SoulType.Common, SoulType.Rare), 0.5f },
                { (SoulType.Common, SoulType.Epic), 0.2f },
                { (SoulType.Common, SoulType.Legendary), 0.1f },
                { (SoulType.Common, SoulType.Mythic), 0.05f },
                
                { (SoulType.Rare, SoulType.Epic), 0.6f },
                { (SoulType.Rare, SoulType.Legendary), 0.25f },
                { (SoulType.Rare, SoulType.Mythic), 0.1f },
                
                { (SoulType.Epic, SoulType.Legendary), 0.7f },
                { (SoulType.Epic, SoulType.Mythic), 0.3f },
                
                { (SoulType.Legendary, SoulType.Mythic), 0.8f },
                
                // 从高级到低级的转换（会有损失）
                { (SoulType.Rare, SoulType.Common), 2.0f },
                { (SoulType.Epic, SoulType.Common), 5.0f },
                { (SoulType.Epic, SoulType.Rare), 3.0f },
                { (SoulType.Legendary, SoulType.Common), 15.0f },
                { (SoulType.Legendary, SoulType.Rare), 7.0f },
                { (SoulType.Legendary, SoulType.Epic), 2.5f },
                { (SoulType.Mythic, SoulType.Common), 40.0f },
                { (SoulType.Mythic, SoulType.Rare), 20.0f },
                { (SoulType.Mythic, SoulType.Epic), 10.0f },
                { (SoulType.Mythic, SoulType.Legendary), 3.0f },
                
                // 灵魂碎片转换
                { (SoulType.SoulShard, SoulType.Common), 0.3f },
                { (SoulType.Common, SoulType.SoulShard), 3.0f },
                
                // 神圣精华转换
                { (SoulType.DivineEssence, SoulType.Mythic), 2.0f },
                { (SoulType.Mythic, SoulType.DivineEssence), 0.4f }
            };

            var key = (fromType, toType);
            return conversionMatrix.ContainsKey(key) ? conversionMatrix[key] : 1.0f;
        }

        /// <summary>
        /// 检查KuKu是否已达到最终进化等级
        /// </summary>
        public static bool IsMaxEvolutionLevel(MythicalKukuData kuku)
        {
            if (kuku == null) return false;
            return kuku.EvolutionLevel >= 5;
        }

        /// <summary>
        /// 执行进化过程
        /// </summary>
        private static MythicalKukuData PerformEvolution(MythicalKukuData kuku)
        {
            MythicalKukuData evolvedKuku = kuku.Clone();

            // 提升进化等级
            evolvedKuku.EvolutionLevel++;

            // 提升基础属性
            evolvedKuku.AttackPower *= 1.3f;
            evolvedKuku.DefensePower *= 1.25f;
            evolvedKuku.Speed *= 1.1f;
            evolvedKuku.Health *= 1.3f;

            // 提升神话属性
            evolvedKuku.DivinePower *= 1.2f;
            evolvedKuku.ProtectionPower *= 1.2f;
            evolvedKuku.PurificationPower *= 1.2f;

            // 在第3级和第5级解锁新能力
            if (evolvedKuku.EvolutionLevel == 3)
            {
                evolvedKuku.CanFuseWithRobots = true;
                evolvedKuku.FusionCompatibility = 0.6f; // 初始融合兼容性
            }
            else if (evolvedKuku.EvolutionLevel == 5)
            {
                evolvedKuku.FusionCompatibility = 1.0f; // 最高融合兼容性
                evolvedKuku.MaxEquipmentSlots = 6; // 解锁6个装备槽
            }

            // 根据进化等级调整稀有度（可能提升）
            if (evolvedKuku.EvolutionLevel % 2 == 0 && evolvedKuku.Rarity < MythicalKukuData.MythicalRarity.Primordial)
            {
                evolvedKuku.Rarity = (MythicalKukuData.MythicalRarity)Mathf.Min((int)evolvedKuku.Rarity + 1, 4);
            }

            // 更新名称以反映进化等级
            evolvedKuku.Name = GetEvolvedName(kuku.Name, evolvedKuku.EvolutionLevel);

            return evolvedKuku;
        }

        /// <summary>
        /// 获取进化后的名称
        /// </summary>
        private static string GetEvolvedName(string originalName, int evolutionLevel)
        {
            string prefix = "";
            switch (evolutionLevel)
            {
                case 2:
                    prefix = "进阶 ";
                    break;
                case 3:
                    prefix = "高级 ";
                    break;
                case 4:
                    prefix = "终极 ";
                    break;
                case 5:
                    prefix = "传说 ";
                    break;
                default:
                    prefix = "";
                    break;
            }

            return prefix + originalName;
        }

        /// <summary>
        /// 获取下一级进化所需的灵魂量
        /// </summary>
        private static float GetSoulRequirementForNextEvolution(MythicalKukuData kuku)
        {
            // 随着进化等级提升，所需灵魂量呈指数增长
            return 50f * Mathf.Pow(2, kuku.EvolutionLevel - 1);
        }

        /// <summary>
        /// 获取进化所需材料
        /// </summary>
        public static (SoulType soulType, int quantity) GetEvolutionRequirements(MythicalKukuData kuku)
        {
            if (kuku == null) return (SoulType.Common, 0);

            // 根据进化等级确定所需灵魂类型和数量
            SoulType soulType = SoulType.Common;
            int quantity = 10;

            if (kuku.EvolutionLevel >= 1)
            {
                soulType = SoulType.Rare;
                quantity = 15;
            }
            if (kuku.EvolutionLevel >= 2)
            {
                soulType = SoulType.Epic;
                quantity = 20;
            }
            if (kuku.EvolutionLevel >= 3)
            {
                soulType = SoulType.Legendary;
                quantity = 25;
            }
            if (kuku.EvolutionLevel >= 4)
            {
                soulType = SoulType.Mythic;
                quantity = 30;
            }

            return (soulType, quantity);
        }

        /// <summary>
        /// 获取当前进化进度百分比
        /// </summary>
        public static float GetEvolutionProgressPercentage(MythicalKukuData kuku)
        {
            if (kuku == null) return 0f;

            // 如果有进化进度值，则使用该值
            if (kuku.EvolutionProgress > 0)
            {
                return Mathf.Clamp01(kuku.EvolutionProgress) * 100f;
            }

            // 否则基于进化等级估算进度
            return (kuku.EvolutionLevel - 1) * 25f; // 每级25%进度
        }

        /// <summary>
        /// 检查是否接近进化
        /// </summary>
        public static bool IsNearEvolution(MythicalKukuData kuku, float threshold = 0.8f)
        {
            return GetEvolutionProgressPercentage(kuku) / 100f >= threshold;
        }
    }
}