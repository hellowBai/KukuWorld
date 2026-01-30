using UnityEngine;
using KukuWorld.Data;

namespace KukuWorld.Systems
{
    /// <summary>
    /// 游戏主管理器 - 管理全局游戏状态
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; } // 单例实例
        
        // 游戏状态
        public enum GameState 
        { 
            MainMenu,      // 主菜单
            CapturePhase,  // 捕捉阶段
            DefensePhase,  // 防守阶段
            Shop,          // 商店
            Settings,      // 设置
            GameOver       // 游戏结束
        }
        
        public GameState CurrentState { get; private set; }      // 当前状态
        public PlayerData PlayerData { get; private set; }       // 玩家数据
        public BattleSystem BattleSystem { get; private set; }   // 战斗系统引用
        public CaptureSystem CaptureSystem { get; private set; } // 捕捉系统引用

        // 事件系统
        public System.Action<GameState> OnStateChanged;          // 状态变化事件
        public System.Action<bool> OnGameOver;                   // 游戏结束事件

        void Awake()
        {
            // 确保只有一个GameManager实例
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            InitializeGame();
        }

        void Update()
        {
            if (CurrentState == GameState.CapturePhase || CurrentState == GameState.DefensePhase)
            {
                UpdateGame(Time.deltaTime);
            }
            
            // 检查是否需要从捕捉阶段切换到防守阶段
            CheckPhaseTransition();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            PlayerData = new PlayerData();
            CurrentState = GameState.MainMenu;
            
            // 初始化子系统
            InitializeSubsystems();
        }

        /// <summary>
        /// 初始化子系统
        /// </summary>
        private void InitializeSubsystems()
        {
            // 查找场景中的系统组件
            BattleSystem = FindObjectOfType<BattleSystem>();
            CaptureSystem = FindObjectOfType<CaptureSystem>();
            
            if (BattleSystem == null)
            {
                Debug.LogWarning("未找到BattleSystem组件，将创建默认实例");
                // 创建BattleSystem的GameObject
                GameObject battleSystemGO = new GameObject("BattleSystem");
                BattleSystem = battleSystemGO.AddComponent<BattleSystem>();
            }
            
            if (CaptureSystem == null)
            {
                Debug.LogWarning("未找到CaptureSystem组件，将创建默认实例");
                // 创建CaptureSystem的GameObject
                GameObject captureSystemGO = new GameObject("CaptureSystem");
                CaptureSystem = captureSystemGO.AddComponent<CaptureSystem>();
            }
        }

        /// <summary>
        /// 切换游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            GameState oldState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"游戏状态从 {oldState} 切换到 {newState}");
            
            // 通知状态变化
            OnStateChanged?.Invoke(newState);
            
            // 根据新状态执行相应操作
            switch (newState)
            {
                case GameState.CapturePhase:
                    OnEnterCapturePhase();
                    break;
                case GameState.DefensePhase:
                    OnEnterDefensePhase();
                    break;
                case GameState.GameOver:
                    OnEnterGameOver();
                    break;
            }
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            // 重置玩家数据
            PlayerData = new PlayerData();
            
            // 重置子系统
            if (BattleSystem != null) BattleSystem.Reset();
            if (CaptureSystem != null) CaptureSystem.Reset();
            
            // 进入捕捉阶段
            ChangeState(GameState.CapturePhase);
        }

        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        public void UpdateGame(float deltaTime)
        {
            // 更新子系统
            if (BattleSystem != null) BattleSystem.UpdateGame(deltaTime);
            if (CaptureSystem != null) CaptureSystem.UpdateCapture(deltaTime);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 检查是否需要从捕捉阶段切换到防守阶段
        /// </summary>
        public void CheckPhaseTransition()
        {
            if (CurrentState == GameState.CapturePhase && CaptureSystem != null)
            {
                // 如果捕捉阶段时间到了，或者玩家主动切换，进入防守阶段
                if (CaptureSystem.IsCapturePhaseComplete() || ShouldTransitionToDefense())
                {
                    ChangeState(GameState.DefensePhase);
                }
            }
        }

        /// <summary>
        /// 是否应该切换到防守阶段（可以由外部逻辑决定）
        /// </summary>
        private bool ShouldTransitionToDefense()
        {
            // 这里可以添加判断逻辑，比如玩家手动切换等
            return false;
        }

        /// <summary>
        /// 进入捕捉阶段
        /// </summary>
        private void OnEnterCapturePhase()
        {
            if (BattleSystem != null) BattleSystem.StartCapturePhase();
            if (CaptureSystem != null) CaptureSystem.StartCapturePhase();
            
            Debug.Log("进入捕捉阶段！在限定时间内捕捉尽可能多的宠物！");
        }

        /// <summary>
        /// 进入防守阶段
        /// </summary>
        private void OnEnterDefensePhase()
        {
            // 更新玩家数据
            PlayerData.EnterDefensePhase();
            
            if (BattleSystem != null) BattleSystem.StartDefensePhase();
            if (CaptureSystem != null) CaptureSystem.EndCapturePhase();
            
            Debug.Log("进入防守阶段！使用捕捉到的宠物和建筑抵御敌人！");
        }

        /// <summary>
        /// 进入游戏结束状态
        /// </summary>
        private void OnEnterGameOver()
        {
            Debug.Log("游戏结束！");
            OnGameOver?.Invoke(true); // 假设是胜利
        }

        /// <summary>
        /// 处理游戏结束
        /// </summary>
        public void HandleGameOver(bool victory)
        {
            ChangeState(GameState.GameOver);
            OnGameOver?.Invoke(victory);
        }

        /// <summary>
        /// 获取当前捕捉阶段剩余时间
        /// </summary>
        public float GetCapturePhaseTimeRemaining()
        {
            if (CaptureSystem != null)
            {
                return CaptureSystem.GetTimeRemaining();
            }
            return 0f;
        }

        /// <summary>
        /// 获取当前波次信息
        /// </summary>
        public (int wave, int enemiesRemaining) GetWaveInfo()
        {
            if (BattleSystem != null)
            {
                return BattleSystem.GetWaveInfo();
            }
            return (0, 0);
        }
    }
}