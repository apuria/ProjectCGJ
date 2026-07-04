using TMPro;
using UnityEngine;

/// <summary>
/// 伤害/治疗数字 —— 向上升起 + 渐隐消失
/// 动画结束后自动归还对象池
/// </summary>
public class HurtNum : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Vector3 _startPos;
    private float _timer;

    private const float Duration = 0.8f;
    private const float RiseDistance = 1f;

    /// <summary>
    /// 传入原始数值（伤害），内部乘 10 显示，白色文字
    /// </summary>
    public void Setup(int value)
    {
        _startPos = transform.position;
        text.SetText((value * 10).ToString());
        text.color = Color.white;
    }

    /// <summary>
    /// 传入原始数值（治疗），内部乘 10 显示，绿色文字
    /// </summary>
    public void SetupHeal(int value)
    {
        _startPos = transform.position;
        text.SetText((value * 10).ToString());
        text.color = Color.green;
    }

    void OnEnable()
    {
        _timer = 0f;

        Color c = text.color;
        c.a = 1f;
        text.color = c;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float t = _timer / Duration;

        // 动画结束，直接销毁
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 向上升
        transform.position = _startPos + Vector3.up * (RiseDistance * t);

        // 渐隐
        Color c = text.color;
        c.a = 1f - t;
        text.color = c;
    }
}
