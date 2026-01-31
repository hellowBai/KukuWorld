using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;
using KukuWorld.Systems;

namespace KukuWorld.Systems
{
    /// <summary>
    /// KuKu战斗控制器 - 控制KuKu的战斗行为
    /// </summary>
    public class KukuCombatController : MonoBehaviour
    {
        // KuKu数据
        private MythicalKukuData kukuData;                           // KuKu数据引用
        private float attackTimer = 0f;                    // 攻击计时器
        private float attackCooldown = 1f;                // 攻击冷却时间
        private List<GameObject> nearbyEnemies = new List<GameObject>(); // 附近敌人列表

        /// <summary>
        /// 初始化KuKu战斗控制器
        /// </summary>
        public void Initialize(MythicalKukuData data)
        {
            kukuData = data;
            attackCooldown = 2f / kukuData.Speed; // 攻击间隔与速度相关
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
                    enemyController.TakeDamage(kukuData.AttackPower);
                    Debug.Log($"{kukuData.Name} 攻击了敌人，造成 {kukuData.AttackPower} 点伤害");
                }
            }
        }
    }
}