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
    /// 商店界面控制器 - 管理商店界面和购买逻辑
    /// </summary>
    public class ShopUIController : MonoBehaviour
    {
        [Header("商店面板")]
        public GameObject shopPanel;
        public Button closeShopButton;
        public Button refreshInventoryButton;
        
        [Header("商品展示")]
        public Transform shopItemContainer;
        public GameObject shopItemCardPrefab;
        
        [Header("商品详情")]
        public TextMeshProUGUI selectedItemNameText;
        public TextMeshProUGUI selectedItemDescriptionText;
        public TextMeshProUGUI selectedItemPriceText;
        public Image selectedItemIcon;
        
        [Header("玩家信息")]
        public TextMeshProUGUI playerCoinsText;
        public TextMeshProUGUI playerGemsText;
        public TextMeshProUGUI playerSoulsText;
        
        [Header("购买按钮")]
        public Button purchaseButton;
        public Button buyCoinsButton;
        public Button buyGemsButton;
        
        // 商店库存
        private List<EquipmentSystem.ShopItem> shopInventory;
        private EquipmentSystem.ShopItem? selectedItem;
        
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
        /// 初始化商店界面
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
            
            // 刷新商店库存
            RefreshShopInventory();
            
            // 初始化显示
            UpdatePlayerResources();
            
            Debug.Log("商店界面控制器初始化完成");
        }
        
        /// <summary>
        /// 初始化按钮事件
        /// </summary>
        private void InitializeButtonEvents()
        {
            if (closeShopButton != null)
                closeShopButton.onClick.AddListener(CloseShopPanel);
                
            if (refreshInventoryButton != null)
                refreshInventoryButton.onClick.AddListener(RefreshShopInventory);
                
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(PurchaseSelectedItem);
                
            if (buyCoinsButton != null)
                buyCoinsButton.onClick.AddListener(() => BuyResource("Coins"));
                
            if (buyGemsButton != null)
                buyGemsButton.onClick.AddListener(() => BuyResource("Gems"));
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
            // 根据实际的游戏流程，商店可能在捕捉阶段或防守阶段之间打开
            // 这里暂时保持面板状态，具体逻辑可根据游戏设计调整
            if (newState == GameManager.GameState.CapturePhase || newState == GameManager.GameState.DefensePhase)
            {
                // 根据具体设计决定是否打开商店面板
            }
            else if (newState == GameManager.GameState.MainMenu)
            {
                CloseShopPanel();
            }
        }
        
        /// <summary>
        /// 打开商店面板
        /// </summary>
        public void OpenShopPanel()
        {
            if (shopPanel != null)
                shopPanel.SetActive(true);
                
            RefreshShopInventory();
            UpdatePlayerResources();
        }
        
        /// <summary>
        /// 关闭商店面板
        /// </summary>
        public void CloseShopPanel()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
                
            selectedItem = null;
        }
        
        /// <summary>
        /// 刷新商店库存
        /// </summary>
        public void RefreshShopInventory()
        {
            if (shopItemContainer == null || shopItemCardPrefab == null) return;
            
            // 清除现有商品卡片
            ClearShopItemCards();
            
            // 获取商店库存
            shopInventory = EquipmentSystem.GetShopInventory();
            
            // 创建商品卡片
            foreach (var shopItem in shopInventory)
            {
                if (shopItem.isAvailable)
                {
                    CreateShopItemCard(shopItem);
                }
            }
        }
        
        /// <summary>
        /// 创建商品卡片
        /// </summary>
        private void CreateShopItemCard(EquipmentSystem.ShopItem shopItem)
        {
            GameObject cardObj = Instantiate(shopItemCardPrefab, shopItemContainer);
            if (cardObj != null)
            {
                // 获取卡片上的UI组件并设置数据
                var nameText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                var priceText = cardObj.GetComponentInChildren<TextMeshProUGUI>();
                
                if (nameText != null)
                {
                    nameText.text = shopItem.item.Name;
                }
                
                if (priceText != null)
                {
                    priceText.text = $"价格: {shopItem.price}";
                }
                
                // 添加点击事件
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    var item = shopItem; // 捕获变量
                    cardButton.onClick.AddListener(() => OnShopItemClicked(item));
                }
            }
        }
        
        /// <summary>
        /// 清除商品卡片
        /// </summary>
        private void ClearShopItemCards()
        {
            foreach (Transform child in shopItemContainer)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        /// <summary>
        /// 商品被点击
        /// </summary>
        private void OnShopItemClicked(EquipmentSystem.ShopItem shopItem)
        {
            selectedItem = shopItem;
            UpdateSelectedItemDisplay();
            UpdatePurchaseButtonAvailability();
        }
        
        /// <summary>
        /// 更新选中商品显示
        /// </summary>
        private void UpdateSelectedItemDisplay()
        {
            if (!selectedItem.HasValue) return;
            
            var item = selectedItem.Value.item;
            var price = selectedItem.Value.price;
            
            if (selectedItemNameText != null)
                selectedItemNameText.text = item.Name;
                
            if (selectedItemDescriptionText != null)
                selectedItemDescriptionText.text = item.Description;
                
            if (selectedItemPriceText != null)
                selectedItemPriceText.text = $"价格: {price}";
                
            // 这里可以设置商品图标
            if (selectedItemIcon != null)
            {
                // 在实际游戏中，这里会根据item.SpriteName加载对应的精灵
                // selectedItemIcon.sprite = LoadSprite(item.SpriteName);
            }
        }
        
        /// <summary>
        /// 更新购买按钮可用性
        /// </summary>
        private void UpdatePurchaseButtonAvailability()
        {
            if (purchaseButton == null || !selectedItem.HasValue) return;
            
            var playerData = gameManager?.PlayerData;
            if (playerData == null) return;
            
            // 检查玩家是否有足够资源
            bool canAfford = playerData.Coins >= selectedItem.Value.price;
            purchaseButton.interactable = canAfford;
        }
        
        /// <summary>
        /// 购买选中商品
        /// </summary>
        private void PurchaseSelectedItem()
        {
            if (!selectedItem.HasValue)
            {
                Debug.LogWarning("请先选择一个商品");
                return;
            }
            
            var item = selectedItem.Value.item;
            var price = selectedItem.Value.price;
            
            // 检查玩家是否有足够资源
            if (gameManager?.PlayerData?.Coins < price)
            {
                Debug.LogWarning("金币不足，无法购买此商品！");
                ShowNotification("金币不足！", Color.red);
                return;
            }
            
            // 执行购买
            bool success = EquipmentSystem.PurchaseItem(item, price);
            
            if (success)
            {
                // 添加到玩家装备库
                if (gameManager?.PlayerData?.OwnedEquipment != null)
                {
                    gameManager.PlayerData.OwnedEquipment.Add(item);
                }
                
                Debug.Log($"成功购买 {item.Name}！");
                ShowNotification($"成功购买 {item.Name}！", Color.green);
                
                // 更新显示
                UpdatePlayerResources();
                RefreshShopInventory();
            }
            else
            {
                Debug.LogWarning("购买失败！");
                ShowNotification("购买失败！", Color.red);
            }
        }
        
        /// <summary>
        /// 更新玩家资源显示
        /// </summary>
        public void UpdatePlayerResources()
        {
            var playerData = gameManager?.PlayerData;
            if (playerData == null) return;
            
            if (playerCoinsText != null)
                playerCoinsText.text = $"金币: {playerData.Coins}";
                
            if (playerGemsText != null)
                playerGemsText.text = $"神石: {playerData.Gems}";
                
            if (playerSoulsText != null)
                playerSoulsText.text = $"灵魂: {playerData.Souls:F1}";
        }
        
        /// <summary>
        /// 购买资源
        /// </summary>
        private void BuyResource(string resourceType)
        {
            var playerData = gameManager?.PlayerData;
            if (playerData == null) return;
            
            switch (resourceType)
            {
                case "Coins":
                    // 在实际游戏中，这里可能会打开充值界面或使用其他货币购买金币
                    // 为了演示，我们简单地增加一些金币
                    playerData.AddCoins(1000);
                    Debug.Log("购买了1000金币");
                    ShowNotification("购买了1000金币", Color.green);
                    break;
                    
                case "Gems":
                    // 同样，实际游戏中会涉及真实货币购买
                    playerData.AddGems(100);
                    Debug.Log("购买了100神石");
                    ShowNotification("购买了100神石", Color.green);
                    break;
            }
            
            UpdatePlayerResources();
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
        /// 添加装备到商店（用于调试或特殊事件）
        /// </summary>
        public void AddItemToShop(WeaponData item, int price)
        {
            // 在实际实现中，这可能需要添加到商店库存列表
            // 重新刷新商店界面
            RefreshShopInventory();
        }
        
        /// <summary>
        /// 设置商品可用性
        /// </summary>
        public void SetItemAvailability(WeaponData item, bool available)
        {
            // 在实际实现中，这会更新商店库存中特定商品的可用性
            RefreshShopInventory();
        }
        
        /// <summary>
        /// 获取商店商品总数
        /// </summary>
        public int GetShopItemCount()
        {
            return shopInventory?.Count ?? 0;
        }
        
        /// <summary>
        /// 获取玩家拥有的装备数量
        /// </summary>
        public int GetPlayerEquipmentCount()
        {
            return gameManager?.PlayerData?.OwnedEquipment?.Count ?? 0;
        }
        
        /// <summary>
        /// 检查玩家是否拥有特定装备
        /// </summary>
        public bool PlayerHasEquipment(string equipmentName)
        {
            if (gameManager?.PlayerData?.OwnedEquipment != null)
            {
                foreach (var equip in gameManager.PlayerData.OwnedEquipment)
                {
                    if (equip.Name == equipmentName)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// UI更新
        /// </summary>
        private void Update()
        {
            // 定期更新玩家资源显示
            UpdatePlayerResources();
            
            // 更新购买按钮状态
            UpdatePurchaseButtonAvailability();
        }
    }
}