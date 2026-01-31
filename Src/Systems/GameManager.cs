using System;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 游戏主管理器 - 管理全局游戏状态、协调各系统交互
    /// </summary>
    public class GameManager
    {
        // 单例实例
        public static GameManager Instance { get; private set; }

        // 游戏状态
        public enum GameState 
        { 
            MainMenu,      // 主菜单
            CapturePhase,  // 捕捉阶段
            DefensePhase,  // 防守阶段
            Shop,          // 商店
            Settings,      // 设置
            GameOver,      // 游戏结束
            Paused         // 暂停
        }

        // 当前状态
        public GameState CurrentState { get; private set; }
        
        // 玩家数据
        public PlayerData PlayerData { get; private set; }
        
        // 系统引用
        public BattleSystem BattleSystem { get; private set; }
        public CaptureSystem CaptureSystem { get; private set; }
        public KukuCollectionSystem CollectionSystem { get; private set; }
        public EvolutionSystem EvolutionSystem { get; private set; }
        public FusionSystem FusionSystem { get; private set; }
        public EquipmentSystem EquipmentSystem { get; private set; }
        public BuildingManager BuildingManager { get; private set; }

        // 时间管理
        private float capturePhaseDuration = 300f; // 捕捉阶段持续时间（5分钟）
        private float timeInCapturePhase = 0f;
        private bool capturePhaseActive = true;

        // 事件
        public event Action<GameState> OnGameStateChange;
        public event Action<float> OnCapturePhaseTimerUpdate;
        public event Action OnCapturePhaseEnded;
        public event Action OnDefensePhaseStarted;
        public event Action<bool> OnGameOver; // true表示胜利，false表示失败

        // 构造函数
        public GameManager()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGame();
            }
            else
            {
                throw new InvalidOperationException("GameManager已经存在！");
            }
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            // 初始化玩家数据
            PlayerData = new PlayerData();
            
            // 初始化各系统
            BattleSystem = new BattleSystem();
            CaptureSystem = new CaptureSystem();
            CollectionSystem = new KukuCollectionSystem();
            BuildingManager = new BuildingManager();
            
            // 设置初始状态
            CurrentState = GameState.MainMenu;
            
            Debug.Log("游戏管理系统初始化完成");
        }

        /// <summary>
        /// 切换游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            GameState oldState = CurrentState;
            CurrentState = newState;

            // 根据状态变化执行特定逻辑
            switch (newState)
            {
                case GameState.CapturePhase:
                    StartCapturePhase();
                    break;
                case GameState.DefensePhase:
                    StartDefensePhase();
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
                case GameState.Paused:
                    PauseGame();
                    break;
                case GameState.MainMenu:
                    ResumeGame();
                    break;
            }

            // 触发状态变更事件
            OnGameStateChange?.Invoke(newState);
            
            Debug.Log($"游戏状态从 {oldState} 切换到 {newState}");
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            // 重置玩家数据
            PlayerData = new PlayerData();
            
            // 重置各系统
            BattleSystem?.Reset();
            CaptureSystem?.ClearAllWildKukus();
            BuildingManager?.Reset();
            
            // 切换到捕捉阶段
            ChangeState(GameState.CapturePhase);
            
            Debug.Log("新游戏开始");
        }

        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        public void UpdateGame(float deltaTime)
        {
            if (CurrentState == GameState.CapturePhase)
            {
                UpdateCapturePhase(deltaTime);
            }
            else if (CurrentState == GameState.DefensePhase)
            {
                BattleSystem?.UpdateBattleLogic(deltaTime);
            }

            // 更新各系统
            CaptureSystem?.UpdateWildKukus(deltaTime);
            BattleSystem?.UpdateBattleLogic(deltaTime);
        }

        /// <summary>
        /// 更新捕捉阶段
        /// </summary>
        private void UpdateCapturePhase(float deltaTime)
        {
            if (capturePhaseActive)
            {
                timeInCapturePhase += deltaTime;
                
                // 检查是否达到捕捉阶段时限
                if (timeInCapturePhase >= capturePhaseDuration)
                {
                    EndCapturePhase();
                }
                else
                {
                    // 更新捕捉阶段计时器
                    float remainingTime = capturePhaseDuration - timeInCapturePhase;
                    OnCapturePhaseTimerUpdate?.Invoke(remainingTime);
                }
            }
        }

        /// <summary>
        /// 开始捕捉阶段
        /// </summary>
        private void StartCapturePhase()
        {
            capturePhaseActive = true;
            timeInCapturePhase = 0f;
            PlayerData.IsInCapturePhase = true;
            
            // 生成一些野生KuKu
            SpawnInitialWildKukus();
            
            Debug.Log("捕捉阶段开始 - 快去捕捉KuKu吧！");
        }

        /// <summary>
        /// 结束捕捉阶段
        /// </summary>
        private void EndCapturePhase()
        {
            capturePhaseActive = false;
            PlayerData.IsInCapturePhase = false;
            
            // 触发捕捉阶段结束事件
            OnCapturePhaseEnded?.Invoke();
            
            Debug.Log($"捕捉阶段结束！共捕获了 {PlayerData.CollectedKukus.Count} 只KuKu");
        }

        /// <summary>
        /// 开始防守阶段
        /// </summary>
        private void StartDefensePhase()
        {
            PlayerData.IsInCapturePhase = false;
            
            // 初始化战斗系统
            BattleSystem?.InitializeDefensePhase(PlayerData);
            
            // 触发防守阶段开始事件
            OnDefensePhaseStarted?.Invoke();
            
            Debug.Log("防守阶段开始 - 保护神殿免受敌人侵袭！");
        }

        /// <summary>
        /// 生成初始野生KuKu
        /// </summary>
        private void SpawnInitialWildKukus()
        {
            // 在随机位置生成几个野生KuKu
            for (int i = 0; i < 5; i++)
            {
                Vector3 position = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0, UnityEngine.Random.Range(-10f, 10f));
                
                // 根据玩家进度生成适当稀有度的KuKu
                var rarity = MythicalKukuData.MythicalRarity.Celestial;
                if (PlayerData.Level > 5) rarity = MythicalKukuData.MythicalRarity.Immortal;
                if (PlayerData.Level > 10) rarity = MythicalKukuData.MythicalRarity.DivineBeast;
                
                CaptureSystem.GenerateWildKuku(position, rarity);
            }
        }

        /// <summary>
        /// 检查是否需要从捕捉阶段切换到防守阶段
        /// </summary>
        public void CheckPhaseTransition()
        {
            if (CurrentState == GameState.CapturePhase && !capturePhaseActive)
            {
                // 捕捉阶段结束，切换到防守阶段
                if (PlayerData.CanStartDefensePhase())
                {
                    ChangeState(GameState.DefensePhase);
                }
                else
                {
                    // 玩家没有KuKu，无法进入防守阶段，游戏结束
                    Debug.Log("没有捕捉到任何KuKu，无法进入防守阶段！");
                    OnGameOver?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// 处理游戏结束
        /// </summary>
        private void HandleGameOver()
        {
            Debug.Log("游戏结束");
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        private void PauseGame()
        {
            Time.timeScale = 0f;
            Debug.Log("游戏已暂停");
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        private void ResumeGame()
        {
            Time.timeScale = 1f;
            Debug.Log("游戏已恢复");
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("退出游戏");
            
            // 在实际游戏中，这里会调用 Application.Quit()
            // Application.Quit();
        }

        /// <summary>
        /// 获取当前捕捉阶段剩余时间
        /// </summary>
        public float GetCapturePhaseRemainingTime()
        {
            return Mathf.Max(0, capturePhaseDuration - timeInCapturePhase);
        }

        /// <summary>
        /// 获取捕捉阶段总时间
        /// </summary>
        public float GetCapturePhaseTotalTime()
        {
            return capturePhaseDuration;
        }

        /// <summary>
        /// 获取捕捉阶段进度百分比
        /// </summary>
        public float GetCapturePhaseProgress()
        {
            return Mathf.Clamp01(timeInCapturePhase / capturePhaseDuration);
        }

        /// <summary>
        /// 强制结束捕捉阶段
        /// </summary>
        public void ForceEndCapturePhase()
        {
            timeInCapturePhase = capturePhaseDuration;
            UpdateCapturePhase(0); // 立即触发结束逻辑
        }

        /// <summary>
        /// 获取游戏统计数据
        /// </summary>
        public Dictionary<string, object> GetGameStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["CurrentState"] = CurrentState.ToString(),
                ["PlayerLevel"] = PlayerData.Level,
                ["PlayerName"] = PlayerData.PlayerName,
                ["Coins"] = PlayerData.Coins,
                ["Gems"] = PlayerData.Gems,
                ["Souls"] = PlayerData.Souls,
                ["KukuCount"] = PlayerData.CollectedKukus.Count,
                ["ActiveKukuTeamSize"] = PlayerData.ActiveKukuTeam.Count,
                ["BuildingsCount"] = PlayerData.BuiltBuildings.Count,
                ["DeployedUnitsCount"] = PlayerData.DeployedUnits.Count,
                ["CapturePhaseRemaining"] = GetCapturePhaseRemainingTime(),
                ["CapturePhaseProgress"] = GetCapturePhaseProgress(),
                ["EnemiesDefeated"] = PlayerData.EnemiesDefeatedInDefense,
                ["CurrentWave"] = PlayerData.CurrentWaveInDefense,
                ["HighestWave"] = PlayerData.HighestWaveReached,
                ["TotalPetsCaught"] = PlayerData.TotalPetsCaught
            };

            return stats;
        }

        /// <summary>
        /// 保存游戏进度
        /// </summary>
        public void SaveGame()
        {
            // 实际游戏中会实现具体的保存逻辑
            Debug.Log("游戏进度已保存");
        }

        /// <summary>
        /// 加载游戏进度
        /// </summary>
        public void LoadGame()
        {
            // 实际游戏中会实现具体的加载逻辑
            Debug.Log("游戏进度已加载");
        }

        /// <summary>
        /// 重置游戏管理器
        /// </summary>
        public void Reset()
        {
            // 重置所有状态
            CurrentState = GameState.MainMenu;
            timeInCapturePhase = 0f;
            capturePhaseActive = true;
            
            // 重置玩家数据
            PlayerData = new PlayerData();
            
            Debug.Log("游戏管理器已重置");
        }

        /// <summary>
        /// 获取游戏运行时间
        /// </summary>
        public float GetGameRunTime()
        {
            // 这里应该返回游戏实际运行的时间
            // 在实际实现中可能需要记录开始时间
            return Time.time; // 使用Unity的时间作为示例
        }

        /// <summary>
        /// 设置捕捉阶段持续时间
        /// </summary>
        public void SetCapturePhaseDuration(float durationInSeconds)
        {
            capturePhaseDuration = durationInSeconds;
            Debug.Log($"捕捉阶段持续时间已设置为 {durationInSeconds} 秒");
        }

        /// <summary>
        /// 检查是否在捕捉阶段
        /// </summary>
        public bool IsInCapturePhase()
        {
            return CurrentState == GameState.CapturePhase;
        }

        /// <summary>
        /// 检查是否在防守阶段
        /// </summary>
        public bool IsInDefensePhase()
        {
            return CurrentState == GameState.DefensePhase;
        }

        /// <summary>
        /// 获取当前游戏状态的字符串描述
        /// </summary>
        public string GetCurrentStateDescription()
        {
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    return "主菜单";
                case GameState.CapturePhase:
                    return $"捕捉阶段 (剩余时间: {(int)GetCapturePhaseRemainingTime()}秒)";
                case GameState.DefensePhase:
                    return "防守阶段";
                case GameState.Shop:
                    return "商店";
                case GameState.Settings:
                    return "设置";
                case GameState.GameOver:
                    return "游戏结束";
                case GameState.Paused:
                    return "已暂停";
                default:
                    return "未知状态";
            }
        }
    }
}