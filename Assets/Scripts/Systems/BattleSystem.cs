using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 战斗系统
    /// </summary>
    public class BattleSystem
    {
        // 配置参数
        public float BaseEnemySpawnInterval = 2.0f;  // 基础敌人生成间隔
        public int MaxEnemiesOnField = 10;           // 最大同时在场敌人数量

        // 战斗数据
        private BattleState currentState = BattleState.Setup; // 当前状态
        private int currentWave = 0;                 // 当前波次
        private int enemiesRemaining = 0;            // 剩余敌人数量
        private int playerLives = 10;                // 玩家生命值（守护点血量）
        private float timeUntilNextWave = 0;         // 下一波倒计时
        private List<GameObject> activeEnemies = new List<GameObject>(); // 活跃敌人列表
        private List<GameObject> activeKukus = new List<GameObject>();    // 活跃KuKu列表
        private List<GameObject> activeUnits = new List<GameObject>();   // 活跃单位列表（机器人/坦克）

        // 事件系统
        public event Action<int, int> OnWaveChanged;      // 波次变化事件 (当前波次, 剩余敌人)
        public event Action<int> OnPlayerLifeChanged;     // 玩家生命值变化事件 (剩余生命)
        public event Action<int> OnWaveStarted;           // 波次开始事件 (波次数)
        public event Action<int> OnEnemyReachedTarget;    // 敌人到达目标事件 (剩余生命)
        public event Action OnBattleVictory;              // 战斗胜利事件
        public event Action OnBattleDefeat;               // 战斗失败事件
        public event Action<float> OnBattleUpdate;        // 战斗更新事件 (游戏时间)

        /// <summary>
        /// 战斗状态
        /// </summary>
        public enum BattleState 
        { 
            Setup,                    // 设置阶段
            CapturePhase,             // 捕捉阶段（前5分钟）
            DefensePhaseSetup,        // 防守阶段设置
            WaveStart,                // 波次开始
            Fighting,                 // 战斗中
            Victory,                  // 胜利
            Defeat                    // 失败
        }

        /// <summary>
        /// 敌人类型
        /// </summary>
        public enum EnemyType 
        { 
            WildPet,                  // 野生KuKu转化的敌人
            Demon,                    // 邪魔
            EliteDemon,               // 精英邪魔
            Boss                      // BOSS
        }

        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        public void Initialize()
        {
            // 重置战斗数据
            currentState = BattleState.Setup;
            currentWave = 0;
            playerLives = 10;
            enemiesRemaining = 0;
            timeUntilNextWave = 0;
            
            // 清理之前的对象
            ClearAllObjects();
            
            Debug.Log("战斗系统初始化完成");
        }

        /// <summary>
        /// 初始化防守阶段
        /// </summary>
        public void InitializeDefensePhase(PlayerData playerData)
        {
            currentState = BattleState.DefensePhaseSetup;
            
            Debug.Log("防守阶段初始化，部署玩家单位！");
            
            // 部署玩家的KuKu
            DeployPlayerKukus(playerData);
            
            // 部署玩家的单位
            DeployPlayerUnits(playerData);
            
            // 开始第一波敌人
            StartNextWave();
            
            currentState = BattleState.WaveStart;
        }

        /// <summary>
        /// 部署玩家KuKu（防守阶段）
        /// </summary>
        private void DeployPlayerKukus(PlayerData playerData)
        {
            try
            {
                var activeKukuIds = playerData.ActiveKukuTeam;
                
                for (int i = 0; i < activeKukuIds.Count && i < 5; i++) // 最多5只KuKu
                {
                    int kukuId = activeKukuIds[i];
                    
                    if (playerData.CollectedKukus.ContainsKey(kukuId))
                    {
                        var kukuData = playerData.CollectedKukus[kukuId];
                        
                        // 在实际游戏中，这里会实例化KuKu预制体
                        // GameObject kukuObject = Instantiate(kukuPrefab, deployPosition, Quaternion.identity);
                        // 为了演示，我们创建一个虚拟对象
                        
                        GameObject kukuObject = new GameObject($"PlayerKuku_{kukuData.Name}_{kukuId}");
                        kukuObject.transform.position = new Vector3(-5 + i * 2, 0, 0); // 在守护点前方布阵
                        
                        // 添加KuKu战斗组件
                        var kukuComponent = kukuObject.AddComponent<KukuCombatController>();
                        kukuComponent.Initialize(kukuData);
                        
                        activeKukus.Add(kukuObject);
                    }
                }
                
                Debug.Log($"部署了 {activeKukus.Count} 只玩家KuKu来保卫守护点");
            }
            catch (Exception e)
            {
                Debug.LogError($"部署玩家KuKu时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 部署玩家单位（防守阶段）
        /// </summary>
        private void DeployPlayerUnits(PlayerData playerData)
        {
            try
            {
                // 部署玩家的已部署单位
                foreach (var unit in playerData.DeployedUnits)
                {
                    GameObject unitObject = new GameObject($"PlayerUnit_{unit.Name}_{unit.Id}");
                    unitObject.transform.position = new Vector3(-4 + UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-2f, 2f));
                    
                    var unitComponent = unitObject.AddComponent<UnitCombatController>();
                    unitComponent.Initialize(unit);
                    
                    activeUnits.Add(unitObject);
                }
                
                Debug.Log($"部署了 {activeUnits.Count} 个玩家单位");
            }
            catch (Exception e)
            {
                Debug.LogError($"部署玩家单位时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        private void SpawnEnemy(EnemyType type = EnemyType.WildPet)
        {
            try
            {
                // 在实际游戏中，这里会从预设的出生点生成敌人
                Vector3 spawnPosition = new Vector3(10, 0, UnityEngine.Random.Range(-5f, 5f)); // 从右侧生成
                
                // 创建敌人对象
                GameObject enemyObject = new GameObject($"Enemy_Type{type}_Wave{currentWave}");
                enemyObject.transform.position = spawnPosition;
                
                // 添加敌人组件
                var enemyComponent = enemyObject.AddComponent<EnemyController>();
                
                // 根据波次和类型设置敌人属性
                float health = 50f + currentWave * 10f;
                float speed = 1f + currentWave * 0.1f;
                float damage = 5f + currentWave * 0.5f;
                
                // 根据敌人类型调整属性
                switch (type)
                {
                    case EnemyType.Demon:
                        health *= 1.5f;
                        damage *= 1.3f;
                        break;
                    case EnemyType.EliteDemon:
                        health *= 2.5f;
                        damage *= 2.0f;
                        speed *= 1.2f;
                        break;
                    case EnemyType.Boss:
                        health *= 5.0f;
                        damage *= 3.0f;
                        break;
                }
                
                enemyComponent.Initialize(health, speed, damage, type);
                
                activeEnemies.Add(enemyObject);
                enemiesRemaining--;
                
                Debug.Log($"生成敌人，类型: {type}，剩余 {enemiesRemaining} 只");
            }
            catch (Exception e)
            {
                Debug.LogError($"生成敌人时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 开始下一波
        /// </summary>
        public void StartNextWave()
        {
            try
            {
                currentWave++;
                
                // 计算当前波次的敌人数量和类型
                int enemyCount = Mathf.Min(currentWave * 3, 30); // 每波最多30只敌人
                enemiesRemaining = enemyCount;
                
                // 通知UI更新
                OnWaveChanged?.Invoke(currentWave, enemiesRemaining);
                OnWaveStarted?.Invoke(currentWave);
                
                Debug.Log($"第 {currentWave} 波敌人来袭，共 {enemyCount} 只敌人，守护守护点！");
                
                // 生成不同类型的敌人
                for (int i = 0; i < enemyCount; i++)
                {
                    EnemyType type = GetEnemyTypeForWave(currentWave);
                    SpawnEnemy(type);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"开始新波次时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 获取当前波次的敌人类型
        /// </summary>
        private EnemyType GetEnemyTypeForWave(int waveNum)
        {
            // 随着波次增加，出现更高级的敌人
            float rand = UnityEngine.Random.value;
            
            if (waveNum >= 10 && rand < 0.1f)
            {
                return EnemyType.Boss; // 第10波后有10%概率出现BOSS
            }
            else if (waveNum >= 5 && rand < 0.3f)
            {
                return EnemyType.EliteDemon; // 第5波后有30%概率出现精英
            }
            else if (waveNum >= 2 && rand < 0.6f)
            {
                return EnemyType.Demon; // 第2波后有60%概率出现邪魔
            }
            else
            {
                return EnemyType.WildPet; // 其他是野生KuKu
            }
        }

        /// <summary>
        /// 更新战斗逻辑
        /// </summary>
        public void UpdateBattleLogic(float deltaTime)
        {
            switch (currentState)
            {
                case BattleState.Fighting:
                    UpdateDefensePhase(deltaTime);
                    break;
            }
            
            // 检查胜利条件
            if (currentState == BattleState.Fighting && enemiesRemaining <= 0 && activeEnemies.Count <= 0)
            {
                currentState = BattleState.Victory;
                OnBattleVictory?.Invoke();
                Debug.Log("战斗胜利！");
            }
            
            // 检查失败条件
            if (playerLives <= 0)
            {
                currentState = BattleState.Defeat;
                OnBattleDefeat?.Invoke();
                Debug.Log("战斗失败！");
            }
        }

        /// <summary>
        /// 更新防守阶段
        /// </summary>
        private void UpdateDefensePhase(float deltaTime)
        {
            // 检查是否需要开始下一波
            if (enemiesRemaining <= 0 && activeEnemies.Count <= 0)
            {
                StartNextWave();
            }
        }

        /// <summary>
        /// 敌人被击败
        /// </summary>
        public void EnemyDefeated(GameObject enemy)
        {
            try
            {
                if (activeEnemies.Contains(enemy))
                {
                    activeEnemies.Remove(enemy);
                    
                    // 给玩家奖励
                    int coins = UnityEngine.Random.Range(5, 15);
                    int gems = UnityEngine.Random.Range(0, 3);
                    float souls = UnityEngine.Random.Range(0.5f, 2.0f);
                    
                    // 在实际游戏中，这会更新玩家数据
                    // GameManager.Instance.PlayerData.AddCoins(coins);
                    // GameManager.Instance.PlayerData.AddGems(gems);
                    // GameManager.Instance.PlayerData.AddSouls(souls);
                    
                    Debug.Log($"敌人被击败！获得奖励：{coins}金币，{gems}神石，{souls}灵魂");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"处理敌人被击败时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 敌人到达终点（破坏守护点）
        /// </summary>
        public void EnemyReachedEnd(GameObject enemy)
        {
            try
            {
                if (activeEnemies.Contains(enemy))
                {
                    activeEnemies.Remove(enemy);
                    playerLives--;
                    
                    OnPlayerLifeChanged?.Invoke(playerLives);
                    OnEnemyReachedTarget?.Invoke(playerLives);
                    
                    Debug.Log($"敌人突破防线，正在破坏守护点！剩余生命: {playerLives}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"处理敌人到达守护点时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 获取当前波次信息
        /// </summary>
        public (int wave, int remaining, int total) GetWaveInfo()
        {
            int totalEnemies = currentWave * 3;
            return (currentWave, enemiesRemaining, totalEnemies);
        }

        /// <summary>
        /// 获取战斗状态
        /// </summary>
        public (BattleState state, int lives, int wave, int enemies) GetBattleStatus()
        {
            return (currentState, playerLives, currentWave, enemiesRemaining);
        }

        /// <summary>
        /// 获取活跃敌人列表
        /// </summary>
        public List<GameObject> GetActiveEnemies()
        {
            return new List<GameObject>(activeEnemies);
        }

        /// <summary>
        /// 清理所有对象
        /// </summary>
        private void ClearAllObjects()
        {
            // 销毁所有活跃的对象
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null) GameObject.Destroy(enemy);
            }
            activeEnemies.Clear();
            
            foreach (var kuku in activeKukus)
            {
                if (kuku != null) GameObject.Destroy(kuku);
            }
            activeKukus.Clear();
            
            foreach (var unit in activeUnits)
            {
                if (unit != null) GameObject.Destroy(unit);
            }
            activeUnits.Clear();
        }

        /// <summary>
        /// 重置战斗系统
        /// </summary>
        public void Reset()
        {
            ClearAllObjects();
            Initialize();
        }
    }

    /// <summary>
    /// 敌人控制器
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
            // 为了演示，我们使用一个假的目标点
            target = new GameObject("TempTarget").transform;
            target.position = new Vector3(-10, 0, 0); // 守护点在左侧
            
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

    /// <summary>
    /// KuKu战斗控制器
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

    /// <summary>
    /// 单位战斗控制器
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