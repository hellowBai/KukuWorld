using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Utils
{
    /// <summary>
    /// KuKu游戏工具类 - 提供通用的游戏工具函数
    /// </summary>
    public static class KukuGameUtils
    {
        /// <summary>
        /// 随机数生成器
        /// </summary>
        private static System.Random random = new System.Random();
        
        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        public static float Distance(Vector3 pos1, Vector3 pos2)
        {
            return Vector3.Distance(pos1, pos2);
        }
        
        /// <summary>
        /// 生成指定范围内的随机整数
        /// </summary>
        public static int RandomInt(int min, int max)
        {
            return UnityEngine.Random.Range(min, max + 1);
        }
        
        /// <summary>
        /// 生成指定范围内的随机浮点数
        /// </summary>
        public static float RandomFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        
        /// <summary>
        /// 生成指定概率的布尔值
        /// </summary>
        public static bool Chance(float probability)
        {
            return UnityEngine.Random.value <= probability;
        }
        
        /// <summary>
        /// 从列表中随机选择一个元素
        /// </summary>
        public static T RandomChoice<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
                
            int index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }
        
        /// <summary>
        /// 从数组中随机选择一个元素
        /// </summary>
        public static T RandomChoice<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return default(T);
                
            int index = UnityEngine.Random.Range(0, array.Length);
            return array[index];
        }
        
        /// <summary>
        /// 洗牌算法 - 随机打乱列表
        /// </summary>
        public static void Shuffle<T>(List<T> list)
        {
            if (list == null) return;
            
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        /// <summary>
        /// 格式化数字显示（如：10000 显示为 10K）
        /// </summary>
        public static string FormatNumber(long number)
        {
            if (number >= 1000000000) // 10亿
                return (number / 1000000000D).ToString("0.0") + "B";
            if (number >= 1000000) // 100万
                return (number / 1000000D).ToString("0.0") + "M";
            if (number >= 1000) // 1千
                return (number / 1000D).ToString("0.0") + "K";
            
            return number.ToString("#,0");
        }
        
        /// <summary>
        /// 格式化时间显示（如：3665秒显示为 1小时1分5秒）
        /// </summary>
        public static string FormatTime(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            
            if (hours > 0)
                return $"{hours}小时{minutes}分{seconds}秒";
            else if (minutes > 0)
                return $"{minutes}分{seconds}秒";
            else
                return $"{seconds}秒";
        }
        
        /// <summary>
        /// 计算KuKu的稀有度颜色
        /// </summary>
        public static Color GetKukuRarityColor(KukuData.RarityType rarity)
        {
            switch (rarity)
            {
                case KukuData.RarityType.Common:
                    return Color.white;
                case KukuData.RarityType.Rare:
                    return Color.blue;
                case KukuData.RarityType.Epic:
                    return Color.magenta;
                case KukuData.RarityType.Legendary:
                    return Color.yellow;
                case KukuData.RarityType.Mythic:
                    return Color.red;
                default:
                    return Color.gray;
            }
        }
        
        /// <summary>
        /// 计算神话KuKu的稀有度颜色
        /// </summary>
        public static Color GetMythicalKukuRarityColor(MythicalKukuData.MythicalRarity rarity)
        {
            switch (rarity)
            {
                case MythicalKukuData.MythicalRarity.Celestial:
                    return Color.blue;
                case MythicalKukuData.MythicalRarity.Immortal:
                    return Color.cyan;
                case MythicalKukuData.MythicalRarity.DivineBeast:
                    return Color.magenta;
                case MythicalKukuData.MythicalRarity.Sacred:
                    return Color.yellow;
                case MythicalKukuData.MythicalRarity.Primordial:
                    return Color.red;
                default:
                    return Color.gray;
            }
        }
        
        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public static string GetRarityName(KukuData.RarityType rarity)
        {
            switch (rarity)
            {
                case KukuData.RarityType.Common:
                    return "普通";
                case KukuData.RarityType.Rare:
                    return "稀有";
                case KukuData.RarityType.Epic:
                    return "史诗";
                case KukuData.RarityType.Legendary:
                    return "传说";
                case KukuData.RarityType.Mythic:
                    return "神话";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 获取神话KuKu稀有度名称
        /// </summary>
        public static string GetMythicalRarityName(MythicalKukuData.MythicalRarity rarity)
        {
            switch (rarity)
            {
                case MythicalKukuData.MythicalRarity.Celestial:
                    return "天界";
                case MythicalKukuData.MythicalRarity.Immortal:
                    return "仙人";
                case MythicalKukuData.MythicalRarity.DivineBeast:
                    return "神兽";
                case MythicalKukuData.MythicalRarity.Sacred:
                    return "圣者";
                case MythicalKukuData.MythicalRarity.Primordial:
                    return "元始";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 计算两点之间的角度（以度为单位）
        /// </summary>
        public static float AngleBetween(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        
        /// <summary>
        /// 检查点是否在圆形区域内
        /// </summary>
        public static bool IsInCircle(Vector3 center, float radius, Vector3 point)
        {
            return Vector3.Distance(center, point) <= radius;
        }
        
        /// <summary>
        /// 检查点是否在矩形区域内
        /// </summary>
        public static bool IsInRectangle(Vector3 center, Vector2 size, Vector3 point)
        {
            float halfWidth = size.x / 2f;
            float halfHeight = size.y / 2f;
            
            return point.x >= center.x - halfWidth && 
                   point.x <= center.x + halfWidth && 
                   point.y >= center.y - halfHeight && 
                   point.y <= center.y + halfHeight;
        }
        
        /// <summary>
        /// 限制值在指定范围内
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }
        
        /// <summary>
        /// 限制值在指定范围内（整数版）
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }
        
        /// <summary>
        /// 插值计算
        /// </summary>
        public static float Lerp(float from, float to, float t)
        {
            return Mathf.Lerp(from, to, t);
        }
        
        /// <summary>
        /// 平滑插值计算
        /// </summary>
        public static float SmoothStep(float from, float to, float t)
        {
            return Mathf.SmoothStep(from, to, t);
        }
        
        /// <summary>
        /// 计算百分比
        /// </summary>
        public static float Percentage(float value, float total)
        {
            if (total == 0) return 0;
            return (value / total) * 100f;
        }
        
        /// <summary>
        /// 计算百分比（整数版）
        /// </summary>
        public static int Percentage(int value, int total)
        {
            if (total == 0) return 0;
            return (int)((value / (float)total) * 100f);
        }
        
        /// <summary>
        /// 计算两个数值之间的差值
        /// </summary>
        public static float Difference(float a, float b)
        {
            return Mathf.Abs(a - b);
        }
        
        /// <summary>
        /// 计算两个数值之间的差值（整数版）
        /// </summary>
        public static int Difference(int a, int b)
        {
            return Mathf.Abs(a - b);
        }
        
        /// <summary>
        /// 检查数值是否在误差范围内相等
        /// </summary>
        public static bool Approximately(float a, float b, float tolerance = 0.001f)
        {
            return Mathf.Abs(a - b) < tolerance;
        }
        
        /// <summary>
        /// 将数值映射到另一个范围
        /// </summary>
        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }
        
        /// <summary>
        /// 计算加权随机选择
        /// </summary>
        public static T WeightedRandomChoice<T>(Dictionary<T, float> weightedItems)
        {
            if (weightedItems == null || weightedItems.Count == 0)
                return default(T);
                
            float totalWeight = 0f;
            foreach (var item in weightedItems.Values)
            {
                totalWeight += item;
            }
            
            if (totalWeight <= 0)
                return default(T);
                
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var item in weightedItems)
            {
                currentWeight += item.Value;
                if (randomValue <= currentWeight)
                {
                    return item.Key;
                }
            }
            
            // 如果没有找到合适的项，返回最后一个
            var lastItem = new List<T>(weightedItems.Keys)[weightedItems.Count - 1];
            return lastItem;
        }
        
        /// <summary>
        /// 生成唯一的ID
        /// </summary>
        public static int GenerateUniqueId()
        {
            return UnityEngine.Random.Range(10000, 99999);
        }
        
        /// <summary>
        /// 生成带时间戳的唯一ID
        /// </summary>
        public static string GenerateTimestampedId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() + 
                   UnityEngine.Random.Range(100, 999).ToString();
        }
        
        /// <summary>
        /// 深拷贝KuKu数据
        /// </summary>
        public static KukuData DeepCopyKukuData(KukuData original)
        {
            if (original == null) return null;
            
            return new KukuData
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                Rarity = original.Rarity,
                AttackPower = original.AttackPower,
                DefensePower = original.DefensePower,
                Speed = original.Speed,
                Health = original.Health,
                SpriteName = original.SpriteName,
                Tint = original.Tint,
                IsCollected = original.IsCollected,
                Level = original.Level,
                Experience = original.Experience,
                SkillName = original.SkillName,
                SkillDamage = original.SkillDamage,
                SkillCooldown = original.SkillCooldown,
                CaptureDifficulty = original.CaptureDifficulty,
                CanAbsorbSoul = original.CanAbsorbSoul,
                SoulAbsorptionRate = original.SoulAbsorptionRate
            };
        }
        
        /// <summary>
        /// 深拷贝神话KuKu数据
        /// </summary>
        public static MythicalKukuData DeepCopyMythicalKukuData(MythicalKukuData original)
        {
            if (original == null) return null;
            
            return new MythicalKukuData
            {
                Id = original.Id,
                Name = original.Name,
                Description = original.Description,
                MythologicalBackground = original.MythologicalBackground,
                Element = original.Element,
                SkillType = original.SkillType,
                SkillDescription = original.SkillDescription,
                SkillRange = original.SkillRange,
                SkillPower = original.SkillPower,
                AttackPower = original.AttackPower,
                DefensePower = original.DefensePower,
                Speed = original.Speed,
                Health = original.Health,
                DivinePower = original.DivinePower,
                ProtectionPower = original.ProtectionPower,
                PurificationPower = original.PurificationPower,
                Rarity = original.Rarity,
                EvolutionLevel = original.EvolutionLevel,
                EvolutionProgress = original.EvolutionProgress,
                EvolutionStonesRequired = original.EvolutionStonesRequired,
                CanAbsorbSoul = original.CanAbsorbSoul,
                SoulAbsorptionRate = original.SoulAbsorptionRate,
                CanFuseWithRobots = original.CanFuseWithRobots,
                FusionCompatibility = original.FusionCompatibility,
                Level = original.Level,
                Experience = original.Experience,
                SpriteName = original.SpriteName,
                MaxEquipmentSlots = original.MaxEquipmentSlots,
                IsFavorite = original.IsFavorite,
                CaptureDate = original.CaptureDate,
                CaptureLocation = original.CaptureLocation,
                CaptureDifficulty = original.CaptureDifficulty
            };
        }
        
        /// <summary>
        /// 计算经验升级所需的经验值
        /// </summary>
        public static int GetExpForLevel(int level)
        {
            // 使用公式: Exp = Base * Level * (Level + 1) / 2
            int baseExp = 100;
            return baseExp * level * (level + 1) / 2;
        }
        
        /// <summary>
        /// 计算当前等级的经验值需求
        /// </summary>
        public static int GetExpForCurrentLevel(int currentLevel)
        {
            return GetExpForLevel(currentLevel) - GetExpForLevel(currentLevel - 1);
        }
        
        /// <summary>
        /// 计算经验百分比
        /// </summary>
        public static float GetExpPercentage(int currentExp, int currentLevel)
        {
            int expForNextLevel = GetExpForCurrentLevel(currentLevel);
            if (expForNextLevel <= 0) return 100f;
            
            return (float)currentExp / expForNextLevel * 100f;
        }
        
        /// <summary>
        /// 获取当前等级的进度信息
        /// </summary>
        public static (int current, int required, float percentage) GetLevelProgress(int currentExp, int currentLevel)
        {
            int expForNextLevel = GetExpForCurrentLevel(currentLevel);
            float percentage = expForNextLevel > 0 ? (float)currentExp / expForNextLevel * 100f : 100f;
            
            return (currentExp, expForNextLevel, percentage);
        }
        
        /// <summary>
        /// 将字符串转换为颜色
        /// </summary>
        public static Color StringToColor(string colorString)
        {
            Color color = Color.white;
            ColorUtility.TryParseHtmlString(colorString, out color);
            return color;
        }
        
        /// <summary>
        /// 将颜色转换为十六进制字符串
        /// </summary>
        public static string ColorToHexString(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        
        /// <summary>
        /// 计算两点之间的方向向量
        /// </summary>
        public static Vector3 DirectionTo(Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }
        
        /// <summary>
        /// 获取枚举的所有值
        /// </summary>
        public static T[] GetEnumValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
        
        /// <summary>
        /// 获取枚举的值数量
        /// </summary>
        public static int GetEnumCount<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Length;
        }
    }
}