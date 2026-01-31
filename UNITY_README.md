# KukuWorld Unity项目结构

## 项目概述

KukuWorld是一款结合了Kuku收集、塔防战斗、建造升级、融合进化的RPG塔防游戏。本项目已成功迁移到Unity框架，实现了完整的功能和优化的性能。

## Unity项目结构

```
KukuWorld/
├── Assets/                 # Unity资源目录
│   ├── Scripts/            # 游戏脚本
│   │   ├── Core/           # 核心系统
│   │   │   └── GameManager.cs
│   │   ├── Data/           # 数据模型
│   │   │   ├── MythicalKukuData.cs
│   │   │   ├── PlayerData.cs
│   │   │   ├── WeaponData.cs
│   │   │   └── BuildingData.cs
│   │   ├── Systems/        # 游戏系统
│   │   │   ├── CaptureSystem.cs
│   │   │   ├── BattleSystem.cs
│   │   │   ├── EvolutionSystem.cs
│   │   │   ├── FusionSystem.cs
│   │   │   ├── EquipmentSystem.cs
│   │   │   ├── BuildingManager.cs
│   │   │   └── KukuCollectionSystem.cs
│   │   ├── UI/             # 用户界面
│   │   │   └── KukuUIManager.cs
│   │   ├── Controllers/    # 游戏控制器
│   │   │   └── EnemyController.cs
│   │   └── Utils/          # 工具类
│   ├── Prefabs/            # 预制体
│   │   ├── Units/
│   │   ├── UI/
│   │   └── Managers/
│   ├── Scenes/             # Unity场景
│   ├── Resources/          # Unity资源
│   └── Materials/          # 材质
├── Documentation/          # 项目文档
└── UNITY_README.md         # 本文件
```

## 核心系统说明

### 1. GameManager (Assets/Scripts/Core/GameManager.cs)
- 游戏状态管理
- 生命周期管理
- 系统协调

### 2. 数据模型 (Assets/Scripts/Data/)
- MythicalKukuData: 神话KuKu数据模型
- PlayerData: 玩家数据模型
- WeaponData: 武器数据模型
- BuildingData: 建筑数据模型

### 3. 游戏系统 (Assets/Scripts/Systems/)
- CaptureSystem: 捕捉系统
- BattleSystem: 战斗系统
- EvolutionSystem: 进化系统
- FusionSystem: 融合系统
- EquipmentSystem: 装备系统
- BuildingManager: 建筑管理系统
- KukuCollectionSystem: KuKu收集系统

### 4. UI系统 (Assets/Scripts/UI/KukuUIManager.cs)
- 游戏界面管理
- 状态显示
- 交互处理

### 5. 控制器 (Assets/Scripts/Controllers/)
- EnemyController: 敌人控制器

## Unity集成特性

1. **场景管理**: 使用Unity的SceneManager
2. **UI系统**: 使用Unity的UGUI系统
3. **生命周期**: 使用MonoBehaviour生命周期
4. **物理系统**: 可使用Unity物理引擎
5. **资源管理**: 使用Unity的资源加载系统
6. **音频系统**: 可使用Unity音频系统

## 开发说明

### 环境要求
- Unity 2022.3 LTS 或更高版本
- C# 7.0 或更高版本

### 开始开发
1. 打开Unity项目
2. 导航到 Assets/Scenes/ 加载主场景
3. 游戏逻辑主要在 Assets/Scripts/ 目录下

### 脚本规范
- 所有脚本继承MonoBehaviour
- 使用Unity的序列化系统
- 遵循Unity的生命周期方法
- 使用Unity的事件系统

## 项目特点

1. **模块化设计**: 各系统相互独立，易于维护
2. **数据驱动**: 游戏逻辑与数据分离
3. **扩展性强**: 易于添加新功能
4. **性能优化**: 适合移动平台运行

## 运行项目

1. 在Unity中打开项目
2. 加载主场景 (Assets/Scenes/MainScene.unity)
3. 点击播放按钮运行游戏
4. 或构建为对应平台的可执行文件

## 贡献

欢迎提交PR或Issue来改进项目。遵循以下规范：
- 代码风格统一
- 注释清晰
- 功能模块化
- 遵循Unity最佳实践