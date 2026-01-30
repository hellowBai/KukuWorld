using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 游戏主管理器 - 管理全局游戏状态，协调各系统交互
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // 单例实例
        public static GameManager Instance { get; private set; }
        
        // 游戏状态
        public enum GameState { 
            MainMenu,      // 主菜单
            CapturePhase,  // 捕捉阶段
            DefensePhase,  // 防守阶段
            Shop,          // 商店
            Settings,      // 设置
            GameOver       // 游戏结束
        }
        
        // 当前状态
        public GameState CurrentState { get; private set; }
        
        // 数据引用
        public PlayerData PlayerData { get; private set; }
        public BattleSystem BattleSystem { get; private set; }
        public CaptureSystem CaptureSystem { get; private set; }
        public BuildingManager BuildingManager { get; private set; }
        
        // 配置
        public float CapturePhaseDuration = 300f; // 捕捉阶段持续时间（5分钟）
        
        // 事件系统
        public System.Action<GameState> OnStateChanged;
        public System.Action OnCapturePhaseEnded;
        public System.Action OnDefensePhaseStarted;
        public System.Action<bool> OnGameCompleted; // 参数表示是否胜利
        
        // 内部变量
        private float capturePhaseTimer = 0f;
        private bool isInitialized = false;
        
        // 系统组件
        private KukuCollectionSystem collectionSystem;
        private FusionSystem fusionSystem;
        private EquipmentSystem equipmentSystem;
        private NuwaDefenseSystem nuwaDefenseSystem;

        #region Unity 生命周期
        void Awake()
        {
            // 确保单例
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            InitializeGame();
        }

        void Update()
        {
            if (isInitialized)
            {
                UpdateGame(Time.deltaTime);
                CheckPhaseTransition();
            }
        }
        #endregion

        #region 初始化方法
        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            Debug.Log("初始化游戏管理器...");
            
            // 初始化玩家数据
            PlayerData = new PlayerData();
            PlayerData.PlayerName = "KukuHunter";
            
            // 初始化各系统
            InitializeSystems();
            
            // 设置初始状态
            ChangeState(GameState.CapturePhase);
            capturePhaseTimer = CapturePhaseDuration;
            
            isInitialized = true;
            
            Debug.Log("游戏初始化完成！");
        }

        /// <summary>
        /// 初始化各子系统
        /// </summary>
        private void InitializeSystems()
        {
            // 初始化捕捉系统
            CaptureSystem = new CaptureSystem();
            
            // 初始化战斗系统
            BattleSystem = FindObjectOfType<BattleSystem>();
            if (BattleSystem == null)
            {
                GameObject battleGO = new GameObject("BattleSystem");
                BattleSystem = battleGO.AddComponent<BattleSystem>();
            }
            
            // 初始化建筑管理器
            BuildingManager = FindObjectOfType<BuildingManager>();
            if (BuildingManager == null)
            {
                GameObject buildingGO = new GameObject("BuildingManager");
                BuildingManager = buildingGO.AddComponent<BuildingManager>();
            }
            
            // 初始化其他系统
            collectionSystem = new KukuCollectionSystem();
            fusionSystem = new FusionSystem();
            equipmentSystem = new EquipmentSystem();
            nuwaDefenseSystem = FindObjectOfType<NuwaDefenseSystem>();
            if (nuwaDefenseSystem == null)
            {
                GameObject defenseGO = new GameObject("NuwaDefenseSystem");
                nuwaDefenseSystem = defenseGO.AddComponent<NuwaDefenseSystem>();
            }
            
            Debug.Log("所有子系统初始化完成！");
        }
        #endregion

        #region 状态管理
        /// <summary>
        /// 切换游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            GameState oldState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"游戏状态从 {oldState} 切换到 {newState}");
            
            // 根据状态执行特定逻辑
            switch (newState)
            {
                case GameState.CapturePhase:
                    OnCapturePhaseStarted();
                    break;
                case GameState.DefensePhase:
                    OnDefensePhaseStarted?.Invoke();
                    break;
                case GameState.MainMenu:
                    OnMainMenuEntered();
                    break;
                case GameState.Shop:
                    OnShopEntered();
                    break;
                case GameState.Settings:
                    OnSettingsEntered();
                    break;
                case GameState.GameOver:
                    OnGameOver();
                    break;
            }
            
            // 通知监听者状态变化
            OnStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("开始新游戏...");
            
            // 重置玩家数据
            PlayerData = new PlayerData();
            
            // 重置各系统
            ResetSystems();
            
            // 切换到捕捉阶段
            ChangeState(GameState.CapturePhase);
            capturePhaseTimer = CapturePhaseDuration;
        }

        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        public void UpdateGame(float deltaTime)
        {
            switch (CurrentState)
            {
                case GameState.CapturePhase:
                    UpdateCapturePhase(deltaTime);
                    break;
                case GameState.DefensePhase:
                    UpdateDefensePhase(deltaTime);
                    break;
                case GameState.Shop:
                    UpdateShopPhase(deltaTime);
                    break;
            }
        }

        /// <summary>
        /// 检查是否需要从捕捉阶段切换到防守阶段
        /// </summary>
        public void CheckPhaseTransition()
        {
            if (CurrentState == GameState.CapturePhase && capturePhaseTimer <= 0)
            {
                TransitionToDefensePhase();
            }
        }

        /// <summary>
        /// 切换到防守阶段
        /// </summary>
        public void TransitionToDefensePhase()
        {
            Debug.Log("捕捉阶段结束，切换到防守阶段！");
            
            // 通知捕捉阶段结束
            OnCapturePhaseEnded?.Invoke();
            
            // 如果玩家没有任何KuKu，给予基本单位
            if (PlayerData.CollectedKukus.Count == 0)
            {
                // 创建一只基础KuKu
                var basicKuku = CreateBasicKuku();
                PlayerData.AddKuku(basicKuku);
                Debug.Log("由于没有捕捉到任何KuKu，系统赠送了一只基础KuKu");
            }
            
            // 切换到防守阶段
            ChangeState(GameState.DefensePhase);
            
            // 启动战斗系统
            if (BattleSystem != null)
            {
                BattleSystem.StartBattle();
            }
        }

        /// <summary>
        /// 处理游戏结束
        /// </summary>
        public void HandleGameOver(bool victory)
        {
            Debug.Log(victory ? "玩家获胜！" : "玩家失败！");
            
            // 记录游戏结果
            if (victory)
            {
                PlayerData.AddAchievement("Victory");
                PlayerData.TotalGemsEarned += 50; // 胜利奖励
            }
            else
            {
                PlayerData.AddAchievement("Defeat");
            }
            
            // 通知游戏完成
            OnGameCompleted?.Invoke(victory);
            
            // 切换到主菜单
            ChangeState(GameState.MainMenu);
        }
        #endregion

        #region 阶段更新方法
        /// <summary>
        /// 更新捕捉阶段
        /// </summary>
        private void UpdateCapturePhase(float deltaTime)
        {
            capturePhaseTimer -= deltaTime;
            
            // 更新捕捉系统
            if (CaptureSystem != null)
            {
                // 这里可以调用捕捉系统的更新方法
            }
            
            // 更新建筑（捕捉阶段特定建筑）
            if (BuildingManager != null)
            {
                BuildingManager.UpdateBuildingsForCapturePhase(deltaTime);
            }
        }

        /// <summary>
        /// 更新防守阶段
        /// </summary>
        private void UpdateDefensePhase(float deltaTime)
        {
            // 更新战斗系统
            if (BattleSystem != null)
            {
                BattleSystem.UpdateBattle(deltaTime);
            }
            
            // 更新建筑（防守阶段建筑）
            if (BuildingManager != null)
            {
                BuildingManager.UpdateBuildingsForDefensePhase(deltaTime);
            }
        }

        /// <summary>
        /// 更新商店阶段
        /// </summary>
        private void UpdateShopPhase(float deltaTime)
        {
            // 商店阶段的更新逻辑
        }
        #endregion

        #region 阶段开始方法
        /// <summary>
        /// 捕捉阶段开始
        /// </summary>
        private void OnCapturePhaseStarted()
        {
            capturePhaseTimer = CapturePhaseDuration;
            PlayerData.IsInCapturePhase = true;
            
            Debug.Log($"捕捉阶段开始！时间限制：{CapturePhaseDuration}秒");
        }

        /// <summary>
        /// 主菜单进入
        /// </summary>
        private void OnMainMenuEntered()
        {
            Debug.Log("进入主菜单");
        }

        /// <summary>
        /// 商店进入
        /// </summary>
        private void OnShopEntered()
        {
            Debug.Log("进入商店");
        }

        /// <summary>
        /// 设置进入
        /// </summary>
        private void OnSettingsEntered()
        {
            Debug.Log("进入设置");
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        private void OnGameOver()
        {
            Debug.Log("游戏结束");
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 重置各系统
        /// </summary>
        private void ResetSystems()
        {
            if (CaptureSystem != null)
            {
                // 重置捕捉系统
            }
            
            if (BattleSystem != null)
            {
                BattleSystem.ResetBattle();
            }
            
            if (BuildingManager != null)
            {
                BuildingManager.ResetBuildings();
            }
        }

        /// <summary>
        /// 创建基础KuKu
        /// </summary>
        private MythicalKukuData CreateBasicKuku()
        {
            var kuku = new MythicalKukuData
            {
                Id = System.DateTime.Now.Millisecond, // 使用时间戳作为ID
                Name = "基础KuKu",
                Description = "一只基础的KuKu，系统赠送",
                Element = "Nature",
                SkillType = "Heal",
                SkillDescription = "恢复友方单位的生命值",
                AttackPower = 15f,
                DefensePower = 10f,
                Speed = 1.5f,
                Health = 60f,
                Rarity = MythicalKukuData.MythicalRarity.Celestial,
                Level = 1,
                EvolutionLevel = 1,
                CanFuseWithRobots = false,
                SpriteName = "BasicKukuSprite"
            };
            
            return kuku;
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("退出游戏");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 获取捕捉阶段剩余时间
        /// </summary>
        public float GetCapturePhaseTimeRemaining()
        {
            return capturePhaseTimer;
        }

        /// <summary>
        /// 获取捕捉阶段进度百分比
        /// </summary>
        public float GetCapturePhaseProgress()
        {
            return capturePhaseTimer / CapturePhaseDuration;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 检查游戏是否暂停
        /// </summary>
        public bool IsGamePaused()
        {
            return Time.timeScale == 0f;
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame()
        {
            // 这里实现游戏保存逻辑
            Debug.Log("游戏已保存");
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame()
        {
            // 这里实现游戏加载逻辑
            Debug.Log("游戏已加载");
        }
        #endregion
    }
}