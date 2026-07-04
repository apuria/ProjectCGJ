using System;
using UnityEngine;

/// <summary>
/// 可序列化的数值结构体，用于 HP/MP/ATK/DEF/SPD 等属性
/// </summary>
[Serializable]
public struct Stat
{
    public float value;
}
