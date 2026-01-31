using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class KukuUIManager : MonoBehaviour
{
    [Header("通用UI元素")]
    public Canvas gameCanvas;
    public GameObject loadingScreen;
    public TextMeshProUGUI loadingText;
    public Slider loadingBar;
    
    [Header("游戏信息UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resourcesText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI livesText;
    
    [Header("捕捉界面")]
    public GameObject capturePanel;
    public Button captureButton;
    public Button attackButton;
    public TextMeshProUGUI captureStatusText;
    public Slider captureHealthBar;
    
    [Header("商店界面")]
    public GameObject shopPanel;
    public Button buyButton;
    public TextMeshProUGUI shopItemName;
    public TextMeshProUGUI shopItemPrice;
    
    [Header("融合界面")]
    public GameObject fusionPanel;
    public Button fusionButton;
    public TextMeshProUGUI fusionResultText;
    
    [Header("提示系统")]
    public GameObject tooltipPrefab;
    public GameObject notificationPrefab;
    
    private GameManager gameManager;
    private CaptureSystem captureSystem;
    private BattleSystem battleSystem;
    
    private Dictionary<string, GameObject> tooltips = new Dictionary<string, GameObject>();
    private Queue<string> notificationQueue = new Queue<string>();
    
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    void InitializeUI()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            captureSystem = gameManager.captureSystem;
            battleSystem = gameManager.battleSystem;
        }
        
        // 初始化按钮事件
        if (captureButton != null)
            captureButton.onClick.AddListener(() => AttemptCapture());
            
        if (attackButton != null)
            attackButton.onClick.AddListener(() => AttackWildKuku());
            
        if (buyButton != null)
            buyButton.onClick.AddListener(() => BuyItem());
            
        if (fusionButton != null)
            fusionButton.onClick.AddListener(() => PerformFusion());
        
        // 隐藏所有面板
        HideAllPanels();
        
        Debug.Log("UI管理器初始化完成");
    }
    
    void SubscribeToEvents()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChange += OnGameStateChange;
        }
        
        if (captureSystem != null)
        {
            captureSystem.OnWildKukuSpawned += OnWildKukuSpawned;
            captureSystem.OnWildKukuCaptured += OnWildKukuCaptured;
        }
        
        if (battleSystem != null)
        {
            battleSystem.OnWaveStarted += OnWaveStarted;
            battleSystem.OnEnemyReachedTarget += OnEnemyReachedTarget;
        }
    }
    
    void OnGameStateChange(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.CapturePhase:
                ShowCapturePanel();
                break;
            case GameManager.GameState.DefensePhase:
                ShowGameInfoUI();
                break;
            case GameManager.GameState.GameOver:
                ShowGameOverUI();
                break;
            default:
                HideAllPanels();
                break;
        }
        
        UpdateUI();
    }
    
    void OnWildKukuSpawned(int kukuId)
    {
        ShowNotification($"发现野生KuKu！ID: {kukuId}");
    }
    
    void OnWildKukuCaptured(int kukuId)
    {
        ShowNotification($"成功捕捉KuKu！ID: {kukuId}");
    }
    
    void OnWaveStarted(int wave)
    {
        ShowNotification($"第 {wave} 波敌人来袭！");
    }
    
    void OnEnemyReachedTarget(int remainingLives)
    {
        ShowNotification($"敌人突破防线！剩余生命: {remainingLives}");
    }
    
    void Update()
    {
        UpdateUI();
        ProcessNotifications();
    }
    
    void UpdateUI()
    {
        if (gameManager != null)
        {
            UpdateGameInfoUI();
            
            if (gameManager.CurrentState == GameManager.GameState.CapturePhase)
            {
                UpdateCaptureUI();
            }
            else if (gameManager.CurrentState == GameManager.GameState.DefensePhase)
            {
                UpdateBattleUI();
            }
        }
    }
    
    void UpdateGameInfoUI()
    {
        // 更新时间显示
        if (timerText != null && gameManager != null)
        {
            float timeRemaining = gameManager.GetCapturePhaseRemainingTime();
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.RoundToInt(timeRemaining % 60);
            
            if (gameManager.CurrentState == GameManager.GameState.CapturePhase)
            {
                timerText.text = $"捕捉时间: {minutes:D2}:{seconds:D2}";
                
                // 根据剩余时间改变颜色
                if (timeRemaining < 60)
                {
                    timerText.color = Color.red;
                }
                else if (timeRemaining < 120)
                {
                    timerText.color = Color.yellow;
                }
                else
                {
                    timerText.color = Color.green;
                }
            }
            else if (gameManager.CurrentState == GameManager.GameState.DefensePhase && battleSystem != null)
            {
                timerText.text = $"波次: {battleSystem.GetCurrentWave()}";
            }
        }
        
        // 更新资源显示
        if (resourcesText != null)
        {
            var playerData = gameManager?.GetPlayerData();
            if (playerData != null)
            {
                resourcesText.text = $"金币: {playerData.Coins} | 神石: {playerData.Gems} | 灵魂: {playerData.Souls:F1}";
            }
        }
        
        // 更新生命值显示
        if (livesText != null && battleSystem != null)
        {
            livesText.text = $"生命: {battleSystem.GetPlayerLives()}";
        }
    }
    
    void UpdateCaptureUI()
    {
        if (captureStatusText != null)
        {
            var wildKukus = captureSystem?.GetWildKukus();
            if (wildKukus != null && wildKukus.Count > 0)
            {
                var firstKuku = wildKukus[0]; // 简化：只显示第一个野生KuKu的信息
                captureStatusText.text = $"{firstKuku.kukuData.Name}\n" +
                                        $"生命值: {firstKuku.remainingHP:F1}/{firstKuku.kukuData.Health:F1}\n" +
                                        $"稀有度: {firstKuku.kukuData.GetRarityName()}\n" +
                                        $"可捕捉: {(firstKuku.isCapturable ? "是" : "否")}";
                
                if (captureHealthBar != null)
                {
                    captureHealthBar.maxValue = firstKuku.kukuData.Health;
                    captureHealthBar.value = firstKuku.remainingHP;
                }
            }
            else
            {
                captureStatusText.text = "当前没有野生KuKu\n请等待生成...";
                if (captureHealthBar != null)
                    captureHealthBar.value = 0;
            }
        }
    }
    
    void UpdateBattleUI()
    {
        if (waveText != null && battleSystem != null)
        {
            waveText.text = $"波次: {battleSystem.GetCurrentWave()}";
        }
        
        if (livesText != null && battleSystem != null)
        {
            livesText.text = $"生命: {battleSystem.GetPlayerLives()}";
        }
    }
    
    public void ShowLoadingScreen(string message = "加载中...")
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            if (loadingText != null)
                loadingText.text = message;
            if (loadingBar != null)
                loadingBar.value = 0f;
        }
    }
    
    public void UpdateLoadingProgress(float progress, string message = "")
    {
        if (loadingBar != null)
            loadingBar.value = Mathf.Clamp01(progress);
            
        if (loadingText != null && !string.IsNullOrEmpty(message))
            loadingText.text = message;
    }
    
    public void HideLoadingScreen()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
    
    public void ShowCapturePanel()
    {
        HideAllPanels();
        if (capturePanel != null)
            capturePanel.SetActive(true);
    }
    
    public void ShowShopPanel()
    {
        HideAllPanels();
        if (shopPanel != null)
            shopPanel.SetActive(true);
    }
    
    public void ShowFusionPanel()
    {
        HideAllPanels();
        if (fusionPanel != null)
            fusionPanel.SetActive(true);
    }
    
    public void ShowGameInfoUI()
    {
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (resourcesText != null) resourcesText.gameObject.SetActive(true);
        if (waveText != null) waveText.gameObject.SetActive(true);
        if (livesText != null) livesText.gameObject.SetActive(true);
    }
    
    public void ShowGameOverUI()
    {
        HideAllPanels();
        ShowNotification("游戏结束！");
    }
    
    public void HideAllPanels()
    {
        if (capturePanel != null) capturePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (fusionPanel != null) fusionPanel.SetActive(false);
    }
    
    public void ShowTooltip(string message, Vector3 worldPosition)
    {
        if (tooltipPrefab == null) return;
        
        // 转换世界坐标到屏幕坐标
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
        
        // 创建提示框
        GameObject tooltip = Instantiate(tooltipPrefab, gameCanvas.transform);
        var textComponent = tooltip.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }
        
        // 设置位置
        var rectTransform = tooltip.GetComponent<RectTransform>();
        rectTransform.position = screenPosition;
        
        // 生成唯一ID
        string id = System.Guid.NewGuid().ToString();
        tooltips[id] = tooltip;
        
        // 一段时间后自动销毁
        StartCoroutine(DestroyTooltip(id, 3f));
    }
    
    IEnumerator DestroyTooltip(string id, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tooltips.ContainsKey(id) && tooltips[id] != null)
        {
            Destroy(tooltips[id]);
            tooltips.Remove(id);
        }
    }
    
    public void ShowNotification(string message)
    {
        if (notificationPrefab == null) return;
        
        // 将通知添加到队列
        notificationQueue.Enqueue(message);
    }
    
    void ProcessNotifications()
    {
        // 处理通知队列
        if (notificationQueue.Count > 0)
        {
            string message = notificationQueue.Dequeue();
            CreateNotification(message);
        }
    }
    
    void CreateNotification(string message)
    {
        GameObject notification = Instantiate(notificationPrefab, gameCanvas.transform);
        var textComponent = notification.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }
        
        // 设置位置（通常在屏幕顶部）
        var rectTransform = notification.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.9f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.9f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        
        // 动画效果
        StartCoroutine(AnimateNotification(notification));
    }
    
    IEnumerator AnimateNotification(GameObject notification)
    {
        // 简单的淡入淡出效果
        var canvasGroup = notification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = notification.AddComponent<CanvasGroup>();
        
        // 淡入
        float fadeInDuration = 0.5f;
        for (float t = 0; t < fadeInDuration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeInDuration);
            yield return null;
        }
        
        // 显示一段时间
        yield return new WaitForSeconds(2f);
        
        // 淡出
        float fadeOutDuration = 1f;
        for (float t = 0; t < fadeOutDuration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutDuration);
            yield return null;
        }
        
        // 销毁通知
        Destroy(notification);
    }
    
    void AttemptCapture()
    {
        var wildKukus = captureSystem?.GetWildKukus();
        if (wildKukus != null && wildKukus.Count > 0)
        {
            var firstKuku = wildKukus[0];
            var result = captureSystem.AttemptCapture(firstKuku.kukuData.Id);
            
            if (result.success)
            {
                ShowNotification($"成功捕捉到 {result.capturedKuku.Name}！");
                
                // 添加到玩家收藏
                var playerData = gameManager?.GetPlayerData();
                if (playerData != null)
                {
                    playerData.AddKuku(result.capturedKuku);
                }
            }
            else
            {
                ShowNotification(result.message);
            }
        }
        else
        {
            ShowNotification("没有可捕捉的野生KuKu！");
        }
    }
    
    void AttackWildKuku()
    {
        var wildKukus = captureSystem?.GetWildKukus();
        if (wildKukus != null && wildKukus.Count > 0)
        {
            var firstKuku = wildKukus[0];
            string message;
            bool alive = captureSystem.AttackWildKuku(firstKuku.kukuData.Id, 20f, out message);
            
            ShowNotification(message);
        }
        else
        {
            ShowNotification("没有可攻击的野生KuKu！");
        }
    }
    
    void BuyItem()
    {
        ShowNotification("购买功能待实现");
    }
    
    void PerformFusion()
    {
        ShowNotification("融合功能待实现");
    }
    
    public void SetResourceDisplay(int coins, int gems, float souls)
    {
        if (resourcesText != null)
        {
            resourcesText.text = $"金币: {coins} | 神石: {gems} | 灵魂: {souls:F1}";
        }
    }
    
    public void HighlightElement(GameObject element, Color highlightColor, float duration)
    {
        if (element == null) return;
        
        var graphic = element.GetComponent<Graphic>();
        if (graphic != null)
        {
            Color originalColor = graphic.color;
            graphic.color = highlightColor;
            
            StartCoroutine(ResetHighlight(graphic, originalColor, duration));
        }
    }
    
    IEnumerator ResetHighlight(Graphic graphic, Color originalColor, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (graphic != null)
        {
            graphic.color = originalColor;
        }
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChange -= OnGameStateChange;
        }
        
        if (captureSystem != null)
        {
            captureSystem.OnWildKukuSpawned -= OnWildKukuSpawned;
            captureSystem.OnWildKukuCaptured -= OnWildKukuCaptured;
        }
        
        if (battleSystem != null)
        {
            battleSystem.OnWaveStarted -= OnWaveStarted;
            battleSystem.OnEnemyReachedTarget -= OnEnemyReachedTarget;
        }
        
        // 清理所有提示框
        foreach (var tooltip in tooltips.Values)
        {
            if (tooltip != null)
                Destroy(tooltip);
        }
        tooltips.Clear();
    }
}