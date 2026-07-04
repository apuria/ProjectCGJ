using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/New Dialogue", order = 0)]
public class DialogueSetting : ScriptableObject, IStateData
{
    [Header("UI Settings")]
    [Tooltip("是否显示返回按钮")]
    public bool hasReturnButton = false;

    public bool hasBackground = false;
    public Sprite BackGround;

    public List<Speaker> speakers;

    public List<Dialogue> dialogues;

}
