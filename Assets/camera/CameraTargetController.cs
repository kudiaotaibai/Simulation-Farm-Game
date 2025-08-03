using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    [Header("�ƶ��ٶ�")]
    public float panSpeed = 30f;      // ˮƽƽ���ٶ�
    public float verticalSpeed = 25f; // ��ֱ�����ٶ�

    [Header("�ƶ���Χ����")]
    public Vector2 panLimitMin; // ˮƽ��Χ��Сֵ (X, Z)
    public Vector2 panLimitMax; // ˮƽ��Χ���ֵ (X, Z)
    public float minHeight = 10f; // ��С�߶� (Y)
    public float maxHeight = 80f; // ���߶� (Y)

    private Transform cameraTransform;

    void Start()
    {
        // ����Ϸ��ʼʱ��ȡ������������ã��Լ�����ȷ���ƶ�����
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // --- �����޸ģ�ֻ���ڿ�������ʱ��������ִ�������ƶ��߼� ---
        if (PlayerInteraction.IsInteractionEnabled)
        {
            if (cameraTransform == null)
            {
                // �������Ϸ����������������٣��������»�ȡ
                if (Camera.main != null) cameraTransform = Camera.main.transform;
                else return;
            }

            // --- 1. ˮƽƽ�� (W/A/S/D) ---
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

            // --- 2. ��ֱ�ƶ� (�ո�/Shift) ---
            if (Input.GetKey(KeyCode.Space))
            {
                // ���¿ո������ƶ�
                transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // ����Shift�������ƶ�
                transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
            }
        }

        // --- ���Ʊ߽���߼������ڿ������棬ȷ��Ŀ���ʼ���ڵ�ͼ�� ---
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, panLimitMin.x, panLimitMax.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minHeight, maxHeight); // ����Y��߶�
        clampedPos.z = Mathf.Clamp(clampedPos.z, panLimitMin.y, panLimitMax.y);
        transform.position = clampedPos;
    }
}