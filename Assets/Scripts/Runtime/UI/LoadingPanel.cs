using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : BasePanel
{
    public Image point;
    public TextMeshProUGUI text;

    public CanvasGroup canvasGroup;

    private float rotateSpeed = 180f; // 每秒旋转度数
    private float delayDuration = 0.5f;
    private float fadeDuration = 0.5f;

    private bool isLoadComplete = false;
    private float delayTimer = 0f;
    private float fadeTimer = 0f;
    private bool isFading = false;
    private bool isDestroyed = false;

#region 生命周期

    protected void Awake()
    {
        eventGroup = new();
        eventGroup.AddListener<LoadingEventDefine.ShowLoadingText>(OnHandleEventMessage);
        eventGroup.AddListener<LoadingEventDefine.LoadComplete>(OnHandleEventMessage);
    }

    protected void OnDestroy()
    {
        isDestroyed = true;
        eventGroup.RemoveAllListener();
    }

    void Update()
    {
        if (isDestroyed)
            return;
        // 旋转指针
        if (point != null)
            point.transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // 收到加载完成后的延迟
        if (isLoadComplete && !isFading)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= delayDuration)
            {
                isFading = true;
            }
        }

        // 渐变消失
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeDuration);
            canvasGroup.alpha = 1f - t;

            if (t >= 1f)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                // 先重置状态，再发送事件
                // 因为 AnimationEnd 事件会同步触发状态切换 → 本面板被 Destroy，
                // 必须在 SendEventMessage 之前完成所有状态修改
                isLoadComplete = false;
                isFading = false;
                LoadingEventDefine.AnimationEnd.SendEventMessage();
            }
        }
    }

#endregion

#region 逻辑控制

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {
        ResetPanel();
    }

    private void ResetPanel()
    {
        isLoadComplete = false;
        isFading = false;
        delayTimer = 0f;
        fadeTimer = 0f;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

#endregion

#region 事件监听

    public void OnHandleEventMessage(IEventMessage message)
    {
        if (message is LoadingEventDefine.ShowLoadingText showText)
        {
            text.text = showText.loadText;
        }
        else if (message is LoadingEventDefine.LoadComplete)
        {
            isLoadComplete = true;
            delayTimer = 0f;
        }
    }

#endregion
}
