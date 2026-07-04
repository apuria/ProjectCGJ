using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI skillName;

    public void UpdateUI(Sprite icon, string skillName)
    {
        this.icon.sprite = icon;
        this.skillName.text = skillName;
    }
}
