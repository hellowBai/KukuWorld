using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleSystem : MonoBehaviour
{
    [Header("战斗设置")]
    public Transform[] enemySpawnPoints;
    public Transform[] defenseTargets;
    public GameObject enemyPrefab;
    
    [Header("波次设置")]
    public float timeBetweenWaves = 10f;
    public int enemiesPerWave = 5;
    public int startingLives = 10;
    
    [Header("敌人属性")]
    public float baseEnemyHealth = 100f;
    public float baseEnemySpeed = 2f;
    public float baseEnemyDamage = 10f;
    
    private int currentWave = 0;
    private int playerLives;
    private bool isWaveActive = false;
    private bool isBattleActive = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    // 事件
    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action<int> OnEnemyReachedTarget;
    public System.Action<int> OnEnemyDefeated;
    public System.Action<bool> OnBattleCompleted;
    
    // 引用其他系统
    private GameManager gameManager;
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        gameManager = GameManager.Instance;
        Debug.Log("战斗系统初始化");
    }
    
    public void SetupDefensePhase(PlayerData playerData)
    {
        // 设置初始生命值
        playerLives = startingLives;
        
        // 初始化玩家单位
        InitializePlayerUnits(playerData);
        
        Debug.Log("防守阶段设置完成");
    }
    
    void InitializePlayerUnits(PlayerData playerData)
    {
        // 这里可以初始化玩家部署的单位
        // 例如从玩家的KuKu集合中部署单位
        Debug.Log($"初始化玩家单位，共 {playerData.CollectedKukus.Count} 个已收集的KuKu");
    }
    
    public void StartDefense()
    {
        if (isBattleActive) return;
        
        isBattleActive = true;
        StartCoroutine(BattleLoop());
        
        Debug.Log("开始防守阶段");
    }
    
    IEnumerator BattleLoop()
    {
        while (isBattleActive && playerLives > 0)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            
            if (isBattleActive && playerLives > 0)
            {
                currentWave++;
                StartCoroutine(SpawnWave());
            }
        }
        
        if (playerLives <= 0)
        {
            EndBattle(false); // 失败
        }
    }
    
    IEnumerator SpawnWave()
    {
        isWaveActive = true;
        
        OnWaveStarted?.Invoke(currentWave);
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(1f); // 间隔生成敌人
        }
        
        // 等待这一波敌人全部被消灭或到达目标
        yield return StartCoroutine(WaitForWaveComplete());
        
        OnWaveCompleted?.Invoke(currentWave);
        
        isWaveActive = false;
    }
    
    IEnumerator WaitForWaveComplete()
    {
        while (HasActiveEnemies())
        {
            yield return null;
        }
    }
    
    bool HasActiveEnemies()
    {
        // 移除已销毁的敌人
        activeEnemies.RemoveAll(go => go == null);
        return activeEnemies.Count > 0;
    }
    
    void SpawnEnemy()
    {
        if (enemySpawnPoints.Length > 0)
        {
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            
            // 实际游戏中应该实例化不同的敌人类型
            GameObject enemy = new GameObject($"Enemy_Wave{currentWave}_{Random.Range(100, 999)}");
            enemy.transform.position = spawnPoint.position;
            
            // 添加敌人组件
            var enemyCtrl = enemy.AddComponent<EnemyController>();
            enemyCtrl.Initialize(currentWave, defenseTargets);
            
            // 设置敌人属性（随波次增强）
            float waveMultiplier = 1.0f + (currentWave - 1) * 0.2f;
            enemyCtrl.SetStats(
                baseEnemyHealth * waveMultiplier,
                baseEnemySpeed * (0.8f + currentWave * 0.05f),
                baseEnemyDamage * waveMultiplier
            );
            
            // 添加碰撞器和刚体
            var collider2D = enemy.AddComponent<BoxCollider2D>();
            var rigidbody2D = enemy.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0;
            
            activeEnemies.Add(enemy);
            
            Debug.Log($"生成敌人 波次:{currentWave}, 位置:{spawnPoint.position}");
        }
    }
    
    public void EnemyReachedTarget(int damage = 1)
    {
        playerLives -= damage;
        
        OnEnemyReachedTarget?.Invoke(playerLives);
        
        Debug.Log($"敌人到达目标！剩余生命: {playerLives}");
        
        if (playerLives <= 0)
        {
            EndBattle(false);
        }
    }
    
    public void EnemyDefeated(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Destroy(enemy);
            
            // 给玩家奖励
            if (gameManager != null)
            {
                gameManager.AddPlayerCoins(Random.Range(5, 15));
                gameManager.AddPlayerSouls(Random.Range(0.5f, 1.5f));
            }
            
            OnEnemyDefeated?.Invoke(currentWave);
            
            Debug.Log("敌人被击败");
        }
    }
    
    public void UnitDefeated(GameObject unit)
    {
        // 处理单位被击败的逻辑
        Debug.Log("单位被击败");
    }
    
    void EndBattle(bool isVictory)
    {
        isBattleActive = false;
        
        // 清理所有敌人
        foreach (var enemy in activeEnemies.ToArray())
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
        
        OnBattleCompleted?.Invoke(isVictory);
        
        if (gameManager != null)
        {
            gameManager.EndGame(isVictory);
        }
        
        Debug.Log(isVictory ? "战斗胜利！" : "战斗失败！");
    }
    
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetPlayerLives()
    {
        return playerLives;
    }
    
    public int GetEnemiesRemaining()
    {
        return activeEnemies.Count;
    }
    
    public float GetWaveProgress()
    {
        if (!isWaveActive) return 0f;
        
        // 简化实现：返回当前波次敌人的清除进度
        return (float)(enemiesPerWave - GetEnemiesRemaining()) / enemiesPerWave;
    }
    
    public bool IsBattleActive()
    {
        return isBattleActive;
    }
    
    public bool IsWaveActive()
    {
        return isWaveActive;
    }
    
    public void SetStartingLives(int lives)
    {
        startingLives = Mathf.Max(1, lives);
    }
    
    public void SetTimeBetweenWaves(float time)
    {
        timeBetweenWaves = Mathf.Max(1f, time);
    }
    
    public void SetEnemiesPerWave(int count)
    {
        enemiesPerWave = Mathf.Max(1, count);
    }
    
    public void SetBaseEnemyStats(float health, float speed, float damage)
    {
        baseEnemyHealth = Mathf.Max(1f, health);
        baseEnemySpeed = Mathf.Max(0.1f, speed);
        baseEnemyDamage = Mathf.Max(1f, damage);
    }
    
    public void IncreaseDifficulty(float multiplier)
    {
        baseEnemyHealth *= multiplier;
        baseEnemyDamage *= multiplier;
        timeBetweenWaves /= multiplier; // 更快的波次间隔
    }
    
    public void AddDefenseTarget(Transform target)
    {
        if (target != null && !System.Array.Exists(defenseTargets, t => t == target))
        {
            System.Array.Resize(ref defenseTargets, defenseTargets.Length + 1);
            defenseTargets[defenseTargets.Length - 1] = target;
        }
    }
    
    public void RemoveDefenseTarget(Transform target)
    {
        if (defenseTargets.Length <= 1) return; // 至少保留一个目标
        
        var list = new List<Transform>(defenseTargets);
        list.Remove(target);
        defenseTargets = list.ToArray();
    }
    
    void OnDestroy()
    {
        // 清理所有敌人
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
    }
}