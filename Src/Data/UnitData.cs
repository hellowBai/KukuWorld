using System.Collections.Generic;

namespace KukuWorld.Data
{
    /// <summary>
    /// 单位数据结构
    /// </summary>
    [System.Serializable]
    public class UnitData
    {
        // 基础信息
        public int Id { get; set; }                              // 单位ID
        public string Name { get; set; }                         // 单位名称
        public string Description { get; set; }                  // 单位描述

        // 单位类型
        public UnitType Type { get; set; }                       // 单位类型
        public enum UnitType { Robot, Tank, Hybrid, PetHybrid }  // PetHybrid是宠物与机器人融合后的单位

        // 战斗属性
        public float AttackPower { get; set; }                   // 攻击力
        public float DefensePower { get; set; }                  // 防御力
        public float Speed { get; set; }                         // 速度
        public float Health { get; set; }                        // 生命值
        public float Range { get; set; }                         // 射程

        // 生产相关
        public int ProductionCost { get; set; } = 100;          // 生产花费
        public int RequiredResearchLevel { get; set; } = 1;     // 需要研究等级
        public float ProductionTime { get; set; } = 10f;        // 生产时间

        // 融合相关
        public bool CanFuse { get; set; } = true;               // 是否可融合
        public int FusionCost { get; set; } = 200;              // 融合花费
        public string FusionRecipe { get; set; } = "";          // 融合配方
        public bool IsFinalForm { get; set; } = false;          // 是否为最终形态（可装备6件装备）

        // 状态
        public bool IsDeployed { get; set; } = false;           // 是否已部署
        public int Level { get; set; } = 1;                     // 等级
        public float Experience { get; set; } = 0;              // 经验值

        // 装备槽位
        public int MaxEquipmentSlots { get; set; } = 0;         // 最大装备槽位数
        public List<WeaponData> EquippedItems { get; set; }     // 已装备物品

        // 构造函数
        public UnitData()
        {
            Id = 0;
            Name = "Unknown Unit";
            Description = "An unknown unit";
            Type = UnitType.Robot;
            AttackPower = 20f;
            DefensePower = 10f;
            Speed = 1f;
            Health = 100f;
            Range = 2f;
            ProductionCost = 100;
            RequiredResearchLevel = 1;
            ProductionTime = 10f;
            CanFuse = true;
            FusionCost = 200;
            FusionRecipe = "";
            IsFinalForm = false;
            IsDeployed = false;
            Level = 1;
            Experience = 0;
            MaxEquipmentSlots = 0;
            EquippedItems = new List<WeaponData>();
        }

        // 升级单位
        public void LevelUp()
        {
            Level++;
            Experience = 0;
            // 增加属性
            AttackPower *= 1.1f;
            DefensePower *= 1.1f;
            Health *= 1.1f;
        }

        // 添加经验值
        public bool AddExperience(float exp)
        {
            Experience += exp;
            float expForNextLevel = Level * 100f; // 每级需要的经验值
            
            if (Experience >= expForNextLevel)
            {
                LevelUp();
                return true; // 等级提升了
            }
            return false; // 等级未提升
        }

        // 获取生产花费
        public int GetProductionCost()
        {
            return ProductionCost;
        }

        // 设置融合配方
        public void SetFusionRecipe(string recipe)
        {
            FusionRecipe = recipe;
        }

        // 融合两个单位
        public static UnitData FuseUnits(UnitData unit1, UnitData unit2, string fusionName)
        {
            if (unit1 == null || unit2 == null) return null;
            
            UnitData result = new UnitData();
            result.Name = fusionName;
            result.Description = $"A fusion of {unit1.Name} and {unit2.Name}";
            result.Type = UnitType.Hybrid;
            
            // 合并属性
            result.AttackPower = (unit1.AttackPower + unit2.AttackPower) * 1.15f; // 15%加成
            result.DefensePower = (unit1.DefensePower + unit2.DefensePower) * 1.15f;
            result.Speed = (unit1.Speed + unit2.Speed) / 2;
            result.Health = (unit1.Health + unit2.Health) * 1.1f;
            result.Range = Mathf.Max(unit1.Range, unit2.Range);
            
            // 融合后的单位可装备更多装备
            result.MaxEquipmentSlots = 4;
            result.IsFinalForm = false; // 仍可进一步融合
            
            return result;
        }

        // 宠物与机器人融合
        public static UnitData FusePetWithRobot(MythicalPetData pet, UnitData robot, string fusionName)
        {
            if (pet == null || robot == null) return null;
            
            UnitData result = new UnitData();
            result.Name = fusionName;
            result.Description = $"A fusion of {pet.Name} and {robot.Name}";
            result.Type = UnitType.PetHybrid;
            
            // 合并属性（考虑宠物的特殊能力）
            result.AttackPower = (pet.AttackPower + robot.AttackPower) * 1.2f; // 20%加成
            result.DefensePower = (pet.DefensePower + robot.DefensePower) * 1.2f;
            result.Speed = (pet.Speed + robot.Speed) / 2;
            result.Health = (pet.Health + robot.Health) * 1.15f;
            result.Range = Mathf.Max(pet.SkillRange, robot.Range);
            
            // 融合后的单位可装备6件装备
            result.MaxEquipmentSlots = 6;
            result.IsFinalForm = true; // 达到最终形态
            
            return result;
        }

        // 检查是否可以装备物品
        public bool CanEquipItem(WeaponData item)
        {
            if (item == null) return false;
            if (EquippedItems.Count >= MaxEquipmentSlots) return false;
            
            // 检查是否已有相同类型的装备
            foreach (var equipped in EquippedItems)
            {
                if (equipped.Slot == item.Slot)
                {
                    return false; // 已有相同槽位的装备
                }
            }
            
            return true;
        }

        // 装备物品
        public bool EquipItem(WeaponData item)
        {
            if (!CanEquipItem(item)) return false;
            
            EquippedItems.Add(item);
            item.IsEquipped = true;
            
            return true;
        }

        // 卸下物品
        public bool UnequipItem(WeaponData item)
        {
            if (item == null || !EquippedItems.Contains(item)) return false;
            
            EquippedItems.Remove(item);
            item.IsEquipped = false;
            
            return true;
        }

        // 获取总属性（包含装备加成）
        public (float atk, float def, float spd, float hp, float rng) GetTotalAttributes()
        {
            float atk = AttackPower;
            float def = DefensePower;
            float spd = Speed;
            float hp = Health;
            float rng = Range;
            
            // 应用装备加成
            foreach (var item in EquippedItems)
            {
                atk += item.AttackBonus;
                def += item.DefenseBonus;
                spd += item.SpeedBonus;
                hp += item.HealthBonus;
                rng += item.RangeBonus;
            }
            
            return (atk, def, spd, hp, rng);
        }
    }
}