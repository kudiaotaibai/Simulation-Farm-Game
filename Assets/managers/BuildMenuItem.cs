using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuItem : MonoBehaviour
{
    [Header("UI元素引用")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;

    private Button itemButton; // 我们在代码内部获取它

    /// <summary>
    /// 接收数据并设置UI项的显示。由UIManager调用。
    /// </summary>
    /// 

    public void Setup(GameObject buildingPrefab, UIManager uiManager)
    {
        // 在脚本开始时，获取自己身上的Button组件
        itemButton = GetComponent<Button>();

        // 从建筑预制体的Building脚本中获取信息来更新UI
        Building buildingData = buildingPrefab.GetComponent<Building>();
        if (buildingData != null)
        {
            nameText.text = buildingData.buildingName;
            // 这里我们暂时使用固定成本，未来可以从buildingData里读取
            costText.text = "成本: 100 G";
            if (iconImage != null && buildingData.icon != null)
            {
                iconImage.sprite = buildingData.icon;
            }
        }

        // 为按钮添加点击事件
        if (itemButton != null)
        {
            // 先移除所有旧的监听，防止重复绑定
            itemButton.onClick.RemoveAllListeners();
            // 添加新的监听，当被点击时，调用UIManager的OnBuildRequest方法
            itemButton.onClick.AddListener(() => {
                uiManager.OnBuildRequest(buildingPrefab);
            });
        }
    }
}