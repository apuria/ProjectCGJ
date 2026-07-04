using UnityEngine;
using UnityEngine.UI;

public class EnemyChooseSlot : MonoBehaviour
{
    public Button button;
    public Image icon;
    public void UpdateUI(Sprite icon)
    {
        this.icon.sprite = icon;
    }
}
