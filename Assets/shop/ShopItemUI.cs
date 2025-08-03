using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI元素引用")]

    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Image nameTextImage;
    public Image amountImage;
    public Image priceTextImage;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI priceText; // 新增：用来显示价格
    public Button sellButton;

    private ResourceData resourceData;

    /// <summary>
    /// 用传入的资源数据来设置这个商店项（已修正，不再需要ShopManager参数）
    /// </summary>
    public void Setup(ShopItem shopItem) // 直接接收包含价格的ShopItem
    {
        this.resourceData = shopItem.resource;

        // 更新显示
        if (resourceData != null)
        {
            iconImage.sprite = resourceData.icon;
            nameText.text = resourceData.resourceName;
        }

        // 更新价格显示
        if (priceText != null)
        {
            priceText.text = "售价: " + shopItem.sellPrice + " G";
        }

        // 从ResourceManager获取并显示当前拥有数量
        UpdateAmount();

        // 为出售按钮绑定点击事件
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            // 当按钮被点击时，直接调用ShopManager的单例
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }
    }

    /// <summary>
    /// 更新拥有数量的显示
    /// </summary>
    public void UpdateAmount()
    {
        if (ResourceManager.Instance != null && resourceData != null)
        {
            amountText.text = "拥有: " + ResourceManager.Instance.GetResourceAmount(resourceData);
        }
    }

    /// <summary>
    /// 当出售按钮被点击时调用的方法
    /// </summary>
    private void OnSellButtonClicked()
    {
        if (ShopManager.Instance != null && resourceData != null)
        {
            ShopManager.Instance.SellAll(resourceData);
            // 卖出后，立即更新数量显示
            UpdateAmount();
        }
    }
}