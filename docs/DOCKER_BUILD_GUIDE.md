# KukuWorld Unity 项目 Docker 编译指南

## 概述
本指南介绍如何使用Docker容器来编译KukuWorld Unity项目。这允许您在隔离环境中进行构建，无需在本地安装Unity编辑器。

## 前提条件
- Docker 已安装并运行
- 至少 8GB 可用磁盘空间
- 稳定的互联网连接（用于下载Unity镜像）

## 使用方法

### 1. 自动构建脚本
运行提供的自动构建脚本：

```bash
cd KukuWorld
./build_with_official_image.sh
```

### 2. 手动构建
如果您想手动控制构建过程：

```bash
# 拉取Unity官方镜像
docker pull unityci/editor:2022.3.22f1-base-3.0.2

# 运行构建（Windows示例）
docker run --rm \
  -v $(pwd):/project \
  -v $(pwd)/Builds:/builds \
  -w /project \
  unityci/editor:2022.3.22f1-base-3.0.2 \
  /opt/unity/Editor/Unity \
  -batchmode \
  -quit \
  -projectPath /project \
  -executeMethod BuildScript.PerformWindowsBuild \
  -logFile -
```

### 3. 支持的构建目标
- Android (.apk)
- Windows (.exe)
- WebGL (Web Build)
- macOS (.app)
- Linux (.x86_64)

## Dockerfile 说明
当前的Dockerfile使用Unity官方的CI/CD镜像，这些镜像是：
- 预配置了Unity编辑器和必要的依赖
- 专为CI/CD环境优化
- 包含了IL2CPP构建支持

## 项目结构
确保您的项目具有以下标准Unity结构：
```
KukuWorld/
├── Assets/
│   ├── Scripts/          # 游戏脚本
│   ├── Scenes/           # Unity场景
│   ├── Prefabs/          # 预制件
│   ├── Resources/        # 资源
│   └── Editor/           # 编辑器脚本（包含BuildScript.cs）
├── ProjectSettings/      # 项目设置
├── Packages/            # 包管理器清单
└── Builds/              # 构建输出目录（自动生成）
```

## 构建脚本
BuildScript.cs 文件提供了多种构建目标的自动化构建方法：
- `BuildScript.PerformWindowsBuild()` - 构建Windows版本
- `BuildScript.PerformAndroidBuild()` - 构建Android版本
- `BuildScript.PerformWebGLBuild()` - 构建WebGL版本
- `BuildScript.PerformMacOSBuild()` - 构建macOS版本
- `BuildScript.PerformLinuxBuild()` - 构建Linux版本

## 故障排除

### 常见问题
1. **Docker内存不足**：Unity构建过程需要大量内存，请确保Docker至少分配8GB内存
2. **权限问题**：确保Docker可以访问项目目录
3. **网络问题**：首次运行需要下载大型镜像，请保持网络连接稳定

### 检查构建日志
构建日志会输出到控制台，如果构建失败，请仔细查看错误信息。

## 自定义构建
您可以根据需要修改Dockerfile或构建脚本来适应不同的Unity版本或构建需求。

## 注意事项
- Unity官方镜像需要遵守Unity的许可协议
- 构建过程可能需要10-30分钟，具体取决于项目大小和目标平台
- 某些平台可能需要额外的SDK或工具链（如Android SDK）