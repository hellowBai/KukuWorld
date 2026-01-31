# KukuWorld 框架选择和集成文档

## 1. 框架选择概述

KukuWorld 游戏可以在多个成熟的游戏框架上开发。以下是几种主流选择：

## 2. Unity 框架

### 2.1 Unity 优势
- 强大的生态系统
- 丰富的资源和插件
- 跨平台支持
- 优秀的2D/3D渲染引擎
- 成熟的UI系统

### 2.2 在Unity中集成KukuWorld功能

#### 2.2.1 项目结构
```
Assets/
├── Scripts/                 # 游戏逻辑脚本
│   ├── Core/               # 核心系统
│   ├── Data/               # 数据模型
│   ├── Systems/            # 游戏系统
│   ├── UI/                 # 用户界面
│   └── Utils/              # 工具类
├── Prefabs/                # 预制体
├── Scenes/                 # 场景
├── Resources/              # 资源文件
├── Sprites/                # 精灵图像
└── Audio/                  # 音频文件
```

#### 2.2.2 核心集成点

##### GameManager 集成
```csharp
using UnityEngine;
using System.Collections;

public class KukuWorldGameManager : MonoBehaviour
{
    public static KukuWorldGameManager Instance { get; private set; }
    
    [Header("游戏设置")]
    public float capturePhaseDuration = 300f; // 5分钟捕捉时间
    public int initialPlayerLives = 10;
    
    [Header("系统引用")]
    public CaptureSystem captureSystem;
    public BattleSystem battleSystem;
    public EvolutionSystem evolutionSystem;
    
    public enum GameState
    {
        MainMenu,
        CapturePhase,
        DefensePhase,
        GameOver
    }
    
    public GameState CurrentState { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeSystems()
    {
        // 初始化各个系统
        captureSystem = gameObject.AddComponent<CaptureSystem>();
        battleSystem = gameObject.AddComponent<BattleSystem>();
        evolutionSystem = gameObject.AddComponent<EvolutionSystem>();
    }
    
    public void StartGame()
    {
        CurrentState = GameState.CapturePhase;
        StartCoroutine(CapturePhaseRoutine());
    }
    
    IEnumerator CapturePhaseRoutine()
    {
        float timer = capturePhaseDuration;
        
        while (timer > 0 && CurrentState == GameState.CapturePhase)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        
        if (CurrentState == GameState.CapturePhase)
        {
            StartDefensePhase();
        }
    }
    
    public void StartDefensePhase()
    {
        CurrentState = GameState.DefensePhase;
        battleSystem.StartDefense();
    }
    
    public void GameOver(bool isVictory)
    {
        CurrentState = GameState.GameOver;
        // 处理游戏结束逻辑
    }
}
```

##### 捕捉系统集成
```csharp
using UnityEngine;
using System.Collections.Generic;

public class KukuCaptureSystem : MonoBehaviour
{
    [Header("捕捉设置")]
    public float spawnInterval = 5f;
    public int maxWildKukus = 10;
    public Transform[] spawnPoints;
    
    private List<GameObject> wildKukus = new List<GameObject>();
    private float lastSpawnTime = 0f;
    
    void Update()
    {
        if (KukuWorldGameManager.Instance.CurrentState == KukuWorldGameManager.GameState.CapturePhase)
        {
            SpawnWildKukus();
        }
    }
    
    void SpawnWildKukus()
    {
        if (Time.time - lastSpawnTime >= spawnInterval && wildKukus.Count < maxWildKukus)
        {
            if (spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                
                // 在Unity中实例化野生KuKu预制体
                GameObject wildKuku = new GameObject("WildKuku");
                wildKuku.transform.position = spawnPoint.position;
                
                // 添加KuKu组件
                var kukuData = wildKuku.AddComponent<KukuDataComponent>();
                kukuData.InitializeAsWildKuku();
                
                // 添加捕捉控制器
                var captureCtrl = wildKuku.AddComponent<KukuCaptureController>();
                
                wildKukus.Add(wildKuku);
                lastSpawnTime = Time.time;
            }
        }
    }
    
    public void RemoveWildKuku(GameObject kuku)
    {
        wildKukus.Remove(kuku);
        Destroy(kuku);
    }
}
```

##### 战斗系统集成
```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KukuBattleSystem : MonoBehaviour
{
    [Header("战斗设置")]
    public Transform[] enemySpawnPoints;
    public Transform[] defenseTargets;
    public GameObject enemyPrefab;
    
    [Header("波次设置")]
    public float timeBetweenWaves = 10f;
    public int enemiesPerWave = 5;
    
    private int currentWave = 0;
    private int playerLives;
    private bool isWaveActive = false;
    
    public void StartDefense()
    {
        playerLives = KukuWorldGameManager.Instance.initialPlayerLives;
        StartCoroutine(WaveSpawningRoutine());
    }
    
    IEnumerator WaveSpawningRoutine()
    {
        while (KukuWorldGameManager.Instance.CurrentState == KukuWorldGameManager.GameState.DefensePhase)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            
            if (KukuWorldGameManager.Instance.CurrentState == KukuWorldGameManager.GameState.DefensePhase)
            {
                currentWave++;
                SpawnWave();
            }
        }
    }
    
    void SpawnWave()
    {
        isWaveActive = true;
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(1f); // 间隔生成敌人
        }
        
        isWaveActive = false;
    }
    
    void SpawnEnemy()
    {
        if (enemySpawnPoints.Length > 0)
        {
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            var enemyCtrl = enemy.GetComponent<EnemyController>();
            
            if (enemyCtrl != null)
            {
                enemyCtrl.Initialize(currentWave, defenseTargets);
            }
        }
    }
    
    public void EnemyReachedTarget()
    {
        playerLives--;
        
        if (playerLives <= 0)
        {
            KukuWorldGameManager.Instance.GameOver(false);
        }
    }
    
    public void EnemyDefeated()
    {
        // 处理敌人被击败的逻辑
        // 给予玩家奖励
    }
}
```

## 3. Godot 框架

### 3.1 Godot 集成示例

#### 3.1.1 主场景脚本
```gdscript
extends Node

# KukuWorld 游戏主控制器
var capture_phase_duration = 300  # 5分钟
var current_state = "MENU"
var player_lives = 10

func _ready():
    initialize_systems()

func initialize_systems():
    # 初始化游戏系统
    var capture_system = preload("res://systems/CaptureSystem.tscn").instance()
    var battle_system = preload("res://systems/BattleSystem.tscn").instance()
    
    add_child(capture_system)
    add_child(battle_system)

func start_game():
    current_state = "CAPTURE"
    start_capture_phase()

func start_capture_phase():
    print("开始捕捉阶段")
    yield(get_tree().create_timer(capture_phase_duration), "timeout")
    
    if current_state == "CAPTURE":
        start_defense_phase()

func start_defense_phase():
    current_state = "DEFENSE"
    print("开始防守阶段")
    # 启动战斗系统
```

## 4. Unreal Engine 框架

### 4.1 UE5 集成示例

#### 4.1.1 主游戏模式
```cpp
// KukuWorldGameMode.h
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "KukuWorldGameMode.generated.h"

UENUM(BlueprintType)
enum class EKukuWorldGameState : uint8
{
    MainMenu,
    CapturePhase,
    DefensePhase,
    GameOver
};

UCLASS()
class KUKUWORLD_API AKukuWorldGameMode : public AGameModeBase
{
    GENERATED_BODY()

public:
    AKukuWorldGameMode();

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Game Settings")
    float CapturePhaseDuration = 300.0f; // 5分钟

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Game Settings")
    int32 InitialPlayerLives = 10;

    UFUNCTION(BlueprintCallable, Category = "Game State")
    void StartGame();

    UFUNCTION(BlueprintCallable, Category = "Game State")
    void StartCapturePhase();

    UFUNCTION(BlueprintCallable, Category = "Game State")
    void StartDefensePhase();

    UPROPERTY(BlueprintReadOnly, Category = "Game State")
    EKukuWorldGameState CurrentGameState = EKukuWorldGameState::MainMenu;

protected:
    virtual void BeginPlay() override;

private:
    float RemainingCaptureTime;
    FTickerDelegate CapturePhaseTicker;
    FDelegateHandle CapturePhaseTickerHandle;
};
```

## 5. 选择建议

### 5.1 Unity (推荐)
- **适用场景**: 2D/2.5D塔防游戏
- **优势**: 
  - 成熟的2D工具集
  - 丰富的UI系统
  - 强大的社区支持
  - 跨平台发布便捷
- **集成难度**: 中等

### 5.2 Godot
- **适用场景**: 轻量级2D游戏
- **优势**:
  - 免费开源
  - 轻量级
  - GDScript易学易用
- **集成难度**: 中等

### 5.3 Unreal Engine
- **适用场景**: 3D高质量游戏
- **优势**:
  - 强大的渲染能力
  - 蓝图可视化编程
  - 高质量输出
- **集成难度**: 较高

## 6. Unity 集成步骤

### 6.1 项目设置
1. 创建新的Unity 2D项目
2. 设置项目结构
3. 导入必要的包

### 6.2 系统集成
1. 创建核心管理器
2. 实现捕捉系统
3. 实现战斗系统
4. 实现进化和融合系统
5. 实现UI系统

### 6.3 资源管理
1. 创建资源文件夹结构
2. 配置资源加载
3. 实现对象池系统

## 7. 迁移路径

如果我们将现有的代码迁移到Unity框架上，需要进行以下步骤：

1. **脚本适配**: 将现有C#代码适配为Unity MonoBehaviour
2. **场景管理**: 创建Unity场景并配置游戏对象
3. **UI系统**: 使用Unity的UI系统替代自定义UI
4. **资源管理**: 使用Unity的资源管理系统
5. **输入处理**: 使用Unity的输入系统

这种迁移方式可以保留我们现有的游戏逻辑，同时利用Unity强大的引擎功能。