using UnityEngine;
using System.Collections.Generic;

// 这个辅助类保持不变
[System.Serializable]
public class ShopItem
{
    public ResourceData resource;
    public int sellPrice;
}

public class ShopManager : MonoBehaviour
{
    // --- 单例模式 ---
    public static ShopManager Instance { get; private set; }

    [Header("商店设置")]
    public List<ShopItem> itemsForSale;

    [Header("升级设置")]
    public int initialUpgradeCost = 50;

    // --- 私有变量 ---
    private int shopLevel = 1;
    private int currentUpgradeCost;
    private Dictionary<ResourceData, int> productPriceDict = new Dictionary<ResourceData, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        InitializePriceDictionary();
        currentUpgradeCost = initialUpgradeCost;
    }

    private void InitializePriceDictionary()
    {
        productPriceDict.Clear();
        foreach (var item in itemsForSale)
        {
            if (item.resource != null && !productPriceDict.ContainsKey(item.resource))
            {
                productPriceDict.Add(item.resource, item.sellPrice);
            }
        }
    }

    // --- 核心售卖逻辑 ---
    public void SellAll(ResourceData resourceToSell)
    {
        if (ResourceManager.Instance == null) return;
        if (resourceToSell == null) return;

        int amount = ResourceManager.Instance.GetResourceAmount(resourceToSell);
        if (amount > 0)
        {
            if (productPriceDict.ContainsKey(resourceToSell))
            {
                int pricePerUnit = productPriceDict[resourceToSell];
                int totalGain = amount * pricePerUnit;

                ResourceManager.Instance.ConsumeResource(resourceToSell, amount);

                // --- 关键修改：通过ResourceManager增加金币 ---
                // 我们需要拿到代表金币的ResourceData对象
                ResourceData gold = ResourceManager.Instance.goldResourceData;
                ResourceManager.Instance.AddResource(gold, totalGain);

                Debug.Log("成功卖出 " + amount + " 个 " + resourceToSell.name + ", 共获得 " + totalGain + " 金币。");
            }
        }
    }

    // --- 商店升级逻辑 ---
    public void UpgradeShop()
    {
        // ---  关键修改：通过ResourceManager检查并消耗金币 ---
        ResourceData gold = ResourceManager.Instance.goldResourceData;
        if (ResourceManager.Instance.ConsumeResource(gold, currentUpgradeCost))
        {
            // 消耗成功后，才执行升级逻辑
            shopLevel++;
            currentUpgradeCost = (int)(currentUpgradeCost * 1.5f);
            Debug.Log($"商店升级成功！当前等级: {shopLevel}, 下次升级费用: {currentUpgradeCost}");
        }
        else
        {
            Debug.Log("金币不足，无法升级商店");
        }
    }
}