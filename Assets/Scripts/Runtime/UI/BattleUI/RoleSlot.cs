using UnityEngine;
using UnityEngine.UI;

public class RoleSlot : MonoBehaviour
{
    public RectTransform controller;
    public Image icon;
    public Transform VFXPoint;
    public void UpdateUI(Sprite icon, float scale)
    {
        controller.transform.localScale = new Vector3(scale, scale, scale);
        this.icon.sprite = icon;
        this.icon.color = Color.white;
    }
}
