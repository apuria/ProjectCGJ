using System;
using UniFramework.Machine;

/// <summary>
/// 加载过渡信息，携带目标状态的切换信息
/// </summary>
public class LoadingInfo : IStateData
{
    /// <summary>
    /// 加载完成后要切换到的目标状态类型
    /// </summary>
    public Type targetStateType;
    /// <summary>
    /// 目标状态的标签
    /// </summary>
    public string targetTag;
    /// <summary>
    /// 目标状态的数据
    /// </summary>
    public IStateData targetData;
}
