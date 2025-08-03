using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI amountText;

    /// <summary>
    /// 用传入的数据更新这个UI项的显示
    /// </summary>
    public void UpdateDisplay(Sprite icon, int amount)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null); // 如果没有图标，就隐藏Image
        }
        if (amountText != null)
        {
            amountText.text = amount.ToString();
        }
    }
}