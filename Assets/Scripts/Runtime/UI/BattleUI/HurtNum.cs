using TMPro;
using UnityEngine;

/// <summary>
/// 伤害数字 —— 向上升起 + 渐隐消失
/// </summary>
public class HurtNum : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Vector3 _startPos;
    private float _timer;

    private const float Duration = 0.8f;
    private const float RiseDistance = 40f;

    /// <summary>
    /// 传入原始数值，内部乘 10 显示
    /// </summary>
    public void Setup(int value)
    {
        _startPos = transform.localPosition;
        text.SetText((value * 10).ToString());
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

        // 动画结束，回池
        if (t >= 1f)
        {
            PoolMgr.PushObj(gameObject);
            return;
        }

        // 向上升
        transform.localPosition = _startPos + Vector3.up * (RiseDistance * t);

        // 渐隐
        Color c = text.color;
        c.a = 1f - t;
        text.color = c;
    }
}
