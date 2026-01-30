# Assets 文件夹结构文档

## 概述
Assets 文件夹是 Unity 项目的资源根目录，包含游戏中所有的资源文件，包括脚本、美术资源、音效、预制体等。

## 目录结构

### Scripts/
- **用途**: 存放所有的 C# 脚本文件
- **子目录**:
  - Core/: 核心系统脚本
  - Managers/: 管理器脚本
  - Controllers/: 控制器脚本
  - UI/: 用户界面脚本
  - Data/: 数据结构脚本
  - Systems/: 游戏系统脚本

### Prefabs/
- **用途**: 存放预制体文件
- **子目录**:
  - Characters/: 角色预制体
  - Enemies/: 敌人预制体
  - Pets/: KuKu预制体
  - Buildings/: 建筑预制体
  - Items/: 道具预制体
  - Effects/: 特效预制体

### Sprites/
- **用途**: 存放精灵图片资源
- **子目录**:
  - Characters/: 角色精灵
  - UI/: UI 精灵
  - Environment/: 环境精灵
  - Icons/: 图标精灵
  - Pets/: KuKu精灵

### Audio/
- **用途**: 存放音频文件
- **子目录**:
  - Music/: 背景音乐
  - SFX/: 音效
  - Voice/: 语音

### Scenes/
- **用途**: 存放场景文件
- **子目录**:
  - Main/: 主要场景
  - Levels/: 关卡场景
  - UI/: UI 场景

### Materials/
- **用途**: 存放材质球文件

### Shaders/
- **用途**: 存放着色器文件

### Animations/
- **用途**: 存放动画文件

### Fonts/
- **用途**: 存放字体文件

## 设计方案
- 采用模块化组织结构，便于团队协作开发
- 遵循 Unity 最佳实践，确保资源加载效率
- 便于资源查找和管理