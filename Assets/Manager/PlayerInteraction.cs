using UnityEngine;
using Cinemachine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [Header("�����۽�����")]
    public CinemachineFreeLook mainFreeLookCamera;
    public Transform freeRoamCameraTarget;

    public static bool IsInteractionEnabled = true;

    private Tile selectedTile;
    private Tile lastHoveredTile;
    private CinemachineBrain cinemachineBrain;

    // �������ԣ��������ű����԰�ȫ�ض�ȡ��ǰѡ�еĵؿ�
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
        // ��������UI�ϣ��򲻽��г�������
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
            SelectTile(null); // ѡ���̵�ʱ��ȡ���Եؿ��ѡ��
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

        // ȡ���ɵؿ��ѡ��״̬
        if (selectedTile != null)
        {
            selectedTile.SetSelected(false); // ����Tile�Ż���ķ���
        }

        selectedTile = tileToSelect;

        // �����µؿ��ѡ��״̬
        if (selectedTile != null)
        {
            selectedTile.SetSelected(true); // ����Tile�Ż���ķ���
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
            // ȡ������ͣ�ؿ�ĸ���
            if (lastHoveredTile != null)
            {
                lastHoveredTile.SetHovered(false); // ����Tile�Ż���ķ���
            }
            // ��������ͣ�ؿ�ĸ���
            if (currentTile != null)
            {
                currentTile.SetHovered(true); // ����Tile�Ż���ķ���
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