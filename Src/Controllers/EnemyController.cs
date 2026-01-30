using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Controllers
{
    /// <summary>
    /// 敌人控制器 - 管理敌人行为和AI
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        // 敌人属性
        private float health;                              // 生命值
        private float speed;                               // 移动速度
        private float damage;                              // 造成的伤害
        private BattleSystem.EnemyType enemyType;          // 敌人类型

        // 目标信息
        private Transform[] guardianPointPoints;           // 守护点目标点数组
        private Transform currentTarget;                   // 当前目标

        // 状态
        private bool isAlive = true;

        /// <summary>
        /// 初始化敌人
        /// </summary>
        public void Initialize(float h, float s, float d, Transform[] targets, BattleSystem.EnemyType type)
        {
            health = h;
            speed = s;
            damage = d;
            enemyType = type;
            guardianPointPoints = targets;
            
            // 随机选择一个守护点/建筑作为攻击目标
            if (guardianPointPoints != null && guardianPointPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, guardianPointPoints.Length);
                currentTarget = guardianPointPoints[randomIndex];
            }
            
            if (currentTarget != null)
            {
                Debug.Log($"敌人生成，目标是守护点位置: {currentTarget.position}");
            }
            else
            {
                Debug.LogWarning("敌人没有设定目标点！");
            }
            
            isAlive = true;
        }

        void Update()
        {
            if (isAlive && currentTarget != null)
            {
                MoveTowardsGuardianPoint();
            }
        }

        /// <summary>
        /// 移向守护点/建筑目标
        /// </summary>
        private void MoveTowardsGuardianPoint()
        {
            if (currentTarget == null) return;
            
            // 向目标点移动
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
            
            // 检查是否到达目标点
            if (Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
            {
                // 到达守护点，开始破坏
                AttackGuardianPoint();
            }
        }

        /// <summary>
        /// 攻击守护点/建筑
        /// </summary>
        private void AttackGuardianPoint()
        {
            Debug.Log($"敌人到达守护点位置，造成 {damage} 点破坏！");
            
            // 通知战斗系统敌人到达目标
            BattleSystem battleSystem = FindObjectOfType<BattleSystem>();
            if (battleSystem != null)
            {
                battleSystem.EnemyReachedEnd(gameObject);
            }
            
            // 销毁敌人对象
            Die();
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (!isAlive) return;
            
            health -= damageAmount;
            
            if (health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        private void Die()
        {
            if (!isAlive) return;
            
            isAlive = false;
            
            // 通知战斗系统
            BattleSystem battleSystem = FindObjectOfType<BattleSystem>();
            if (battleSystem != null)
            {
                battleSystem.EnemyDefeated(gameObject);
            }
            
            // 掉落物品
            DropItems();
            
            // 销毁游戏对象
            Destroy(gameObject);
        }

        /// <summary>
        /// 掉落物品
        /// </summary>
        private void DropItems()
        {
            // 随机掉落金币、神石、灵魂等
            int coinDrop = Random.Range(5, 15);
            int gemDrop = Random.Range(0, 2);
            float soulDrop = Random.Range(0.5f, 1.5f);
            
            // 通知玩家获得资源
            if (GameManager.Instance?.PlayerData != null)
            {
                GameManager.Instance.PlayerData.AddCoins(coinDrop);
                GameManager.Instance.PlayerData.AddGems(gemDrop);
                GameManager.Instance.PlayerData.AddSouls(soulDrop);
            }
            
            Debug.Log($"敌人死亡，掉落：{coinDrop}金币，{gemDrop}神石，{soulDrop}灵魂");
        }

        /// <summary>
        /// 获取敌人类型
        /// </summary>
        public BattleSystem.EnemyType GetEnemyType()
        {
            return enemyType;
        }

        /// <summary>
        /// 获取当前生命值
        /// </summary>
        public float GetHealth()
        {
            return health;
        }

        /// <summary>
        /// 获取最大生命值
        /// </summary>
        public float GetMaxHealth()
        {
            return health; // 注意：这里应该是初始生命值，但在当前实现中就是health
        }

        /// <summary>
        /// 检查是否存活
        /// </summary>
        public bool IsAlive()
        {
            return isAlive;
        }
    }
}