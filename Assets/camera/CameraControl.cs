using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // --- 公共的静态开关，用于全局控制 ---
    // 其他脚本（如UIManager）可以通过 CameraControl.IsControlEnabled 来禁用或启用镜头控制
    public static bool IsControlEnabled = true;

    [Header("目标与速度")]
    public Transform player;    // 相机追随目标
    public float xSpeed = 200;  // X轴方向拖动速度
    public float ySpeed = 200;  // Y轴方向拖动速度
    public float mSpeed = 10;   // 放大缩小速度

    [Header("限制范围")]
    public float yMinLimit = -50; // 在Y轴最小移动范围
    public float yMaxLimit = 50; // 在Y轴最大移动范围
    public float minDinstance = 2; // 相机视角最小距离
    public float maxDinstance = 30; // 相机视角最大距离

    // --- 私有变量 ---
    private float distance = 10;  // 相机视角距离
    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angle = transform.eulerAngles;
        x = angle.y;
        y = angle.x;
    }

    void LateUpdate()
    {
        // 确保有追随目标
        if (player)
        {
            // --- 核心修改：只有在开关启用时，才处理鼠标输入 ---
            if (IsControlEnabled)
            {
                // 处理鼠标右键旋转
                if (Input.GetMouseButton(1))
                {
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                }

                // 处理鼠标滚轮缩放
                distance -= Input.GetAxis("Mouse ScrollWheel") * mSpeed;
                distance = Mathf.Clamp(distance, minDinstance, maxDinstance);
            }

            // --- 更新相机位置和旋转的逻辑保持不变 ---
            // 这部分逻辑不受开关影响，确保相机始终对准目标
            Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
            Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * disVector + player.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    /// <summary>
    /// 限制某一轴移动范围
    /// </summary>
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}