using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 捕捉系统 - 管理野外KuKu生成、捕捉逻辑等
    /// </summary>
    public static class CaptureSystem
    {
        // 野外KuKu结构
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
                isCapturable = false; // 初始不可捕捉，需要削弱后才能捕捉
                timeUntilDespawn = 300f; // 5分钟内必须捕捉，否则消失
                isSoulAbsorbing = false;
                soulAccumulated = 0f;
            }
        }

        // 捕捉结果结构
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

        // 掉落物结构
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

        // 野外KuKu列表
        private static List<WildKuku> wildKukus = new List<WildKuku>();
        
        // KuKu名称生成器
        private static readonly string[] prefixes = { "森林", "山川", "湖泊", "天空", "海洋", "沙漠", "雪山", "草原", "洞穴", "遗迹" };
        private static readonly string[] middles = { "守护", "游荡", "隐秘", "觅食", "嬉戏", "修炼", "沉睡", "觉醒", "飞翔", "潜行" };
        private static readonly string[] suffixes = { "精灵", "仙子", "神使", "圣兽", "灵童", "天将", "护法", "使者", "神童", "灵君" };

        /// <summary>
        /// 生成野外KuKu
        /// </summary>
        public static void GenerateWildKuku(Vector3 position, MythicalKukuData.MythicalRarity rarity = MythicalKukuData.MythicalRarity.Celestial)
        {
            try
            {
                // 创建野生KuKu数据
                MythicalKukuData wildKuku = new MythicalKukuData();
                wildKuku.Id = UnityEngine.Random.Range(1000, 9999);
                wildKuku.Name = GenerateWildKukuName(rarity);
                wildKuku.Rarity = rarity;
                wildKuku.Description = "在野外游荡的KuKu";
                wildKuku.SpriteName = $"WildKuku_{wildKuku.Id}";
                
                // 设置基础属性
                wildKuku.AttackPower = 30f + (int)rarity * 20f;
                wildKuku.DefensePower = 20f + (int)rarity * 15f;
                wildKuku.Speed = 2f + (int)rarity * 0.5f;
                wildKuku.Health = 100f + (int)rarity * 50f;
                
                // 设置神话属性
                wildKuku.DivinePower = 20f + (int)rarity * 15f;
                wildKuku.ProtectionPower = 15f + (int)rarity * 10f;
                wildKuku.PurificationPower = 10f + (int)rarity * 8f;
                
                // 设置捕捉相关属性
                wildKuku.CaptureDifficulty = 1.0f - (float)rarity * 0.1f; // 稀有度越高越难捕捉
                wildKuku.CanAbsorbSoul = true;
                wildKuku.SoulAbsorptionRate = 0.5f + (int)rarity * 0.1f;
                
                // 创建野外KuKu对象
                WildKuku wild = new WildKuku(wildKuku, position);
                
                // 添加到列表
                wildKukus.Add(wild);
                
                Debug.Log($"在位置 {position} 生成了野生KuKu: {wildKuku.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"生成野外KuKu时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 攻击野生KuKu（削弱其生命值以达到捕捉条件）
        /// </summary>
        public static bool AttackWildKuku(int kukuId, float damage, out string message)
        {
            message = "";
            
            for (int i = 0; i < wildKukus.Count; i++)
            {
                if (wildKukus[i].kukuData.Id == kukuId)
                {
                    WildKuku wild = wildKukus[i];
                    
                    // 减少生命值
                    wild.remainingHP -= damage;
                    
                    // 检查是否可以捕捉（生命值低于阈值）
                    float captureThreshold = wild.kukuData.Health * 0.3f; // 生命值低于30%时可以捕捉
                    wild.isCapturable = wild.remainingHP <= captureThreshold;
                    
                    // 更新列表
                    wildKukus[i] = wild;
                    
                    if (wild.isCapturable)
                    {
                        message = $"{wild.kukuData.Name} 生命值已削弱，现在可以捕捉！";
                    }
                    else
                    {
                        message = $"{wild.kukuData.Name} 受到攻击，剩余生命值: {wild.remainingHP:F0}/{wild.kukuData.Health:F0}";
                    }
                    
                    return true;
                }
            }
            
            message = "未找到指定的野生KuKu";
            return false;
        }

        /// <summary>
        /// 尝试捕捉野生KuKu
        /// </summary>
        public static CaptureResult AttemptCapture(int kukuId, MythicalKukuData capturerKuku = null)
        {
            for (int i = 0; i < wildKukus.Count; i++)
            {
                if (wildKukus[i].kukuData.Id == kukuId)
                {
                    WildKuku wild = wildKukus[i];
                    
                    // 检查是否可以捕捉
                    if (!wild.isCapturable)
                    {
                        return new CaptureResult(false, null, $"{wild.kukuData.Name} 当前不可捕捉，请先削弱其生命值！");
                    }
                    
                    // 计算捕捉成功率
                    float successRate = GetCaptureSuccessRate(kukuId, capturerKuku);
                    
                    if (UnityEngine.Random.value <= successRate)
                    {
                        // 捕捉成功
                        MythicalKukuData capturedKuku = wild.kukuData.Clone();
                        
                        // 生成掉落物
                        List<ItemDrop> drops = new List<ItemDrop>();
                        
                        // 固定掉落：金币
                        drops.Add(new ItemDrop(ItemDrop.ItemType.Coin, 0, UnityEngine.Random.Range(10, 50)));
                        
                        // 随机掉落：灵魂
                        if (UnityEngine.Random.value < 0.7f)
                        {
                            drops.Add(new ItemDrop(ItemDrop.ItemType.Soul, 
                                (float)UnityEngine.Random.Range(1, 5), 1));
                        }
                        
                        // 随机掉落：KuKu蛋
                        if (UnityEngine.Random.value < 0.2f)
                        {
                            drops.Add(new ItemDrop(ItemDrop.ItemType.KukuEgg, 0, 1));
                        }
                        
                        // 从野外列表中移除
                        wildKukus.RemoveAt(i);
                        
                        string message = $"成功捕捉到 {capturedKuku.Name}！获得了资源奖励。";
                        
                        return new CaptureResult(true, capturedKuku, message, drops);
                    }
                    else
                    {
                        // 捕捉失败，野生KuKu逃跑
                        wildKukus.RemoveAt(i);
                        
                        return new CaptureResult(false, null, $"{wild.kukuData.Name} 逃脱了！捕捉失败。");
                    }
                }
            }
            
            return new CaptureResult(false, null, "未找到指定的野生KuKu");
        }

        /// <summary>
        /// 生成野生KuKu名称
        /// </summary>
        private static string GenerateWildKukuName(MythicalKukuData.MythicalRarity rarity)
        {
            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
            string middle = middles[UnityEngine.Random.Range(0, middles.Length)];
            string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
            
            return prefix + middle + suffix;
        }

        /// <summary>
        /// 获取捕捉成功率
        /// </summary>
        public static float GetCaptureSuccessRate(int kukuId, MythicalKukuData capturerKuku = null)
        {
            foreach (var wild in wildKukus)
            {
                if (wild.kukuData.Id == kukuId)
                {
                    // 基础成功率取决于生命值比例
                    float healthRatio = wild.remainingHP / wild.kukuData.Health;
                    float baseRate = 1.0f - healthRatio; // 生命值越低成功率越高
                    
                    // 根据稀有度调整
                    float rarityFactor = 1.0f - ((float)wild.kukuData.Rarity / 10f);
                    
                    // 如果有助手KuKu，提高成功率
                    if (capturerKuku != null)
                    {
                        baseRate *= 1.2f; // 助手KuKu提高20%成功率
                    }
                    
                    // 最终成功率
                    float finalRate = baseRate * rarityFactor;
                    return Mathf.Clamp01(finalRate);
                }
            }
            
            return 0f; // 未找到指定KuKu
        }

        /// <summary>
        /// 检查指定位置是否有野生KuKu
        /// </summary>
        public static bool HasWildKukuAtPosition(Vector3 position, float radius = 1.0f)
        {
            foreach (var wild in wildKukus)
            {
                float distance = Vector3.Distance(wild.position, position);
                if (distance <= radius)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 获取野外KuKu列表
        /// </summary>
        public static List<WildKuku> GetWildKukus()
        {
            return new List<WildKuku>(wildKukus);
        }

        /// <summary>
        /// 获取捕捉信息
        /// </summary>
        public static (bool canCapture, float successRate, float hpRatio, string status) GetCaptureInfo(int kukuId)
        {
            foreach (var wild in wildKukus)
            {
                if (wild.kukuData.Id == kukuId)
                {
                    float hpRatio = wild.remainingHP / wild.kukuData.Health;
                    bool canCapture = wild.isCapturable;
                    float successRate = GetCaptureSuccessRate(kukuId);
                    string status = wild.isCapturable ? "可捕捉" : "不可捕捉";
                    
                    return (canCapture, successRate, hpRatio, status);
                }
            }
            
            return (false, 0f, 0f, "未找到");
        }

        /// <summary>
        /// 更新野生KuKu（处理消失、灵魂积累等）
        /// </summary>
        public static void UpdateWildKukus(float deltaTime)
        {
            for (int i = wildKukus.Count - 1; i >= 0; i--)
            {
                WildKuku wild = wildKukus[i];
                
                // 更新消失倒计时
                wild.timeUntilDespawn -= deltaTime;
                
                // 如果时间到了，移除野生KuKu
                if (wild.timeUntilDespawn <= 0)
                {
                    Debug.Log($"{wild.kukuData.Name} 消失了！");
                    wildKukus.RemoveAt(i);
                    continue;
                }
                
                // 更新灵魂积累（如果在吸收灵魂）
                if (wild.isSoulAbsorbing)
                {
                    wild.soulAccumulated += wild.kukuData.SoulAbsorptionRate * deltaTime;
                    
                    // 如果积累足够的灵魂，可以用于进化
                    if (wild.soulAccumulated >= 10f) // 假设需要10点灵魂
                    {
                        Debug.Log($"{wild.kukuData.Name} 积累了足够的灵魂，可用于进化！");
                        wild.soulAccumulated = 0f; // 重置积累
                    }
                }
                
                // 更新列表
                wildKukus[i] = wild;
            }
        }

        /// <summary>
        /// 野怪死亡处理（掉落物生成）
        /// </summary>
        public static ItemDrop[] ProcessWildKukuDeath(int kukuId)
        {
            // 从野外列表中查找并移除
            for (int i = 0; i < wildKukus.Count; i++)
            {
                if (wildKukus[i].kukuData.Id == kukuId)
                {
                    WildKuku deadKuku = wildKukus[i];
                    wildKukus.RemoveAt(i);
                    
                    // 生成掉落物
                    List<ItemDrop> drops = new List<ItemDrop>();
                    
                    // 固定掉落：金币
                    drops.Add(new ItemDrop(ItemDrop.ItemType.Coin, 0, UnityEngine.Random.Range(5, 25)));
                    
                    // 随机掉落：灵魂
                    if (UnityEngine.Random.value < 0.6f)
                    {
                        drops.Add(new ItemDrop(ItemDrop.ItemType.Soul, 
                            (float)UnityEngine.Random.Range(1, 3), 1));
                    }
                    
                    // 随机掉落：资源
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        drops.Add(new ItemDrop(ItemDrop.ItemType.Resource, 0, 
                            UnityEngine.Random.Range(1, 2)));
                    }
                    
                    return drops.ToArray();
                }
            }
            
            return new ItemDrop[0]; // 未找到指定KuKu
        }

        /// <summary>
        /// 清理所有野外KuKu
        /// </summary>
        public static void ClearAllWildKukus()
        {
            wildKukus.Clear();
        }

        /// <summary>
        /// 设置野生KuKu为灵魂吸收状态
        /// </summary>
        public static bool SetSoulAbsorption(int kukuId, bool absorbing)
        {
            for (int i = 0; i < wildKukus.Count; i++)
            {
                if (wildKukus[i].kukuData.Id == kukuId)
                {
                    WildKuku wild = wildKukus[i];
                    wild.isSoulAbsorbing = absorbing;
                    wildKukus[i] = wild;
                    return true;
                }
            }
            
            return false;
        }
    }
}