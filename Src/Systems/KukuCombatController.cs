using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// KuKu战斗控制器 - 管理KuKu战斗行为和AI
    /// </summary>
    public class KukuCombatController
    {
        // KuKu战斗单位列表
        private List<UnitData> kukuUnits;
        private List<MythicalKukuData> mythicalKukus;
        
        // 战斗配置
        private float combatUpdateInterval = 0.5f; // 战斗更新间隔
        private float lastCombatUpdate = 0f;
        
        // 战斗状态
        private bool isCombatActive = false;
        private bool isAutoCombatEnabled = false;
        
        // AI难度设置
        private int aiDifficulty = 2; // 0-简单, 1-普通, 2-困难
        
        // 事件
        public event Action<UnitData, UnitData> OnUnitAttack;
        public event Action<UnitData, UnitData> OnUnitDefend;
        public event Action<UnitData> OnUnitDamaged;
        public event Action<UnitData> OnUnitDestroyed;
        public event Action<string> OnCombatLog;
        
        // 构造函数
        public KukuCombatController()
        {
            kukuUnits = new List<UnitData>();
            mythicalKukus = new List<MythicalKukuData>();
        }
        
        /// <summary>
        /// 初始化战斗控制器
        /// </summary>
        public void Initialize(PlayerData playerData)
        {
            if (playerData == null)
            {
                Debug.LogError("玩家数据为空，无法初始化战斗控制器");
                return;
            }
            
            // 从玩家数据加载KuKu单位
            LoadKukuUnits(playerData);
            
            Debug.Log($"战斗控制器初始化完成，加载了 {kukuUnits.Count} 个KuKu单位");
        }
        
        /// <summary>
        /// 从玩家数据加载KuKu单位
        /// </summary>
        private void LoadKukuUnits(PlayerData playerData)
        {
            kukuUnits.Clear();
            mythicalKukus.Clear();
            
            // 从玩家收集的KuKu创建战斗单位
            foreach (var kvp in playerData.CollectedKukus)
            {
                UnitData unit = ConvertMythicalKukuToCombatUnit(kvp.Value);
                if (unit != null)
                {
                    kukuUnits.Add(unit);
                }
                mythicalKukus.Add(kvp.Value);
            }
            
            // 添加玩家部署的单位
            kukuUnits.AddRange(playerData.DeployedUnits);
        }
        
        /// <summary>
        /// 将神话KuKu转换为战斗单位
        /// </summary>
        private UnitData ConvertMythicalKukuToCombatUnit(MythicalKukuData kuku)
        {
            if (kuku == null) return null;
            
            UnitData unit = new UnitData
            {
                Name = kuku.Name,
                Description = kuku.Description,
                Type = UnitData.UnitType.KukuHybrid,
                AttackPower = kuku.AttackPower,
                DefensePower = kuku.DefensePower,
                Speed = kuku.Speed,
                Health = kuku.Health,
                Range = kuku.SkillRange,
                Level = kuku.Level,
                Experience = kuku.Experience,
                MaxEquipmentSlots = kuku.MaxEquipmentSlots,
                IsDeployed = true
            };
            
            return unit;
        }
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartCombat()
        {
            if (kukuUnits.Count == 0)
            {
                OnCombatLog?.Invoke("没有可用的KuKu单位参与战斗");
                return;
            }
            
            isCombatActive = true;
            OnCombatLog?.Invoke("战斗开始！");
            
            Debug.Log("KuKu战斗开始");
        }
        
        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndCombat()
        {
            isCombatActive = false;
            isAutoCombatEnabled = false;
            OnCombatLog?.Invoke("战斗结束");
            
            Debug.Log("KuKu战斗结束");
        }
        
        /// <summary>
        /// 更新战斗逻辑
        /// </summary>
        public void UpdateCombat(float deltaTime)
        {
            if (!isCombatActive)
                return;
                
            lastCombatUpdate += deltaTime;
            
            if (lastCombatUpdate >= combatUpdateInterval)
            {
                lastCombatUpdate = 0f;
                
                if (isAutoCombatEnabled)
                {
                    ExecuteAutoCombat();
                }
            }
        }
        
        /// <summary>
        /// 执行自动战斗
        /// </summary>
        private void ExecuteAutoCombat()
        {
            // 简单的AI战斗逻辑
            foreach (var unit in kukuUnits)
            {
                if (unit.CurrentHealth <= 0) continue; // 跳过已死亡的单位
                
                // 寻找最近的敌人进行攻击
                UnitData target = FindNearestEnemy(unit);
                
                if (target != null)
                {
                    AttackTarget(unit, target);
                }
            }
        }
        
        /// <summary>
        /// 寻找最近的敌人
        /// </summary>
        private UnitData FindNearestEnemy(UnitData attacker)
        {
            // 在实际游戏中，这里会寻找敌方单位
            // 为了演示，我们返回一个虚拟的敌人
            if (kukuUnits.Count > 1)
            {
                // 返回除了攻击者之外的第一个单位作为"敌人"
                foreach (var unit in kukuUnits)
                {
                    if (unit != attacker && unit.CurrentHealth > 0)
                    {
                        return unit;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 攻击目标
        /// </summary>
        public void AttackTarget(UnitData attacker, UnitData target)
        {
            if (attacker == null || target == null)
                return;
                
            if (attacker.CurrentHealth <= 0 || target.CurrentHealth <= 0)
                return;
            
            // 计算伤害
            float damage = CalculateDamage(attacker, target);
            
            // 对目标造成伤害
            float remainingHealth = target.CurrentHealth - damage;
            target.CurrentHealth = Mathf.Max(0, remainingHealth);
            
            // 触发事件
            OnUnitAttack?.Invoke(attacker, target);
            OnUnitDamaged?.Invoke(target);
            
            string logMessage = $"{attacker.Name} 攻击 {target.Name}，造成 {damage:F2} 点伤害";
            OnCombatLog?.Invoke(logMessage);
            
            Debug.Log(logMessage);
            
            // 检查目标是否被击败
            if (target.CurrentHealth <= 0)
            {
                OnUnitDestroyed?.Invoke(target);
                OnCombatLog?.Invoke($"{target.Name} 被击败了！");
                
                Debug.Log($"{target.Name} 被击败");
            }
        }
        
        /// <summary>
        /// 计算伤害
        /// </summary>
        private float CalculateDamage(UnitData attacker, UnitData target)
        {
            // 基础伤害
            float baseDamage = attacker.AttackPower;
            
            // 防御减免
            float defenseReduction = target.DefensePower * 0.1f;
            float finalDamage = Mathf.Max(1, baseDamage - defenseReduction);
            
            // 根据AI难度调整
            switch (aiDifficulty)
            {
                case 0: // 简单 - 伤害减少20%
                    finalDamage *= 0.8f;
                    break;
                case 2: // 困难 - 伤害增加20%
                    finalDamage *= 1.2f;
                    break;
            }
            
            // 添加随机因素
            float randomFactor = UnityEngine.Random.Range(0.8f, 1.2f);
            finalDamage *= randomFactor;
            
            return finalDamage;
        }
        
        /// <summary>
        /// 治疗单位
        /// </summary>
        public void HealUnit(UnitData unit, float healAmount)
        {
            if (unit == null || healAmount <= 0)
                return;
                
            if (unit.CurrentHealth <= 0)
            {
                OnCombatLog?.Invoke($"{unit.Name} 已经死亡，无法治疗");
                return;
            }
            
            float newHealth = Mathf.Min(unit.MaxHealth, unit.CurrentHealth + healAmount);
            float actualHeal = newHealth - unit.CurrentHealth;
            unit.CurrentHealth = newHealth;
            
            OnCombatLog?.Invoke($"{unit.Name} 恢复了 {actualHeal:F2} 点生命值");
            
            Debug.Log($"{unit.Name} 恢复了 {actualHeal:F2} 生命值");
        }
        
        /// <summary>
        /// 手动控制KuKu攻击
        /// </summary>
        public void ManualAttack(int attackerId, int targetId)
        {
            UnitData attacker = GetUnitById(attackerId);
            UnitData target = GetUnitById(targetId);
            
            if (attacker == null)
            {
                OnCombatLog?.Invoke("攻击者不存在");
                return;
            }
            
            if (target == null)
            {
                OnCombatLog?.Invoke("目标不存在");
                return;
            }
            
            AttackTarget(attacker, target);
        }
        
        /// <summary>
        /// 通过ID获取单位
        /// </summary>
        private UnitData GetUnitById(int id)
        {
            // 由于UnitData没有ID属性，我们使用列表索引作为ID
            if (id >= 0 && id < kukuUnits.Count)
            {
                return kukuUnits[id];
            }
            
            return null;
        }
        
        /// <summary>
        /// 启用自动战斗
        /// </summary>
        public void EnableAutoCombat()
        {
            isAutoCombatEnabled = true;
            OnCombatLog?.Invoke("自动战斗已启用");
        }
        
        /// <summary>
        /// 禁用自动战斗
        /// </summary>
        public void DisableAutoCombat()
        {
            isAutoCombatEnabled = false;
            OnCombatLog?.Invoke("自动战斗已禁用");
        }
        
        /// <summary>
        /// 设置AI难度
        /// </summary>
        public void SetAIDifficulty(int difficulty)
        {
            if (difficulty >= 0 && difficulty <= 2)
            {
                aiDifficulty = difficulty;
                
                string[] difficultyNames = { "简单", "普通", "困难" };
                OnCombatLog?.Invoke($"AI难度已设置为: {difficultyNames[difficulty]}");
            }
        }
        
        /// <summary>
        /// 获取AI难度
        /// </summary>
        public int GetAIDifficulty()
        {
            return aiDifficulty;
        }
        
        /// <summary>
        /// 获取所有KuKu单位
        /// </summary>
        public List<UnitData> GetKukuUnits()
        {
            return new List<UnitData>(kukuUnits);
        }
        
        /// <summary>
        /// 获取神话KuKu列表
        /// </summary>
        public List<MythicalKukuData> GetMythicalKukus()
        {
            return new List<MythicalKukuData>(mythicalKukus);
        }
        
        /// <summary>
        /// 添加KuKu单位
        /// </summary>
        public void AddKukuUnit(UnitData unit)
        {
            if (unit != null)
            {
                kukuUnits.Add(unit);
                OnCombatLog?.Invoke($"添加了新的KuKu单位: {unit.Name}");
            }
        }
        
        /// <summary>
        /// 移除KuKu单位
        /// </summary>
        public bool RemoveKukuUnit(UnitData unit)
        {
            bool removed = kukuUnits.Remove(unit);
            if (removed)
            {
                OnCombatLog?.Invoke($"移除了KuKu单位: {unit.Name}");
            }
            return removed;
        }
        
        /// <summary>
        /// 使用KuKu技能
        /// </summary>
        public void UseKukuSkill(int unitId, int skillId)
        {
            UnitData unit = GetUnitById(unitId);
            if (unit == null)
            {
                OnCombatLog?.Invoke("单位不存在");
                return;
            }
            
            // 在实际游戏中，这里会实现技能系统
            OnCombatLog?.Invoke($"{unit.Name} 使用了技能 {skillId}");
        }
        
        /// <summary>
        /// 检查战斗是否活跃
        /// </summary>
        public bool IsCombatActive()
        {
            return isCombatActive;
        }
        
        /// <summary>
        /// 检查自动战斗是否启用
        /// </summary>
        public bool IsAutoCombatEnabled()
        {
            return isAutoCombatEnabled;
        }
        
        /// <summary>
        /// 获取战斗单位数量
        /// </summary>
        public int GetUnitCount()
        {
            return kukuUnits.Count;
        }
        
        /// <summary>
        /// 获取存活单位数量
        /// </summary>
        public int GetAliveUnitCount()
        {
            int count = 0;
            foreach (var unit in kukuUnits)
            {
                if (unit.CurrentHealth > 0)
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 重置战斗控制器
        /// </summary>
        public void Reset()
        {
            kukuUnits.Clear();
            mythicalKukus.Clear();
            isCombatActive = false;
            isAutoCombatEnabled = false;
            lastCombatUpdate = 0f;
            
            Debug.Log("战斗控制器已重置");
        }
        
        /// <summary>
        /// 获取单位状态信息
        /// </summary>
        public string GetUnitStatus(int unitId)
        {
            UnitData unit = GetUnitById(unitId);
            if (unit == null)
            {
                return "单位不存在";
            }
            
            return $"{unit.Name}: HP {unit.CurrentHealth:F1}/{unit.MaxHealth:F1}, " +
                   $"ATK {unit.AttackPower:F1}, DEF {unit.DefensePower:F1}";
        }
        
        /// <summary>
        /// 获取所有单位状态
        /// </summary>
        public List<string> GetAllUnitStatus()
        {
            List<string> statuses = new List<string>();
            
            for (int i = 0; i < kukuUnits.Count; i++)
            {
                statuses.Add(GetUnitStatus(i));
            }
            
            return statuses;
        }
        
        /// <summary>
        /// 设置战斗更新间隔
        /// </summary>
        public void SetCombatUpdateInterval(float interval)
        {
            combatUpdateInterval = Mathf.Max(0.1f, interval); // 最小间隔0.1秒
        }
        
        /// <summary>
        /// 获取战斗更新间隔
        /// </summary>
        public float GetCombatUpdateInterval()
        {
            return combatUpdateInterval;
        }
        
        /// <summary>
        /// 获取战斗统计信息
        /// </summary>
        public Dictionary<string, object> GetCombatStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["isActive"] = IsCombatActive(),
                ["isAutoCombatEnabled"] = IsAutoCombatEnabled(),
                ["totalUnits"] = GetUnitCount(),
                ["aliveUnits"] = GetAliveUnitCount(),
                ["deadUnits"] = GetUnitCount() - GetAliveUnitCount(),
                ["aiDifficulty"] = GetAIDifficulty(),
                ["combatUpdateInterval"] = GetCombatUpdateInterval()
            };
            
            return stats;
        }
    }
}