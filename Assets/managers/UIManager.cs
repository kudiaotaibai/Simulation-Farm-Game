using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 一个简单的状态锁，防止UI面板在同一帧被打开又关闭
    private bool isPanelOpen = false;

    [Header("HUD")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI timeText;

    [Header("UI面板")]
    public GameObject purchasePanel;
    public GameObject buildMenuPanel;
    public GameObject shopPanel;
    public GameObject buildingInfoPanel;

    [Header("购买面板元素")]
    public TextMeshProUGUI purchasePriceText;
    public Button purchaseConfirmButton;
    public Button purchaseCancelButton;

    [Header("建造菜单设置")]
    public Transform buildMenuContent;
    public GameObject buildMenuItemPrefab;

    [Header("建筑预制体")]
    public List<GameObject> buildablePrefabs;

    [Header("库存UI设置")]
    public Transform resourcePanelContent;
    public GameObject resourceItemPrefab;

    [Header("商店UI设置")]
    public Transform shopContent;
    public GameObject shopItemPrefab;

    // 已移除 private Tile currentSelectedTile; 因为它会导致状态不一致

    private Dictionary<ResourceData, ResourceItemUI> resourceUIs = new Dictionary<ResourceData, ResourceItemUI>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 发现自己是重复的实例
            Destroy(gameObject);
            // 关键！立即返回，不再执行此脚本的任何后续代码（包括Start方法）
            return;
        }

        // 如果自己是第一个实例，则正常进行
        Instance = this;

        // 如果你的UIManager需要跨场景存在，可以在这里加上 DontDestroyOnLoad
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CloseAllPanels();

        // 核心修复：在添加监听前，先移除所有监听，防止重复绑定
        if (purchaseConfirmButton != null)
        {
            purchaseConfirmButton.onClick.RemoveAllListeners();
            purchaseConfirmButton.onClick.AddListener(OnPurchaseConfirm);
        }
        if (purchaseCancelButton != null)
        {
            purchaseCancelButton.onClick.RemoveAllListeners();
            purchaseCancelButton.onClick.AddListener(CloseAllPanels);
        }

        InitializeResourceUI();
        if (GameManager.Instance != null)
        {
            UpdateTimeUI(GameManager.Instance.currentTimeUnit);
        }
    }

    public void ShowPurchasePanel(Tile tile)
    {
        if (isPanelOpen || tile == null) return;
        isPanelOpen = true;
        PlayerInteraction.IsInteractionEnabled = false;

        if (purchasePriceText != null) purchasePriceText.text = "价格: " + tile.purchaseCost + " G";
        if (purchasePanel != null) purchasePanel.SetActive(true);
    }

    public void ShowBuildMenu(Tile tile)
    {
        if (isPanelOpen || tile == null) return;
        isPanelOpen = true;
        PlayerInteraction.IsInteractionEnabled = false;
        PopulateBuildMenu();
        if (buildMenuPanel != null) buildMenuPanel.SetActive(true);
    }

    public void ShowShop()
    {
        if (isPanelOpen) return;
        isPanelOpen = true;
        PlayerInteraction.IsInteractionEnabled = false;
        PopulateShop();
        if (shopPanel != null) shopPanel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        isPanelOpen = false;
        PlayerInteraction.IsInteractionEnabled = true;

        if (purchasePanel != null) purchasePanel.SetActive(false);
        if (buildMenuPanel != null) buildMenuPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (buildingInfoPanel != null) buildingInfoPanel.SetActive(false);

        if (PlayerInteraction.Instance != null)
        {
            PlayerInteraction.Instance.DeselectCurrentTile();
        }
    }

    public void OnPurchaseConfirm()
    {
        // 从 PlayerInteraction 获取最新的地块信息，这是唯一的状态源
        Tile tileToPurchase = PlayerInteraction.Instance.SelectedTile;

        if (tileToPurchase != null)
        {
            ResourceData gold = ResourceManager.Instance.goldResourceData;
            int cost = tileToPurchase.purchaseCost;
            if (ResourceManager.Instance.ConsumeResource(gold, cost))
            {
                tileToPurchase.PurchaseTile();
                CloseAllPanels();
            }
            else
            {
                Debug.Log("金币不足！");
            }
        }
        else
        {
            // 如果执行到这里，通常是因为按钮事件被重复触发，
            // 第一次调用成功后把 selectedTile 设为了 null，导致第二次调用失败。
            Debug.LogError("购买确认失败：没有有效的地块被选中！");
        }
    }

    public void OnBuildRequest(GameObject buildingPrefab)
    {
        // 从 PlayerInteraction 获取最新的地块信息
        Tile tileToBuildOn = PlayerInteraction.Instance.SelectedTile;

        if (tileToBuildOn != null && buildingPrefab != null)
        {
            tileToBuildOn.PlaceBuilding(buildingPrefab);
            CloseAllPanels();
        }
        else
        {
            Debug.LogError("建造请求失败：没有有效的地块被选中或建筑预制体为空！");
        }
    }

    private void InitializeResourceUI()
    {
        if (ResourceManager.Instance == null || resourcePanelContent == null || resourceItemPrefab == null) return;
        foreach (var resourceData in ResourceManager.Instance.allGameResources)
        {
            if (resourceData == null) continue;
            GameObject itemGO = Instantiate(resourceItemPrefab, resourcePanelContent);
            ResourceItemUI itemUI = itemGO.GetComponent<ResourceItemUI>();
            if (itemUI != null)
            {
                int initialAmount = ResourceManager.Instance.GetResourceAmount(resourceData);
                itemUI.UpdateDisplay(resourceData.icon, initialAmount);
                if (!resourceUIs.ContainsKey(resourceData))
                {
                    resourceUIs.Add(resourceData, itemUI);
                }
            }
        }
    }

    public void UpdateResourceDisplay(ResourceData resource, int newAmount)
    {
        if (resource == null) return;
        if (resource == ResourceManager.Instance.goldResourceData)
        {
            UpdateGoldUI(newAmount);
        }
        if (resourceUIs.ContainsKey(resource))
        {
            resourceUIs[resource].UpdateDisplay(resource.icon, newAmount);
        }
    }

    public void UpdateGoldUI(int amount)
    {
        if (goldText != null) goldText.text = "金币: " + amount;
    }

    public void UpdateTimeUI(int timeUnit)
    {
        if (timeText != null) timeText.text = "天数: " + timeUnit;
    }

    private void PopulateBuildMenu()
    {
        if (buildMenuContent == null || buildMenuItemPrefab == null) return;
        foreach (Transform child in buildMenuContent) Destroy(child.gameObject);
        foreach (GameObject prefab in buildablePrefabs)
        {
            GameObject itemGO = Instantiate(buildMenuItemPrefab, buildMenuContent);
            BuildMenuItem itemUI = itemGO.GetComponent<BuildMenuItem>();
            if (itemUI != null) itemUI.Setup(prefab, this);
        }
    }

    private void PopulateShop()
    {
        if (shopContent == null || shopItemPrefab == null || ShopManager.Instance == null) return;
        foreach (Transform child in shopContent) Destroy(child.gameObject);
        foreach (var shopItem in ShopManager.Instance.itemsForSale)
        {
            GameObject itemGO = Instantiate(shopItemPrefab, shopContent);
            ShopItemUI itemUI = itemGO.GetComponent<ShopItemUI>();
            if (itemUI != null) itemUI.Setup(shopItem);
        }
    }
}