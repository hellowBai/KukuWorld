using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("游戏设置")]
    public float capturePhaseDuration = 300f; // 5分钟
    public int initialPlayerLives = 10;
    
    [Header("系统引用")]
    public CaptureSystem captureSystem;
    public BattleSystem battleSystem;
    public EvolutionSystem evolutionSystem;
    public FusionSystem fusionSystem;
    public EquipmentSystem equipmentSystem;
    public BuildingManager buildingManager;
    public KukuCollectionSystem collectionSystem;
    
    public enum GameState
    {
        MainMenu,
        CapturePhase,
        DefensePhaseSetup,
        DefensePhase,
        GameOver,
        Paused
    }
    
    public GameState CurrentState { get; private set; }
    public float CapturePhaseTimeRemaining { get; private set; }
    
    // 事件
    public System.Action<GameState> OnGameStateChange;
    public System.Action<bool> OnGameOver;
    
    private PlayerData playerData;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeGame()
    {
        // 初始化玩家数据
        playerData = new PlayerData();
        playerData.PlayerName = "KuKu Hunter";
        playerData.Level = 1;
        playerData.Coins = 1000;
        playerData.Gems = 100;
        playerData.Souls = 0;
        
        // 初始化系统
        InitializeSystems();
        
        // 设置初始状态
        CurrentState = GameState.MainMenu;
        CapturePhaseTimeRemaining = capturePhaseDuration;
    }
    
    void InitializeSystems()
    {
        // 创建系统对象
        GameObject systemsObj = new GameObject("GameSystems");
        
        captureSystem = systemsObj.AddComponent<CaptureSystem>();
        battleSystem = systemsObj.AddComponent<BattleSystem>();
        evolutionSystem = systemsObj.AddComponent<EvolutionSystem>();
        fusionSystem = systemsObj.AddComponent<FusionSystem>();
        equipmentSystem = systemsObj.AddComponent<EquipmentSystem>();
        buildingManager = systemsObj.AddComponent<BuildingManager>();
        collectionSystem = systemsObj.AddComponent<KukuCollectionSystem>();
        
        // 初始化系统
        captureSystem.Initialize();
        battleSystem.Initialize();
        evolutionSystem.Initialize();
        fusionSystem.Initialize();
        equipmentSystem.Initialize();
        buildingManager.Initialize();
        collectionSystem.Initialize();
    }
    
    void Update()
    {
        if (CurrentState == GameState.CapturePhase)
        {
            UpdateCapturePhase();
        }
    }
    
    void UpdateCapturePhase()
    {
        CapturePhaseTimeRemaining -= Time.deltaTime;
        
        if (CapturePhaseTimeRemaining <= 0)
        {
            EndCapturePhase();
        }
    }
    
    public void StartNewGame()
    {
        ChangeState(GameState.CapturePhase);
        CapturePhaseTimeRemaining = capturePhaseDuration;
        
        // 初始化捕捉阶段
        captureSystem.StartCapturePhase();
    }
    
    public void StartDefensePhase()
    {
        ChangeState(GameState.DefensePhaseSetup);
        
        // 设置战斗场景
        battleSystem.SetupDefensePhase(playerData);
        
        // 切换到战斗状态
        Invoke(nameof(EnterDefensePhase), 1f); // 给予1秒准备时间
    }
    
    void EnterDefensePhase()
    {
        ChangeState(GameState.DefensePhase);
        battleSystem.StartDefense();
    }
    
    void EndCapturePhase()
    {
        // 保存捕捉阶段的结果
        captureSystem.EndCapturePhase();
        
        // 进入防守阶段
        StartDefensePhase();
    }
    
    public void EndGame(bool isVictory)
    {
        ChangeState(GameState.GameOver);
        OnGameOver?.Invoke(isVictory);
        
        Debug.Log(isVictory ? "游戏胜利！" : "游戏失败！");
    }
    
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        GameState oldState = CurrentState;
        CurrentState = newState;
        
        Debug.Log($"游戏状态改变: {oldState} -> {newState}");
        
        OnGameStateChange?.Invoke(newState);
        
        // 根据状态执行特定逻辑
        OnStateChanged(newState, oldState);
    }
    
    void OnStateChanged(GameState newState, GameState oldState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.CapturePhase:
                HandleCapturePhaseState();
                break;
            case GameState.DefensePhase:
                HandleDefensePhaseState();
                break;
            case GameState.GameOver:
                HandleGameOverState();
                break;
        }
    }
    
    void HandleMainMenuState()
    {
        // 主菜单逻辑
    }
    
    void HandleCapturePhaseState()
    {
        // 捕捉阶段逻辑
    }
    
    void HandleDefensePhaseState()
    {
        // 防守阶段逻辑
    }
    
    void HandleGameOverState()
    {
        // 游戏结束逻辑
    }
    
    public float GetCapturePhaseRemainingTime()
    {
        return CapturePhaseTimeRemaining;
    }
    
    public PlayerData GetPlayerData()
    {
        return playerData;
    }
    
    public void AddPlayerSouls(float souls)
    {
        if (playerData != null)
        {
            playerData.AddSouls(souls);
        }
    }
    
    public void AddPlayerCoins(int coins)
    {
        if (playerData != null)
        {
            playerData.AddCoins(coins);
        }
    }
    
    public void AddPlayerGems(int gems)
    {
        if (playerData != null)
        {
            playerData.AddGems(gems);
        }
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public void PauseGame()
    {
        if (CurrentState != GameState.GameOver && CurrentState != GameState.MainMenu)
        {
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;
        }
    }
    
    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeState(GetPreviousState());
            Time.timeScale = 1f;
        }
    }
    
    GameState GetPreviousState()
    {
        // 这里需要保存之前的状态
        return GameState.CapturePhase; // 简化实现
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}