using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BranchPanel : BasePanel
{
    /*
    1. 接收 BranchState 发来的 ShowUI 事件，根据分支列表生成按钮
    2. 玩家点击按钮后发送 ChooseBranch 事件
    */

    #region 属性

    public GameObject slot;
    public Transform content;

    private List<GameObject> buttonList = new();

    #endregion

    #region 生命周期

    protected void Awake()
    {
        eventGroup = new();
        eventGroup.AddListener<BranchEventDefine.ShowUI>(OnHandleEventMessage);
    }

    protected void Start()
    {

    }

    protected void OnDestroy()
    {
        eventGroup.RemoveAllListener();
    }
    #endregion

    #region 逻辑控制

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {

    }

    public void UpdateUI(List<Branch> branches)
    {
        // 清空旧按钮
        foreach (var btn in buttonList)
        {
            Destroy(btn);
        }
        buttonList.Clear();

        if (branches == null) return;
        if (slot == null || content == null)
        {
            Debug.LogError("BranchPanel: slot 或 content 未在 Inspector 中赋值");
            return;
        }

        // 按顺序生成按钮
        for (int i = 0; i < branches.Count; i++)
        {
            if (branches[i] == null) continue;

            int index = i; // 闭包捕获
            var go = Instantiate(slot, content);
            buttonList.Add(go);

            // 设置按钮文本
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = branches[i].text;
            }

            // 监听点击
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    BranchEventDefine.ChooseBranch.SendEventMessage(index);
                });
            }
        }
    }

    #endregion

    #region 事件监听

    public void OnHandleEventMessage(IEventMessage message)
    {
        if (message is BranchEventDefine.ShowUI showUI)
        {
            UpdateUI(showUI.branches);
        }
    }

    #endregion
}
