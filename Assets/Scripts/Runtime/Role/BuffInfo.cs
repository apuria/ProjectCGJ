using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    
}

public enum StatType
{
    MAXHP,
    HP,
    MAXMP,
    MP,
    ATK,
    DEF,
    SPD,
}

[CreateAssetMenu(fileName = "BuffInfo", menuName = "BuffInfo", order = 1)]
public class BuffInfo : ScriptableObject
{
    public string buffName;
    public StatType buffType;
    public Sprite buffIcon;
    public float value;
    public int duration;
}
