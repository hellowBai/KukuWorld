using System;
using UnityEngine;
using KukuWorld.Systems;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 敌人控制器 - 控制敌人的行为和AI
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        // 敌人属性
        private float health;                              // 生命值
        private float speed;                               // 移动速度
        private float damage;                              // 造成的伤害
        private BattleSystem.EnemyType enemyType;          // 敌人类型

        // 目标信息
        private Transform target;                          // 目标（守护点）

        /// <summary>
        /// 初始化敌人
        /// </summary>
        public void Initialize(float h, float s, float d, BattleSystem.EnemyType type)
        {
            health = h;
            speed = s;
            damage = d;
            enemyType = type;
            
            // 在实际游戏中，这里会设置目标点（守护点位置）
            // target = GameObject.FindGameObjectWithTag("GuardianPoint").transform;
            // 为了演示，我们创建一个临时目标点
            GameObject tempTarget = new GameObject("GuardianPoint");
            tempTarget.transform.position = new Vector3(-10, 0, 0); // 守护点在左侧
            target = tempTarget.transform;
            
            Debug.Log($"敌人生成，目标是守护点位置: {target.position}");
        }

        /// <summary>
        /// Unity Update方法
        /// </summary>
        void Update()
        {
            if (health > 0)
            {
                MoveTowardsGuardianPoint();
            }
        }

        /// <summary>
        /// 移向守护点/建筑目标
        /// </summary>
        private void MoveTowardsGuardianPoint()
        {
            if (target == null) return;
            
            // 向目标点移动
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            
            // 检查是否到达目标点
            if (Vector3.Distance(transform.position, target.position) < 0.5f)
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
            Destroy(gameObject);
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
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
            BattleSystem battleSystem = FindObjectOfType<BattleSystem>();
            if (battleSystem != null)
            {
                battleSystem.EnemyDefeated(gameObject);
            }
            
            // 掉落物品
            DropItems();
            
            Destroy(gameObject);
        }

        /// <summary>
        /// 掉落物品
        /// </summary>
        private void DropItems()
        {
            // 随机掉落金币、神石、灵魂等
            int coinDrop = UnityEngine.Random.Range(5, 15);
            int gemDrop = UnityEngine.Random.Range(0, 2);
            float soulDrop = UnityEngine.Random.Range(0.5f, 1.5f);
            
            // 通知玩家获得资源
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerData.AddCoins(coinDrop);
                GameManager.Instance.PlayerData.AddGems(gemDrop);
                GameManager.Instance.PlayerData.AddSouls(soulDrop);
            }
            
            Debug.Log($"敌人死亡，掉落：{coinDrop}金币，{gemDrop}神石，{soulDrop}灵魂");
        }
    }
}