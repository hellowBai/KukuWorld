# KukuWorld Unity 项目综合构建指南

## 概述
本指南提供完整的KukuWorld Unity项目构建说明，包括本地构建和Docker构建方式。

## 项目结构分析
目前的KukuWorld项目是一个Unity项目，但在构建前需要确保具备标准的Unity项目结构。

### 标准Unity项目结构
```
KukuWorld/
├── Assets/
│   ├── Scripts/          # 游戏脚本（来自Src目录）
│   ├── Scenes/           # Unity场景
│   ├── Prefabs/          # 预制件
│   ├── Resources/        # 资源
│   ├── Editor/           # 编辑器脚本（包含BuildScript.cs）
│   └── ...
├── ProjectSettings/      # Unity项目设置
├── Packages/            # 包管理器清单
├── UserSettings/        # 用户设置
└── Builds/              # 构建输出目录
```

## 本地Unity构建

### 前提条件
- 安装Unity Hub
- 安装Unity 2022.3.22f1或兼容版本
- 有效的Unity许可证

### 编译步骤

#### 1. 检查项目完整性
- 确保Assets, ProjectSettings, Packages, UserSettings等Unity标准目录存在
- 检查Assembly-CSharp.csproj是否正确生成
- 将Src目录中的脚本移动到Assets/Scripts/

#### 2. Unity 命令行编译选项

如果您的系统中安装了Unity编辑器，可以通过以下命令编译：

##### Windows:
```
Unity.exe -batchmode -quit -projectPath "C:\path\to\KukuWorld" -executeMethod BuildScript.PerformAndroidBuild
```

##### Linux/Mac:
```
/Applications/Unity/Hub/Editor/2022.3.22f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath "/path/to/KukuWorld" -executeMethod BuildScript.PerformAndroidBuild
```

#### 3. Unity 构建脚本示例 (Assets/Editor/BuildScript.cs)

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    private static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    private static string GetBuildPath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "Builds");
    }

    [MenuItem("Custom/Build Android")]
    public static void PerformAndroidBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), GetProjectName() + ".apk");
        BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.Android,
            BuildOptions.None
        );
    }

    [MenuItem("Custom/Build iOS")]
    public static void PerformIOSBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), GetProjectName());
        BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.iOS,
            BuildOptions.None
        );
    }

    [MenuItem("Custom/Build Windows")]
    public static void PerformWindowsBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), GetProjectName() + ".exe");
        BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.StandaloneWindows64,
            BuildOptions.None
        );
    }

    [MenuItem("Custom/Build WebGL")]
    public static void PerformWebGLBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), "WebGLBuild");
        BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.WebGL,
            BuildOptions.None
        );
    }

    private static string[] GetScenePaths()
    {
        string[] scenes = new string[1];
        scenes[0] = "Assets/Scenes/MainMenu.unity";  // 根据实际场景路径调整
        return scenes;
    }
}
```

#### 4. 依赖项

确保安装必要的Unity包：
- UnityEngine.CoreModule
- UnityEngine.UIElementsModule
- UnityEngine.PhysicsModule
- UnityEngine.AudioModule

#### 5. 编译前准备

1. 将Src目录中的脚本移动到Assets/Scripts/
2. 确保有至少一个场景文件(.unity)在Assets/Scenes/
3. 确保ProjectSettings文件夹包含必需的设置文件
4. 检查Assembly-CSharp.csproj文件是否正确引用了所有脚本

## Docker构建方式

### 前提条件
- Docker 已安装并运行
- 至少 8GB 可用磁盘空间
- 稳定的互联网连接（用于下载Unity镜像）

### 使用方法

#### 1. 自动构建脚本
运行提供的自动构建脚本：

```bash
cd KukuWorld
./build_with_official_image.sh
```

#### 2. 手动构建
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

#### 3. 支持的构建目标
- Android (.apk)
- Windows (.exe)
- WebGL (Web Build)
- macOS (.app)
- Linux (.x86_64)

#### 4. Dockerfile 说明
当前的Dockerfile使用Unity官方的CI/CD镜像，这些镜像是：
- 预配置了Unity编辑器和必要的依赖
- 专为CI/CD环境优化
- 包含了IL2CPP构建支持

## 故障排除

### 常见问题
1. **Docker内存不足**：Unity构建过程需要大量内存，请确保Docker至少分配8GB内存
2. **权限问题**：确保Docker可以访问项目目录
3. **网络问题**：首次运行需要下载大型镜像，请保持网络连接稳定
4. **Unity许可证**：Unity构建需要有效的许可证，商业用途需购买Unity Pro或Unity Plus

### 检查构建日志
构建日志会输出到控制台，如果构建失败，请仔细查看错误信息。

## 自定义构建
您可以根据需要修改Dockerfile或构建脚本来适应不同的Unity版本或构建需求。

## 注意事项
- Unity官方镜像需要遵守Unity的许可协议
- 构建过程可能需要10-30分钟，具体取决于项目大小和目标平台
- 某些平台可能需要额外的SDK或工具链（如Android SDK）
- 注意Unity编译需要有效的Unity许可证，对于商业用途需要购买Unity Pro或Unity Plus许可证。