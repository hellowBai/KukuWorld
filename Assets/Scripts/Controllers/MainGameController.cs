using System;
using System.Collections;
using UnityEngine;
using KukuWorld.Data;
using KukuWorld.Systems;
using KukuWorld.UI;

namespace KukuWorld.Controllers
{
    /// <summary>
    /// 主游戏控制器 - 协调各个系统和UI组件
    /// </summary>
    public class MainGameController : MonoBehaviour
    {
        [Header("系统引用")]
        public GameManager gameManager;
        public CaptureSystem captureSystem;
        public BattleSystem battleSystem;
        public BuildingManager buildingManager;
        public KukuCollectionSystem collectionSystem;
        public EvolutionSystem evolutionSystem;
        public FusionSystem fusionSystem;
        public EquipmentSystem equipmentSystem;
        
        [Header("UI引用")]
        public KukuUIManager uiManager;
        public CaptureUIController captureUIController;
        public ShopUIController shopUIController;
        public FusionUIController fusionUIController;
        
        [Header("游戏设置")]
        public bool autoInitialize = true;
        public bool enableLogging = true;
        
        // 是否已经初始化
        private bool isInitialized = false;
        
        // 事件订阅
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// 启动时初始化
        /// </summary>
        private void Start()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// 初始化主游戏控制器
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Log("主游戏控制器已初始化，跳过重复初始化");
                return;
            }
            
            Log("开始初始化主游戏控制器...");
            
            // 初始化游戏管理系统
            InitializeGameSystems();
            
            // 初始化UI系统
            InitializeUISystems();
            
            // 设置初始游戏状态
            SetupInitialGameState();
            
            isInitialized = true;
            
            Log("主游戏控制器初始化完成！");
        }
        
        /// <summary>
        /// 初始化游戏系统
        /// </summary>
        private void InitializeGameSystems()
        {
            Log("初始化游戏系统...");
            
            // 确保GameManager是单例
            if (GameManager.Instance == null)
            {
                // 如果场景中没有GameManager实例，创建一个新的
                GameObject managerObj = new GameObject("GameManager");
                gameManager = managerObj.AddComponent<GameManager>();
            }
            else
            {
                gameManager = GameManager.Instance;
            }
            
            // 初始化其他系统
            collectionSystem = new KukuCollectionSystem();
            buildingManager = new BuildingManager();
            battleSystem = new BattleSystem();
            
            Log("游戏系统初始化完成");
        }
        
        /// <summary>
        /// 初始化UI系统
        /// </summary>
        private void InitializeUISystems()
        {
            Log("初始化UI系统...");
            
            // 初始化UI管理器
            if (uiManager != null)
            {
                uiManager.Initialize();
            }
            else
            {
                Log("警告: UI管理器未设置", LogLevel.Warning);
            }
            
            // 初始化捕捉UI控制器
            if (captureUIController != null)
            {
                captureUIController.Initialize();
            }
            
            // 初始化商店UI控制器
            if (shopUIController != null)
            {
                shopUIController.Initialize();
            }
            
            // 初始化融合UI控制器
            if (fusionUIController != null)
            {
                fusionUIController.Initialize();
            }
            
            Log("UI系统初始化完成");
        }
        
        /// <summary>
        /// 设置初始游戏状态
        /// </summary>
        private void SetupInitialGameState()
        {
            Log("设置初始游戏状态...");
            
            // 设置玩家初始数据
            if (gameManager?.PlayerData != null)
            {
                gameManager.PlayerData.PlayerName = "KuKu Hunter";
                gameManager.PlayerData.Level = 1;
                gameManager.PlayerData.Coins = 1000;
                gameManager.PlayerData.Gems = 100;
                gameManager.PlayerData.Souls = 0;
                
                Log($"玩家数据初始化: {gameManager.PlayerData.PlayerName}");
            }
            
            // 设置初始游戏状态
            gameManager?.ChangeState(GameManager.GameState.CapturePhase);
            
            Log("初始游戏状态设置完成");
        }
        
        /// <summary>
        /// 订阅系统事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChange += OnGameStateChange;
                gameManager.OnGameOver += OnGameOver;
            }
            
            Log("已订阅系统事件");
        }
        
        /// <summary>
        /// 取消订阅系统事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChange -= OnGameStateChange;
                gameManager.OnGameOver -= OnGameOver;
            }
            
            Log("已取消订阅系统事件");
        }
        
        /// <summary>
        /// 游戏状态改变事件处理
        /// </summary>
        private void OnGameStateChange(GameManager.GameState newState)
        {
            Log($"游戏状态改变为: {newState}");
            
            // 根据新状态执行相应操作
            switch (newState)
            {
                case GameManager.GameState.CapturePhase:
                    OnEnterCapturePhase();
                    break;
                case GameManager.GameState.DefensePhase:
                    OnEnterDefensePhase();
                    break;
                case GameManager.GameState.GameOver:
                    OnEnterGameOverState();
                    break;
            }
        }
        
        /// <summary>
        /// 进入捕捉阶段
        /// </summary>
        private void OnEnterCapturePhase()
        {
            Log("进入捕捉阶段");
            
            // 激活捕捉相关系统
            buildingManager?.ActivateBuildingsForPhase(BattleSystem.BattleState.CapturePhase);
            
            // 生成初始野生KuKu
            StartCoroutine(SpawnInitialWildKukus());
        }
        
        /// <summary>
        /// 进入防守阶段
        /// </summary>
        private void OnEnterDefensePhase()
        {
            Log("进入防守阶段");
            
            // 初始化战斗系统
            battleSystem?.InitializeDefensePhase(gameManager.PlayerData);
            
            // 激活防守相关系统
            buildingManager?.ActivateBuildingsForPhase(BattleSystem.BattleState.Fighting);
        }
        
        /// <summary>
        /// 进入游戏结束状态
        /// </summary>
        private void OnEnterGameOverState()
        {
            Log("进入游戏结束状态");
        }
        
        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        private void OnGameOver(bool isVictory)
        {
            Log(isVictory ? "游戏胜利！" : "游戏失败！");
        }
        
        /// <summary>
        /// 生成初始野生KuKu
        /// </summary>
        private IEnumerator SpawnInitialWildKukus()
        {
            yield return new WaitForSeconds(2f); // 等待2秒后开始生成
            
            // 生成几个初始的野生KuKu
            for (int i = 0; i < 5; i++)
            {
                Vector3 position = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0, UnityEngine.Random.Range(-10f, 10f));
                CaptureSystem.GenerateWildKuku(position, MythicalKukuData.MythicalRarity.Celestial);
                
                yield return new WaitForSeconds(1f); // 每秒生成一个
            }
            
            Log("初始野生KuKu生成完成");
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            Log("开始新游戏");
            
            // 重置所有系统
            ResetAllSystems();
            
            // 开始新游戏
            gameManager?.StartNewGame();
        }
        
        /// <summary>
        /// 重置所有系统
        /// </summary>
        private void ResetAllSystems()
        {
            Log("重置所有系统...");
            
            // 重置游戏管理器
            gameManager?.Reset();
            
            // 重置建筑管理器
            buildingManager?.Reset();
            
            // 重置战斗系统
            battleSystem?.Reset();
            
            // 重置捕捉系统
            CaptureSystem.ClearAllWildKukus();
            
            Log("所有系统重置完成");
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void ExitGame()
        {
            Log("退出游戏");
            
            gameManager?.QuitGame();
        }
        
        /// <summary>
        /// 强制结束捕捉阶段
        /// </summary>
        public void ForceEndCapturePhase()
        {
            Log("强制结束捕捉阶段");
            
            gameManager?.ForceEndCapturePhase();
        }
        
        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        private void Update()
        {
            if (!isInitialized) return;
            
            // 更新游戏管理系统
            gameManager?.UpdateGame(Time.deltaTime);
            
            // 更新建筑系统
            buildingManager?.UpdateBuildings(Time.deltaTime);
            
            // 更新捕捉系统
            CaptureSystem.UpdateWildKukus(Time.deltaTime);
            
            // 更新战斗系统
            battleSystem?.UpdateBattleLogic(Time.deltaTime);
        }
        
        /// <summary>
        /// 获取游戏统计数据
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> GetGameStats()
        {
            if (gameManager != null)
            {
                return gameManager.GetGameStats();
            }
            
            return new System.Collections.Generic.Dictionary<string, object>();
        }
        
        /// <summary>
        /// 保存游戏进度
        /// </summary>
        public void SaveGame()
        {
            Log("保存游戏进度");
            
            gameManager?.SaveGame();
        }
        
        /// <summary>
        /// 加载游戏进度
        /// </summary>
        public void LoadGame()
        {
            Log("加载游戏进度");
            
            gameManager?.LoadGame();
        }
        
        /// <summary>
        /// 日志级别枚举
        /// </summary>
        private enum LogLevel
        {
            Info,
            Warning,
            Error
        }
        
        /// <summary>
        /// 写入日志
        /// </summary>
        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!enableLogging) return;
            
            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log($"[MainGameController] {message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"[MainGameController] {message}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"[MainGameController] {message}");
                    break;
            }
        }
        
        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public bool IsInitialized()
        {
            return isInitialized;
        }
        
        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public GameManager.GameState GetCurrentGameState()
        {
            return gameManager?.CurrentState ?? GameManager.GameState.MainMenu;
        }
        
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public PlayerData GetPlayerData()
        {
            return gameManager?.PlayerData;
        }
    }
}