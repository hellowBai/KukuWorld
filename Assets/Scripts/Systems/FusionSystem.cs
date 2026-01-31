using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 融合系统 - 处理各类融合逻辑
    /// </summary>
    public static class FusionSystem
    {
        /// <summary>
        /// 融合结果
        /// </summary>
        public struct FusionResult
        {
            public bool success;
            public object fusedObject;
            public string message;
            public int equipmentSlots;        // 融合后可用的装备槽位数（通常是6个）
            
            public FusionResult(bool isSuccess, object obj, string msg, int slots = 0)
            {
                success = isSuccess;
                fusedObject = obj;
                message = msg;
                equipmentSlots = slots;
            }
        }

        /// <summary>
        /// 融合配方
        /// </summary>
        public struct FusionRecipe
        {
            public string resultName;
            public int fusionCost;
            public string description;
            public FusionType type;                               // 融合类型
            public int maxEquipmentSlots;                         // 融合后装备槽数量
            
            public enum FusionType { 
                KukuRobot,                                        // KuKu+机器人
                RobotRobot,                                       // 机器人+机器人
                KukuKuku,                                         // KuKu+KuKu
                UnitUnit,                                         // 单位+单位
                WeaponWeapon,                                     // 武器+武器
                BuildingBuilding                                  // 建筑+建筑
            }
        }

        // 预定义的融合配方
        private static List<FusionRecipe> predefinedRecipes;

        static FusionSystem()
        {
            InitializeRecipes();
        }

        /// <summary>
        /// 初始化融合配方
        /// </summary>
        private static void InitializeRecipes()
        {
            predefinedRecipes = new List<FusionRecipe>();

            // KuKu与机器人融合配方
            predefinedRecipes.Add(new FusionRecipe
            {
                resultName = "KuKu融合体",
                fusionCost = 500,
                description = "将神话KuKu与高级机器人融合，创造强大的战斗单位",
                type = FusionRecipe.FusionType.KukuRobot,
                maxEquipmentSlots = 6
            });

            // 机器人与机器人融合配方
            predefinedRecipes.Add(new FusionRecipe
            {
                resultName = "高级机器人",
                fusionCost = 300,
                description = "将两个相同类型的机器人融合，提升性能",
                type = FusionRecipe.FusionType.RobotRobot,
                maxEquipmentSlots = 2
            });

            // KuKu与KuKu融合配方
            predefinedRecipes.Add(new FusionRecipe
            {
                resultName = "复合KuKu",
                fusionCost = 400,
                description = "将两个KuKu融合，获得新的能力",
                type = FusionRecipe.FusionType.KukuKuku,
                maxEquipmentSlots = 0
            });
        }

        /// <summary>
        /// 尝试融合两个对象
        /// </summary>
        public static FusionResult FuseObjects(object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                return new FusionResult(false, null, "融合对象不能为空");
            }

            // 根据对象类型决定融合方式
            if (obj1 is MythicalKukuData && obj2 is UnitData)
            {
                return FuseKukuWithRobot(obj1 as MythicalKukuData, obj2 as UnitData);
            }
            else if (obj1 is UnitData && obj2 is MythicalKukuData)
            {
                return FuseKukuWithRobot(obj2 as MythicalKukuData, obj1 as UnitData);
            }
            else if (obj1 is UnitData && obj2 is UnitData)
            {
                return FuseUnits(obj1 as UnitData, obj2 as UnitData);
            }
            else if (obj1 is MythicalKukuData && obj2 is MythicalKukuData)
            {
                return FuseKukus(obj1 as MythicalKukuData, obj2 as MythicalKukuData);
            }
            else if (obj1 is WeaponData && obj2 is WeaponData)
            {
                return FuseWeapons(obj1 as WeaponData, obj2 as WeaponData);
            }
            else if (obj1 is BuildingData && obj2 is BuildingData)
            {
                return FuseBuildings(obj1 as BuildingData, obj2 as BuildingData);
            }
            else
            {
                return new FusionResult(false, null, "不支持的对象类型融合");
            }
        }

        /// <summary>
        /// KuKu与机器人融合
        /// </summary>
        public static FusionResult FuseKukuWithRobot(MythicalKukuData kuku, UnitData robot, string customName = "")
        {
            if (kuku == null || robot == null)
            {
                return new FusionResult(false, null, "融合对象不能为空");
            }

            // 检查KuKu是否可以融合
            if (!kuku.CanFuseWithRobots)
            {
                return new FusionResult(false, null, "该KuKu无法与机器人融合");
            }

            // 检查KuKu是否达到融合要求的进化等级
            if (kuku.EvolutionLevel < 5)
            {
                return new FusionResult(false, null, $"KuKu需要达到第5进化等级才能融合，当前为第{kuku.EvolutionLevel}级");
            }

            // 检查机器人是否达到融合要求的等级
            if (robot.Level < 10)
            {
                return new FusionResult(false, null, $"机器人需要达到第10等级才能融合，当前为第{robot.Level}级");
            }

            // 检查机器人类型是否支持融合
            if (robot.Type != UnitData.UnitType.Robot && robot.Type != UnitData.UnitType.Tank)
            {
                return new FusionResult(false, null, "只有机器人和坦克类型可以与KuKu融合");
            }

            // 创建融合后的单位
            UnitData fusedUnit = UnitData.FuseKukuWithRobot(kuku, robot, customName);

            if (fusedUnit == null)
            {
                return new FusionResult(false, null, "融合失败，可能是由于兼容性问题");
            }

            string message = $"融合成功！{kuku.Name}与{robot.Name}融合成为强大的{fusedUnit.Name}！";
            return new FusionResult(true, fusedUnit, message, fusedUnit.MaxEquipmentSlots);
        }

        /// <summary>
        /// KuKu与机器人融合（简化版，使用默认名称）
        /// </summary>
        public static FusionResult FuseKukuWithRobot(MythicalKukuData kuku, UnitData robot)
        {
            return FuseKukuWithRobot(kuku, robot, "");
        }

        /// <summary>
        /// 机器人与机器人融合
        /// </summary>
        public static FusionResult FuseUnits(UnitData unit1, UnitData unit2)
        {
            if (unit1 == null || unit2 == null)
            {
                return new FusionResult(false, null, "融合对象不能为空");
            }

            // 检查是否都支持融合
            if (!unit1.CanFuse || !unit2.CanFuse)
            {
                return new FusionResult(false, null, "其中一个单位不支持融合");
            }

            // 检查是否为相同的单位类型（或兼容类型）
            if (unit1.Type != unit2.Type && !(unit1.Type == UnitData.UnitType.Robot && unit2.Type == UnitData.UnitType.Tank) &&
                !(unit1.Type == UnitData.UnitType.Tank && unit2.Type == UnitData.UnitType.Robot))
            {
                return new FusionResult(false, null, "单位类型不兼容，无法融合");
            }

            // 创建融合后的单位
            string fusionName = $"{unit1.Name}融合体";
            UnitData fusedUnit = UnitData.FuseUnits(unit1, unit2, fusionName);

            if (fusedUnit == null)
            {
                return new FusionResult(false, null, "融合失败");
            }

            string message = $"融合成功！{unit1.Name}与{unit2.Name}融合成为{fusedUnit.Name}！";
            return new FusionResult(true, fusedUnit, message, fusedUnit.MaxEquipmentSlots);
        }

        /// <summary>
        /// KuKu与KuKu融合
        /// </summary>
        public static FusionResult FuseKukus(MythicalKukuData kuku1, MythicalKukuData kuku2)
        {
            if (kuku1 == null || kuku2 == null)
            {
                return new FusionResult(false, null, "融合对象不能为空");
            }

            // 检查是否都支持融合
            if (!kuku1.CanFuseWithRobots || !kuku2.CanFuseWithRobots)
            {
                return new FusionResult(false, null, "KuKu需要能够融合才能进行KuKu间融合");
            }

            // 检查进化等级
            if (kuku1.EvolutionLevel < 3 || kuku2.EvolutionLevel < 3)
            {
                return new FusionResult(false, null, "KuKu需要至少达到第3进化等级才能进行KuKu间融合");
            }

            // 创建融合后的KuKu
            MythicalKukuData fusedKuku = kuku1.Clone();
            fusedKuku.Name = $"{kuku1.Name}·{kuku2.Name}";
            fusedKuku.Description = $"由{kuku1.Name}和{kuku2.Name}融合而成的复合KuKu";

            // 融合属性
            fusedKuku.AttackPower = (kuku1.AttackPower + kuku2.AttackPower) * 0.9f; // 略微降低攻击力以保持平衡
            fusedKuku.DefensePower = (kuku1.DefensePower + kuku2.DefensePower) * 0.9f;
            fusedKuku.Health = (kuku1.Health + kuku2.Health) * 1.1f; // 略微提升血量
            fusedKuku.DivinePower = (kuku1.DivinePower + kuku2.DivinePower) * 1.05f;
            fusedKuku.ProtectionPower = (kuku1.ProtectionPower + kuku2.ProtectionPower) * 1.05f;
            fusedKuku.PurificationPower = (kuku1.PurificationPower + kuku2.PurificationPower) * 1.05f;

            // 融合后进化等级提升
            fusedKuku.EvolutionLevel = Mathf.Max(kuku1.EvolutionLevel, kuku2.EvolutionLevel) + 1;
            if (fusedKuku.EvolutionLevel > 5) fusedKuku.EvolutionLevel = 5;

            // 融合兼容性提升
            fusedKuku.FusionCompatibility = (kuku1.FusionCompatibility + kuku2.FusionCompatibility) / 2 * 1.2f;
            if (fusedKuku.FusionCompatibility > 1.0f) fusedKuku.FusionCompatibility = 1.0f;

            // 可能解锁装备槽
            if (fusedKuku.EvolutionLevel >= 5)
            {
                fusedKuku.MaxEquipmentSlots = 6;
            }

            string message = $"融合成功！{kuku1.Name}与{kuku2.Name}融合成为{fusedKuku.Name}！";
            return new FusionResult(true, fusedKuku, message, fusedKuku.MaxEquipmentSlots);
        }

        /// <summary>
        /// 武器与武器融合（合成升级）
        /// </summary>
        public static FusionResult FuseWeapons(WeaponData weapon1, WeaponData weapon2)
        {
            if (weapon1 == null || weapon2 == null)
            {
                return new FusionResult(false, null, "融合对象不能为空");
            }

            // 检查是否都可以合成
            if (!weapon1.IsCombinable || !weapon2.IsCombinable)
            {
                return new FusionResult(false, null, "其中一个武器不可合成");
            }

            // 检查稀有度是否匹配（只能合成相同或相邻稀有度的武器）
            if (Mathf.Abs((int)weapon1.Tier - (int)weapon2.Tier) > 1)
            {
                return new FusionResult(false, null, "武器稀有度过差过大，无法合成");
            }

            // 计算合成成功率
            float successRate = CalculateWeaponFusionSuccessRate(weapon1, weapon2);
            bool success = UnityEngine.Random.value <= successRate;

            if (success)
            {
                // 合成成功，创建新的武器
                WeaponData fusedWeapon = CreateFusedWeapon(weapon1, weapon2);

                string message = $"合成成功！{weapon1.Name}与{weapon2.Name}合成为{fusedWeapon.Name}！";
                return new FusionResult(true, fusedWeapon, message, 0);
            }
            else
            {
                // 合成失败，消耗材料
                string message = $"合成失败！{weapon1.Name}与{weapon2.Name}合成失败，材料被消耗。";
                return new FusionResult(false, null, message, 0);
            }
        }

        /// <summary>
        /// 建筑与建筑融合
        /// </summary>
        public static FusionResult FuseBuildings(BuildingData building1, BuildingData building2)
        {
            if (building1 == null || building2 == null)
            {
                return new FusionResult(false, null, "融合对象不能为空");
            }

            // 检查是否都是相同类型的建筑
            if (building1.Type != building2.Type)
            {
                return new FusionResult(false, null, "只能融合相同类型的建筑");
            }

            // 检查等级要求
            if (building1.Level < 3 || building2.Level < 3)
            {
                return new FusionResult(false, null, "建筑需要至少3级才能融合");
            }

            // 创建融合后的建筑
            BuildingData fusedBuilding = building1.Clone();
            fusedBuilding.Name = $"{building1.Name}融合体";
            fusedBuilding.Description = $"由{building1.Name}和{building2.Name}融合而成的强化建筑";
            fusedBuilding.Level = Mathf.Max(building1.Level, building2.Level) + 1;

            // 提升属性
            fusedBuilding.AttackPower = (building1.AttackPower + building2.AttackPower) * 1.5f;
            fusedBuilding.Health = (building1.Health + building2.Health) * 1.4f;
            fusedBuilding.Range = Mathf.Max(building1.Range, building2.Range) * 1.2f;

            // 增加建造和升级成本
            fusedBuilding.RequiredCoins = (int)((building1.RequiredCoins + building2.RequiredCoins) * 2.0f);
            fusedBuilding.RequiredGems = (int)((building1.RequiredGems + building2.RequiredGems) * 1.8f);

            string message = $"融合成功！{building1.Name}与{building2.Name}融合成为{fusedBuilding.Name}！";
            return new FusionResult(true, fusedBuilding, message, 0);
        }

        /// <summary>
        /// 获取可用融合配方
        /// </summary>
        public static List<FusionRecipe> GetAvailableRecipes()
        {
            return new List<FusionRecipe>(predefinedRecipes);
        }

        /// <summary>
        /// 检查是否可以融合
        /// </summary>
        public static bool CanFuse(object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
                return false;

            // 检查各种融合可能性
            if (obj1 is MythicalKukuData && obj2 is UnitData)
            {
                MythicalKukuData kuku = obj1 as MythicalKukuData;
                UnitData robot = obj2 as UnitData;
                return kuku.CanFuseWithRobots && kuku.EvolutionLevel >= 5 && robot.Level >= 10;
            }
            else if (obj1 is UnitData && obj2 is MythicalKukuData)
            {
                MythicalKukuData kuku = obj2 as MythicalKukuData;
                UnitData robot = obj1 as UnitData;
                return kuku.CanFuseWithRobots && kuku.EvolutionLevel >= 5 && robot.Level >= 10;
            }
            else if (obj1 is UnitData && obj2 is UnitData)
            {
                UnitData unit1 = obj1 as UnitData;
                UnitData unit2 = obj2 as UnitData;
                return unit1.CanFuse && unit2.CanFuse && unit1.Type == unit2.Type;
            }
            else if (obj1 is MythicalKukuData && obj2 is MythicalKukuData)
            {
                MythicalKukuData kuku1 = obj1 as MythicalKukuData;
                MythicalKukuData kuku2 = obj2 as MythicalKukuData;
                return kuku1.CanFuseWithRobots && kuku2.CanFuseWithRobots && 
                       kuku1.EvolutionLevel >= 3 && kuku2.EvolutionLevel >= 3;
            }
            else if (obj1 is WeaponData && obj2 is WeaponData)
            {
                WeaponData wpn1 = obj1 as WeaponData;
                WeaponData wpn2 = obj2 as WeaponData;
                return wpn1.IsCombinable && wpn2.IsCombinable && 
                       Mathf.Abs((int)wpn1.Tier - (int)wpn2.Tier) <= 1;
            }

            return false;
        }

        /// <summary>
        /// 检查KuKu是否达到最终进化等级（可融合）
        /// </summary>
        public static bool CanKukuFuseWithRobot(MythicalKukuData kuku)
        {
            if (kuku == null) return false;
            return kuku.CanFuseWithRobots && kuku.EvolutionLevel >= 5;
        }

        /// <summary>
        /// 检查机器人是否达到融合要求等级
        /// </summary>
        public static bool CanRobotFuseWithKuku(UnitData robot)
        {
            if (robot == null) return false;
            return robot.CanFuse && robot.Level >= 10;
        }

        /// <summary>
        /// 获取融合后可装备的槽位数
        /// </summary>
        public static int GetFusionEquipmentSlots(FusionRecipe recipe)
        {
            return recipe.maxEquipmentSlots;
        }

        /// <summary>
        /// 计算武器融合成功率
        /// </summary>
        private static float CalculateWeaponFusionSuccessRate(WeaponData weapon1, WeaponData weapon2)
        {
            // 基础成功率
            float baseRate = 0.7f;

            // 根据稀有度调整
            float tierAdjustment = 0.1f - (Mathf.Abs((int)weapon1.Tier - (int)weapon2.Tier) * 0.05f);

            // 根据等级调整
            float levelAdjustment = 0.05f * Mathf.Min((int)weapon1.Tier, (int)weapon2.Tier);

            return Mathf.Clamp01(baseRate + tierAdjustment + levelAdjustment);
        }

        /// <summary>
        /// 创建融合后的武器
        /// </summary>
        private static WeaponData CreateFusedWeapon(WeaponData weapon1, WeaponData weapon2)
        {
            WeaponData fusedWeapon = new WeaponData
            {
                Name = $"{weapon1.Name}·{weapon2.Name}",
                Description = $"由{weapon1.Name}和{weapon2.Name}合成的强化武器",
                Type = weapon1.Type, // 保持第一个武器的类型
                Tier = (WeaponData.WeaponTier)Mathf.Max((int)weapon1.Tier, (int)weapon2.Tier), // 取较高等级
                AttackBonus = (weapon1.AttackBonus + weapon2.AttackBonus) * 1.2f, // 提升攻击力
                DefenseBonus = (weapon1.DefenseBonus + weapon2.DefenseBonus) * 1.2f, // 提升防御力
                SpeedBonus = (weapon1.SpeedBonus + weapon2.SpeedBonus) * 1.1f, // 提升速度
                HealthBonus = (weapon1.HealthBonus + weapon2.HealthBonus) * 1.1f, // 提升血量
                RangeBonus = (weapon1.RangeBonus + weapon2.RangeBonus) * 1.1f, // 提升射程
                SpecialEffect = Mathf.Max(weapon1.SpecialEffect, weapon2.SpecialEffect) * 1.3f, // 特殊效果增强
                Slot = weapon1.Slot, // 保持第一个武器的槽位
                IsCombinable = weapon1.IsCombinable || weapon2.IsCombinable, // 继承合成能力
                CombineCost = (weapon1.CombineCost + weapon2.CombineCost) * 1.5f, // 增加合成成本
                RequiredTierToCombine = Mathf.Max(weapon1.RequiredTierToCombine, weapon2.RequiredTierToCombine),
                ComponentsNeeded = Mathf.Max(weapon1.ComponentsNeeded, weapon2.ComponentsNeeded),
                EquipCost = (weapon1.EquipCost + weapon2.EquipCost) * 1.2f, // 增加装备成本
                RequiredLevel = Mathf.Max(weapon1.RequiredLevel, weapon2.RequiredLevel), // 需要更高的等级
                IsEquipped = false, // 新武器未装备
                Durability = (weapon1.Durability + weapon2.Durability) / 2, // 平均耐久度
                MaxDurability = (weapon1.MaxDurability + weapon2.MaxDurability) * 1.1f, // 增加最大耐久度
                Price = (weapon1.Price + weapon2.Price) * 1.8f, // 增加价格
                SpriteName = $"Fused_{weapon1.SpriteName}", // 新的精灵名称
                Tint = Color.Lerp(weapon1.Tint, weapon2.Tint, 0.5f), // 混合颜色
                EffectDuration = Mathf.Max(weapon1.EffectDuration, weapon2.EffectDuration) * 1.2f,
                CooldownReduction = (weapon1.CooldownReduction + weapon2.CooldownReduction) * 1.1f,
                CriticalChance = (weapon1.CriticalChance + weapon2.CriticalChance) * 1.1f,
                CriticalDamage = (weapon1.CriticalDamage + weapon2.CriticalDamage) / 2,
                IsUnique = weapon1.IsUnique || weapon2.IsUnique, // 如果任一是唯一的，则合成后也是唯一的
                SetName = string.IsNullOrEmpty(weapon1.SetName) ? weapon2.SetName : weapon1.SetName, // 继承套装
                SetPieceCount = Mathf.Max(weapon1.SetPieceCount, weapon2.SetPieceCount),
                SpecialAbility = $"{weapon1.SpecialAbility} + {weapon2.SpecialAbility}" // 组合特殊能力
            };

            // 如果合成后稀有度提升了，增加额外属性
            if ((int)fusedWeapon.Tier > Mathf.Max((int)weapon1.Tier, (int)weapon2.Tier))
            {
                fusedWeapon.AttackBonus *= 1.3f;
                fusedWeapon.DefenseBonus *= 1.3f;
            }

            return fusedWeapon;
        }

        /// <summary>
        /// 获取特定类型的融合配方
        /// </summary>
        public static List<FusionRecipe> GetRecipesByType(FusionRecipe.FusionType type)
        {
            List<FusionRecipe> recipes = new List<FusionRecipe>();
            foreach (var recipe in predefinedRecipes)
            {
                if (recipe.type == type)
                {
                    recipes.Add(recipe);
                }
            }
            return recipes;
        }

        /// <summary>
        /// 获取KuKu融合配方
        /// </summary>
        public static FusionRecipe GetKukuRobotRecipe()
        {
            foreach (var recipe in predefinedRecipes)
            {
                if (recipe.type == FusionRecipe.FusionType.KukuRobot)
                {
                    return recipe;
                }
            }
            return new FusionRecipe(); // 返回默认值
        }
    }
}