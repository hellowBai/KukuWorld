using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("敌人属性")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float speed = 2f;
    public float damage = 10f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    
    [Header("寻路设置")]
    public Transform[] targetPoints;
    public int currentTargetIndex = 0;
    
    [Header("视觉效果")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    private float attackTimer = 0f;
    private bool isAlive = true;
    private bool isMoving = true;
    private BattleSystem battleSystem;
    private CaptureSystem captureSystem;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        currentHealth = maxHealth;
        
        // 获取系统引用
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            battleSystem = gameManager.battleSystem;
            captureSystem = gameManager.captureSystem;
        }
        
        // 初始化视觉组件
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // 设置敌人外观
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetEnemyColorByWave();
        }
        
        Debug.Log($"敌人初始化: HP={maxHealth}, Speed={speed}, Damage={damage}");
    }
    
    void Update()
    {
        if (!isAlive || targetPoints == null || targetPoints.Length == 0) return;
        
        if (isMoving)
        {
            MoveTowardsTarget();
        }
        
        UpdateAttackTimer();
    }
    
    void MoveTowardsTarget()
    {
        if (currentTargetIndex >= targetPoints.Length) return;
        
        Transform target = targetPoints[currentTargetIndex];
        if (target == null) return;
        
        // 移向目标点
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // 检查是否到达目标点
        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            // 到达当前目标点
            if (currentTargetIndex < targetPoints.Length - 1)
            {
                // 移动到下一个目标点
                currentTargetIndex++;
            }
            else
            {
                // 到达最终目标点
                ReachedTarget();
            }
        }
    }
    
    void UpdateAttackTimer()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }
    
    void ReachedTarget()
    {
        // 通知战斗系统敌人到达目标
        if (battleSystem != null)
        {
            battleSystem.EnemyReachedTarget(Mathf.CeilToInt(damage));
        }
        else
        {
            // 如果没有战斗系统，直接减少玩家生命
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // 简化处理：直接结束游戏
                gameManager.EndGame(false);
            }
        }
        
        // 销毁敌人
        Die();
    }
    
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        currentHealth -= damage;
        
        Debug.Log($"敌人受到 {damage} 点伤害，剩余生命: {currentHealth}/{maxHealth}");
        
        // 更新视觉效果
        if (spriteRenderer != null)
        {
            // 闪红效果
            StartCoroutine(FlashRed());
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        isMoving = false;
        
        Debug.Log("敌人被击败");
        
        // 通知战斗系统
        if (battleSystem != null)
        {
            battleSystem.EnemyDefeated(gameObject);
        }
        
        // 播放死亡效果
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 销毁对象
        Destroy(gameObject, 0.5f); // 等待死亡动画播放
    }
    
    public void Initialize(int wave, Transform[] targets)
    {
        targetPoints = targets;
        currentTargetIndex = 0;
        
        // 根据波次调整属性
        float waveMultiplier = 1.0f + (wave - 1) * 0.2f;
        maxHealth *= waveMultiplier;
        currentHealth = maxHealth;
        damage *= waveMultiplier;
        
        // 设置速度（随波次略有变化）
        speed *= (0.8f + wave * 0.05f);
        
        Debug.Log($"敌人初始化 - 波次: {wave}, HP: {maxHealth}, Damage: {damage}");
    }
    
    public void SetStats(float health, float spd, float dmg)
    {
        maxHealth = health;
        currentHealth = health;
        speed = spd;
        damage = dmg;
    }
    
    public bool IsAlive()
    {
        return isAlive;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (currentHealth / maxHealth) * 100f : 0f;
    }
    
    public float GetSpeed()
    {
        return speed;
    }
    
    public float GetDamage()
    {
        return damage;
    }
    
    public bool CanAttack()
    {
        return attackTimer <= 0f;
    }
    
    public void Attack()
    {
        if (!CanAttack()) return;
        
        // 重置攻击计时器
        attackTimer = attackCooldown;
        
        // 执行攻击逻辑
        Debug.Log($"敌人发动攻击，伤害: {damage}");
        
        // 在实际游戏中，这里会攻击最近的防御单位
    }
    
    Color GetEnemyColorByWave()
    {
        // 根据波次返回不同颜色
        if (currentTargetIndex > 0)
        {
            float hue = (currentTargetIndex % 10) * 0.1f; // 循环色调
            return Color.HSVToRGB(hue, 0.8f, 0.9f);
        }
        return Color.red;
    }
    
    public void Slow(float slowFactor, float duration)
    {
        if (!isAlive) return;
        
        speed *= slowFactor;
        
        // 恢复正常速度
        StartCoroutine(RestoreSpeed(duration));
    }
    
    IEnumerator RestoreSpeed(float duration)
    {
        yield return new WaitForSeconds(duration);
        // 恢复到原始速度（需要保存原始速度）
    }
    
    public void Stun(float duration)
    {
        if (!isAlive) return;
        
        isMoving = false;
        
        // 恢复移动
        StartCoroutine(RestoreMovement(duration));
    }
    
    IEnumerator RestoreMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        isMoving = true;
    }
    
    public void Poison(float damagePerSecond, float duration)
    {
        if (!isAlive) return;
        
        StartCoroutine(ApplyPoison(damagePerSecond, duration));
    }
    
    IEnumerator ApplyPoison(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && isAlive)
        {
            yield return new WaitForSeconds(1f); // 每秒应用一次伤害
            TakeDamage(damagePerSecond);
            elapsed += 1f;
        }
    }
    
    public void SetTargetPoints(Transform[] targets)
    {
        targetPoints = targets;
        currentTargetIndex = 0;
    }
    
    public Transform GetCurrentTarget()
    {
        if (targetPoints != null && currentTargetIndex < targetPoints.Length)
        {
            return targetPoints[currentTargetIndex];
        }
        return null;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 处理与其他对象的碰撞
        if (other.CompareTag("PlayerUnit") || other.CompareTag("Obstacle"))
        {
            // 在实际游戏中，这可能是攻击玩家单位的时机
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 处理物理碰撞
    }
    
    void OnDestroy()
    {
        Debug.Log($"敌人对象 {gameObject.name} 已销毁");
    }
}