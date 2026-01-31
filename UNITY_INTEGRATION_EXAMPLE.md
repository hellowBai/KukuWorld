# KukuWorld Unity 集成示例

## 1. Unity项目设置

### 1.1 创建新项目
1. 打开Unity Hub
2. 点击"New Project"
3. 选择"2D Template"（适用于KukuWorld的2D塔防玩法）
4. 项目名称设置为"KukuWorld"
5. 点击"Create"

### 1.2 项目结构配置
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── GameState.cs
│   │   └── EventManager.cs
│   ├── Data/
│   │   ├── KukuData.cs
│   │   ├── MythicalKukuData.cs
│   │   ├── PlayerData.cs
│   │   ├── WeaponData.cs
│   │   ├── BuildingData.cs
│   │   └── UnitData.cs
│   ├── Systems/
│   │   ├── CaptureSystem.cs
│   │   ├── BattleSystem.cs
│   │   ├── EvolutionSystem.cs
│   │   ├── FusionSystem.cs
│   │   ├── EquipmentSystem.cs
│   │   ├── BuildingManager.cs
│   │   └── KukuCollectionSystem.cs
│   ├── UI/
│   │   ├── Common/
│   │   │   └── UIManager.cs
│   │   ├── Capture/
│   │   │   └── CaptureUIController.cs
│   │   ├── Shop/
│   │   │   └── ShopUIController.cs
│   │   └── Fusion/
│   │       └── FusionUIController.cs
│   ├── Controllers/
│   │   ├── MainGameController.cs
│   │   ├── EnemyController.cs
│   │   ├── KukuCombatController.cs
│   │   └── UnitCombatController.cs
│   └── Utils/
│       └── KukuGameUtils.cs
├── Prefabs/
│   ├── Units/
│   ├── UI/
│   └── Managers/
├── Scenes/
│   ├── MainMenu.unity
│   ├── CaptureScene.unity
│   ├── BattleScene.unity
│   └── GameScene.unity
├── Resources/
├── Sprites/
├── Audio/
└── Materials/
```

## 2. 核心管理器实现

### 2.1 Unity Manager场景设置
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerSceneSetup : MonoBehaviour
{
    [Header("场景设置")]
    public string mainMenuScene = "MainMenu";
    public string captureScene = "CaptureScene";
    public string battleScene = "BattleScene";
    
    [Header("管理器预制体")]
    public GameObject gameManagerPrefab;
    public GameObject eventManagerPrefab;
    public GameObject audioManagerPrefab;
    
    void Awake()
    {
        // 检查是否已存在管理器
        if (FindObjectOfType<GameManager>() == null)
        {
            // 创建GameManager
            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            else
            {
                // 创建默认GameManager
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }
        }
        
        if (FindObjectOfType<EventManager>() == null)
        {
            // 创建EventManager
            if (eventManagerPrefab != null)
            {
                Instantiate(eventManagerPrefab);
            }
            else
            {
                GameObject emObj = new GameObject("EventManager");
                emObj.AddComponent<EventManager>();
            }
        }
        
        if (FindObjectOfType<AudioManager>() == null)
        {
            // 创建AudioManager
            if (audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
            }
            else
            {
                GameObject amObj = new GameObject("AudioManager");
                amObj.AddComponent<AudioManager>();
            }
        }
        
        // 确保管理器对象不会被销毁
        DontDestroyOnLoad(this);
    }
    
    void Start()
    {
        // 加载主菜单场景
        SceneManager.LoadScene(mainMenuScene);
    }
}
```

### 2.2 事件管理器
```csharp
using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    // 事件字典
    private Dictionary<System.Type, System.Delegate> eventDict = new Dictionary<System.Type, System.Delegate>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartListening<T>(System.Action<T> listener)
    {
        System.Delegate d;
        if (eventDict.TryGetValue(typeof(T), out d))
        {
            eventDict[typeof(T)] = System.Delegate.Combine(d, listener);
        }
        else
        {
            eventDict[typeof(T)] = listener;
        }
    }
    
    public void StopListening<T>(System.Action<T> listener)
    {
        System.Delegate d;
        if (eventDict.TryGetValue(typeof(T), out d))
        {
            eventDict[typeof(T)] = System.Delegate.Remove(d, listener);
            if (eventDict[typeof(T)] == null)
            {
                eventDict.Remove(typeof(T));
            }
        }
    }
    
    public void TriggerEvent<T>(T @event)
    {
        System.Delegate d;
        if (eventDict.TryGetValue(typeof(T), out d))
        {
            System.Action<T> callback = d as System.Action<T>;
            if (callback != null)
            {
                callback(@event);
            }
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            eventDict.Clear();
        }
    }
}

// 事件定义
public struct CapturePhaseStartedEvent
{
    public float Duration;
    public int InitialResources;
    
    public CapturePhaseStartedEvent(float duration, int resources)
    {
        Duration = duration;
        InitialResources = resources;
    }
}

public struct CapturePhaseEndedEvent
{
    public int TotalCaptures;
    public int TotalResourcesGained;
    public List<MythicalKukuData> CapturedKukus;
    
    public CapturePhaseEndedEvent(int captures, int resources, List<MythicalKukuData> kukus)
    {
        TotalCaptures = captures;
        TotalResourcesGained = resources;
        CapturedKukus = kukus;
    }
}

public struct DefensePhaseStartedEvent
{
    public int StartingLives;
    public int TotalWaves;
    
    public DefensePhaseStartedEvent(int lives, int waves)
    {
        StartingLives = lives;
        TotalWaves = waves;
    }
}

public struct EnemyReachedTargetEvent
{
    public int RemainingLives;
    public float DamageDealt;
    
    public EnemyReachedTargetEvent(int lives, float damage)
    {
        RemainingLives = lives;
        DamageDealt = damage;
    }
}

public struct PlayerDefeatedEvent
{
    public int FinalWave;
    public int TotalEnemiesDefeated;
    public int Score;
    
    public PlayerDefeatedEvent(int wave, int enemiesDefeated, int score)
    {
        FinalWave = wave;
        TotalEnemiesDefeated = enemiesDefeated;
        Score = score;
    }
}

public struct PlayerVictoryEvent
{
    public int FinalWave;
    public int TotalEnemiesDefeated;
    public int Score;
    
    public PlayerVictoryEvent(int wave, int enemiesDefeated, int score)
    {
        FinalWave = wave;
        TotalEnemiesDefeated = enemiesDefeated;
        Score = score;
    }
}
```

### 2.3 音频管理器
```csharp
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("音频源")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    
    [Header("音效剪辑")]
    public AudioClip captureReadySound;
    public AudioClip captureSuccessSound;
    public AudioClip captureFailedSound;
    public AudioClip kukuEvolvedSound;
    public AudioClip fusionCompletedSound;
    public AudioClip battleVictorySound;
    public AudioClip battleDefeatSound;
    
    [Header("音量设置")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 0.75f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        
        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
        }
    }
    
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = masterVolume * bgmVolume;
            bgmSource.Play();
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volume);
        }
    }
    
    public void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (voiceSource != null && clip != null)
        {
            voiceSource.PlayOneShot(clip, masterVolume * voiceVolume * volume);
        }
    }
    
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
    
    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }
    
    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    void UpdateAllVolumes()
    {
        if (bgmSource != null)
            bgmSource.volume = masterVolume * bgmVolume;
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
        if (voiceSource != null)
            voiceSource.volume = masterVolume * voiceVolume;
    }
    
    public void PlayCaptureReadySound()
    {
        PlaySFX(captureReadySound);
    }
    
    public void PlayCaptureSuccessSound()
    {
        PlaySFX(captureSuccessSound);
    }
    
    public void PlayCaptureFailedSound()
    {
        PlaySFX(captureFailedSound);
    }
    
    public void PlayKukuEvolvedSound()
    {
        PlaySFX(kukuEvolvedSound);
    }
    
    public void PlayFusionCompletedSound()
    {
        PlaySFX(fusionCompletedSound);
    }
    
    public void PlayBattleVictorySound()
    {
        PlaySFX(battleVictorySound);
    }
    
    public void PlayBattleDefeatSound()
    {
        PlaySFX(battleDefeatSound);
    }
}
```

## 3. Unity场景集成

### 3.1 主菜单场景
```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI元素")]
    public Button startGameButton;
    public Button settingsButton;
    public Button quitButton;
    public Button creditsButton;
    
    [Header("设置面板")]
    public GameObject settingsPanel;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;
    
    [Header("制作人员面板")]
    public GameObject creditsPanel;
    
    void Start()
    {
        InitializeUI();
        LoadSettings();
    }
    
    void InitializeUI()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettingsPanel);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ToggleCreditsPanel);
            
        // 设置面板UI
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }
    
    void StartGame()
    {
        // 播放音效
        AudioManager.Instance?.PlaySFX(null); // 使用合适的音效
        
        // 开始游戏
        GameManager.Instance?.StartNewGame();
        
        // 加载游戏场景
        SceneManager.LoadScene("GameScene");
    }
    
    void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
    
    void ToggleCreditsPanel()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(!creditsPanel.activeSelf);
        }
    }
    
    void QuitGame()
    {
        GameManager.Instance?.QuitGame();
    }
    
    void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(volume);
        }
    }
    
    void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
    }
    
    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    void SetResolution(int dropdownIndex)
    {
        if (dropdownIndex >= 0 && dropdownIndex < Screen.resolutions.Length)
        {
            Resolution res = Screen.resolutions[dropdownIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
    }
    
    void LoadSettings()
    {
        // 加载音量设置
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            
        // 加载全屏设置
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            
        // 加载分辨率设置
        if (resolutionDropdown != null)
        {
            int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
            if (savedResIndex >= 0 && savedResIndex < Screen.resolutions.Length)
            {
                resolutionDropdown.value = savedResIndex;
            }
        }
    }
    
    void SaveSettings()
    {
        // 保存设置
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        
        if (resolutionDropdown != null)
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
            
        PlayerPrefs.Save();
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveSettings(); // 应用失去焦点时保存设置
        }
    }
    
    void OnDestroy()
    {
        SaveSettings(); // 销毁时保存设置
    }
}
```

### 3.2 捕捉场景
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptureSceneController : MonoBehaviour
{
    [Header("UI元素")]
    public CaptureUIController captureUIController;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resourcesText;
    public Button endCaptureButton;
    
    [Header("游戏世界")]
    public Transform playerTransform;
    public Camera gameCamera;
    
    [Header("野生KuKu显示")]
    public Transform wildKukuContainer;
    public GameObject wildKukuIndicatorPrefab;
    
    private CaptureSystem captureSystem;
    private GameManager gameManager;
    
    void Start()
    {
        InitializeScene();
    }
    
    void InitializeScene()
    {
        gameManager = GameManager.Instance;
        captureSystem = gameManager.captureSystem;
        
        if (captureUIController != null)
        {
            captureUIController.Initialize();
        }
        
        if (endCaptureButton != null)
        {
            endCaptureButton.onClick.AddListener(ForceEndCapturePhase);
        }
        
        // 订阅事件
        gameManager.OnGameStateChange += OnGameStateChange;
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
        UpdateCameraFollow();
        UpdateWildKukuIndicators();
    }
    
    void UpdateUI()
    {
        if (timerText != null && gameManager != null)
        {
            float timeRemaining = gameManager.GetCapturePhaseRemainingTime();
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.RoundToInt(timeRemaining % 60);
            
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
        
        if (resourcesText != null && gameManager != null)
        {
            var playerData = gameManager.GetPlayerData();
            if (playerData != null)
            {
                resourcesText.text = $"金币: {playerData.Coins} | 神石: {playerData.Gems} | 灵魂: {playerData.Souls:F1}";
            }
        }
    }
    
    void UpdateCameraFollow()
    {
        // 相机跟随玩家
        if (playerTransform != null && gameCamera != null)
        {
            gameCamera.transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                gameCamera.transform.position.z
            );
        }
    }
    
    void UpdateWildKukuIndicators()
    {
        // 更新野生KuKu指示器
        // 这里可以创建或更新指向野生KuKu的UI指示器
    }
    
    void OnGameStateChange(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.DefensePhase)
        {
            // 捕捉阶段结束，切换到战斗场景
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
    }
    
    void ForceEndCapturePhase()
    {
        gameManager?.EndCapturePhase();
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChange -= OnGameStateChange;
        }
    }
}
```

### 3.3 战斗场景
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSceneController : MonoBehaviour
{
    [Header("UI元素")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI enemiesRemainingText;
    public Slider progressSlider;
    public Button pauseButton;
    
    [Header("防御点")]
    public Transform[] defensePoints;
    public GameObject templePrefab;
    
    [Header("敌人生成点")]
    public Transform[] spawnPoints;
    
    private BattleSystem battleSystem;
    private GameManager gameManager;
    
    void Start()
    {
        InitializeScene();
    }
    
    void InitializeScene()
    {
        gameManager = GameManager.Instance;
        battleSystem = gameManager.battleSystem;
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        // 订阅事件
        gameManager.OnGameStateChange += OnGameStateChange;
        
        InitializeDefensePoints();
        UpdateUI();
    }
    
    void InitializeDefensePoints()
    {
        // 初始化防御点
        foreach (var point in defensePoints)
        {
            if (templePrefab != null)
            {
                Instantiate(templePrefab, point.position, Quaternion.identity);
            }
        }
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (waveText != null && battleSystem != null)
        {
            waveText.text = $"波次: {battleSystem.GetCurrentWave()}";
        }
        
        if (livesText != null && battleSystem != null)
        {
            livesText.text = $"生命: {battleSystem.GetPlayerLives()}";
        }
        
        if (enemiesRemainingText != null && battleSystem != null)
        {
            enemiesRemainingText.text = $"剩余敌人: {battleSystem.GetEnemiesRemaining()}";
        }
        
        if (progressSlider != null && battleSystem != null)
        {
            float progress = battleSystem.GetWaveProgress();
            progressSlider.value = progress;
        }
    }
    
    void OnGameStateChange(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GameOver)
        {
            // 游戏结束，显示结果
            ShowGameOverScreen();
        }
        else if (newState == GameManager.GameState.Paused)
        {
            Time.timeScale = 0f;
        }
        else if (newState != GameManager.GameState.Paused)
        {
            Time.timeScale = 1f;
        }
    }
    
    void TogglePause()
    {
        if (gameManager.CurrentState == GameManager.GameState.Paused)
        {
            gameManager.ResumeGame();
        }
        else
        {
            gameManager.PauseGame();
        }
    }
    
    void ShowGameOverScreen()
    {
        // 显示游戏结束界面
        Debug.Log("显示游戏结束界面");
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChange -= OnGameStateChange;
        }
    }
}
```

## 4. 预制体和组件

### 4.1 KuKu单位预制体
```csharp
using UnityEngine;
using UnityEngine.UI;

public class KukuUnit : MonoBehaviour
{
    [Header("KuKu数据")]
    public MythicalKukuData kukuData;
    public Image unitImage;
    public Slider healthBar;
    public TextMeshProUGUI nameText;
    
    [Header("战斗属性")]
    public float attackPower;
    public float defensePower;
    public float speed;
    public float health;
    public float maxHealth;
    public float attackRange;
    public float attackCooldown;
    
    private float attackTimer;
    private bool isAlive = true;
    private Animator animator;
    
    void Start()
    {
        InitializeUnit();
    }
    
    public void InitializeUnit()
    {
        if (kukuData != null)
        {
            // 设置基础属性
            attackPower = kukuData.AttackPower;
            defensePower = kukuData.DefensePower;
            speed = kukuData.Speed;
            health = kukuData.Health;
            maxHealth = kukuData.Health;
            
            // 设置UI
            if (nameText != null)
                nameText.text = kukuData.Name;
                
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = health;
            }
            
            // 设置稀有度颜色
            if (unitImage != null)
                unitImage.color = kukuData.GetRarityColor();
        }
        
        attackTimer = 0f;
        animator = GetComponent<Animator>();
        
        Debug.Log($"KuKu单位 {kukuData.Name} 初始化完成");
    }
    
    void Update()
    {
        if (isAlive)
        {
            UpdateCombatTimer();
        }
    }
    
    void UpdateCombatTimer()
    {
        attackTimer -= Time.deltaTime;
    }
    
    public bool IsAlive()
    {
        return isAlive;
    }
    
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        // 计算实际伤害（考虑防御力）
        float actualDamage = Mathf.Max(1f, damage - defensePower * 0.1f);
        health -= actualDamage;
        
        // 更新血条
        if (healthBar != null)
            healthBar.value = health;
        
        Debug.Log($"{kukuData.Name} 受到 {actualDamage:F1} 点伤害，剩余生命: {health:F1}");
        
        // 检查是否死亡
        if (health <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        if (!isAlive) return;
        
        health = Mathf.Min(maxHealth, health + healAmount);
        
        if (healthBar != null)
            healthBar.value = health;
            
        Debug.Log($"{kukuData.Name} 恢复 {healAmount:F1} 点生命值，当前生命: {health:F1}");
    }
    
    public bool CanAttack()
    {
        return attackTimer <= 0f;
    }
    
    public void Attack()
    {
        if (!CanAttack()) return;
        
        // 重置攻击计时器
        attackTimer = attackCooldown;
        
        // 执行攻击动画
        if (animator != null)
            animator.SetTrigger("Attack");
        
        Debug.Log($"{kukuData.Name} 发动攻击，攻击力: {attackPower}");
    }
    
    public float GetAttackPower()
    {
        return attackPower;
    }
    
    public float GetHealth()
    {
        return health;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? (health / maxHealth) * 100f : 0f;
    }
    
    public bool InAttackRange(Transform target)
    {
        if (target == null) return false;
        
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= attackRange;
    }
    
    void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        
        // 播放死亡动画
        if (animator != null)
            animator.SetTrigger("Die");
        
        Debug.Log($"{kukuData.Name} 已死亡");
        
        // 通知系统
        BattleSystem.Instance?.OnUnitDefeated(this);
        
        // 销毁对象
        Destroy(gameObject, 1f); // 等待死亡动画播放完毕
    }
    
    public void Revive()
    {
        if (isAlive) return;
        
        isAlive = true;
        health = maxHealth;
        
        if (healthBar != null)
            healthBar.value = health;
            
        Debug.Log($"{kukuData.Name} 复活");
    }
    
    public void LevelUp()
    {
        if (kukuData != null)
        {
            kukuData.Level++;
            maxHealth *= 1.2f;
            health = maxHealth;
            attackPower *= 1.15f;
            defensePower *= 1.15f;
            
            Debug.Log($"{kukuData.Name} 升级至 {kukuData.Level} 级");
            
            // 更新UI
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = health;
            }
        }
    }
    
    public void ApplyBuff(float attackMultiplier = 1f, float defenseMultiplier = 1f, float speedMultiplier = 1f)
    {
        attackPower *= attackMultiplier;
        defensePower *= defenseMultiplier;
        speed *= speedMultiplier;
        
        Debug.Log($"{kukuData.Name} 获得增益效果");
    }
    
    public void ApplyDebuff(float attackMultiplier = 1f, float defenseMultiplier = 1f, float speedMultiplier = 1f)
    {
        attackPower *= attackMultiplier;
        defensePower *= defenseMultiplier;
        speed *= speedMultiplier;
        
        Debug.Log($"{kukuData.Name} 获得减益效果");
    }
    
    void OnDestroy()
    {
        // 清理资源
        Debug.Log($"KuKu单位 {kukuData?.Name ?? "Unknown"} 已销毁");
    }
}
```

这个集成示例展示了如何将我们的KukuWorld游戏系统完整地集成到Unity框架中，包括：
1. 核心管理器系统
2. 事件管理
3. 音频管理
4. 场景管理
5. UI系统
6. 游戏对象管理

所有代码都已适配Unity的MonoBehaviour系统，并充分利用了Unity引擎的特性，如场景管理、UI系统、音频系统等。