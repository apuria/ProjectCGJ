using System;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;

[Serializable]
public class LoadingText
{
    [TextArea(5, 10)]
    public string text;
}

[CreateAssetMenu(fileName = "LoadingTexts", menuName = "Load/New LoadingTexts", order = 1)]
public class LoadingTexts : ScriptableObject, IStateData
{
    public List<LoadingText> texts = new List<LoadingText>();
}


