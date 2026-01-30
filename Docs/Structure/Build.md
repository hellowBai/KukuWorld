# Build 文件夹结构文档

## 概述
Build 文件夹是项目的构建输出目录，包含编译后的可执行文件和发布包。

## 目录结构

### Windows/
- **用途**: Windows 平台构建输出
- **内容**:
  - x86/: 32位构建
  - x64/: 64位构建
  - Installer/: 安装包

### Android/
- **用途**: Android 平台构建输出
- **内容**:
  - APK/: APK 文件
  - AAB/: AAB 文件
  - Obb/: 扩展文件

### iOS/
- **用途**: iOS 平台构建输出
- **内容**:
  - IPA/: IPA 文件
  - XcodeProject/: Xcode 项目

### WebGL/
- **用途**: WebGL 平台构建输出
- **内容**:
  - Build/: 构建文件
  - TemplateData/: 模板数据

### Linux/
- **用途**: Linux 平台构建输出
- **内容**:
  - x86/: 32位构建
  - x64/: 64位构建

### macOS/
- **用途**: macOS 平台构建输出
- **内容**:
  - AppBundle/: 应用程序包
  - DMG/: 磁盘镜像

## 设计方案
- 按平台分类构建输出，便于发布管理
- 包含多种格式，满足不同需求
- 自动清理旧构建，节省空间