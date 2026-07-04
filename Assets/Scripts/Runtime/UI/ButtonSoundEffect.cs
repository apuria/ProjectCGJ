using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂载在按钮上，在按下时播放点击音效反馈。
/// 需要按钮所在 Canvas 上有 GraphicRaycaster。
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundEffect : MonoBehaviour, IPointerDownHandler
{
    [Header("音效参数")]
    [SerializeField]
    private string clickSoundName = "点击";

    private Button _button;

    // ========== 生命周期 ==========

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // ========== 指针事件 ==========

    public void OnPointerDown(PointerEventData eventData)
    {
        // 按钮不可交互时不播放音效
        if (_button != null && !_button.interactable) return;

        MusicMgr.Instance.PlaySound(clickSoundName);
    }

    // ========== 公共 API ==========

    /// <summary>
    /// 为指定按钮添加点击音效组件（如果尚未添加）。
    /// 适用于需要在代码中动态为按钮添加效果的场景。
    /// </summary>
    public static ButtonSoundEffect AddTo(Button button)
    {
        if (button == null) return null;

        var existing = button.GetComponent<ButtonSoundEffect>();
        if (existing != null) return existing;

        return button.gameObject.AddComponent<ButtonSoundEffect>();
    }
}
