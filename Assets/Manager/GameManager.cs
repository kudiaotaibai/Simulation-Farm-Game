using UnityEngine;
using System;

/// <summary>
/// 游戏的核心管理器。
/// 现在只负责管理全局时间流逝。
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- 单例模式 ---
    public static GameManager Instance { get; private set; }

    [Header("时间系统")]
    public float secondsPerTimeUnit = 10f;
    public int currentTimeUnit = 1;

    public event Action OnTimeUnitPassed;

    private float timer;

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
    }

    void Start()
    {
        // 游戏一开始，更新一次时间UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimeUI(currentTimeUnit);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= secondsPerTimeUnit)
        {
            timer -= secondsPerTimeUnit;
            currentTimeUnit++;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTimeUI(currentTimeUnit);
            }

            OnTimeUnitPassed?.Invoke();
        }
    }
}