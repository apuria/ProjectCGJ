using UnityEngine;

/// <summary>
/// 对象池获取策略接口
/// </summary>
public interface IPoolStrategy
{
    /// <summary>
    /// 策略中当前缓存的对象数量
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 尝试从缓存中取出一个对象。返回 null 表示缓存为空。
    /// </summary>
    GameObject TryPop();

    /// <summary>
    /// 将一个对象放入缓存
    /// </summary>
    void Push(GameObject obj);

    /// <summary>
    /// 向策略注册一个新创建的对象（加入活跃跟踪）
    /// </summary>
    void RegisterActive(GameObject obj);

    /// <summary>
    /// 清空策略中的所有对象
    /// </summary>
    void Clear();
}
