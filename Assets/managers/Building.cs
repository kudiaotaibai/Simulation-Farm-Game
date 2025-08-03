using UnityEngine;
using System.Collections.Generic;

// --- 新增：一个辅助类，用于在Inspector中方便地设置资源和其所需数量 ---
[System.Serializable]
public class ResourceCost
{
    public ResourceData resource;
    public int amount;
}

/// <summary>
/// 定义建筑的类型，决定其工作模式。
/// </summary>
public enum BuildingType
{
    Crop,      // 农作物: 有多阶段生长，成熟后一次性收获。
    Resource,  // 资源建筑: 无消耗，按固定周期产出。
    Animal,    // 动物牧场: 有消耗，按固定周期产出。
    Processor  // 新增：加工厂，消耗多种资源，产出一种新资源。
}

/// <summary>
/// 挂载在所有建筑预制体上的核心脚本。
/// 负责管理该建筑的类型、数据、生长和生产逻辑。
/// </summary>
public class Building : MonoBehaviour
{
    [Header("1. 建筑类型")]
    public BuildingType type;

    [Header("2. 基础信息")]
    public string buildingName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("3. 生产设置")]
    public ResourceData producedResource;
    public int productionAmount;

    [Header("4. 周期设置")]
    public int productionCycleDays = 3;

    // --- ⭐ 核心修改：将单一消耗，升级为资源消耗列表 ---
    [Header("5. 消耗设置 (动物牧场/加工厂需要)")]
    [Tooltip("这个建筑生产时需要消耗的所有资源列表")]
    public List<ResourceCost> requiredResources;

    [Header("6. 生长模型 (仅农作物需要)")]
    public List<GameObject> growthStagePrefabs;

    // --- 私有变量 ---
    private int currentStage = 0;
    private GameObject currentStageObject;
    private int productionTimer = 0;

    // --- Unity生命周期方法 ---
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeUnitPassed += OnNewDay;
        }
        if (type == BuildingType.Crop)
        {
            UpdateGrowthVisuals();
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeUnitPassed -= OnNewDay;
        }
    }

    // --- 核心逻辑 ---
    private void OnNewDay()
    {
        switch (type)
        {
            case BuildingType.Crop:
                HandleCropGrowth();
                break;

            case BuildingType.Resource:
            case BuildingType.Animal:
            case BuildingType.Processor: // 让加工厂也走周期性生产的逻辑
                HandlePeriodicProduction();
                break;
        }
    }

    private void HandleCropGrowth()
    {
        if (currentStage < productionCycleDays - 1)
        {
            currentStage++;
            UpdateGrowthVisuals();
        }
        else
        {
            // 对于农作物，它没有消耗，直接生产
            ResourceManager.Instance.AddResource(producedResource, productionAmount);
            Debug.Log(buildingName + " 成功产出了 " + productionAmount + " " + producedResource.resourceName);

            currentStage = 0;
            UpdateGrowthVisuals();
        }
    }

    private void HandlePeriodicProduction()
    {
        productionTimer++;
        if (productionTimer >= productionCycleDays)
        {
            productionTimer = 0;
            Produce();
        }
    }

    private void Produce()
    {
        // 1. 检查是否可以生产 (检查所有必需的资源是否足够)
        bool canProduce = CheckRequiredResources();
        if (!canProduce)
        {
            Debug.Log(buildingName + " 因缺少原料而停止生产。");
            return;
        }

        // 2. 如果可以生产，则消耗所有必需的资源
        ConsumeRequiredResources();

        // 3. 产出最终产品
        ResourceManager.Instance.AddResource(producedResource, productionAmount);
        Debug.Log(buildingName + " 成功产出了 " + productionAmount + " " + producedResource.resourceName);
    }

    private bool CheckRequiredResources()
    {
        if (requiredResources == null || requiredResources.Count == 0)
        {
            return true; // 如果没有消耗需求，则总能生产 (比如伐木场)
        }
        foreach (var cost in requiredResources)
        {
            if (!ResourceManager.Instance.HasEnoughResource(cost.resource, cost.amount))
            {
                return false; // 只要有一种资源不足，就不能生产
            }
        }
        return true;
    }

    private void ConsumeRequiredResources()
    {
        if (requiredResources == null || requiredResources.Count == 0) return;
        foreach (var cost in requiredResources)
        {
            ResourceManager.Instance.ConsumeResource(cost.resource, cost.amount);
        }
    }

    private void UpdateGrowthVisuals()
    {
        if (currentStageObject != null)
        {
            Destroy(currentStageObject);
        }
        if (growthStagePrefabs != null && currentStage < growthStagePrefabs.Count)
        {
            GameObject stagePrefab = growthStagePrefabs[currentStage];
            if (stagePrefab != null)
            {
                currentStageObject = Instantiate(stagePrefab, this.transform.position, this.transform.rotation, this.transform);
            }
        }
    }
}