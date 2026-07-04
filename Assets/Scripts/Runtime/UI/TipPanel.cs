using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TipPanel : BasePanel
{


#region 属性

    public TextMeshProUGUI tipText;

    public Button leftButton;
    public TextMeshProUGUI leftButtonText;
    public UnityEvent leftButtonEvent;

    public Button rightButton;
    public TextMeshProUGUI rightButtonText;
    public UnityEvent rightButtonEvent;

    private UnityAction currentLeftAction;
    private UnityAction currentRightAction;

#endregion
#region 生命周期

    protected void Awake()
    {

    }

    protected void Start()
    {
        leftButton.onClick.AddListener(() => OnLeftButtonClick());
        rightButton.onClick.AddListener(() => OnRightButtonClick());
    }

    protected void OnDestroy()
    {
        UIMgr.Instance.HidePanel<TipPanel>(true);
    }
#endregion

#region 逻辑控制

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {

    }

    /// <summary>
    /// 配置并显示提示面板
    /// </summary>
    public void Setup(string tip, string leftBtnText, UnityAction leftAction, string rightBtnText, UnityAction rightAction)
    {
        tipText.text = tip;

        // 左按钮
        if (!string.IsNullOrEmpty(leftBtnText))
        {
            leftButtonText.text = leftBtnText;
            currentLeftAction = leftAction;
            leftButton.gameObject.SetActive(true);
        }
        else
        {
            leftButton.gameObject.SetActive(false);
        }

        // 右按钮
        if (!string.IsNullOrEmpty(rightBtnText))
        {
            rightButtonText.text = rightBtnText;
            currentRightAction = rightAction;
            rightButton.gameObject.SetActive(true);
        }
        else
        {
            rightButton.gameObject.SetActive(false);
        }
    }

    private void OnLeftButtonClick()
    {
        currentLeftAction?.Invoke();
        Destroy(gameObject);
    }

    private void OnRightButtonClick()
    {
        currentRightAction?.Invoke();
        Destroy(gameObject);
    }

#endregion
}
