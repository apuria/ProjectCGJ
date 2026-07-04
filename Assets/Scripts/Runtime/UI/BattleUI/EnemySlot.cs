using UnityEngine;
using UnityEngine.UI;

public class EnemySlot : MonoBehaviour
{
    public RectTransform controller;
    public Image icon;
    public Transform VFXPoint;
    public Transform Tip;
    public void UpdateUI(Sprite icon, float scale)
    {
        controller.transform.localScale = new Vector3(scale, scale, scale);
        this.icon.sprite = icon;
    }

    /// <summary>
    /// 设置行动提示 Tip 的显隐
    /// </summary>
    public void SetTipActive(bool active)
    {
        if (Tip != null)
            Tip.gameObject.SetActive(active);
    }
}
