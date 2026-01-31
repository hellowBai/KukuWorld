using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;
using KukuWorld.Systems;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 单位战斗控制器 - 控制单位的战斗行为
    /// </summary>
    public class UnitCombatController : MonoBehaviour
    {
        // 单位数据
        private UnitData unitData;                           // 单位数据引用
        private float attackTimer = 0f;                    // 攻击计时器
        private float attackCooldown = 1f;                // 攻击冷却时间

        /// <summary>
        /// 初始化单位战斗控制器
        /// </summary>
        public void Initialize(UnitData data)
        {
            unitData = data;
            attackCooldown = 2f / unitData.Speed; // 攻击间隔与速度相关
        }

        /// <summary>
        /// Unity Update方法
        /// </summary>
        void Update()
        {
            BattleSystem battleSystem = FindObjectOfType<BattleSystem>();
            if (battleSystem != null)
            {
                AttackNearestEnemy();
            }
        }

        /// <summary>
        /// 攻击最近的敌人
        /// </summary>
        private void AttackNearestEnemy()
        {
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= attackCooldown)
            {
                // 寻找最近的敌人
                GameObject nearestEnemy = FindNearestEnemy();
                
                if (nearestEnemy != null)
                {
                    // 执行攻击
                    ExecuteAttack(nearestEnemy);
                    attackTimer = 0f;
                }
            }
        }

        /// <summary>
        /// 寻找最近的敌人
        /// </summary>
        private GameObject FindNearestEnemy()
        {
            GameObject nearest = null;
            float nearestDistance = float.MaxValue;
            
            BattleSystem battleSystem = FindObjectOfType<BattleSystem>();
            if (battleSystem != null)
            {
                // 遍历所有活跃敌人
                foreach (GameObject enemy in battleSystem.GetActiveEnemies())
                {
                    if (enemy == null) continue;
                    
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = enemy;
                    }
                }
            }
            
            return nearest;
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        private void ExecuteAttack(GameObject enemy)
        {
            if (enemy != null)
            {
                // 对敌人造成伤害
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    float totalAttack = unitData.AttackPower;
                    // 加上装备加成
                    var totalAttrs = unitData.GetTotalAttributes();
                    totalAttack = totalAttrs.atk;
                    
                    enemyController.TakeDamage(totalAttack);
                    Debug.Log($"{unitData.Name} 攻击了敌人，造成 {totalAttack} 点伤害");
                }
            }
        }
    }
}