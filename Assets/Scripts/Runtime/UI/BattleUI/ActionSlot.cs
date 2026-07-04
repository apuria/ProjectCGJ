using UnityEngine;
using UnityEngine.UI;

public class ActionSlot : MonoBehaviour
{
    public Image icon;

    public void UpdateUI(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}
