using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KukuWorld;
using KukuWorld.Controllers;

public class KukuWorldGame : MonoBehaviour
{
    private MainGameController mainGameController;

    void Start()
    {
        Debug.Log("KukuWorld Game Starting...");
        
        // 初始化游戏
        InitializeGame();
    }

    void Update()
    {
        // 游戏主循环
    }

    private void InitializeGame()
    {
        // 创建主游戏控制器
        GameObject controllerObj = new GameObject("MainGameController");
        mainGameController = controllerObj.AddComponent<MainGameController>();
        
        // 初始化游戏
        mainGameController.Initialize();
        
        Debug.Log("KukuWorld Game Initialized Successfully!");
    }

    void OnDestroy()
    {
        // 清理资源
        if (mainGameController != null)
        {
            Destroy(mainGameController);
        }
    }
}