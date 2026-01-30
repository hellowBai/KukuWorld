using System.Collections;
using UnityEngine;
using PetCollector.Data;

namespace PetCollector.Systems
{
    /// <summary>
    /// 女娲守护系统 - 管理守护据点的防御战斗
    /// </summary>
    public class NuwaDefenseSystem : MonoBehaviour
    {
        // 守护点相关
        [Header("守护点设置")]
        public GameObject nuwaTemple;                            // 女娲神殿
        public int nuwaTempleHealth = 1000;                      // 神殿生命值
        public int maxTempleHealth = 1000;                       // 神殿最大生命值

        // 战场设置
        [Header("战场设置")]
        public Transform[] evilSpawnPoints;                      // 邪魔出生点
        public Transform[] nuwaTemplePoints;                     // 神殿目标点

        // 难度设置
        [Header("难度设置")]
        public float difficultyIncreaseRate = 0.1f;              // 难度递增率

        // 战斗数据
        private int waveNumber = 1;                              // 波次数
        private int evilCount = 0;                               // 邪魂数量
        private bool isDefending = false;                        // 是否在防守
        private float cumulativeDifficulty = 1.0f;              // 累积难度

        // 事件
        public System.Action<int> OnWaveStarted;                  // 波次开始事件
        public System.Action<int, int> OnEvilSpawned;             // 邪魔生成事件
        public System.Action<int> OnTempleHealthChanged;          // 神殿生命变化事件
        public System.Action OnDefenseSuccess;                    // 防守成功事件
        public System.Action OnDefenseFailed;                     // 防守失败事件

        void Start()
        {
            InitializeDefenseSystem();
        }

        /// <summary>
        /// 初始化守护系统
        /// </summary>
        private void InitializeDefenseSystem()
        {
            nuwaTempleHealth = maxTempleHealth;
            waveNumber = 1;
            evilCount = 0;
            isDefending = false;
            cumulativeDifficulty = 1.0f;
            
            Debug.Log("守护点防御系统初始化完成");
        }

        /// <summary>
        /// 开始守护
        /// </summary>
        public void StartDefense()
        {
            try
            {
                if (isDefending)
                {
                    Debug.LogWarning("正在防守中，请勿重复开始");
                    return;
                }
                
                isDefending = true;
                StartWave(waveNumber);
                
                Debug.Log("开始守护！敌人即将来袭...");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"开始守护时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 开始指定波次
        /// </summary>
        private void StartWave(int waveNum)
        {
            try
            {
                waveNumber = waveNum;
                
                // 计算当前波次的敌人数量
                int enemyToSpawn = CalculateEnemyCountForWave(waveNum);
                evilCount = enemyToSpawn;
                
                Debug.Log($"第 {waveNum} 波敌人来袭！共 {enemyToSpawn} 只敌人试图破坏守护点！");
                
                // 通知UI
                OnWaveStarted?.Invoke(waveNum);
                
                // 开始生成敌人
                StartCoroutine(SpawnEnemiesOverTime(enemyToSpawn, waveNum));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"开始第 {waveNum} 波时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 计算波次敌人数量
        /// </summary>
        private int CalculateEnemyCountForWave(int waveNum)
        {
            // 随着波次增加，敌人数量递增
            int baseCount = 5 + waveNum * 2;
            int maxCount = 50; // 最大敌人数量
            
            return Mathf.Min(baseCount, maxCount);
        }

        /// <summary>
        /// 持续生成敌人
        /// </summary>
        private IEnumerator SpawnEnemiesOverTime(int totalEnemies, int waveNum)
        {
            float spawnInterval = Mathf.Max(0.5f, 3.0f - waveNum * 0.1f); // 随着波次增加，生成间隔变短
            
            for (int i = 0; i < totalEnemies && isDefending; i++)
            {
                SpawnSingleEnemy(waveNum);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// 生成单个敌人
        /// </summary>
        private void SpawnSingleEnemy(int waveNum)
        {
            try
            {
                if (evilSpawnPoints.Length == 0) return;
                
                // 随机选择出生点
                Transform spawnPoint = evilSpawnPoints[Random.Range(0, evilSpawnPoints.Length)];
                
                // 随机选择攻击目标
                Transform targetPoint = nuwaTemplePoints.Length > 0 ? 
                    nuwaTemplePoints[Random.Range(0, nuwaTemplePoints.Length)] : null;
                
                // 创建敌人对象
                GameObject enemyObject = new GameObject($"Evil_Wave{waveNum}_{evilCount}");
                enemyObject.transform.position = spawnPoint.position;
                
                // 添加敌人组件
                var enemyComponent = enemyObject.AddComponent<EnemyController>();
                
                // 根据波次设置敌人属性
                float difficultyMultiplier = GetCurrentDifficultyMultiplier();
                float health = (50f + waveNum * 15f) * difficultyMultiplier;
                float speed = (1f + waveNum * 0.1f) * difficultyMultiplier;
                float damage = (20f + waveNum * 5f) * difficultyMultiplier;
                
                enemyComponent.Initialize(health, speed, damage, new Transform[] { targetPoint }, BattleSystem.EnemyType.Demon);
                
                evilCount--;
                
                // 通知UI
                OnEvilSpawned?.Invoke(waveNumber, evilCount);
                
                Debug.Log($"生成邪魔，距离守护点还有 {evilCount} 只");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成邪魔时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 邪魔攻击神殿
        /// </summary>
        public void EvilAttackedTemple(float damage)
        {
            try
            {
                nuwaTempleHealth -= (int)damage;
                
                // 限制生命值在合理范围内
                nuwaTempleHealth = Mathf.Clamp(nuwaTempleHealth, 0, maxTempleHealth);
                
                Debug.Log($"守护点受到攻击！当前生命值: {nuwaTempleHealth}/{maxTempleHealth}");
                
                // 通知UI
                OnTempleHealthChanged?.Invoke(nuwaTempleHealth);
                
                // 检查是否失败
                if (nuwaTempleHealth <= 0)
                {
                    FailDefense();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理邪魔攻击时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 邪魔被击败
        /// </summary>
        public void EvilDefeated()
        {
            Debug.Log("邪魔被击败！");
            
            // 增加玩家经验和奖励
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerData.AddExperience(10);
                GameManager.Instance.PlayerData.AddCoins(5);
            }
        }

        /// <summary>
        /// 完成当前波次
        /// </summary>
        public void CompleteWave()
        {
            try
            {
                Debug.Log($"第 {waveNumber} 波成功防守！守护点安然无恙！");
                
                // 增加玩家经验和奖励
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerData.AddExperience(waveNumber * 50);
                    GameManager.Instance.PlayerData.AddCoins(waveNumber * 10);
                }
                
                // 检查是否达到最终波次
                if (waveNumber >= 10)
                {
                    SuccessDefense();
                }
                else
                {
                    // 开始下一波
                    waveNumber++;
                    StartWave(waveNumber);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"完成波次时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 防守成功
        /// </summary>
        private void SuccessDefense()
        {
            isDefending = false;
            Debug.Log("恭喜！成功守护守护点！获得了守护者的祝福！");
            
            // 给予大量奖励
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerData.AddGems(50);
                GameManager.Instance.PlayerData.AddCoins(500);
                GameManager.Instance.PlayerData.AddSouls(10.0f);
            }
            
            OnDefenseSuccess?.Invoke();
        }

        /// <summary>
        /// 防守失败
        /// </summary>
        private void FailDefense()
        {
            isDefending = false;
            Debug.Log("防守失败！敌人破坏了守护点！");
            
            // 通知游戏管理器
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandleGameOver(false);
            }
            
            OnDefenseFailed?.Invoke();
        }

        /// <summary>
        /// 获取守护状态
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
        public GameObject SpawnSpecificEvil(BattleSystem.EnemyType type, int waveNum)
        {
            try
            {
                if (evilSpawnPoints.Length == 0) return null;
                
                // 随机选择出生点
                Transform spawnPoint = evilSpawnPoints[Random.Range(0, evilSpawnPoints.Length)];
                
                // 随机选择攻击目标
                Transform targetPoint = nuwaTemplePoints.Length > 0 ? 
                    nuwaTemplePoints[Random.Range(0, nuwaTemplePoints.Length)] : null;
                
                // 创建敌人对象
                GameObject enemyObject = new GameObject($"SpecificEvil_Type{type}_Wave{waveNum}");
                enemyObject.transform.position = spawnPoint.position;
                
                // 添加敌人组件
                var enemyComponent = enemyObject.AddComponent<EnemyController>();
                
                // 根据类型和波次设置敌人属性
                float difficultyMultiplier = GetCurrentDifficultyMultiplier();
                float health, speed, damage;
                
                switch (type)
                {
                    case BattleSystem.EnemyType.Boss:
                        health = (500f + waveNum * 100f) * difficultyMultiplier;
                        speed = (0.5f + waveNum * 0.05f) * difficultyMultiplier;
                        damage = (100f + waveNum * 20f) * difficultyMultiplier;
                        break;
                    case BattleSystem.EnemyType.EliteDemon:
                        health = (150f + waveNum * 30f) * difficultyMultiplier;
                        speed = (0.8f + waveNum * 0.08f) * difficultyMultiplier;
                        damage = (40f + waveNum * 8f) * difficultyMultiplier;
                        break;
                    case BattleSystem.EnemyType.Demon:
                        health = (100f + waveNum * 20f) * difficultyMultiplier;
                        speed = (1f + waveNum * 0.1f) * difficultyMultiplier;
                        damage = (30f + waveNum * 6f) * difficultyMultiplier;
                        break;
                    case BattleSystem.EnemyType.WildPet:
                    default:
                        health = (50f + waveNum * 15f) * difficultyMultiplier;
                        speed = (1.2f + waveNum * 0.12f) * difficultyMultiplier;
                        damage = (20f + waveNum * 5f) * difficultyMultiplier;
                        break;
                }
                
                enemyComponent.Initialize(health, speed, damage, new Transform[] { targetPoint }, type);
                
                evilCount--;
                
                return enemyObject;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成特定邪魔时发生错误: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 增加累积难度
        /// </summary>
        public void IncreaseDifficulty()
        {
            cumulativeDifficulty += difficultyIncreaseRate;
            Debug.Log($"难度提升，当前难度系数: {cumulativeDifficulty}");
        }

        /// <summary>
        /// 重置难度
        /// </summary>
        public void ResetDifficulty()
        {
            cumulativeDifficulty = 1.0f;
        }

        /// <summary>
        /// 获取当前波次
        /// </summary>
        public int GetCurrentWave()
        {
            return waveNumber;
        }

        /// <summary>
        /// 获取剩余邪魂数量
        /// </summary>
        public int GetRemainingEvils()
        {
            return evilCount;
        }

        /// <summary>
        /// 检查是否正在防守
        /// </summary>
        public bool IsCurrentlyDefending()
        {
            return isDefending;
        }
    }
}