using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KukuWorld.Data;
using KukuWorld.Systems;

namespace KukuWorld.UI
{
    /// <summary>
    /// 融合界面控制器 - 管理KuKu与机器人融合界面
    /// </summary>
    public class FusionUIController : MonoBehaviour
    {
        [Header("融合面板")]
        public GameObject fusionPanel;
        public Button closeFusionButton;
        public Button startFusionButton;
        
        [Header("KuKu选择")]
        public Transform kukuSelectionPanel;
        public GameObject kukuCardPrefab;
        
        [Header("机器人选择")]
        public Transform robotSelectionPanel;
        public GameObject robotCardPrefab;
        
        [Header("融合结果")]
        public TextMeshProUGUI fusionResultText;
        public Image fusionResultImage;
        public TextMeshProUGUI fusionResultDescription;
        
        [Header("融合要求显示")]
        public TextMeshProUGUI fusionRequirementsText;
        public TextMeshProUGUI playerResourcesText;
        
        [Header("选择状态")]
        public TextMeshProUGUI selectedKukuText;
        public TextMeshProUGUI selectedRobotText;
        
        // 选择的KuKu和机器人
        private MythicalKukuData selectedKuku;
        private UnitData selectedRobot;
        
        // 系统引用
        private GameManager gameManager;
        
        // 事件订阅
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// 初始化融合界面
        /// </summary>
        public void Initialize()
        {
            // 获取系统引用
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManager实例未找到！");
                return;
            }
            
            // 初始化按钮事件
            InitializeButtonEvents();
            
            // 初始化显示
            UpdatePlayerResources();
            
            Debug.Log("融合界面控制器初始化完成");
        }
        
        /// <summary>
        /// 初始化按钮事件
        /// </summary>
        private void InitializeButtonEvents()
        {
            if (closeFusionButton != null)
                closeFusionButton.onClick.AddListener(CloseFusionPanel);
                
            if (startFusionButton != null)
                startFusionButton.onClick.AddListener(PerformFusion);
        }
        
        /// <summary>
        /// 订阅系统事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChange += OnGameStateChange;
            }
        }
        
        /// <summary>
        /// 取消订阅系统事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChange -= OnGameStateChange;
            }
        }
        
        /// <summary>
        /// 游戏状态改变事件处理
        /// </summary>
        private void OnGameStateChange(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.Shop) // 假设融合功能在商店中
            {
                OpenFusionPanel();
            }
            else if (newState != GameManager.GameState.Shop)
            {
                CloseFusionPanel();
            }
        }
        
        /// <summary>
        /// 打开融合面板
        /// </summary>
        public void OpenFusionPanel()
        {
            if (fusionPanel != null)
                fusionPanel.SetActive(true);
                
            RefreshKukuSelection();
            RefreshRobotSelection();
            UpdateFusionRequirements();
        }
        
        /// <summary>
        /// 关闭融合面板
        /// </summary>
        public void CloseFusionPanel()
        {
            if (fusionPanel != null)
                fusionPanel.SetActive(false);
                
            ClearSelections();
        }
        
        /// <summary>
        /// 刷新KuKu选择列表
        /// </summary>
        public void RefreshKukuSelection()
        {
            if (kukuSelectionPanel == null || kukuCardPrefab == null) return;
            
            // 清除现有卡片
            foreach (Transform child in kukuSelectionPanel)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // 获取玩家的KuKu列表
            if (gameManager?.PlayerData?.CollectedKukus != null)
            {
                foreach (var kvp in gameManager.PlayerData.CollectedKukus.Values)
                {
                    // 只显示可以融合的KuKu（进化等级达到5级）
                    if (kvp.EvolutionLevel >= 5)
                    {
                        CreateKukuCard(kvp);
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建KuKu卡片
        /// </summary>
        private void CreateKukuCard(MythicalKukuData kuku)
        {
            GameObject cardObj = Instantiate(kukuCardPrefab, kukuSelectionPanel);
            if (cardObj != null)
            {
                var nameText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = kuku.Name;
                }
                
                // 添加选择事件
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    var kukuData = kuku; // 捕获变量
                    cardButton.onClick.AddListener(() => SelectKuku(kukuData));
                }
            }
        }
        
        /// <summary>
        /// 刷新机器人选择列表
        /// </summary>
        public void RefreshRobotSelection()
        {
            if (robotSelectionPanel == null || robotCardPrefab == null) return;
            
            // 清除现有卡片
            foreach (Transform child in robotSelectionPanel)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // 获取玩家的单位列表（主要是机器人）
            if (gameManager?.PlayerData?.DeployedUnits != null)
            {
                foreach (var unit in gameManager.PlayerData.DeployedUnits)
                {
                    // 只显示可以融合的机器人（等级达到10级且可以融合）
                    if (unit.Type == UnitData.UnitType.Robot && unit.Level >= 10 && unit.CanFuse)
                    {
                        CreateRobotCard(unit);
                    }
                }
            }
        }
        
        /// <summary>
        /// 创建机器人卡片
        /// </summary>
        private void CreateRobotCard(UnitData robot)
        {
            GameObject cardObj = Instantiate(robotCardPrefab, robotSelectionPanel);
            if (cardObj != null)
            {
                var nameText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = robot.Name;
                }
                
                // 添加选择事件
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    var robotData = robot; // 捕获变量
                    cardButton.onClick.AddListener(() => SelectRobot(robotData));
                }
            }
        }
        
        /// <summary>
        /// 选择KuKu
        /// </summary>
        private void SelectKuku(MythicalKukuData kuku)
        {
            selectedKuku = kuku;
            if (selectedKukuText != null)
            {
                selectedKukuText.text = $"选择的KuKu: {kuku.Name} (Lv.{kuku.EvolutionLevel})";
            }
            
            Debug.Log($"选择了KuKu: {kuku.Name}");
            UpdateFusionRequirements();
        }
        
        /// <summary>
        /// 选择机器人
        /// </summary>
        private void SelectRobot(UnitData robot)
        {
            selectedRobot = robot;
            if (selectedRobotText != null)
            {
                selectedRobotText.text = $"选择的机器人: {robot.Name} (Lv.{robot.Level})";
            }
            
            Debug.Log($"选择了机器人: {robot.Name}");
            UpdateFusionRequirements();
        }
        
        /// <summary>
        /// 清除选择
        /// </summary>
        private void ClearSelections()
        {
            selectedKuku = null;
            selectedRobot = null;
            
            if (selectedKukuText != null)
                selectedKukuText.text = "选择的KuKu: 无";
                
            if (selectedRobotText != null)
                selectedRobotText.text = "选择的机器人: 无";
                
            if (fusionResultText != null)
                fusionResultText.text = "";
                
            if (fusionResultDescription != null)
                fusionResultDescription.text = "";
        }
        
        /// <summary>
        /// 更新融合要求显示
        /// </summary>
        public void UpdateFusionRequirements()
        {
            if (fusionRequirementsText == null) return;
            
            string requirements = "融合要求:\n";
            
            if (selectedKuku != null)
            {
                requirements += $"- KuKu: {selectedKuku.Name} (Lv.{selectedKuku.EvolutionLevel}/5)\n";
                requirements += $"  状态: {(selectedKuku.EvolutionLevel >= 5 ? "✓ 可融合" : "✗ 未达到融合要求")}\n";
            }
            else
            {
                requirements += "- KuKu: 未选择\n";
            }
            
            if (selectedRobot != null)
            {
                requirements += $"- 机器人: {selectedRobot.Name} (Lv.{selectedRobot.Level}/10)\n";
                requirements += $"  状态: {(selectedRobot.Level >= 10 && selectedRobot.CanFuse ? "✓ 可融合" : "✗ 未达到融合要求")}\n";
            }
            else
            {
                requirements += "- 机器人: 未选择\n";
            }
            
            // 检查资源要求
            if (selectedKuku != null && selectedRobot != null)
            {
                requirements += "- 资源: ";
                if (gameManager?.PlayerData.Souls >= 50)
                {
                    requirements += "✓ 灵魂足够 (需要50点)\n";
                }
                else
                {
                    requirements += $"✗ 灵魂不足 (需要50点，当前{gameManager?.PlayerData.Souls:F0}点)\n";
                }
                
                if (gameManager?.PlayerData.Gems >= 10)
                {
                    requirements += "✓ 神石足够 (需要10个)\n";
                }
                else
                {
                    requirements += $"✗ 神石不足 (需要10个，当前{gameManager?.PlayerData.Gems}个)\n";
                }
            }
            
            fusionRequirementsText.text = requirements;
            
            // 更新融合按钮可用性
            UpdateFusionButtonAvailability();
        }
        
        /// <summary>
        /// 更新融合按钮可用性
        /// </summary>
        private void UpdateFusionButtonAvailability()
        {
            if (startFusionButton == null) return;
            
            bool canFuse = CanPerformFusion();
            startFusionButton.interactable = canFuse;
        }
        
        /// <summary>
        /// 检查是否可以执行融合
        /// </summary>
        private bool CanPerformFusion()
        {
            if (selectedKuku == null || selectedRobot == null) return false;
            
            // 检查KuKu是否可以融合
            if (selectedKuku.EvolutionLevel < 5) return false;
            
            // 检查机器人是否可以融合
            if (selectedRobot.Level < 10 || !selectedRobot.CanFuse) return false;
            
            // 检查资源是否足够
            if (gameManager?.PlayerData.Souls < 50) return false;
            if (gameManager?.PlayerData.Gems < 10) return false;
            
            return true;
        }
        
        /// <summary>
        /// 执行融合
        /// </summary>
        private void PerformFusion()
        {
            if (!CanPerformFusion())
            {
                Debug.LogWarning("无法执行融合，条件不满足");
                ShowNotification("融合条件不满足！", Color.red);
                return;
            }
            
            // 执行融合
            var fusionResult = FusionSystem.FuseKukuWithRobot(selectedKuku, selectedRobot);
            
            if (fusionResult.success)
            {
                Debug.Log(fusionResult.message);
                
                // 将融合后的单位添加到玩家数据
                if (gameManager?.PlayerData != null && fusionResult.fusedObject is UnitData fusedUnit)
                {
                    gameManager.PlayerData.AddDeployedUnit(fusedUnit);
                    
                    // 从原有列表中移除融合前的KuKu和机器人
                    if (gameManager.PlayerData.CollectedKukus.ContainsKey(selectedKuku.Id))
                    {
                        gameManager.PlayerData.CollectedKukus.Remove(selectedKuku.Id);
                    }
                    
                    if (gameManager.PlayerData.DeployedUnits.Contains(selectedRobot))
                    {
                        gameManager.PlayerData.DeployedUnits.Remove(selectedRobot);
                    }
                }
                
                // 显示融合结果
                DisplayFusionResult(fusionResult);
                
                ShowNotification("融合成功！", Color.green);
                
                // 刷新界面
                RefreshKukuSelection();
                RefreshRobotSelection();
                UpdatePlayerResources();
            }
            else
            {
                Debug.LogWarning(fusionResult.message);
                ShowNotification(fusionResult.message, Color.red);
            }
        }
        
        /// <summary>
        /// 显示融合结果
        /// </summary>
        private void DisplayFusionResult(FusionSystem.FusionResult result)
        {
            if (result.success && result.fusedObject is UnitData fusedUnit)
            {
                if (fusionResultText != null)
                    fusionResultText.text = $"融合成功: {fusedUnit.Name}";
                    
                if (fusionResultDescription != null)
                {
                    string description = $"类型: {fusedUnit.Type}\n";
                    description += $"攻击力: {fusedUnit.AttackPower:F1}\n";
                    description += $"防御力: {fusedUnit.DefensePower:F1}\n";
                    description += $"生命值: {fusedUnit.Health:F1}\n";
                    description += $"速度: {fusedUnit.Speed:F1}\n";
                    description += $"可装备槽位: {result.equipmentSlots}\n";
                    description += $"描述: {fusedUnit.Description}";
                    
                    fusionResultDescription.text = description;
                }
            }
            else
            {
                if (fusionResultText != null)
                    fusionResultText.text = "融合失败";
                    
                if (fusionResultDescription != null)
                    fusionResultDescription.text = result.message;
            }
        }
        
        /// <summary>
        /// 更新玩家资源显示
        /// </summary>
        public void UpdatePlayerResources()
        {
            var playerData = gameManager?.PlayerData;
            if (playerData == null) return;
            
            if (playerResourcesText != null)
            {
                playerResourcesText.text = $"灵魂: {playerData.Souls:F1} | 神石: {playerData.Gems} | 金币: {playerData.Coins}";
            }
        }
        
        /// <summary>
        /// 显示通知信息
        /// </summary>
        private void ShowNotification(string message, Color color = default(Color))
        {
            if (color == default(Color))
                color = Color.white;
                
            Debug.Log(message);
            // 这里可以实现UI通知系统的具体逻辑
        }
        
        /// <summary>
        /// 获取可融合的KuKu数量
        /// </summary>
        public int GetFusableKukuCount()
        {
            int count = 0;
            if (gameManager?.PlayerData?.CollectedKukus != null)
            {
                foreach (var kvp in gameManager.PlayerData.CollectedKukus.Values)
                {
                    if (kvp.EvolutionLevel >= 5) // 进化等级达到5级可融合
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// 获取可融合的机器人数量
        /// </summary>
        public int GetFusableRobotCount()
        {
            int count = 0;
            if (gameManager?.PlayerData?.DeployedUnits != null)
            {
                foreach (var unit in gameManager.PlayerData.DeployedUnits)
                {
                    if (unit.Type == UnitData.UnitType.Robot && unit.Level >= 10 && unit.CanFuse)
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// UI更新
        /// </summary>
        private void Update()
        {
            // 定期更新资源显示
            UpdatePlayerResources();
            
            // 更新融合要求显示
            UpdateFusionRequirements();
        }
    }
}