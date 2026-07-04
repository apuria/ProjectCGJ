using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.Events;

public class TipPanelEventDefine
{
    public class ShowTip : IEventMessage
    {
        /// <summary>
        /// 提示文本
        /// </summary>
        public string tipText;
        /// <summary>
        /// 左按钮文字
        /// </summary>
        public string leftButtonText;
        /// <summary>
        /// 左按钮点击事件
        /// </summary>
        public UnityAction leftButtonAction;
        /// <summary>
        /// 右按钮文字
        /// </summary>
        public string rightButtonText;
        /// <summary>
        /// 右按钮点击事件
        /// </summary>
        public UnityAction rightButtonAction;

        public static void SendEventMessage(string tipText, string leftButtonText = null, UnityAction leftButtonAction = null, string rightButtonText = null, UnityAction rightButtonAction = null)
        {
            var msg = new ShowTip();
            msg.tipText = tipText;
            msg.leftButtonText = leftButtonText;
            msg.leftButtonAction = leftButtonAction;
            msg.rightButtonText = rightButtonText;
            msg.rightButtonAction = rightButtonAction;
            UniEvent.SendMessage(msg);
        }
    }
}
