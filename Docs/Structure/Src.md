# Src 文件夹结构文档

## 概述
Src 文件夹是项目的源代码根目录，包含所有 C# 脚本文件，按照功能模块进行组织。

## 目录结构

### Data/
- **用途**: 存放数据结构类
- **内容**:
  - KuKuData.cs: 基础KuKu数据结构
  - MythicalKuKuData.cs: 神话KuKu数据结构
  - UnitData.cs: 单位数据结构
  - WeaponData.cs: 武器数据结构
  - BuildingData.cs: 建筑数据结构
  - HeroData.cs: 英雄数据结构
  - PlayerData.cs: 玩家数据结构

### Systems/
- **用途**: 存放游戏核心系统
- **内容**:
  - GameManager.cs: 游戏主管理器
  - BattleSystem.cs: 战斗系统
  - CaptureSystem.cs: 捕捉系统
  - NuwaDefenseSystem.cs: 女娲守护系统
  - PetCollectionManager.cs: KuKu收集管理器
  - EvolutionSystem.cs: 进化系统
  - FusionSystem.cs: 融合系统
  - EquipmentSystem.cs: 装备系统
  - BuildingManager.cs: 建筑管理器

### Controllers/
- **用途**: 存放游戏对象控制器
- **内容**:
  - EnemyController.cs: 敌人控制器
  - PetCombatController.cs: KuKu战斗控制器
  - BuildingController.cs: 建筑控制器
  - UnitController.cs: 单位控制器
  - WeaponController.cs: 武器控制器

### UI/
- **用途**: 存放用户界面相关脚本
- **内容**:
  - UIManager.cs: UI管理器
  - CaptureUI.cs: 捕捉界面
  - DefenseUI.cs: 防守界面
  - ShopUI.cs: 商店界面
  - PetCollectionUI.cs: KuKu收集界面

### Managers/
- **用途**: 存放各类管理器
- **内容**:
  - ResourceManager.cs: 资源管理器
  - AudioManager.cs: 音频管理器
  - SceneManager.cs: 场景管理器
  - SaveManager.cs: 存档管理器

## 设计方案
- 采用分层架构设计，分离数据、逻辑和表现层
- 遵循 SOLID 原则，提高代码可维护性
- 模块化设计，便于单元测试和功能扩展