using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("游戏内所有资源类型")]
    public List<ResourceData> allGameResources;

    [Header("特殊资源设置")]
    public ResourceData goldResourceData;
    public int startingGold = 500;

    private Dictionary<ResourceData, int> playerResources = new Dictionary<ResourceData, int>();

    void Awake()
    {
        // 在Awake中，只做最基础的、不依赖别人的初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // 只初始化字典结构，不发放资源
        InitializeResourceDictionary();
    }

    void Start()
    {
        // --- 关键修改：在Start中给予初始资源 ---
        // 因为此时，可以保证所有其他管理器的Awake都已经执行完毕，UIManager.Instance不再为空
        GiveStartingResources();
    }

    /// <summary>
    /// 初始化资源字典，将所有游戏中可能出现的资源添加进去，初始值为0。
    /// (这个方法现在只在Awake中被调用一次)
    /// </summary>
    private void InitializeResourceDictionary()
    {
        playerResources.Clear();
        foreach (ResourceData resourceData in allGameResources)
        {
            if (resourceData != null && !playerResources.ContainsKey(resourceData))
            {
                playerResources.Add(resourceData, 0);
            }
        }
    }

    /// <summary>
    /// 在游戏开始时，给予玩家初始资源。
    /// (这个方法现在只在Start中被调用一次)
    /// </summary>
    private void GiveStartingResources()
    {
        if (goldResourceData != null)
        {
            AddResource(goldResourceData, startingGold);
        }
        // 未来可以在这里给予其他初始资源，比如初始饲料
        // AddResource(FindResourceDataByName("饲料"), 20);
    }


    // --- 下面的核心方法保持你之前的版本，它们是正确的 ---

    public void AddResource(ResourceData resource, int amount)
    {
        if (resource == null) return;

        if (!playerResources.ContainsKey(resource))
        {
            playerResources.Add(resource, 0);
        }

        playerResources[resource] += amount;
        Debug.Log("增加了 " + amount + " " + resource.resourceName + ". 当前拥有: " + playerResources[resource]);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateResourceDisplay(resource, playerResources[resource]);
        }
        else
        {
            // 这个错误日志现在应该不会再出现了
            Debug.LogError("ResourceManager无法更新UI，因为UIManager.Instance为空！");
        }
    }

    public bool ConsumeResource(ResourceData resource, int amount)
    {
        if (HasEnoughResource(resource, amount))
        {
            playerResources[resource] -= amount;
            Debug.Log("消耗了 " + amount + " " + resource.resourceName + ". 剩余: " + playerResources[resource]);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateResourceDisplay(resource, playerResources[resource]);
            }
            return true;
        }
        return false;
    }

    public bool HasEnoughResource(ResourceData resource, int amount)
    {
        if (resource == null) return false;
        return playerResources.ContainsKey(resource) && playerResources[resource] >= amount;
    }

    public int GetResourceAmount(ResourceData resource)
    {
        if (resource == null) return 0;
        if (playerResources.ContainsKey(resource))
        {
            return playerResources[resource];
        }
        return 0;
    }
}