using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Controllers
{
    /// <summary>
    /// 宠物战斗控制器 - 管理宠物战斗AI
    /// </summary>
    public class PetCombatController : MonoBehaviour
    {
        // 宠物数据
        private PetData petData;                           // 宠物数据引用
        private float attackTimer = 0f;                    // 攻击计时器
        private float attackCooldown = 1f;                // 攻击冷却时间
        private List<GameObject> nearbyEnemies = new List<GameObject>(); // 附近敌人列表

        /// <summary>
        /// 初始化宠物战斗控制器
        /// </summary>
        public void Initialize(PetData data)
        {
            petData = data;
            if (petData != null)
            {
                attackCooldown = 2f / Mathf.Max(0.1f, petData.Speed); // 攻击间隔与速度相关
            }
            else
            {
                attackCooldown = 1f; // 默认攻击间隔
            }
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.DefensePhase)
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
            
            // 获取战斗系统中的活跃敌人
            BattleSystem battleSystem = FindObjectOfType<BattleSystem>();
            if (battleSystem != null)
            {
                List<GameObject> activeEnemies = battleSystem.GetActiveEnemies();
                
                foreach (GameObject enemy in activeEnemies)
                {
                    if (enemy == null) continue;
                    
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    
                    // 检查敌人是否还在有效范围内
                    EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
                    if (enemyCtrl != null && enemyCtrl.IsAlive() && distance < 10f) // 10单位范围内的敌人
                    {
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearest = enemy;
                        }
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
            if (enemy != null && petData != null)
            {
                // 对敌人造成伤害
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(petData.AttackPower);
                    Debug.Log($"{petData.Name} 攻击了敌人，造成 {petData.AttackPower} 点伤害");
                }
            }
        }

        /// <summary>
        /// 获取宠物数据
        /// </summary>
        public PetData GetPetData()
        {
            return petData;
        }

        /// <summary>
        /// 设置宠物数据
        /// </summary>
        public void SetPetData(PetData data)
        {
            petData = data;
            if (petData != null)
            {
                attackCooldown = 2f / Mathf.Max(0.1f, petData.Speed); // 重新计算攻击间隔
            }
        }

        /// <summary>
        /// 检查是否在防守阶段
        /// </summary>
        private bool IsInDefensePhase()
        {
            return GameManager.Instance?.CurrentState == GameManager.GameState.DefensePhase;
        }

        /// <summary>
        /// 获取攻击冷却时间
        /// </summary>
        public float GetAttackCooldown()
        {
            return attackCooldown;
        }

        /// <summary>
        /// 获取当前攻击计时器
        /// </summary>
        public float GetAttackTimer()
        {
            return attackTimer;
        }
    }
}