using UniFramework.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂载在按钮上，提供按下缩放 + 释放回弹的点击反馈效果。
/// 需要按钮所在 Canvas 上有 GraphicRaycaster。
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放参数")]
    [SerializeField, Range(0.5f, 1f)]
    private float pressedScale = 0.9f;

    [SerializeField, Range(0.02f, 0.3f)]
    private float pressDuration = 0.06f;

    [SerializeField, Range(0.05f, 0.5f)]
    private float releaseDuration = 0.15f;

    private Vector3 _originalScale;
    private TweenHandle _currentTween;
    private Button _button;

    // ========== 生命周期 ==========

    private void Awake()
    {
        _originalScale = transform.localScale;
        _button = GetComponent<Button>();
    }

    private void OnDestroy()
    {
        _currentTween?.Abort();
    }

    private void OnDisable()
    {
        // 按钮被禁用时终止动画并还原缩放
        _currentTween?.Abort();
        transform.localScale = _originalScale;
    }

    // ========== 指针事件 ==========

    public void OnPointerDown(PointerEventData eventData)
    {
        // 按钮不可交互时不播放动画
        if (_button != null && !_button.interactable) return;

        _currentTween?.Abort();

        var targetScale = _originalScale * pressedScale;
        _currentTween = UniTween.Play(
            transform.TweenScaleTo(pressDuration, targetScale)
                .SetEase(TweenEase.Quad.EaseOut),
            gameObject
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _currentTween?.Abort();

        _currentTween = UniTween.Play(
            transform.TweenScaleTo(releaseDuration, _originalScale)
                .SetEase(TweenEase.Back.EaseOut),
            gameObject
        );
    }

    // ========== 公共 API ==========

    /// <summary>
    /// 脚本在被添加时自动记录初始缩放。如果外部修改了原始缩放，
    /// 调用此方法更新内部记录的基准值。
    /// </summary>
    public void UpdateOriginalScale()
    {
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// 为指定按钮添加缩放效果组件（如果尚未添加）。
    /// 适用于需要在代码中动态为按钮添加效果的场景。
    /// </summary>
    public static ButtonScaleEffect AddTo(Button button)
    {
        if (button == null) return null;

        var existing = button.GetComponent<ButtonScaleEffect>();
        if (existing != null) return existing;

        return button.gameObject.AddComponent<ButtonScaleEffect>();
    }
}
