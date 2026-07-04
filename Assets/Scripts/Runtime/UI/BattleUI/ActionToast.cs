using TMPro;
using UnityEngine;

/// <summary>
/// 敌人行动提示 —— 显示"XX 使用了 普通攻击/防御/技能"，停留后渐隐回池
/// </summary>
public class ActionToast : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private const float Duration = 2.2f;
    private const float FadeOutStart = 1.2f;

    private float _timer;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// 设置显示内容：敌人名字 + 行动类型
    /// </summary>
    public void Setup(string enemyName, EnemyActionType actionType)
    {
        string actionText = actionType switch
        {
            EnemyActionType.Attack => "普通攻击",
            EnemyActionType.Skill => "技能",
            EnemyActionType.Defence => "防御",
            _ => "未知行动"
        };

        text.SetText($"{enemyName} 使用了 {actionText}");
    }

    void OnEnable()
    {
        _timer = 0f;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // 渐隐阶段
        if (_timer >= FadeOutStart)
        {
            float fadeT = (_timer - FadeOutStart) / (Duration - FadeOutStart);
            float alpha = 1f - fadeT;
            if (_canvasGroup != null)
                _canvasGroup.alpha = alpha;
        }

        // 动画结束，失活
        if (_timer >= Duration)
        {
            gameObject.SetActive(false);
        }
    }
}
