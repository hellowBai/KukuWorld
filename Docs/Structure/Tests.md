# Tests 文件夹结构文档

## 概述
Tests 文件夹是项目的测试代码根目录，包含单元测试、集成测试等各类测试代码。

## 目录结构

### Unit/
- **用途**: 存放单元测试
- **子目录**:
  - Data/: 数据结构单元测试
  - Systems/: 系统单元测试
  - Controllers/: 控制器单元测试

### Integration/
- **用途**: 存放集成测试
- **子目录**:
  - Systems/: 系统集成测试
  - Features/: 功能集成测试
  - UI/: UI 集成测试

### Mocks/
- **用途**: 存放模拟对象
- **内容**:
  - MockPlayerData.cs: 模拟玩家数据
  - MockBattleSystem.cs: 模拟战斗系统
  - MockCaptureSystem.cs: 模拟捕捉系统

### Fixtures/
- **用途**: 存放测试固件
- **内容**:
  - TestPetData.cs: 测试用宠物数据
  - TestScenarios.cs: 测试场景

## 设计方案
- 测试代码与源代码分离，便于维护
- 遵循测试最佳实践，确保代码质量
- 包含多种测试类型，覆盖不同层面