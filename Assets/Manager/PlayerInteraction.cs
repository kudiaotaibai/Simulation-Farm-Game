using UnityEngine;
using Cinemachine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [Header("相机与聚焦设置")]
    public CinemachineFreeLook mainFreeLookCamera;
    public Transform freeRoamCameraTarget;

    public static bool IsInteractionEnabled = true;

    private Tile selectedTile;
    private Tile lastHoveredTile;
    private CinemachineBrain cinemachineBrain;

    // 公共属性，让其他脚本可以安全地读取当前选中的地块
    public Tile SelectedTile => selectedTile;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (Camera.main != null) cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (freeRoamCameraTarget == null)
        {
            CameraTargetController targetController = FindObjectOfType<CameraTargetController>();
            if (targetController != null) freeRoamCameraTarget = targetController.transform;
        }
    }

    void Update()
    {
        // 如果鼠标在UI上，则不进行场景交互
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            UpdateHighlight(null);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            UpdateHighlight(hitInfo.collider.GetComponent<Tile>());

            if (IsInteractionEnabled)
            {
                if (Input.GetMouseButtonDown(0)) HandleLeftClick(hitInfo.collider);
                else if (Input.GetMouseButtonDown(1)) HandleRightClick(hitInfo.collider);
            }
        }
        else
        {
            UpdateHighlight(null);
            if (Input.GetMouseButtonDown(0) && IsInteractionEnabled)
            {
                DeselectCurrentTile();
            }
        }
    }

    private void HandleLeftClick(Collider hitCollider)
    {
        Tile tile = hitCollider.GetComponent<Tile>();
        if (tile != null && tile.currentState == Tile.TileState.Locked)
        {
            SelectTile(tile);
            UIManager.Instance.ShowPurchasePanel(tile);
        }
        else if (hitCollider.CompareTag("Shop"))
        {
            SelectTile(null); // 选中商店时，取消对地块的选择
            UIManager.Instance.ShowShop();
        }
        else
        {
            DeselectCurrentTile();
        }
    }

    private void HandleRightClick(Collider hitCollider)
    {
        Tile tile = hitCollider.GetComponent<Tile>();
        if (tile != null && tile.currentState == Tile.TileState.Unlocked)
        {
            SelectTile(tile);
            UIManager.Instance.ShowBuildMenu(tile);
        }
    }

    private void SelectTile(Tile tileToSelect)
    {
        if (selectedTile == tileToSelect) return;

        // 取消旧地块的选中状态
        if (selectedTile != null)
        {
            selectedTile.SetSelected(false); // 调用Tile优化后的方法
        }

        selectedTile = tileToSelect;

        // 设置新地块的选中状态
        if (selectedTile != null)
        {
            selectedTile.SetSelected(true); // 调用Tile优化后的方法
            FocusCameraOn(selectedTile.transform);
        }
        else
        {
            UnfocusCamera();
        }
    }

    public void DeselectCurrentTile()
    {
        SelectTile(null);
    }

    private void UpdateHighlight(Tile currentTile)
    {
        if (currentTile != lastHoveredTile)
        {
            // 取消旧悬停地块的高亮
            if (lastHoveredTile != null)
            {
                lastHoveredTile.SetHovered(false); // 调用Tile优化后的方法
            }
            // 设置新悬停地块的高亮
            if (currentTile != null)
            {
                currentTile.SetHovered(true); // 调用Tile优化后的方法
            }
            lastHoveredTile = currentTile;
        }
    }

    private void FocusCameraOn(Transform target)
    {
        if (mainFreeLookCamera == null) return;
        mainFreeLookCamera.Follow = target;
        mainFreeLookCamera.LookAt = target;
        if (cinemachineBrain != null) cinemachineBrain.ManualUpdate();
    }

    private void UnfocusCamera()
    {
        if (mainFreeLookCamera == null || freeRoamCameraTarget == null) return;
        mainFreeLookCamera.Follow = freeRoamCameraTarget;
        mainFreeLookCamera.LookAt = freeRoamCameraTarget;
        if (cinemachineBrain != null) cinemachineBrain.ManualUpdate();
    }
}