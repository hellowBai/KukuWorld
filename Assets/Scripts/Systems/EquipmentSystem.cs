using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 装备系统 - 管理装备合成逻辑、装备穿戴、属性加成等
    /// </summary>
    public static class EquipmentSystem
    {
        /// <summary>
        /// 合成结果
        /// </summary>
        public struct CombineResult
        {
            public bool success;
            public WeaponData combinedWeapon;
            public string message;
        }

        /// <summary>
        /// 装备结果
        /// </summary>
        public struct EquipResult
        {
            public bool success;
            public string message;
        }

        /// <summary>
        /// 商店物品
        /// </summary>
        public struct ShopItem
        {
            public WeaponData item;
            public int price;
            public bool isAvailable;
        }

        // 商店库存
        private static List<WeaponData> shopInventory;

        static EquipmentSystem()
        {
            InitializeShopInventory();
        }

        /// <summary>
        /// 初始化商店库存
        /// </summary>
        private static void InitializeShopInventory()
        {
            shopInventory = new List<WeaponData>();

            // 添加基础武器到商店
            AddBasicWeaponsToShop();
            AddAdvancedWeaponsToShop();
            AddRareWeaponsToShop();
        }

        /// <summary>
        /// 添加基础武器到商店
        /// </summary>
        private static void AddBasicWeaponsToShop()
        {
            // 基础剑
            WeaponData basicSword = new WeaponData
            {
                Name = "铁剑",
                Description = "一把普通的铁剑，适合初学者使用",
                Type = WeaponData.WeaponType.Sword,
                Tier = WeaponData.WeaponTier.Basic,
                AttackBonus = 10f,
                DefenseBonus = 2f,
                Slot = WeaponData.EquipmentSlot.Weapon,
                Price = 100,
                SpriteName = "IronSword"
            };
            shopInventory.Add(basicSword);

            // 基础盾
            WeaponData basicShield = new WeaponData
            {
                Name = "木盾",
                Description = "一面简单的木盾，提供基本防护",
                Type = WeaponData.WeaponType.Shield,
                Tier = WeaponData.WeaponTier.Basic,
                AttackBonus = 1f,
                DefenseBonus = 8f,
                Slot = WeaponData.EquipmentSlot.Shield,
                Price = 80,
                SpriteName = "WoodShield"
            };
            shopInventory.Add(basicShield);

            // 基础护甲
            WeaponData basicArmor = new WeaponData
            {
                Name = "皮甲",
                Description = "轻便的皮甲，提供基础防护",
                Type = WeaponData.WeaponType.Armor,
                Tier = WeaponData.WeaponTier.Basic,
                AttackBonus = 0f,
                DefenseBonus = 5f,
                HealthBonus = 20f,
                Slot = WeaponData.EquipmentSlot.Body,
                Price = 120,
                SpriteName = "LeatherArmor"
            };
            shopInventory.Add(basicArmor);
        }

        /// <summary>
        /// 添加高级武器到商店
        /// </summary>
        private static void AddAdvancedWeaponsToShop()
        {
            // 高级剑
            WeaponData advancedSword = new WeaponData
            {
                Name = "银剑",
                Description = "锋利的银剑，附带轻微魔法效果",
                Type = WeaponData.WeaponType.Sword,
                Tier = WeaponData.WeaponTier.Advanced,
                AttackBonus = 25f,
                DefenseBonus = 5f,
                SpeedBonus = 1f,
                Slot = WeaponData.EquipmentSlot.Weapon,
                Price = 300,
                SpriteName = "SilverSword"
            };
            shopInventory.Add(advancedSword);

            // 高级护甲
            WeaponData advancedArmor = new WeaponData
            {
                Name = "链甲",
                Description = "坚固的链甲，提供优秀防护",
                Type = WeaponData.WeaponType.Armor,
                Tier = WeaponData.WeaponTier.Advanced,
                AttackBonus = 2f,
                DefenseBonus = 15f,
                HealthBonus = 50f,
                Slot = WeaponData.EquipmentSlot.Body,
                Price = 400,
                SpriteName = "ChainArmor"
            };
            shopInventory.Add(advancedArmor);
        }

        /// <summary>
        /// 添加稀有武器到商店
        /// </summary>
        private static void AddRareWeaponsToShop()
        {
            // 稀有武器
            WeaponData rareSword = new WeaponData
            {
                Name = "火焰之刃",
                Description = "燃烧着烈焰的神剑，威力巨大",
                Type = WeaponData.WeaponType.Sword,
                Tier = WeaponData.WeaponTier.Rare,
                AttackBonus = 45f,
                DefenseBonus = 8f,
                SpecialEffect = 10f, // 火焰伤害
                Slot = WeaponData.EquipmentSlot.Weapon,
                RequiredLevel = 5,
                Price = 800,
                SpriteName = "FlameBlade"
            };
            shopInventory.Add(rareSword);

            // 稀有护盾
            WeaponData rareShield = new WeaponData
            {
                Name = "圣光之盾",
                Description = "散发着圣光的护盾，不仅能防护还能治愈",
                Type = WeaponData.WeaponType.Shield,
                Tier = WeaponData.WeaponTier.Rare,
                AttackBonus = 5f,
                DefenseBonus = 35f,
                HealthBonus = 100f,
                SpecialEffect = 5f, // 治疗效果
                Slot = WeaponData.EquipmentSlot.Shield,
                RequiredLevel = 6,
                Price = 1000,
                SpriteName = "HolyShield"
            };
            shopInventory.Add(rareShield);
        }

        /// <summary>
        /// 合成武器
        /// </summary>
        public static CombineResult CombineWeapons(WeaponData component1, WeaponData component2, string customName = "")
        {
            if (component1 == null || component2 == null)
            {
                return new CombineResult { success = false, message = "合成材料不能为空" };
            }

            // 检查是否可以合成
            if (!CanCombine(component1, component2))
            {
                return new CombineResult { success = false, message = "这两个物品无法合成" };
            }

            // 计算合成成功率
            float successRate = CalculateCombineSuccessRate(component1, component2);
            bool isSuccess = UnityEngine.Random.value <= successRate;

            if (isSuccess)
            {
                // 合成成功，创建新武器
                WeaponData combinedWeapon = CreateCombinedWeapon(component1, component2, customName);

                string message = $"合成成功！{component1.Name}与{component2.Name}合成了{combinedWeapon.Name}！";
                return new CombineResult { success = true, combinedWeapon = combinedWeapon, message = message };
            }
            else
            {
                // 合成失败，消耗材料
                string message = $"合成失败！{component1.Name}与{component2.Name}合成失败，材料被消耗。";
                return new CombineResult { success = false, message = message };
            }
        }

        /// <summary>
        /// 为单位装备武器
        /// </summary>
        public static EquipResult EquipWeapon(object unit, WeaponData weapon)
        {
            if (unit == null || weapon == null)
            {
                return new EquipResult { success = false, message = "单位或装备不能为空" };
            }

            // 检查单位是否可以装备
            if (!CanEquip(unit, weapon))
            {
                return new EquipResult { success = false, message = "该单位无法装备此物品" };
            }

            // 根据单位类型应用装备
            if (unit is UnitData)
            {
                UnitData gameUnit = unit as UnitData;
                if (gameUnit.EquipItem(weapon))
                {
                    return new EquipResult { success = true, message = $"{gameUnit.Name} 成功装备了 {weapon.Name}" };
                }
                else
                {
                    return new EquipResult { success = false, message = "装备失败，可能是因为装备槽已满" };
                }
            }
            else if (unit is MythicalKukuData)
            {
                // 如果是KuKu，需要特殊处理
                MythicalKukuData kuku = unit as MythicalKukuData;
                if (kuku.MaxEquipmentSlots > 0) // 只有能装备的KuKu才能装备
                {
                    // 注意：MythicalKukuData本身可能没有装备系统，这里只是示例
                    return new EquipResult { success = true, message = $"{kuku.Name} 尝试装备了 {weapon.Name}" };
                }
                else
                {
                    return new EquipResult { success = false, message = "该KuKu无法装备物品" };
                }
            }
            else
            {
                return new EquipResult { success = false, message = "不支持的单位类型" };
            }
        }

        /// <summary>
        /// 卸下装备
        /// </summary>
        public static bool UnequipWeapon(object unit, WeaponData weapon)
        {
            if (unit == null || weapon == null)
                return false;

            if (unit is UnitData)
            {
                UnitData gameUnit = unit as UnitData;
                return gameUnit.UnequipItem(weapon);
            }

            return false;
        }

        /// <summary>
        /// 获取可用装备槽位
        /// </summary>
        public static List<WeaponData.EquipmentSlot> GetAvailableSlots(object unit)
        {
            List<WeaponData.EquipmentSlot> slots = new List<WeaponData.EquipmentSlot>();

            if (unit is UnitData)
            {
                UnitData gameUnit = unit as UnitData;
                
                // 根据单位类型确定可用槽位
                if (gameUnit.MaxEquipmentSlots > 0)
                {
                    // 对于可以装备的单位，返回常用槽位
                    slots.Add(WeaponData.EquipmentSlot.Weapon);
                    slots.Add(WeaponData.EquipmentSlot.Body);
                    slots.Add(WeaponData.EquipmentSlot.Head);
                    slots.Add(WeaponData.EquipmentSlot.Legs);
                    slots.Add(WeaponData.EquipmentSlot.Shield);
                    slots.Add(WeaponData.EquipmentSlot.Accessory);
                }
            }

            return slots;
        }

        /// <summary>
        /// 获取兼容武器类型
        /// </summary>
        public static List<WeaponData.WeaponType> GetCompatibleWeaponTypes(object unit)
        {
            List<WeaponData.WeaponType> types = new List<WeaponData.WeaponType>();

            if (unit is UnitData)
            {
                UnitData gameUnit = unit as UnitData;
                
                // 根据单位类型确定兼容的武器类型
                types.Add(WeaponData.WeaponType.Sword);
                types.Add(WeaponData.WeaponType.Cannon);
                types.Add(WeaponData.WeaponType.Laser);
                types.Add(WeaponData.WeaponType.Armor);
                types.Add(WeaponData.WeaponType.Shield);
            }
            else if (unit is MythicalKukuData)
            {
                // KuKu可能有特殊的兼容类型
                types.Add(WeaponData.WeaponType.Crystal);
                types.Add(WeaponData.WeaponType.Orb);
                types.Add(WeaponData.WeaponType.Ring);
                types.Add(WeaponData.WeaponType.Necklace);
            }

            return types;
        }

        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public static bool CanCombine(WeaponData weapon1, WeaponData weapon2)
        {
            if (weapon1 == null || weapon2 == null)
                return false;

            // 检查两个武器是否都可以合成
            if (!weapon1.IsCombinable || !weapon2.IsCombinable)
                return false;

            // 检查稀有度是否合适（一般只能合成相近稀有度的物品）
            int tierDiff = Math.Abs((int)weapon1.Tier - (int)weapon2.Tier);
            if (tierDiff > 1)
                return false;

            // 检查是否为同类型或兼容类型
            if (weapon1.Type != weapon2.Type)
            {
                // 允许某些特定类型的组合
                if (!((weapon1.Type == WeaponData.WeaponType.Sword && weapon2.Type == WeaponData.WeaponType.Sword) ||
                      (weapon1.Type == WeaponData.WeaponType.Armor && weapon2.Type == WeaponData.WeaponType.Armor) ||
                      (weapon1.Type == WeaponData.WeaponType.Shield && weapon2.Type == WeaponData.WeaponType.Shield)))
                {
                    // 检查是否为允许的跨类型组合
                    if (!IsCompatibleCombination(weapon1, weapon2))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查是否为兼容的组合
        /// </summary>
        private static bool IsCompatibleCombination(WeaponData weapon1, WeaponData weapon2)
        {
            // 定义允许的跨类型组合
            var allowedCombinations = new List<(WeaponData.WeaponType, WeaponData.WeaponType)>
            {
                (WeaponData.WeaponType.Sword, WeaponData.WeaponType.Crystal),
                (WeaponData.WeaponType.Crystal, WeaponData.WeaponType.Sword),
                (WeaponData.WeaponType.Armor, WeaponData.WeaponType.Crystal),
                (WeaponData.WeaponType.Crystal, WeaponData.WeaponType.Armor),
                (WeaponData.WeaponType.Shield, WeaponData.WeaponType.Crystal),
                (WeaponData.WeaponType.Crystal, WeaponData.WeaponType.Shield)
            };

            return allowedCombinations.Contains((weapon1.Type, weapon2.Type));
        }

        /// <summary>
        /// 检查单位是否可以装备
        /// </summary>
        public static bool CanEquip(object unit, WeaponData weapon)
        {
            if (unit == null || weapon == null)
                return false;

            if (unit is UnitData)
            {
                UnitData gameUnit = unit as UnitData;
                
                // 检查单位是否有可用的装备槽位
                if (gameUnit.MaxEquipmentSlots <= 0)
                    return false;

                // 检查单位等级是否满足装备要求
                if (gameUnit.Level < weapon.RequiredLevel)
                    return false;

                // 检查装备槽位是否还有空间
                return gameUnit.CanEquipItem(weapon);
            }
            else if (unit is MythicalKukuData)
            {
                MythicalKukuData kuku = unit as MythicalKukuData;
                
                // 检查KuKu是否可以装备物品
                return kuku.MaxEquipmentSlots > 0;
            }

            return false;
        }

        /// <summary>
        /// 获取商店可用物品
        /// </summary>
        public static List<ShopItem> GetShopInventory()
        {
            List<ShopItem> items = new List<ShopItem>();

            foreach (var weapon in shopInventory)
            {
                ShopItem item = new ShopItem
                {
                    item = weapon,
                    price = weapon.Price,
                    isAvailable = true
                };
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// 购买商店物品
        /// </summary>
        public static bool PurchaseItem(WeaponData item, int price)
        {
            if (item == null)
                return false;

            // 这里需要连接到玩家数据来检查是否有足够金币
            // 为了演示目的，我们假设购买总是成功的
            return true;
        }

        /// <summary>
        /// 更新商店库存
        /// </summary>
        public static void RefreshShopInventory()
        {
            shopInventory.Clear();
            InitializeShopInventory();
        }

        /// <summary>
        /// 计算合成成功率
        /// </summary>
        private static float CalculateCombineSuccessRate(WeaponData weapon1, WeaponData weapon2)
        {
            // 基础成功率
            float baseRate = 0.7f;

            // 根据稀有度调整
            float tierAdjustment = 0.1f - (Math.Abs((int)weapon1.Tier - (int)weapon2.Tier) * 0.05f);

            // 根据等级调整
            float levelAdjustment = 0.05f * Math.Min((int)weapon1.Tier, (int)weapon2.Tier);

            // 计算最终成功率
            float finalRate = baseRate + tierAdjustment + levelAdjustment;

            return Mathf.Clamp01(finalRate);
        }

        /// <summary>
        /// 创建合成后的武器
        /// </summary>
        private static WeaponData CreateCombinedWeapon(WeaponData weapon1, WeaponData weapon2, string customName)
        {
            WeaponData combined = new WeaponData
            {
                Name = string.IsNullOrEmpty(customName) ? 
                    $"{weapon1.Name}·{weapon2.Name}" : customName,
                Description = $"由{weapon1.Name}和{weapon2.Name}合成的强化装备",
                Type = weapon1.Type, // 保留第一个武器的类型
                Tier = (WeaponData.WeaponTier)Mathf.Max((int)weapon1.Tier, (int)weapon2.Tier), // 取较高等级
                AttackBonus = (weapon1.AttackBonus + weapon2.AttackBonus) * 1.2f, // 攻击力提升20%
                DefenseBonus = (weapon1.DefenseBonus + weapon2.DefenseBonus) * 1.2f, // 防御力提升20%
                SpeedBonus = (weapon1.SpeedBonus + weapon2.SpeedBonus) * 1.1f, // 速度提升10%
                HealthBonus = (weapon1.HealthBonus + weapon2.HealthBonus) * 1.1f, // 血量提升10%
                RangeBonus = (weapon1.RangeBonus + weapon2.RangeBonus) * 1.1f, // 射程提升10%
                SpecialEffect = (weapon1.SpecialEffect + weapon2.SpecialEffect) * 1.3f, // 特殊效果提升30%
                Slot = weapon1.Slot, // 保留第一个武器的槽位
                IsCombinable = false, // 合成后的武器通常不能再合成
                CombineCost = (weapon1.CombineCost + weapon2.CombineCost) * 2, // 合成成本翻倍
                RequiredTierToCombine = Math.Max(weapon1.RequiredTierToCombine, weapon2.RequiredTierToCombine),
                ComponentsNeeded = Math.Max(weapon1.ComponentsNeeded, weapon2.ComponentsNeeded),
                EquipCost = (weapon1.EquipCost + weapon2.EquipCost) * 1.2f, // 装备成本提升20%
                RequiredLevel = Math.Max(weapon1.RequiredLevel, weapon2.RequiredLevel), // 需要较高等级
                IsEquipped = false, // 新武器未装备
                Durability = (weapon1.Durability + weapon2.Durability) / 2, // 平均耐久度
                MaxDurability = (int)((weapon1.MaxDurability + weapon2.MaxDurability) * 1.1f), // 最大耐久度提升10%
                Price = (int)((weapon1.Price + weapon2.Price) * 1.8f), // 价格提升80%
                SpriteName = $"Combined_{weapon1.SpriteName}", // 新的精灵名称
                Tint = Color.Lerp(weapon1.Tint, weapon2.Tint, 0.5f), // 混合颜色
                EffectDuration = Math.Max(weapon1.EffectDuration, weapon2.EffectDuration) * 1.2f, // 效果持续时间提升
                CooldownReduction = (weapon1.CooldownReduction + weapon2.CooldownReduction) * 1.1f, // 冷却缩减提升
                CriticalChance = (weapon1.CriticalChance + weapon2.CriticalChance) * 1.1f, // 暴击率提升
                CriticalDamage = (weapon1.CriticalDamage + weapon2.CriticalDamage) / 2, // 平均暴击伤害
                IsUnique = weapon1.IsUnique || weapon2.IsUnique, // 如果任一是唯一的，则合成后也是唯一的
                SetName = string.IsNullOrEmpty(weapon1.SetName) ? weapon2.SetName : weapon1.SetName, // 继承套装名
                SetPieceCount = Math.Max(weapon1.SetPieceCount, weapon2.SetPieceCount), // 套装件数取大值
                SpecialAbility = $"{weapon1.SpecialAbility} + {weapon2.SpecialAbility}" // 组合特殊能力
            };

            // 如果合成后稀有度提升了，增加额外属性
            if ((int)combined.Tier > Math.Max((int)weapon1.Tier, (int)weapon2.Tier))
            {
                combined.AttackBonus *= 1.3f;
                combined.DefenseBonus *= 1.3f;
                combined.SpecialEffect *= 1.4f;
            }

            return combined;
        }

        /// <summary>
        /// 强化装备
        /// </summary>
        public static CombineResult EnhanceWeapon(WeaponData weapon, int enhancementLevel = 1)
        {
            if (weapon == null)
            {
                return new CombineResult { success = false, message = "装备不能为空" };
            }

            // 计算强化成功率（强化等级越高，成功率越低）
            float successRate = 1.0f - (enhancementLevel * 0.15f);
            bool isSuccess = UnityEngine.Random.value <= successRate;

            if (isSuccess)
            {
                // 强化成功，提升装备属性
                WeaponData enhancedWeapon = weapon.Clone();
                
                // 根据强化等级提升属性
                float enhancementMultiplier = 1.0f + (enhancementLevel * 0.1f);
                enhancedWeapon.AttackBonus *= enhancementMultiplier;
                enhancedWeapon.DefenseBonus *= enhancementMultiplier;
                enhancedWeapon.HealthBonus *= enhancementMultiplier;
                enhancedWeapon.SpecialEffect *= enhancementMultiplier;
                
                // 强化后可能提升稀有度
                if (enhancementLevel >= 3 && enhancedWeapon.Tier < WeaponData.WeaponTier.Legendary)
                {
                    enhancedWeapon.Tier = (WeaponData.WeaponTier)((int)enhancedWeapon.Tier + 1);
                }

                enhancedWeapon.Name = $"(+{enhancementLevel}){weapon.Name}";

                string message = $"强化成功！{weapon.Name} 强化至 +{enhancementLevel}，属性得到提升！";
                return new CombineResult { success = true, combinedWeapon = enhancedWeapon, message = message };
            }
            else
            {
                // 强化失败，装备可能降级或损坏
                string message = $"强化失败！{weapon.Name} 强化失败{(enhancementLevel > 1 ? "，装备降级" : "")}。";
                return new CombineResult { success = false, message = message };
            }
        }

        /// <summary>
        /// 分解装备获取材料
        /// </summary>
        public static (List<WeaponData> materials, string message) DisassembleWeapon(WeaponData weapon)
        {
            if (weapon == null)
            {
                return (null, "装备不能为空");
            }

            List<WeaponData> materials = new List<WeaponData>();

            // 根据装备的稀有度和属性分解为相应材料
            int materialCount = Mathf.Max(1, (int)weapon.Tier + 1);
            for (int i = 0; i < materialCount; i++)
            {
                WeaponData material = new WeaponData
                {
                    Name = "装备碎片",
                    Description = "分解装备获得的材料",
                    Type = WeaponData.WeaponType.Crystal, // 使用水晶作为通用材料
                    Tier = WeaponData.WeaponTier.Basic,
                    AttackBonus = 1f,
                    DefenseBonus = 1f,
                    Price = 10,
                    IsCombinable = true
                };
                materials.Add(material);
            }

            string message = $"{weapon.Name} 已分解，获得 {materialCount} 个装备碎片。";
            return (materials, message);
        }

        /// <summary>
        /// 获取装备属性总览
        /// </summary>
        public static string GetEquipmentSummary(WeaponData weapon)
        {
            if (weapon == null) return "无装备";

            return $"{weapon.Name}\n" +
                   $"类型: {weapon.GetTypeName()}\n" +
                   $"稀有度: {weapon.GetTierName()}\n" +
                   $"攻击力: +{weapon.AttackBonus:F1}\n" +
                   $"防御力: +{weapon.DefenseBonus:F1}\n" +
                   $"生命值: +{weapon.HealthBonus:F1}\n" +
                   $"速度: +{weapon.SpeedBonus:F1}\n" +
                   $"特殊效果: +{weapon.SpecialEffect:F1}";
        }
    }
}