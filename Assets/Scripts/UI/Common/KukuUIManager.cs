using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KukuWorld.Data;
using KukuWorld.Systems;

namespace KukuWorld.UI
{
    /// <summary>
    /// KuKu UI管理器 - 统一管理游戏中的UI界面
    /// </summary>
    public class KukuUIManager : MonoBehaviour
    {
        // UI面板引用
        [Header("通用面板")]
        public GameObject mainMenuPanel;
        public GameObject capturePanel;
        public GameObject defensePanel;
        public GameObject shopPanel;
        public GameObject settingsPanel;
        public GameObject gameOverPanel;
        
        [Header("信息显示")]
        public TextMeshProUGUI resourceDisplay; // 资源显示
        public TextMeshProUGUI timerDisplay;   // 计时器显示
        public TextMeshProUGUI waveDisplay;    // 波次显示
        public TextMeshProUGUI statusDisplay;  // 状态显示
        
        [Header("按钮")]
        public Button startGameButton;
        public Button endCapturePhaseButton;
        public Button pauseButton;
        public Button exitButton;
        
        // 系统引用
        private GameManager gameManager;
        private CaptureSystem captureSystem;
        private BattleSystem battleSystem;
        
        // UI状态管理
        private Dictionary<string, GameObject> uiPanels;
        
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
        /// 初始化UI管理器
        /// </summary>
        public void Initialize()
        {
            // 获取系统引用
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManager实例未找到！");
                return;
            }
            
            // 初始化面板字典
            InitializePanelsDictionary();
            
            // 绑定按钮事件
            BindButtonEvents();
            
            // 初始化显示
            UpdateResourceDisplay();
            UpdateTimerDisplay();
            
            Debug.Log("KuKu UI管理器初始化完成");
        }
        
        /// <summary>
        /// 初始化面板字典
        /// </summary>
        private void InitializePanelsDictionary()
        {
            uiPanels = new Dictionary<string, GameObject>
            {
                ["MainMenu"] = mainMenuPanel,
                ["Capture"] = capturePanel,
                ["Defense"] = defensePanel,
                ["Shop"] = shopPanel,
                ["Settings"] = settingsPanel,
                ["GameOver"] = gameOverPanel
            };
            
            // 初始化所有面板为隐藏状态
            foreach (var panel in uiPanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        private void BindButtonEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
                
            if (endCapturePhaseButton != null)
                endCapturePhaseButton.onClick.AddListener(OnEndCapturePhaseClicked);
                
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);
                
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }
        
        /// <summary>
        /// 订阅系统事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChange += OnGameStateChange;
            }
        }
        
        /// <summary>
        /// 取消订阅系统事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChange -= OnGameStateChange;
            }
        }
        
        /// <summary>
        /// 游戏状态改变事件处理
        /// </summary>
        private void OnGameStateChange(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    ShowPanel("MainMenu");
                    break;
                case GameManager.GameState.CapturePhase:
                    ShowPanel("Capture");
                    break;
                case GameManager.GameState.DefensePhase:
                    ShowPanel("Defense");
                    break;
                case GameManager.GameState.Shop:
                    ShowPanel("Shop");
                    break;
                case GameManager.GameState.Settings:
                    ShowPanel("Settings");
                    break;
                case GameManager.GameState.GameOver:
                    ShowPanel("GameOver");
                    break;
            }
            
            UpdateUIForGameState(newState);
        }
        
        /// <summary>
        /// 根据游戏状态更新UI
        /// </summary>
        private void UpdateUIForGameState(GameManager.GameState state)
        {
            switch (state)
            {
                case GameManager.GameState.CapturePhase:
                    if (endCapturePhaseButton != null)
                        endCapturePhaseButton.gameObject.SetActive(true);
                    break;
                case GameManager.GameState.DefensePhase:
                    if (endCapturePhaseButton != null)
                        endCapturePhaseButton.gameObject.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// 显示指定面板
        /// </summary>
        public void ShowPanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName) && uiPanels[panelName] != null)
            {
                // 隐藏所有面板
                foreach (var panel in uiPanels.Values)
                {
                    if (panel != null)
                        panel.SetActive(false);
                }
                
                // 显示指定面板
                uiPanels[panelName].SetActive(true);
            }
            else
            {
                Debug.LogWarning($"未找到面板: {panelName}");
            }
        }
        
        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        public void HidePanel(string panelName)
        {
            if (uiPanels.ContainsKey(panelName) && uiPanels[panelName] != null)
            {
                uiPanels[panelName].SetActive(false);
            }
        }
        
        /// <summary>
        /// 更新资源显示
        /// </summary>
        public void UpdateResourceDisplay()
        {
            if (resourceDisplay != null && gameManager?.PlayerData != null)
            {
                var playerData = gameManager.PlayerData;
                resourceDisplay.text = $"金币: {playerData.Coins} | 神石: {playerData.Gems} | 灵魂: {playerData.Souls:F1}";
            }
        }
        
        /// <summary>
        /// 更新计时器显示
        /// </summary>
        public void UpdateTimerDisplay()
        {
            if (timerDisplay != null && gameManager != null)
            {
                if (gameManager.IsInCapturePhase())
                {
                    float remainingTime = gameManager.GetCapturePhaseRemainingTime();
                    int minutes = Mathf.FloorToInt(remainingTime / 60);
                    int seconds = Mathf.RoundToInt(remainingTime % 60);
                    
                    timerDisplay.text = $"捕捉时间: {minutes:D2}:{seconds:D2}";
                    
                    // 根据剩余时间改变颜色
                    if (remainingTime < 60)
                    {
                        timerDisplay.color = Color.red;
                    }
                    else if (remainingTime < 120)
                    {
                        timerDisplay.color = Color.yellow;
                    }
                    else
                    {
                        timerDisplay.color = Color.green;
                    }
                }
                else
                {
                    timerDisplay.text = "防守阶段";
                    timerDisplay.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// 更新波次显示
        /// </summary>
        public void UpdateWaveDisplay()
        {
            if (waveDisplay != null && battleSystem != null)
            {
                var (state, wave, lives, enemies) = battleSystem.GetBattleStatus();
                if (state == BattleSystem.BattleState.Fighting)
                {
                    waveDisplay.text = $"波次: {wave} | 敌人: {enemies} | 生命: {lives}";
                }
            }
        }
        
        /// <summary>
        /// 更新状态显示
        /// </summary>
        public void UpdateStatusDisplay(string message)
        {
            if (statusDisplay != null)
            {
                statusDisplay.text = message;
            }
        }
        
        /// <summary>
        /// 按钮点击事件处理
        /// </summary>
        private void OnStartGameClicked()
        {
            gameManager?.StartNewGame();
        }
        
        private void OnEndCapturePhaseClicked()
        {
            gameManager?.ForceEndCapturePhase();
        }
        
        private void OnPauseClicked()
        {
            if (gameManager != null)
            {
                if (gameManager.CurrentState == GameManager.GameState.Paused)
                {
                    gameManager.ChangeState(GameManager.GameState.CapturePhase); // 或当前状态
                }
                else
                {
                    gameManager.ChangeState(GameManager.GameState.Paused);
                }
            }
        }
        
        private void OnExitClicked()
        {
            gameManager?.QuitGame();
        }
        
        /// <summary>
        /// UI更新
        /// </summary>
        private void Update()
        {
            // 定期更新显示信息
            UpdateResourceDisplay();
            UpdateTimerDisplay();
            UpdateWaveDisplay();
        }
        
        /// <summary>
        /// 显示提示信息
        /// </summary>
        public void ShowNotification(string message, float duration = 3f)
        {
            UpdateStatusDisplay(message);
            
            // 可以在这里实现一个协程来定时清除提示信息
            StartCoroutine(ClearNotificationAfterDelay(duration));
        }
        
        /// <summary>
        /// 延迟清除提示信息
        /// </summary>
        private System.Collections.IEnumerator ClearNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (statusDisplay != null)
            {
                statusDisplay.text = "";
            }
        }
        
        /// <summary>
        /// 刷新所有UI显示
        /// </summary>
        public void RefreshAllDisplays()
        {
            UpdateResourceDisplay();
            UpdateTimerDisplay();
            UpdateWaveDisplay();
        }
    }
}