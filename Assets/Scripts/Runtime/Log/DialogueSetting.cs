using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Setting/Dialogue", order = 0)]
public class DialogueSetting : ScriptableObject, IStateData
{
    [Header("Music Settings")]
    [Tooltip("背景音乐名称（留空则播放 DefaultBGM）")]
    public string bgmName;

    [Header("UI Settings")]
    [Tooltip("是否显示返回按钮")]
    public bool hasReturnButton = false;

    public bool hasBackground = false;
    public Sprite BackGround;

    public List<Speaker> speakers;

    public List<Dialogue> dialogues;

}
