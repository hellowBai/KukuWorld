using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;
using KukuWorld.Systems;
using KukuWorld.UI;
using KukuWorld.Utils;

namespace KukuWorld
{
    /// <summary>
    /// KuKu世界游戏入口 - 游戏启动和初始化
    /// </summary>
    public class KukuWorldGame : MonoBehaviour
    {
        // 游戏管理器引用
        private GameManager gameManager;
        private KukuCollectionSystem collectionSystem;
        private BuildingManager buildingManager;
        private BattleSystem battleSystem;
        
        // UI系统
        private KukuUIManager uiManager;
        private CaptureUIController captureUIController;
        private ShopUIController shopUIController;
        private FusionUIController fusionUIController;
        
        // 游戏配置
        [Header("游戏配置")]
        public bool autoInitialize = true;
        public float capturePhaseDuration = 300f; // 5分钟捕捉阶段
        public int initialCoins = 1000;
        public int initialGems = 100;
        
        // 是否已初始化
        private bool isInitialized = false;
        
        /// <summary>
        /// Unity Awake - 最早的初始化
        /// </summary>
        void Awake()
        {
            // 确保游戏管理器单例存在
            if (GameManager.Instance == null)
            {
                GameObject managerObj = new GameObject("GameManager");
                gameManager = managerObj.AddComponent<GameManager>();
            }
            else
            {
                gameManager = GameManager.Instance;
            }
        }
        
        /// <summary>
        /// Unity Start - 游戏开始
        /// </summary>
        void Start()
        {
            if (autoInitialize)
            {
                InitializeGame();
            }
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitializeGame()
        {
            if (isInitialized)
            {
                Debug.LogWarning("游戏已经初始化过了");
                return;
            }
            
            Debug.Log("开始初始化KuKu世界游戏...");
            
            // 初始化核心系统
            InitializeCoreSystems();
            
            // 初始化UI系统
            InitializeUISystems();
            
            // 初始化游戏数据
            InitializeGameData();
            
            // 设置初始状态
            SetInitialState();
            
            isInitialized = true;
            
            Debug.Log("KuKu世界游戏初始化完成！");
        }
        
        /// <summary>
        /// 初始化核心系统
        /// </summary>
        private void InitializeCoreSystems()
        {
            // 初始化系统管理器
            collectionSystem = new KukuCollectionSystem();
            buildingManager = new BuildingManager();
            battleSystem = new BattleSystem();
            
            // 初始化战斗系统
            battleSystem.Initialize();
            
            // 设置捕捉阶段时长
            gameManager?.SetCapturePhaseDuration(capturePhaseDuration);
            
            Debug.Log("核心系统初始化完成");
        }
        
        /// <summary>
        /// 初始化UI系统
        /// </summary>
        private void InitializeUISystems()
        {
            // 初始化UI管理器
            if (uiManager == null)
            {
                GameObject uiManagerObj = new GameObject("KukuUIManager");
                uiManager = uiManagerObj.AddComponent<KukuUIManager>();
                uiManager.Initialize();
            }
            
            // 初始化各UI控制器
            if (captureUIController == null)
            {
                GameObject captureUIObj = new GameObject("CaptureUIController");
                captureUIController = captureUIObj.AddComponent<CaptureUIController>();
                captureUIController.Initialize();
            }
            
            if (shopUIController == null)
            {
                GameObject shopUIObj = new GameObject("ShopUIController");
                shopUIController = shopUIObj.AddComponent<ShopUIController>();
                shopUIController.Initialize();
            }
            
            if (fusionUIController == null)
            {
                GameObject fusionUIObj = new GameObject("FusionUIController");
                fusionUIController = fusionUIObj.AddComponent<FusionUIController>();
                fusionUIController.Initialize();
            }
            
            Debug.Log("UI系统初始化完成");
        }
        
        /// <summary>
        /// 初始化游戏数据
        /// </summary>
        private void InitializeGameData()
        {
            // 初始化玩家数据
            if (gameManager?.PlayerData != null)
            {
                var playerData = gameManager.PlayerData;
                
                playerData.PlayerName = "KuKu Hunter";
                playerData.Coins = initialCoins;
                playerData.Gems = initialGems;
                playerData.Souls = 0;
                
                // 初始化英雄
                playerData.Hero = new HeroData
                {
                    HeroName = "KuKu守护者",
                    Class = HeroData.HeroClass.Ranger
                };
                
                // 添加一些基础技能
                playerData.AvailableSkills.Add(new HeroSkill
                {
                    Name = "快速射击",
                    Description = "对单个敌人造成额外伤害",
                    Type = HeroSkill.HeroSkillType.Active,
                    EffectValue = 1.5f,
                    Cooldown = 5f,
                    IsUnlocked = true
                });
                
                playerData.AvailableSkills.Add(new HeroSkill
                {
                    Name = "团队增益",
                    Description = "提升所有单位的攻击力",
                    Type = HeroSkill.HeroSkillType.Active,
                    EffectValue = 1.2f,
                    Cooldown = 30f,
                    IsUnlocked = true
                });
                
                Debug.Log($"玩家数据初始化完成: {playerData.PlayerName}");
            }
            
            Debug.Log("游戏数据初始化完成");
        }
        
        /// <summary>
        /// 设置初始状态
        /// </summary>
        private void SetInitialState()
        {
            // 设置游戏初始状态为捕捉阶段
            gameManager?.ChangeState(GameManager.GameState.CapturePhase);
            
            Debug.Log("游戏状态设置为捕捉阶段");
        }
        
        /// <summary>
        /// Unity Update - 游戏主循环
        /// </summary>
        void Update()
        {
            if (!isInitialized) return;
            
            // 更新游戏管理器
            gameManager?.UpdateGame(Time.deltaTime);
            
            // 更新战斗系统
            battleSystem?.UpdateBattleLogic(Time.deltaTime);
            
            // 更新捕捉系统
            CaptureSystem.UpdateWildKukus(Time.deltaTime);
        }
        
        /// <summary>
        /// Unity OnDestroy - 清理资源
        /// </summary>
        void OnDestroy()
        {
            Cleanup();
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            // 清理系统
            collectionSystem = null;
            buildingManager = null;
            battleSystem = null;
            
            // 清理UI
            uiManager = null;
            captureUIController = null;
            shopUIController = null;
            fusionUIController = null;
            
            Debug.Log("KuKu世界游戏资源清理完成");
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            gameManager?.StartNewGame();
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void ExitGame()
        {
            gameManager?.QuitGame();
        }
        
        /// <summary>
        /// 获取游戏统计信息
        /// </summary>
        public Dictionary<string, object> GetGameStats()
        {
            return gameManager?.GetGameStats() ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame()
        {
            gameManager?.SaveGame();
        }
        
        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame()
        {
            gameManager?.LoadGame();
        }
        
        /// <summary>
        /// 强制结束捕捉阶段
        /// </summary>
        public void ForceEndCapturePhase()
        {
            gameManager?.ForceEndCapturePhase();
        }
        
        /// <summary>
        /// 检查游戏是否已初始化
        /// </summary>
        public bool IsGameInitialized()
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
        
        /// <summary>
        /// 获取KuKu收集系统
        /// </summary>
        public KukuCollectionSystem GetCollectionSystem()
        {
            return collectionSystem;
        }
        
        /// <summary>
        /// 获取建筑管理器
        /// </summary>
        public BuildingManager GetBuildingManager()
        {
            return buildingManager;
        }
        
        /// <summary>
        /// 获取战斗系统
        /// </summary>
        public BattleSystem GetBattleSystem()
        {
            return battleSystem;
        }
    }
}