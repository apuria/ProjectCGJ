using UnityEngine;
using UnityEngine.UI;

public class HPandMP : MonoBehaviour
{
    public Image HP;
    public Image MP;
    public void UpdateUI(float hpPercentage, float mpPercentage)
    {
        HP.fillAmount = hpPercentage;
        MP.fillAmount = mpPercentage;
    }
}
