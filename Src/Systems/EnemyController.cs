using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 敌人控制器 - 管理敌人行为和AI
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("敌人属性")]
        public float health = 100f;
        public float maxHealth = 100f;
        public float speed = 2f;
        public float damage = 10f;
        public float attackRange = 1f;
        public float attackCooldown = 1f;
        
        [Header("目标设置")]
        public Transform[] targetPoints;  // 目标点数组（如守护点）
        public Transform currentTarget;   // 当前目标
        
        [Header("敌人类型")]
        public CaptureSystem.ItemDrop.ItemType enemyType = CaptureSystem.ItemDrop.ItemType.Coin;
        
        // 私有变量
        private float attackTimer = 0f;
        private bool isAlive = true;
        private BattleSystem battleSystem;
        private NuwaDefenseSystem defenseSystem;
        private List<Transform> pathToTarget;
        
        // 事件
        public event Action<GameObject> OnEnemyDestroyed;
        public event Action<float> OnTakeDamage;
        public event Action OnReachTarget;
        
        // Start is called before the first frame update
        void Start()
        {
            InitializeEnemy();
        }
        
        // Update is called once per frame
        void Update()
        {
            if (!isAlive) return;
            
            UpdateEnemyBehavior();
        }
        
        /// <summary>
        /// 初始化敌人
        /// </summary>
        public void Initialize(float h, float s, float d, Transform[] targets, CaptureSystem.ItemDrop.ItemType type)
        {
            health = h;
            maxHealth = h;
            speed = s;
            damage = d;
            targetPoints = targets;
            enemyType = type;
            
            // 设置初始目标
            SetInitialTarget();
            
            // 获取系统引用
            battleSystem = FindObjectOfType<BattleSystem>();
            defenseSystem = FindObjectOfType<NuwaDefenseSystem>();
            
            Debug.Log($"敌人初始化: HP={health}, Speed={speed}, Damage={damage}, Type={type}");
        }
        
        /// <summary>
        /// 设置初始目标
        /// </summary>
        private void SetInitialTarget()
        {
            if (targetPoints != null && targetPoints.Length > 0)
            {
                // 选择最近的目标点
                Transform closestTarget = targetPoints[0];
                float closestDistance = Vector3.Distance(transform.position, targetPoints[0].position);
                
                for (int i = 1; i < targetPoints.Length; i++)
                {
                    float distance = Vector3.Distance(transform.position, targetPoints[i].position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = targetPoints[i];
                    }
                }
                
                currentTarget = closestTarget;
            }
        }
        
        /// <summary>
        /// 初始化敌人（从预制体实例化时调用）
        /// </summary>
        private void InitializeEnemy()
        {
            isAlive = true;
            health = maxHealth;
            attackTimer = 0f;
            
            Debug.Log($"敌人 {gameObject.name} 已初始化");
        }
        
        /// <summary>
        /// 更新敌人行为
        /// </summary>
        private void UpdateEnemyBehavior()
        {
            if (currentTarget == null)
            {
                Debug.LogWarning("敌人没有目标，寻找新目标...");
                SetInitialTarget();
                if (currentTarget == null) return; // 仍然没有目标，跳过更新
            }
            
            // 移动到目标
            MoveTowardsTarget();
            
            // 检查是否到达目标
            CheckTargetReached();
            
            // 更新攻击计时器
            attackTimer += Time.deltaTime;
        }
        
        /// <summary>
        /// 移动到目标点
        /// </summary>
        private void MoveTowardsTarget()
        {
            if (currentTarget == null) return;
            
            // 计算到目标的方向
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            
            // 移动
            transform.position += direction * speed * Time.deltaTime;
            
            // 朝向目标
            if (direction != Vector3.zero)
            {
                transform.forward = direction;
            }
        }
        
        /// <summary>
        /// 检查是否到达目标
        /// </summary>
        private void CheckTargetReached()
        {
            if (currentTarget == null) return;
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            
            if (distanceToTarget <= attackRange)
            {
                // 到达目标，攻击
                AttackTarget();
            }
        }
        
        /// <summary>
        /// 攻击目标
        /// </summary>
        private void AttackTarget()
        {
            if (attackTimer >= attackCooldown)
            {
                // 通知防御系统敌人到达目标
                if (defenseSystem != null)
                {
                    defenseSystem.EnemyAttackedTemple(damage);
                }
                else if (battleSystem != null)
                {
                    battleSystem.EnemyReachedEnd(gameObject);
                }
                
                // 触发到达目标事件
                OnReachTarget?.Invoke();
                
                Debug.Log($"敌人攻击目标，造成 {damage} 点伤害");
                
                // 重置攻击计时器
                attackTimer = 0f;
                
                // 如果是立即杀死敌人（因为已经到达目标）
                Die();
            }
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (!isAlive) return;
            
            health -= damageAmount;
            
            // 触发受伤事件
            OnTakeDamage?.Invoke(damageAmount);
            
            Debug.Log($"敌人受到 {damageAmount} 点伤害，剩余生命值: {health}");
            
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
            
            Debug.Log($"敌人死亡");
            
            // 通知战斗系统敌人被击败
            if (defenseSystem != null)
            {
                defenseSystem.EnemyDefeated();
            }
            else if (battleSystem != null)
            {
                battleSystem.EnemyDefeated(gameObject);
            }
            
            // 掉落物品
            DropLoot();
            
            // 触发死亡事件
            OnEnemyDestroyed?.Invoke(gameObject);
            
            // 销毁游戏对象
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 掉落物品
        /// </summary>
        private void DropLoot()
        {
            // 根据敌人类型和等级掉落不同的物品
            int coinDrop = UnityEngine.Random.Range(5, 15);
            int gemDrop = UnityEngine.Random.Range(0, 2);
            float soulDrop = UnityEngine.Random.Range(0.5f, 1.5f);
            
            // 通知玩家获得资源
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.PlayerData.AddCoins(coinDrop);
                gameManager.PlayerData.AddGems(gemDrop);
                gameManager.PlayerData.AddSouls(soulDrop);
                
                Debug.Log($"敌人掉落: {coinDrop}金币, {gemDrop}神石, {soulDrop:F1}灵魂");
            }
        }
        
        /// <summary>
        /// 设置新目标
        /// </summary>
        public void SetNewTarget(Transform newTarget)
        {
            currentTarget = newTarget;
        }
        
        /// <summary>
        /// 设置多个目标点
        /// </summary>
        public void SetTargetPoints(Transform[] targets)
        {
            targetPoints = targets;
            SetInitialTarget();
        }
        
        /// <summary>
        /// 获取敌人状态
        /// </summary>
        public (bool alive, float currentHealth, float maxHealth, float healthPercentage) GetStatus()
        {
            float healthPercentage = maxHealth > 0 ? (health / maxHealth) * 100f : 0f;
            return (isAlive, health, maxHealth, healthPercentage);
        }
        
        /// <summary>
        /// 获取敌人类型
        /// </summary>
        public CaptureSystem.ItemDrop.ItemType GetEnemyType()
        {
            return enemyType;
        }
        
        /// <summary>
        /// 设置敌人速度
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
        
        /// <summary>
        /// 设置敌人伤害
        /// </summary>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }
        
        /// <summary>
        /// 设置攻击范围
        /// </summary>
        public void SetAttackRange(float newRange)
        {
            attackRange = newRange;
        }
        
        /// <summary>
        /// 设置攻击冷却时间
        /// </summary>
        public void SetAttackCooldown(float newCooldown)
        {
            attackCooldown = newCooldown;
        }
        
        /// <summary>
        /// 检查敌人是否存活
        /// </summary>
        public bool IsAlive()
        {
            return isAlive;
        }
        
        /// <summary>
        /// 恢复健康状态
        /// </summary>
        public void Heal(float healAmount)
        {
            if (!isAlive) return;
            
            health = Mathf.Min(maxHealth, health + healAmount);
            
            Debug.Log($"敌人被治疗 {healAmount} 点，当前生命值: {health}");
        }
        
        /// <summary>
        /// 检查敌人是否在攻击范围内
        /// </summary>
        public bool IsInAttackRange(Transform target)
        {
            if (target == null) return false;
            
            float distance = Vector3.Distance(transform.position, target.position);
            return distance <= attackRange;
        }
        
        /// <summary>
        /// 获取到目标的距离
        /// </summary>
        public float GetDistanceToTarget()
        {
            if (currentTarget == null) return float.MaxValue;
            
            return Vector3.Distance(transform.position, currentTarget.position);
        }
        
        /// <summary>
        /// 获取当前生命值百分比
        /// </summary>
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? (health / maxHealth) * 100f : 0f;
        }
        
        /// <summary>
        /// 设置最大生命值
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            health = Mathf.Min(health, maxHealth); // 确保当前生命值不超过最大值
        }
        
        /// <summary>
        /// 重置敌人状态
        /// </summary>
        public void ResetEnemy()
        {
            health = maxHealth;
            isAlive = true;
            attackTimer = 0f;
            
            Debug.Log("敌人状态已重置");
        }
        
        /// <summary>
        /// 在 OnDestroy 中清理事件订阅
        /// </summary>
        void OnDestroy()
        {
            // 清理事件订阅，防止内存泄漏
            OnEnemyDestroyed = null;
            OnTakeDamage = null;
            OnReachTarget = null;
        }
    }
}