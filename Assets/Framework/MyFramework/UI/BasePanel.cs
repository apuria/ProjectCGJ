using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{

    public EventGroup eventGroup;
    /// <summary>
    /// 面板显示时会调用的逻辑
    /// </summary>
    public abstract void ShowMe();

    /// <summary>
    /// 面板隐藏时会调用的逻辑
    /// </summary>
    public abstract void HideMe();

    protected virtual void ClickBtn(string btnName)
    {

    }

    protected virtual void SliderValueChange(string sliderName, float value)
    {

    }

    protected virtual void ToggleValueChange(string sliderName, bool value)
    {

    }

}
