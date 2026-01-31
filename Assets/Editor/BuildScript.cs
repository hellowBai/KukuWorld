using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

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
        BuildReport report = BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.Android,
            BuildOptions.None
        );
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    [MenuItem("Custom/Build Windows")]
    public static void PerformWindowsBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), GetProjectName() + ".exe");
        BuildReport report = BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.StandaloneWindows64,
            BuildOptions.None
        );
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    [MenuItem("Custom/Build WebGL")]
    public static void PerformWebGLBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), "WebGLBuild");
        BuildReport report = BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.WebGL,
            BuildOptions.None
        );
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    [MenuItem("Custom/Build MacOS")]
    public static void PerformMacOSBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), GetProjectName() + ".app");
        BuildReport report = BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.StandaloneOSX,
            BuildOptions.None
        );
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    [MenuItem("Custom/Build Linux")]
    public static void PerformLinuxBuild()
    {
        string buildPath = Path.Combine(GetBuildPath(), GetProjectName() + ".x86_64");
        BuildReport report = BuildPipeline.BuildPlayer(
            GetScenePaths(),
            buildPath,
            BuildTarget.StandaloneLinux64,
            BuildOptions.None
        );
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    private static string[] GetScenePaths()
    {
        // 查找Assets/Scenes目录下的所有场景文件
        string[] scenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
            .Select guid => AssetDatabase.GUIDToAssetPath(guid)
            .ToArray();
        
        if (scenes.Length == 0)
        {
            // 如果没找到场景，使用默认场景
            scenes = new[] { "Assets/Scenes/MainMenu.unity" };
            Debug.LogWarning("No scenes found, using default scene path.");
        }
        
        return scenes;
    }
}