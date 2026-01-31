using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 战斗系统 - 管理塔防战斗逻辑、敌人生成、战斗状态等
    /// </summary>
    public class BattleSystem
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
            WildPet,                  // 野生KuKu转化的敌人
            Demon,                    // 邪魔
            EliteDemon,               // 精英邪魔
            Boss                      // BOSS
        }

        // 配置参数
        public float BaseEnemySpawnInterval = 2.0f;              // 基础生成间隔
        public int MaxEnemiesOnField = 10;                       // 最大敌人数量
        public Transform[] SpawnPoints;                          // 生成点
        public Transform[] TemplePoints;                         // 目标点

        // 战斗数据
        private BattleState currentState = BattleState.Setup;     // 当前状态
        private int currentWave = 1;                             // 当前波次
        private int enemiesRemaining = 0;                        // 剩余敌人
        private int playerLives = 10;                            // 玩家生命（守护点血量）
        private float timeUntilNextWave = 5.0f;                  // 下一波倒计时
        private List<GameObject> activeEnemies = new List<GameObject>(); // 活跃敌人
        private List<UnitData> activePets = new List<UnitData>();        // 活跃KuKu（玩家防守单位）
        private List<UnitData> activeUnits = new List<UnitData>();       // 活跃单位（建筑生产的机器人/坦克）
        private List<BuildingData> activeTowers = new List<BuildingData>(); // 活跃防御塔

        // 神殿相关
        public GameObject nuwaTemple;                            // 女娲神殿
        public int nuwaTempleHealth = 1000;                      // 神殿生命值
        public int maxTempleHealth = 1000;                       // 神殿最大生命值

        // 配置参数
        public Transform[] evilSpawnPoints;                      // 邪魔出生点
        public Transform[] nuwaTemplePoints;                     // 神殿目标点
        public float difficultyIncreaseRate = 0.1f;              // 难度递增率

        // 战斗数据（续）
        private int waveNumber = 1;                              // 波次数
        private int evilCount = 0;                               // 邪魂数量
        private bool isDefending = false;                        // 是否在防守
        private float cumulativeDifficulty = 1.0f;               // 累积难度

        // 事件
        public event Action<int, int> OnWaveChanged;             // 波次变化
        public event Action<int> OnPlayerLifeChanged;            // 生命变化
        public event Action OnBattleVictory;                     // 战斗胜利
        public event Action OnBattleDefeat;                      // 战斗失败
        public event Action OnCapturePhaseEnded;                 // 捕捉阶段结束
        public event Action OnDefensePhaseStarted;               // 防守阶段开始
        public event Action<int> OnWaveStarted;                  // 波次开始
        public event Action<int, int> OnEvilSpawned;             // 邪魔生成
        public event Action<int> OnTempleHealthChanged;          // 神殿生命变化
        public event Action OnDefenseSuccess;                    // 防守成功
        public event Action OnDefenseFailed;                     // 防守失败

        // 构造函数
        public BattleSystem()
        {
            InitializeBattle();
        }

        /// <summary>
        /// 初始化战斗
        /// </summary>
        private void InitializeBattle()
        {
            currentState = BattleState.Setup;
            currentWave = 1;
            playerLives = 10;
            nuwaTempleHealth = maxTempleHealth;
            
            Debug.Log("战斗系统初始化完成");
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            if (currentState == BattleState.Setup)
            {
                currentState = BattleState.CapturePhase;
                Debug.Log("战斗开始 - 捕捉阶段");
            }
        }

        /// <summary>
        /// 初始化防守阶段
        /// </summary>
        public void InitializeDefensePhase(PlayerData playerData)
        {
            if (playerData == null)
            {
                Debug.LogError("玩家数据为空，无法初始化防守阶段");
                return;
            }

            // 将玩家的KuKu添加到活跃单位列表
            foreach (var kvp in playerData.CollectedKukus)
            {
                // 将神话KuKu转换为可部署单位
                UnitData petUnit = ConvertMythicalKukuToUnit(kvp.Value);
                if (petUnit != null)
                {
                    activePets.Add(petUnit);
                }
            }

            // 添加玩家部署的单位
            activeUnits.AddRange(playerData.DeployedUnits);

            // 获取玩家建造的防御塔
            // 这里需要连接到BuildingManager来获取塔类建筑
            currentState = BattleState.DefensePhaseSetup;
            isDefending = true;

            Debug.Log($"防守阶段初始化完成，玩家单位数量: {activePets.Count + activeUnits.Count}");
        }

        /// <summary>
        /// 将神话KuKu转换为单位
        /// </summary>
        private UnitData ConvertMythicalKukuToUnit(MythicalKukuData kuku)
        {
            if (kuku == null) return null;

            UnitData unit = new UnitData
            {
                Name = kuku.Name,
                Description = kuku.Description,
                Type = UnitData.UnitType.Hybrid, // KuKu单位类型
                AttackPower = kuku.AttackPower,
                DefensePower = kuku.DefensePower,
                Speed = kuku.Speed,
                Health = kuku.Health,
                Range = kuku.SkillRange,
                Level = kuku.Level,
                MaxEquipmentSlots = kuku.MaxEquipmentSlots,
                IsDeployed = true
            };

            return unit;
        }

        /// <summary>
        /// 开始下一波
        /// </summary>
        public void StartNextWave()
        {
            currentWave++;
            waveNumber = currentWave;

            // 计算当前波次的敌人数量和强度
            enemiesRemaining = CalculateEnemiesForWave(currentWave);
            evilCount = enemiesRemaining;

            // 更新难度
            cumulativeDifficulty = 1.0f + (currentWave - 1) * difficultyIncreaseRate;

            currentState = BattleState.WaveStart;
            
            // 生成敌人
            SpawnWaveEnemies();

            OnWaveStarted?.Invoke(currentWave);
            OnWaveChanged?.Invoke(currentWave, enemiesRemaining);

            Debug.Log($"第 {currentWave} 波开始，敌人数量: {enemiesRemaining}");
        }

        /// <summary>
        /// 计算波次敌人数量
        /// </summary>
        private int CalculateEnemiesForWave(int wave)
        {
            // 随着波次增加，敌人数量线性增长
            return 5 + (wave - 1) * 2;
        }

        /// <summary>
        /// 生成波次敌人
        /// </summary>
        private void SpawnWaveEnemies()
        {
            for (int i = 0; i < enemiesRemaining; i++)
            {
                // 根据波次和难度生成敌人
                EnemyType enemyType = DetermineEnemyTypeForWave(currentWave);
                SpawnEnemy(enemyType);
                
                // 为了避免同时生成过多敌人，稍微延迟
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 确定波次敌人类型
        /// </summary>
        private EnemyType DetermineEnemyTypeForWave(int wave)
        {
            if (wave <= 5)
            {
                return EnemyType.WildPet;
            }
            else if (wave <= 10)
            {
                return UnityEngine.Random.value < 0.7f ? EnemyType.WildPet : EnemyType.Demon;
            }
            else if (wave <= 15)
            {
                float rand = UnityEngine.Random.value;
                if (rand < 0.5f) return EnemyType.WildPet;
                else if (rand < 0.85f) return EnemyType.Demon;
                else return EnemyType.EliteDemon;
            }
            else
            {
                float rand = UnityEngine.Random.value;
                if (rand < 0.3f) return EnemyType.Demon;
                else if (rand < 0.6f) return EnemyType.EliteDemon;
                else return EnemyType.Boss;
            }
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        public void SpawnEnemy(EnemyType type = EnemyType.WildPet)
        {
            // 在实际游戏中，这里会实例化敌人的GameObject
            // 暂时用日志代替
            Debug.Log($"生成敌人: {type}");

            // 更新活跃敌人列表（虚拟）
            // 在实际实现中，这里会创建真正的敌人对象

            OnEvilSpawned?.Invoke(waveNumber, activeEnemies.Count + 1);
        }

        /// <summary>
        /// 敌人被击败
        /// </summary>
        public void EnemyDefeated(GameObject enemy)
        {
            enemiesRemaining--;
            evilCount--;

            // 给玩家奖励
            RewardPlayerForDefeat();

            if (enemiesRemaining <= 0)
            {
                // 当前波次完成
                WaveCompleted();
            }

            Debug.Log($"敌人被击败，剩余: {enemiesRemaining}");
        }

        /// <summary>
        /// 给玩家击败敌人的奖励
        /// </summary>
        private void RewardPlayerForDefeat()
        {
            // 给玩家金币、灵魂等奖励
            // 在实际游戏中，这里会连接到玩家数据
        }

        /// <summary>
        /// 敌人到达终点
        /// </summary>
        public void EnemyReachedEnd(GameObject enemy)
        {
            // 损失神殿生命值
            nuwaTempleHealth -= 100; // 每个敌人造成100点伤害

            if (nuwaTempleHealth <= 0)
            {
                // 神殿被摧毁，防守失败
                Defeat();
            }
            else
            {
                // 更新神殿生命值
                OnTempleHealthChanged?.Invoke(nuwaTempleHealth);
            }

            // 从活跃敌人列表中移除
            activeEnemies.Remove(enemy);

            Debug.Log($"敌人到达终点，神殿生命值: {nuwaTempleHealth}");
        }

        /// <summary>
        /// 波次完成
        /// </summary>
        private void WaveCompleted()
        {
            currentState = BattleState.Fighting;

            Debug.Log($"第 {currentWave} 波完成！");

            // 检查是否完成所有波次
            if (currentWave >= 20) // 假设有20波
            {
                Victory();
            }
            else
            {
                // 准备下一波
                timeUntilNextWave = 5.0f; // 5秒后开始下一波
            }
        }

        /// <summary>
        /// 胜利
        /// </summary>
        private void Victory()
        {
            currentState = BattleState.Victory;
            isDefending = false;

            OnBattleVictory?.Invoke();
            OnDefenseSuccess?.Invoke();

            Debug.Log("防守成功！胜利！");
        }

        /// <summary>
        /// 失败
        /// </summary>
        private void Defeat()
        {
            currentState = BattleState.Defeat;
            isDefending = false;

            OnBattleDefeat?.Invoke();
            OnDefenseFailed?.Invoke();

            Debug.Log("防守失败！神殿被摧毁！");
        }

        /// <summary>
        /// 更新战斗逻辑
        /// </summary>
        public void UpdateBattleLogic(float deltaTime)
        {
            switch (currentState)
            {
                case BattleState.WaveStart:
                    currentState = BattleState.Fighting;
                    break;

                case BattleState.Fighting:
                    // 更新敌人和塔的互动
                    UpdateTowerEnemyInteractions(deltaTime);
                    
                    // 检查是否需要开始下一波
                    if (enemiesRemaining <= 0 && timeUntilNextWave > 0)
                    {
                        timeUntilNextWave -= deltaTime;
                        if (timeUntilNextWave <= 0)
                        {
                            StartNextWave();
                        }
                    }
                    break;

                case BattleState.CapturePhase:
                    // 捕捉阶段结束检查
                    // 这里需要外部信号来切换到防守阶段
                    break;

                case BattleState.DefensePhaseSetup:
                    // 设置完成后开始第一波
                    StartNextWave();
                    currentState = BattleState.Fighting;
                    break;
            }
        }

        /// <summary>
        /// 更新塔与敌人的互动
        /// </summary>
        private void UpdateTowerEnemyInteractions(float deltaTime)
        {
            // 在实际游戏中，这里会处理塔对敌人的攻击逻辑
            // 检测敌人是否在塔的射程内并进行攻击
        }

        /// <summary>
        /// 切换到防守阶段
        /// </summary>
        public void TransitionToDefensePhase()
        {
            if (currentState == BattleState.CapturePhase)
            {
                currentState = BattleState.DefensePhaseSetup;
                OnCapturePhaseEnded?.Invoke();
                OnDefensePhaseStarted?.Invoke();
                
                Debug.Log("切换到防守阶段");
            }
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
            return currentState == BattleState.DefensePhaseSetup || 
                   currentState == BattleState.WaveStart || 
                   currentState == BattleState.Fighting;
        }

        /// <summary>
        /// 获取战斗状态信息
        /// </summary>
        public (BattleState state, int wave, int lives, int enemies) GetBattleStatus()
        {
            return (currentState, currentWave, playerLives, enemiesRemaining);
        }

        /// <summary>
        /// 获取防守状态
        /// </summary>
        public (bool defending, int wave, int health, int maxHealth) GetDefenseStatus()
        {
            return (isDefending, waveNumber, nuwaTempleHealth, maxTempleHealth);
        }

        /// <summary>
        /// 计算当前难度系数
        /// </summary>
        public float GetCurrentDifficultyMultiplier()
        {
            return cumulativeDifficulty;
        }

        /// <summary>
        /// 生成特定类型的邪魔
        /// </summary>
        public GameObject SpawnSpecificEvil(EnemyType type, int waveNum)
        {
            // 在实际游戏中，这里会创建特定类型的敌人
            Debug.Log($"生成特定敌人: {type}, 波次: {waveNum}");
            
            // 返回虚拟的游戏对象
            return new GameObject($"Evil_{type}_{waveNum}");
        }

        /// <summary>
        /// 邪魔攻击神殿
        /// </summary>
        public void EvilAttackedTemple(float damage)
        {
            nuwaTempleHealth -= (int)(damage * cumulativeDifficulty);

            if (nuwaTempleHealth <= 0)
            {
                nuwaTempleHealth = 0;
                Defeat();
            }

            OnTempleHealthChanged?.Invoke(nuwaTempleHealth);

            Debug.Log($"神殿受到攻击，当前生命值: {nuwaTempleHealth}");
        }

        /// <summary>
        /// 邪魔被击败
        /// </summary>
        public void EvilDefeated()
        {
            // 在实际游戏中，这里会处理邪魔被击败的逻辑
        }

        /// <summary>
        /// 完成当前波次
        /// </summary>
        public void CompleteWave()
        {
            WaveCompleted();
        }

        /// <summary>
        /// 开始指定波次
        /// </summary>
        private void StartWave(int waveNum)
        {
            currentWave = waveNum;
            StartNextWave();
        }

        /// <summary>
        /// 重置战斗系统
        /// </summary>
        public void Reset()
        {
            currentState = BattleState.Setup;
            currentWave = 1;
            enemiesRemaining = 0;
            playerLives = 10;
            timeUntilNextWave = 5.0f;
            activeEnemies.Clear();
            activePets.Clear();
            activeUnits.Clear();
            nuwaTempleHealth = maxTempleHealth;
            waveNumber = 1;
            evilCount = 0;
            isDefending = false;
            cumulativeDifficulty = 1.0f;

            Debug.Log("战斗系统已重置");
        }

        /// <summary>
        /// 获取当前波次信息
        /// </summary>
        public (int currentWave, int enemiesRemaining, float difficulty) GetCurrentWaveInfo()
        {
            return (currentWave, enemiesRemaining, cumulativeDifficulty);
        }

        /// <summary>
        /// 强制开始战斗
        /// </summary>
        public void ForceStartBattle()
        {
            currentState = BattleState.DefensePhaseSetup;
            StartNextWave();
        }

        /// <summary>
        /// 设置最大敌人数量
        /// </summary>
        public void SetMaxEnemiesOnField(int max)
        {
            MaxEnemiesOnField = max;
        }

        /// <summary>
        /// 获取当前战斗状态描述
        /// </summary>
        public string GetBattleStatusDescription()
        {
            switch (currentState)
            {
                case BattleState.Setup:
                    return "设置阶段";
                case BattleState.CapturePhase:
                    return "捕捉阶段";
                case BattleState.DefensePhaseSetup:
                    return "防守准备";
                case BattleState.WaveStart:
                    return $"第 {currentWave} 波开始";
                case BattleState.Fighting:
                    return $"战斗中 - 第 {currentWave} 波，剩余敌人: {enemiesRemaining}";
                case BattleState.Victory:
                    return "胜利";
                case BattleState.Defeat:
                    return "失败";
                default:
                    return "未知状态";
            }
        }

        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        public bool IsBattleFinished()
        {
            return currentState == BattleState.Victory || currentState == BattleState.Defeat;
        }

        /// <summary>
        /// 获取胜利条件
        /// </summary>
        public bool CheckWinCondition()
        {
            return currentState == BattleState.Victory;
        }

        /// <summary>
        /// 获取失败条件
        /// </summary>
        public bool CheckLossCondition()
        {
            return currentState == BattleState.Defeat;
        }
    }
}