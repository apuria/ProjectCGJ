using UnityEngine;

/// <summary>
/// 单个预设体类型的对象池抽屉
/// 通过策略决定获取/归还行为，通过 PoolLayout 决定层级组织
/// </summary>
public class GameObjectPool
{
    private readonly string _prefabName;
    private readonly int _maxNum;
    private readonly IPoolStrategy _strategy;
    private readonly GameObject _layoutNode;

    /// <summary>
    /// 预设体名称
    /// </summary>
    public string PrefabName => _prefabName;

    /// <summary>
    /// 空闲对象数量
    /// </summary>
    public int IdleCount => _strategy.Count;

    /// <summary>
    /// 活跃对象数量
    /// </summary>
    public int ActiveCount { get; private set; }

    /// <summary>
    /// 是否需要创建新对象（活跃数未达上限）
    /// </summary>
    public bool NeedCreate => ActiveCount < _maxNum;

    /// <summary>
    /// 创建对象池抽屉
    /// </summary>
    /// <param name="prefabName">预设体名称（对应 Resources 路径或工厂标识）</param>
    /// <param name="maxNum">池容量上限</param>
    /// <param name="strategy">获取/归还策略</param>
    /// <param name="layout">布局管理器（可为 null）</param>
    public GameObjectPool(string prefabName, int maxNum, IPoolStrategy strategy, PoolLayout layout)
    {
        _prefabName = prefabName ?? throw new System.ArgumentNullException(nameof(prefabName));
        _maxNum = maxNum;
        _strategy = strategy ?? throw new System.ArgumentNullException(nameof(strategy));

        // 为这个池创建布局子节点
        _layoutNode = layout?.CreateDrawerRoot(prefabName);
    }

    /// <summary>
    /// 从池中弹出一个对象
    /// </summary>
    public GameObject Pop(PoolLayout layout)
    {
        // 尝试从空闲缓存中获取
        GameObject obj = _strategy.TryPop();

        if (obj != null)
        {
            // 从空闲缓存中取到了
            obj.SetActive(true);
            layout?.Detach(obj);
            ActiveCount++;
            return obj;
        }

        // 空闲缓存为空 —— 必须通过策略驱逐活跃对象（超上限情况）
        // 调用方应在此前检查 NeedCreate 并在容量未满时直接实例化新对象
        obj = TryEvictActive();
        if (obj != null)
        {
            obj.SetActive(true);
            layout?.Detach(obj);
            return obj;
        }

        throw new System.InvalidOperationException(
            $"GameObjectPool '{_prefabName}': 无法获取对象。空闲缓存为空且无活跃对象可驱逐。");
    }

    /// <summary>
    /// 将一个对象归还到池中
    /// </summary>
    public void Push(GameObject obj, PoolLayout layout)
    {
        if (obj == null) return;

        obj.SetActive(false);
        layout?.Attach(obj, _layoutNode);
        _strategy.Push(obj);
        ActiveCount--;
    }

    /// <summary>
    /// 注册一个新创建的对象到活跃列表
    /// </summary>
    public void RegisterActive(GameObject obj, PoolLayout layout)
    {
        _strategy.RegisterActive(obj);
        ActiveCount++;
        layout?.Detach(obj);
    }

    /// <summary>
    /// 清空池中所有对象
    /// </summary>
    public void Clear()
    {
        _strategy.Clear();
        ActiveCount = 0;
    }

    /// <summary>
    /// 驱逐最旧的活跃对象（由策略决定）
    /// </summary>
    private GameObject TryEvictActive()
    {
        // 再次调用 TryPop()，策略在空闲为空时会自行处理驱逐
        // LRU/FIFO 策略在空闲为空时驱逐活跃列表[0]
        // Stack 策略在空闲为空时直接返回 null
        var evicted = _strategy.TryPop();
        if (evicted != null)
        {
            // 被驱逐的对象需要重新注册到活跃列表（它将作为"最新"活跃对象）
            _strategy.RegisterActive(evicted);
        }
        return evicted;
    }
}
