# Unity KukuWorld 项目编译说明

## 项目结构分析
目前的KukuWorld项目似乎是一个Unity项目，但缺少Unity项目的一些标准结构（如Assets、ProjectSettings等在正确的位置）。

## 编译步骤

### 1. 检查项目完整性
- 确保Assets, ProjectSettings, Packages, UserSettings等Unity标准目录存在
- 检查Assembly-CSharp.csproj是否正确生成

### 2. Unity 命令行编译选项

如果您的系统中安装了Unity编辑器，可以通过以下命令编译：

#### Windows:
```
Unity.exe -batchmode -quit -projectPath "C:\path\to\KukuWorld" -executeMethod BuildScript.PerformAndroidBuild
```

#### Linux/Mac:
```
/Applications/Unity/Hub/Editor/2022.3.22f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath "/path/to/KukuWorld" -executeMethod BuildScript.PerformAndroidBuild
```

### 3. Unity 构建脚本示例 (Assets/Editor/BuildScript.cs)

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

### 4. Docker 方式（需要Unity许可证）

```bash
# 构建Docker镜像
docker build -t kukuworld-builder .

# 运行构建（需要挂载许可证）
docker run -it --rm -v $(pwd):/project kukuworld-builder \
  /Unity/Editor/Unity -batchmode -quit \
  -projectPath /project \
  -executeMethod BuildScript.PerformWindowsBuild
```

### 5. 项目结构调整建议

当前项目结构需要调整为标准Unity项目结构：

```
KukuWorld/
├── Assets/
│   ├── Scripts/          # 您的Src目录内容应放在这里
│   ├── Scenes/           # Unity场景
│   ├── Prefabs/          # 预制件
│   ├── Resources/        # 资源
│   └── ...
├── ProjectSettings/      # Unity项目设置
├── Packages/            # 包管理器清单
├── UserSettings/        # 用户设置
└── Library/             # Unity生成的库文件（不应提交到版本控制）
```

### 6. 依赖项

确保安装必要的Unity包：
- UnityEngine.CoreModule
- UnityEngine.UIElementsModule
- UnityEngine.PhysicsModule
- UnityEngine.AudioModule

### 7. 编译前准备

1. 将Src目录中的脚本移动到Assets/Scripts/
2. 确保有至少一个场景文件(.unity)在Assets/Scenes/
3. 确保ProjectSettings文件夹包含必需的设置文件
4. 检查Assembly-CSharp.csproj文件是否正确引用了所有脚本

注意：Unity编译需要有效的Unity许可证，对于商业用途需要购买Unity Pro或Unity Plus许可证。