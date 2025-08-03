using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    [Header("移动速度")]
    public float panSpeed = 30f;      // 水平平移速度
    public float verticalSpeed = 25f; // 垂直升降速度

    [Header("移动范围限制")]
    public Vector2 panLimitMin; // 水平范围最小值 (X, Z)
    public Vector2 panLimitMax; // 水平范围最大值 (X, Z)
    public float minHeight = 10f; // 最小高度 (Y)
    public float maxHeight = 80f; // 最大高度 (Y)

    private Transform cameraTransform;

    void Start()
    {
        // 在游戏开始时获取主摄像机的引用，以计算正确的移动方向
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // --- 核心修改：只有在开关启用时，才允许执行所有移动逻辑 ---
        if (PlayerInteraction.IsInteractionEnabled)
        {
            if (cameraTransform == null)
            {
                // 如果在游戏过程中摄像机被销毁，尝试重新获取
                if (Camera.main != null) cameraTransform = Camera.main.transform;
                else return;
            }

            // --- 1. 水平平移 (W/A/S/D) ---
            float moveX = Input.GetAxis("Horizontal"); // A/D
            float moveZ = Input.GetAxis("Vertical");   // W/S

            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = cameraTransform.right;
            right.y = 0;
            right.Normalize();
            Vector3 moveDirection = (forward * moveZ) + (right * moveX);
            transform.Translate(moveDirection * panSpeed * Time.deltaTime, Space.World);

            // --- 2. 垂直移动 (空格/Shift) ---
            if (Input.GetKey(KeyCode.Space))
            {
                // 按下空格，向上移动
                transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // 按下Shift，向下移动
                transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
            }
        }

        // --- 限制边界的逻辑保留在开关外面，确保目标点始终在地图内 ---
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, panLimitMin.x, panLimitMax.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minHeight, maxHeight); // 限制Y轴高度
        clampedPos.z = Mathf.Clamp(clampedPos.z, panLimitMin.y, panLimitMax.y);
        transform.position = clampedPos;
    }
}