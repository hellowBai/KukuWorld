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
    /// 捕捉界面控制器 - 管理捕捉阶段的用户界面
    /// </summary>
    public class CaptureUIController : MonoBehaviour
    {
        [Header("捕捉面板")]
        public GameObject capturePanel;
        public Button closeCaptureButton;
        
        [Header("野生KuKu显示")]
        public Transform wildKukuContainer;
        public GameObject wildKukuCardPrefab;
        
        [Header("选中KuKu信息显示")]
        public TextMeshProUGUI selectedKukuNameText;
        public TextMeshProUGUI selectedKukuRarityText;
        public TextMeshProUGUI selectedKukuHpText;
        public Slider selectedKukuHpSlider;
        public TextMeshProUGUI captureStatusText;
        public TextMeshProUGUI captureSuccessRateText;
        public TextMeshProUGUI capturePhaseTimerText;
        
        [Header("操作按钮")]
        public Button attackButton;
        public Button useKukuAttackButton;
        public Button captureButton;
        public Button useSpecialCaptureButton;
        public Button soulAbsorbButton;
        
        [Header("玩家KuKu选择")]
        public Transform playerKukuSelectionPanel;
        public GameObject playerKukuCardPrefab;
        
        // 状态变量
        private CaptureSystem.WildKuku? selectedWildKuku;
        private MythicalKukuData selectedCapturerKuku;
        private List<MythicalKukuData> playerKukus;
        
        // 系统引用
        private GameManager gameManager;
        private CaptureSystem captureSystem;
        
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
        /// 初始化捕捉界面
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
            
            // 初始化按钮事件
            InitializeButtonEvents();
            
            // 初始化显示
            UpdateCaptureTimerDisplay(gameManager.GetCapturePhaseRemainingTime());
            
            Debug.Log("捕捉界面控制器初始化完成");
        }
        
        /// <summary>
        /// 初始化按钮事件
        /// </summary>
        private void InitializeButtonEvents()
        {
            if (closeCaptureButton != null)
                closeCaptureButton.onClick.AddListener(CloseCapturePanel);
                
            if (attackButton != null)
                attackButton.onClick.AddListener(() => AttackSelectedWildKuku(false));
                
            if (useKukuAttackButton != null)
                useKukuAttackButton.onClick.AddListener(() => AttackSelectedWildKuku(true));
                
            if (captureButton != null)
                captureButton.onClick.AddListener(() => AttemptCaptureSelectedKuku(false));
                
            if (useSpecialCaptureButton != null)
                useSpecialCaptureButton.onClick.AddListener(() => AttemptCaptureSelectedKuku(true));
                
            if (soulAbsorbButton != null)
                soulAbsorbButton.onClick.AddListener(AttemptSoulAbsorption);
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
            
            // 订阅捕捉系统事件（如果有的话）
            // captureSystem?.OnWildKukuSpotted += OnWildKukuSpotted;
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
            
            // 取消订阅捕捉系统事件
            // captureSystem?.OnWildKukuSpotted -= OnWildKukuSpotted;
        }
        
        /// <summary>
        /// 游戏状态改变事件处理
        /// </summary>
        private void OnGameStateChange(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.CapturePhase)
            {
                OpenCapturePanel();
            }
            else if (newState != GameManager.GameState.CapturePhase)
            {
                CloseCapturePanel();
            }
        }
        
        /// <summary>
        /// 打开捕捉面板
        /// </summary>
        public void OpenCapturePanel()
        {
            if (capturePanel != null)
                capturePanel.SetActive(true);
                
            RefreshWildKukuList();
            RefreshPlayerKukuSelection();
        }
        
        /// <summary>
        /// 关闭捕捉面板
        /// </summary>
        public void CloseCapturePanel()
        {
            if (capturePanel != null)
                capturePanel.SetActive(false);
                
            selectedWildKuku = null;
            selectedCapturerKuku = null;
        }
        
        /// <summary>
        /// 刷新野生KuKu列表
        /// </summary>
        public void RefreshWildKukuList()
        {
            if (wildKukuContainer == null || wildKukuCardPrefab == null) return;
            
            // 清除现有卡片
            ClearWildKukuCards();
            
            // 获取当前野生KuKu列表
            var wildKukus = CaptureSystem.GetWildKukus();
            
            foreach (var wildKuku in wildKukus)
            {
                CreateWildKukuCard(wildKuku);
            }
        }
        
        /// <summary>
        /// 创建野生KuKu卡片
        /// </summary>
        private void CreateWildKukuCard(CaptureSystem.WildKuku wildKuku)
        {
            GameObject cardObj = Instantiate(wildKukuCardPrefab, wildKukuContainer);
            if (cardObj != null)
            {
                // 获取卡片上的UI组件并设置数据
                var nameText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = wildKuku.kukuData.Name;
                }
                
                // 添加点击事件
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    int kukuId = wildKuku.kukuData.Id;
                    cardButton.onClick.AddListener(() => OnWildKukuSelected(kukuId));
                }
            }
        }
        
        /// <summary>
        /// 清除野生KuKu卡片
        /// </summary>
        private void ClearWildKukuCards()
        {
            foreach (Transform child in wildKukuContainer)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        /// <summary>
        /// 刷新玩家KuKu选择面板
        /// </summary>
        private void RefreshPlayerKukuSelection()
        {
            if (playerKukuSelectionPanel == null || playerKukuCardPrefab == null) return;
            
            // 清除现有卡片
            foreach (Transform child in playerKukuSelectionPanel)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // 获取玩家的KuKu列表
            if (gameManager?.PlayerData?.CollectedKukus != null)
            {
                playerKukus = new List<MythicalKukuData>();
                
                foreach (var kvp in gameManager.PlayerData.CollectedKukus.Values)
                {
                    playerKukus.Add(kvp);
                    
                    GameObject cardObj = Instantiate(playerKukuCardPrefab, playerKukuSelectionPanel);
                    if (cardObj != null)
                    {
                        var nameText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                        if (nameText != null)
                        {
                            nameText.text = kvp.Name;
                        }
                        
                        // 添加选择事件
                        Button cardButton = cardObj.GetComponent<Button>();
                        if (cardButton != null)
                        {
                            var kukuData = kvp; // 捕获变量
                            cardButton.onClick.AddListener(() => SelectCapturerKuku(kukuData));
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 选择捕捉助手KuKu
        /// </summary>
        private void SelectCapturerKuku(MythicalKukuData kuku)
        {
            selectedCapturerKuku = kuku;
            Debug.Log($"选择了捕捉助手: {kuku.Name}");
        }
        
        /// <summary>
        /// 野生KuKu被选中
        /// </summary>
        private void OnWildKukuSelected(int kukuId)
        {
            // 在野生KuKu列表中查找指定ID的KuKu
            var wildKukus = CaptureSystem.GetWildKukus();
            foreach (var wildKuku in wildKukus)
            {
                if (wildKuku.kukuData.Id == kukuId)
                {
                    selectedWildKuku = wildKuku;
                    UpdateSelectedKukuInfo();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 更新选中KuKu信息
        /// </summary>
        private void UpdateSelectedKukuInfo()
        {
            if (!selectedWildKuku.HasValue) return;
            
            var wildKuku = selectedWildKuku.Value;
            var kukuData = wildKuku.kukuData;
            
            // 更新显示信息
            if (selectedKukuNameText != null)
                selectedKukuNameText.text = kukuData.Name;
                
            if (selectedKukuRarityText != null)
                selectedKukuRarityText.text = kukuData.GetMythicalRarityName();
                
            if (selectedKukuHpText != null)
                selectedKukuHpText.text = $"HP: {wildKuku.remainingHP:F1}/{kukuData.Health:F1}";
                
            if (selectedKukuHpSlider != null)
            {
                selectedKukuHpSlider.maxValue = kukuData.Health;
                selectedKukuHpSlider.value = wildKuku.remainingHP;
            }
            
            // 更新捕捉状态
            var (canCapture, successRate, hpRatio, status) = CaptureSystem.GetCaptureInfo(kukuData.Id);
            if (captureStatusText != null)
                captureStatusText.text = $"状态: {status}";
                
            if (captureSuccessRateText != null)
                captureSuccessRateText.text = $"成功率: {(successRate * 100):F1}%";
                
            // 更新按钮可用性
            UpdateButtonAvailability(canCapture);
        }
        
        /// <summary>
        /// 攻击选中的野生KuKu
        /// </summary>
        private void AttackSelectedWildKuku(bool useKuku)
        {
            if (!selectedWildKuku.HasValue)
            {
                Debug.LogWarning("请先选择一个野生KuKu");
                return;
            }
            
            int kukuId = selectedWildKuku.Value.kukuData.Id;
            float damage = 10f; // 基础伤害
            
            // 如果使用KuKu攻击，伤害会更高
            if (useKuku && selectedCapturerKuku != null)
            {
                damage = selectedCapturerKuku.AttackPower * 0.5f; // KuKu攻击力的一半
            }
            
            string message;
            bool success = CaptureSystem.AttackWildKuku(kukuId, damage, out message);
            
            if (success)
            {
                Debug.Log(message);
                UpdateSelectedKukuInfo(); // 更新显示信息
                
                // 显示通知
                ShowNotification(message);
            }
            else
            {
                Debug.LogWarning(message);
                ShowNotification(message, Color.red);
            }
        }
        
        /// <summary>
        /// 尝试捕捉选中的KuKu
        /// </summary>
        private void AttemptCaptureSelectedKuku(bool useSpecialMethod)
        {
            if (!selectedWildKuku.HasValue)
            {
                Debug.LogWarning("请先选择一个野生KuKu");
                return;
            }
            
            int kukuId = selectedWildKuku.Value.kukuData.Id;
            
            CaptureSystem.CaptureResult result;
            if (useSpecialMethod)
            {
                // 使用特殊捕捉方法（如果有）
                result = CaptureSystem.AttemptCapture(kukuId, selectedCapturerKuku);
            }
            else
            {
                result = CaptureSystem.AttemptCapture(kukuId, selectedCapturerKuku);
            }
            
            if (result.success)
            {
                Debug.Log(result.message);
                
                // 将捕捉到的KuKu添加到玩家数据
                if (gameManager?.PlayerData != null && result.capturedKuku != null)
                {
                    gameManager.PlayerData.AddKuku(result.capturedKuku);
                    
                    // 处理掉落物
                    foreach (var drop in result.drops)
                    {
                        ProcessItemDrop(drop);
                    }
                }
                
                // 刷新列表
                RefreshWildKukuList();
                selectedWildKuku = null;
                
                ShowNotification(result.message, Color.green);
            }
            else
            {
                Debug.LogWarning(result.message);
                ShowNotification(result.message, Color.red);
                
                // 如果捕捉失败但KuKu消失了，也刷新列表
                RefreshWildKukuList();
                selectedWildKuku = null;
            }
            
            UpdateSelectedKukuInfo();
        }
        
        /// <summary>
        /// 尝试灵魂吸收
        /// </summary>
        private void AttemptSoulAbsorption()
        {
            if (!selectedWildKuku.HasValue)
            {
                Debug.LogWarning("请先选择一个野生KuKu");
                return;
            }
            
            int kukuId = selectedWildKuku.Value.kukuData.Id;
            
            // 在捕捉阶段，野生KuKu通常不会主动吸收灵魂
            // 这里可以实现让玩家的KuKu吸收环境中灵魂的功能
            if (selectedCapturerKuku != null)
            {
                // 模拟使用玩家KuKu吸收环境中的灵魂
                // 在实际游戏中，这可能涉及从环境中获取灵魂或使用道具
                float soulAmount = 5f; // 模拟吸收的灵魂数量
                
                // 使用进化系统来处理灵魂吸收
                var evolutionResult = EvolutionSystem.EvolveKukuWithSoul(
                    selectedCapturerKuku, 
                    soulAmount, 
                    EvolutionSystem.SoulType.Common);
                
                string message = evolutionResult.success ? 
                    evolutionResult.message : 
                    $"灵魂吸收失败: {evolutionResult.message}";
                    
                Debug.Log(message);
                ShowNotification(message, evolutionResult.success ? Color.green : Color.red);
            }
            else
            {
                ShowNotification("请选择一个KuKu进行灵魂吸收", Color.yellow);
            }
        }
        
        /// <summary>
        /// 处理物品掉落
        /// </summary>
        private void ProcessItemDrop(CaptureSystem.ItemDrop drop)
        {
            if (gameManager?.PlayerData == null) return;
            
            switch (drop.type)
            {
                case CaptureSystem.ItemDrop.ItemType.Coin:
                    gameManager.PlayerData.AddCoins(drop.quantity);
                    break;
                case CaptureSystem.ItemDrop.ItemType.Soul:
                    gameManager.PlayerData.AddSouls(drop.value);
                    break;
                case CaptureSystem.ItemDrop.ItemType.KukuEgg:
                    // 处理KuKu蛋（可能用于孵化新KuKu）
                    Debug.Log($"获得KuKu蛋 x{drop.quantity}");
                    break;
                case CaptureSystem.ItemDrop.ItemType.Resource:
                    gameManager.PlayerData.AddGems(drop.quantity);
                    break;
            }
        }
        
        /// <summary>
        /// 更新按钮可用性
        /// </summary>
        private void UpdateButtonAvailability(bool canCapture)
        {
            if (attackButton != null)
                attackButton.interactable = selectedWildKuku.HasValue;
                
            if (useKukuAttackButton != null)
                useKukuAttackButton.interactable = selectedWildKuku.HasValue && selectedCapturerKuku != null;
                
            if (captureButton != null)
                captureButton.interactable = selectedWildKuku.HasValue && canCapture;
                
            if (useSpecialCaptureButton != null)
                useSpecialCaptureButton.interactable = selectedWildKuku.HasValue && canCapture && selectedCapturerKuku != null;
                
            if (soulAbsorbButton != null)
                soulAbsorbButton.interactable = selectedCapturerKuku != null;
        }
        
        /// <summary>
        /// 更新捕捉阶段倒计时显示
        /// </summary>
        public void UpdateCaptureTimerDisplay(float timeRemaining)
        {
            if (capturePhaseTimerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.RoundToInt(timeRemaining % 60);
                
                capturePhaseTimerText.text = $"捕捉时间: {minutes:D2}:{seconds:D2}";
                
                // 根据剩余时间改变颜色（时间紧张时变红）
                if (timeRemaining < 60)
                {
                    capturePhaseTimerText.color = Color.red;
                }
                else if (timeRemaining < 120)
                {
                    capturePhaseTimerText.color = Color.yellow;
                }
                else
                {
                    capturePhaseTimerText.color = Color.green;
                }
            }
        }
        
        /// <summary>
        /// 显示通知信息
        /// </summary>
        private void ShowNotification(string message, Color color = default(Color))
        {
            if (color == default(Color))
                color = Color.white;
                
            Debug.Log(message);
            // 这里可以实现UI通知系统的具体逻辑
        }
        
        /// <summary>
        /// UI更新
        /// </summary>
        private void Update()
        {
            // 定期更新倒计时显示
            if (gameManager != null && capturePhaseTimerText != null)
            {
                UpdateCaptureTimerDisplay(gameManager.GetCapturePhaseRemainingTime());
            }
        }
    }
}