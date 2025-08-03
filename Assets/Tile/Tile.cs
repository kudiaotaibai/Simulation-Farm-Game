using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileState { Locked, Unlocked, Occupied }

    [Header("地块状态")]
    public TileState currentState = TileState.Locked;

    [Header("地块数据")]
    public int purchaseCost = 100;

    [Header("对象引用")]
    public GameObject vegetation;
    public GameObject ownershipSignPrefab;

    [Header("高亮设置")]
    public Renderer tileRenderer;
    [Tooltip("鼠标悬停时的高亮材质")]
    public Material hoverHighlightMaterial;
    [Tooltip("地块被选中时的高亮材质")]
    public Material selectedHighlightMaterial;

    private GameObject currentSignInstance;
    private GameObject currentBuilding;
    private Material normalMaterial;

    // --- 新增状态变量，用于追踪高亮状态 ---
    private bool isSelected = false;
    private bool isHovered = false;

    void Start()
    {
        if (tileRenderer == null) tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null && tileRenderer.enabled)
        {
            normalMaterial = tileRenderer.material;
        }
    }

    #region 高亮逻辑 (Highlight Logic)

    /// <summary>
    /// 设置地块的悬停状态，并更新材质。
    /// 由 PlayerInteraction 在鼠标移入/移出时调用。
    /// </summary>
    /// <param name="hovered">是否被悬停</param>
    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        UpdateMaterial();
    }

    /// <summary>
    /// 设置地块的选中状态，并更新材质。
    /// 由 PlayerInteraction 在选中/取消选中时调用。
    /// </summary>
    /// <param name="selected">是否被选中</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateMaterial();
    }

    /// <summary>
    /// 根据当前状态（选中、悬停）更新地块的材质。
    /// 选中状态的优先级高于悬停状态。
    /// </summary>
    private void UpdateMaterial()
    {
        if (tileRenderer == null) return;

        if (isSelected)
        {
            // 如果地块被选中，始终显示选中高亮材质
            tileRenderer.material = selectedHighlightMaterial;
        }
        else if (isHovered)
        {
            // 如果未被选中，但鼠标悬停在上面，则显示悬停高亮材质
            tileRenderer.material = hoverHighlightMaterial;
        }
        else
        {
            // 如果既未被选中也未被悬停，则显示普通材质
            tileRenderer.material = normalMaterial;
        }
    }

    #endregion

    #region 核心功能 (Core Functionality)

    /// <summary>
    /// 购买地块。
    /// </summary>
    public void PurchaseTile()
    {
        if (currentState != TileState.Locked) return;
        currentState = TileState.Unlocked;
        if (vegetation != null) vegetation.SetActive(false);
        ShowOwnershipSign();
    }

    /// <summary>
    /// 在地块上放置建筑。
    /// </summary>
    /// <param name="buildingPrefab">要放置的建筑预制体</param>
    public void PlaceBuilding(GameObject buildingPrefab)
    {
        if (currentState != TileState.Unlocked) return;
        HideOwnershipSign();
        currentBuilding = Instantiate(buildingPrefab, this.transform.position, Quaternion.identity, this.transform);
        currentState = TileState.Occupied;
    }

    /// <summary>
    /// 显示所有权标志。
    /// </summary>
    private void ShowOwnershipSign()
    {
        if (ownershipSignPrefab != null && currentSignInstance == null)
        {
            currentSignInstance = Instantiate(ownershipSignPrefab, this.transform.position, Quaternion.identity, this.transform);
        }
    }

    /// <summary>
    /// 隐藏所有权标志。
    /// </summary>
    private void HideOwnershipSign()
    {
        if (currentSignInstance != null)
        {
            Destroy(currentSignInstance);
            currentSignInstance = null;
        }
    }

    #endregion
}