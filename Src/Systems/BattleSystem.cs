using System.Collections.Generic;
using UnityEngine;
using PetCollector.Data;

namespace PetCollector.Systems
{
    /// <summary>
    /// 战斗系统 - 管理塔防战斗逻辑
    /// </summary>
    public class BattleSystem : MonoBehaviour
    {
        // 战斗状态
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

        // 敌人类型
        public enum EnemyType 
        { 
            WildPet,                  // 野生宠物转化的敌人
            Demon,                    // 邪魔
            EliteDemon,               // 精英邪魔
            Boss                      // BOSS
        }

        // 配置参数
        [Header("战斗配置")]
        public float BaseEnemySpawnInterval = 2.0f;              // 基础生成间隔
        public int MaxEnemiesOnField = 10;                       // 最大敌人数量
        public Transform[] SpawnPoints;                          // 生成点
        public Transform[] TemplePoints;                         // 目标点

        // 捕捉阶段配置
        [Header("捕捉阶段配置")]
        public float CapturePhaseDuration = 300f;                // 捕捉阶段持续时间（5分钟）

        // 战斗数据
        private BattleState currentState = BattleState.CapturePhase; // 当前状态
        private int currentWave = 0;                             // 当前波次
        private int enemiesRemaining = 0;                        // 剩余敌人
        private int playerLives = 10;                            // 玩家生命（守护点血量）
        private float timeUntilNextWave = 0;                     // 下一波倒计时
        private float capturePhaseTimer = 300f;                  // 捕捉阶段倒计时
        private List<GameObject> activeEnemies = new List<GameObject>(); // 活跃敌人
        private List<GameObject> activePets = new List<GameObject>();    // 活跃宠物（玩家防守单位）
        private List<GameObject> activeUnits = new List<GameObject>();   // 活跃单位（建筑生产的机器人/坦克）

        // 事件
        public System.Action<int, int> OnWaveChanged;             // 波次变化
        public System.Action<int> OnPlayerLifeChanged;            // 生命变化
        public System.Action OnBattleVictory;                     // 战斗胜利
        public System.Action OnBattleDefeat;                      // 战斗失败
        public System.Action OnCapturePhaseEnded;                 // 捕捉阶段结束
        public System.Action OnDefensePhaseStarted;               // 防守阶段开始

        void Start()
        {
            InitializeBattle();
        }

        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        private void InitializeBattle()
        {
            // 重置战斗数据
            currentState = BattleState.CapturePhase;
            currentWave = 0;
            playerLives = 10;
            enemiesRemaining = 0;
            timeUntilNextWave = 0;
            capturePhaseTimer = CapturePhaseDuration;
            
            // 清理之前的对象
            ClearAllObjects();
            
            Debug.Log("战斗初始化完成，进入捕捉阶段");
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            try
            {
                InitializeBattle();
                StartCapturePhase();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"开始战斗时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 开始捕捉阶段
        /// </summary>
        public void StartCapturePhase()
        {
            currentState = BattleState.CapturePhase;
            capturePhaseTimer = CapturePhaseDuration;
            
            Debug.Log("捕捉阶段开始！在5分钟内尽可能多地捕捉野怪，为防守阶段做准备！");
        }

        /// <summary>
        /// 开始防守阶段
        /// </summary>
        public void StartDefensePhase()
        {
            currentState = BattleState.DefensePhaseSetup;
            
            Debug.Log("捕捉阶段结束，进入防守阶段！使用捕捉到的宠物和建筑抵御敌人！");
            
            // 生成玩家宠物
            SpawnPlayerPets();
            
            // 开始第一波敌人
            StartNextWave();
            
            // 通知UI更新
            OnDefensePhaseStarted?.Invoke();
        }

        /// <summary>
        /// 生成玩家宠物（防守阶段）
        /// </summary>
        private void SpawnPlayerPets()
        {
            try
            {
                if (GameManager.Instance?.PlayerData == null) return;
                
                var activePetIds = GameManager.Instance.PlayerData.ActivePetTeam;
                
                for (int i = 0; i < activePetIds.Count && i < 5; i++) // 最多5只宠物
                {
                    int petId = activePetIds[i];
                    
                    if (GameManager.Instance.PlayerData.CollectedPets.ContainsKey(petId))
                    {
                        var petData = GameManager.Instance.PlayerData.CollectedPets[petId];
                        
                        // 实例化宠物预制体
                        GameObject petObject = new GameObject($"Guardian_{petData.Name}_{petId}");
                        petObject.transform.position = new Vector3(-5 + i * 2, 0, 0); // 在守护点前方布阵
                        
                        // 添加宠物战斗组件
                        var petComponent = petObject.AddComponent<PetCombatController>();
                        petComponent.Initialize(petData);
                        
                        activePets.Add(petObject);
                    }
                }
                
                Debug.Log($"生成了 {activePets.Count} 只守护宠物来保卫守护点");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成守护宠物时发生错误: {e.Message}");
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
                
                currentState = BattleState.WaveStart;
                
                // 通知UI更新
                OnWaveChanged?.Invoke(currentWave, enemiesRemaining);
                
                Debug.Log($"第 {currentWave} 波敌人来袭，共 {enemyCount} 只敌人，守护守护点！");
                
                // 开始生成敌人
                currentState = BattleState.Fighting;
                
                // 生成不同类型的敌人
                for (int i = 0; i < enemyCount; i++)
                {
                    EnemyType type = GetEnemyTypeForWave(currentWave);
                    SpawnEnemy(type);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"开始新波次时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        private void SpawnEnemy(EnemyType type = EnemyType.WildPet)
        {
            try
            {
                if (SpawnPoints.Length == 0) return;
                
                // 选择出生点
                Transform spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                
                // 随机选择守护点目标
                Transform[] selectedTargets = TemplePoints;
                if (TemplePoints != null && TemplePoints.Length > 0)
                {
                    // 随机选择一个目标点作为主要攻击目标
                    int randomIndex = Random.Range(0, TemplePoints.Length);
                    selectedTargets = new Transform[] { TemplePoints[randomIndex] };
                }
                
                // 创建敌人对象
                GameObject enemyObject = new GameObject($"Enemy_Type{type}_Wave{currentWave}");
                enemyObject.transform.position = spawnPoint.position;
                
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
                
                enemyComponent.Initialize(health, speed, damage, selectedTargets, type);
                
                activeEnemies.Add(enemyObject);
                enemiesRemaining--;
                
                Debug.Log($"生成敌人，类型: {type}，剩余 {enemiesRemaining} 只");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成敌人时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 更新战斗逻辑
        /// </summary>
        public void UpdateGame(float deltaTime)
        {
            switch (currentState)
            {
                case BattleState.CapturePhase:
                    UpdateCapturePhase();
                    break;
                    
                case BattleState.Fighting:
                    UpdateDefensePhase();
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
        /// 更新捕捉阶段
        /// </summary>
        private void UpdateCapturePhase()
        {
            capturePhaseTimer -= Time.deltaTime;
            
            if (capturePhaseTimer <= 0)
            {
                EndCapturePhase();
            }
        }

        /// <summary>
        /// 更新防守阶段
        /// </summary>
        private void UpdateDefensePhase()
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
                    int coins = Random.Range(5, 15);
                    int gems = Random.Range(0, 3);
                    float souls = Random.Range(0.5f, 2.0f);
                    
                    if (GameManager.Instance?.PlayerData != null)
                    {
                        GameManager.Instance.PlayerData.AddCoins(coins);
                        GameManager.Instance.PlayerData.AddGems(gems);
                        GameManager.Instance.PlayerData.AddSouls(souls);
                    }
                    
                    Debug.Log($"敌人被击败！获得奖励：{coins}金币，{gems}神石，{souls}灵魂");
                }
            }
            catch (System.Exception e)
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
                    
                    Debug.Log($"敌人突破防线，正在破坏守护点！剩余生命: {playerLives}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理敌人到达守护点时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 获取当前波次信息
        /// </summary>
        public (int wave, int remaining) GetWaveInfo()
        {
            return (currentWave, enemiesRemaining);
        }

        /// <summary>
        /// 获取战斗状态
        /// </summary>
        public (BattleState state, int lives, int wave, int enemies) GetBattleStatus()
        {
            return (currentState, playerLives, currentWave, enemiesRemaining);
        }

        /// <summary>
        /// 检查是否在捕捉阶段
        /// </summary>
        public bool IsInCapturePhase()
        {
            return currentState == BattleState.CapturePhase;
        }

        /// <summary>
        /// 检查是否在防守阶段
        /// </summary>
        public bool IsInDefensePhase()
        {
            return currentState == BattleState.WaveStart || 
                   currentState == BattleState.Fighting ||
                   currentState == BattleState.Victory ||
                   currentState == BattleState.Defeat;
        }

        /// <summary>
        /// 获取捕捉阶段剩余时间
        /// </summary>
        public float GetCapturePhaseTimeRemaining()
        {
            return capturePhaseTimer;
        }

        /// <summary>
        /// 结束捕捉阶段
        /// </summary>
        private void EndCapturePhase()
        {
            currentState = BattleState.DefensePhaseSetup;
            OnCapturePhaseEnded?.Invoke();
            
            Debug.Log("捕捉阶段结束，准备进入防守阶段！");
        }

        /// <summary>
        /// 清理所有对象
        /// </summary>
        private void ClearAllObjects()
        {
            // 销毁活跃的敌人
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            activeEnemies.Clear();
            
            // 销毁活跃的宠物
            foreach (var pet in activePets)
            {
                if (pet != null) Destroy(pet);
            }
            activePets.Clear();
            
            // 销毁活跃的单位
            foreach (var unit in activeUnits)
            {
                if (unit != null) Destroy(unit);
            }
            activeUnits.Clear();
        }

        /// <summary>
        /// 获取活跃敌人列表
        /// </summary>
        public List<GameObject> GetActiveEnemies()
        {
            return activeEnemies;
        }

        /// <summary>
        /// 获取活跃宠物列表
        /// </summary>
        public List<GameObject> GetActivePets()
        {
            return activePets;
        }

        /// <summary>
        /// 获取敌人类型
        /// </summary>
        private EnemyType GetEnemyTypeForWave(int wave)
        {
            // 随着波次增加，出现更高级的敌人
            float rand = Random.value;
            if (wave >= 10 && rand > 0.7f)
            {
                return EnemyType.Boss;
            }
            else if (wave >= 5 && rand > 0.5f)
            {
                return EnemyType.EliteDemon;
            }
            else if (wave >= 3 && rand > 0.3f)
            {
                return EnemyType.Demon;
            }
            else
            {
                return EnemyType.WildPet;
            }
        }

        /// <summary>
        /// 重置战斗系统
        /// </summary>
        public void Reset()
        {
            ClearAllObjects();
            InitializeBattle();
        }
    }
}