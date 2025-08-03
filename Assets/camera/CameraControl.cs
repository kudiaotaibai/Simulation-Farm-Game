using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // --- �����ľ�̬���أ�����ȫ�ֿ��� ---
    // �����ű�����UIManager������ͨ�� CameraControl.IsControlEnabled �����û����þ�ͷ����
    public static bool IsControlEnabled = true;

    [Header("Ŀ�����ٶ�")]
    public Transform player;    // ���׷��Ŀ��
    public float xSpeed = 200;  // X�᷽���϶��ٶ�
    public float ySpeed = 200;  // Y�᷽���϶��ٶ�
    public float mSpeed = 10;   // �Ŵ���С�ٶ�

    [Header("���Ʒ�Χ")]
    public float yMinLimit = -50; // ��Y����С�ƶ���Χ
    public float yMaxLimit = 50; // ��Y������ƶ���Χ
    public float minDinstance = 2; // ����ӽ���С����
    public float maxDinstance = 30; // ����ӽ�������

    // --- ˽�б��� ---
    private float distance = 10;  // ����ӽǾ���
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
        // ȷ����׷��Ŀ��
        if (player)
        {
            // --- �����޸ģ�ֻ���ڿ�������ʱ���Ŵ���������� ---
            if (IsControlEnabled)
            {
                // ��������Ҽ���ת
                if (Input.GetMouseButton(1))
                {
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                }

                // ��������������
                distance -= Input.GetAxis("Mouse ScrollWheel") * mSpeed;
                distance = Mathf.Clamp(distance, minDinstance, maxDinstance);
            }

            // --- �������λ�ú���ת���߼����ֲ��� ---
            // �ⲿ���߼����ܿ���Ӱ�죬ȷ�����ʼ�ն�׼Ŀ��
            Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
            Vector3 disVector = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * disVector + player.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    /// <summary>
    /// ����ĳһ���ƶ���Χ
    /// </summary>
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}