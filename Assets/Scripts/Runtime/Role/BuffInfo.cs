using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("数值：isPercentage=false 时为绝对值；isPercentage=true 时 0.3=30%")]
    public float value;
    [Tooltip("百分比模式：value 按目标最大值百分比计算（如 HP/MP 按 maxHp/maxMp，ATK 按当前攻击力）")]
    public bool isPercentage;
}
