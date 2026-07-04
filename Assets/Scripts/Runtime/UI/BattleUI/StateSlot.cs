using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI roleName;
    public Slider hp;
    public Slider mp;
    public void UpdateUI(Sprite icon, string roleName, float hpPercentage, float mpPercentage)
    {
        this.icon.sprite = icon;
        this.roleName.text = roleName;
        hp.value = hpPercentage;
        mp.value = mpPercentage;
    }
}
