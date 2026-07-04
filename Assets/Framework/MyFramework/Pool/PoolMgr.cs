using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 缓存池(对象池)模块 — 负责 GameObject 对象池
/// C# 数据结构/逻辑类对象池请使用 UniFramework.Reference.UniReference
/// </summary>
public static class PoolMgr
{
    private static bool _isInitialized = false;
    private static PoolLayout _layout;
    private static Func<string, GameObject> _prefabFactory;
    private static readonly Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// 布局是否启用（初始化后不可更改）
    /// </summary>
    public static bool IsOpenLayout { get; private set; } = true;

    /// <summary>
    /// 默认获取策略（可在初始化前或初始化时设置）
    /// </summary>
    public static PoolStrategyType DefaultStrategy { get; set; } = PoolStrategyType.LRU;

    #region 生命周期

    /// <summary>
    /// 初始化对象池系统。
    /// 如未调用，首次使用 GetObj/PushObj 时会自动以默认参数初始化。
    /// </summary>
    /// <param name="enableLayout">是否启用编辑器层级布局</param>
    /// <param name="prefabFactory">
    /// 预设体加载工厂。为 null 时使用默认的 Resources.Load 方式。
    /// 可传入 Addressables.LoadAssetAsync 等自定义加载逻辑。
    /// </param>
    public static void Initialize(bool enableLayout = true, Func<string, GameObject> prefabFactory = null)
    {
        if (_isInitialized)
        {
            PoolLogger.Warning("PoolMgr 已经初始化，忽略重复调用。");
            return;
        }

        _isInitialized = true;
        IsOpenLayout = enableLayout;
        _layout = new PoolLayout(enableLayout);
        _prefabFactory = prefabFactory ?? DefaultPrefabFactory;

        PoolLogger.Log("PoolMgr initialized.");
    }

    /// <summary>
    /// 销毁对象池系统，清理所有池和布局对象
    /// </summary>
    public static void Destroy()
    {
        if (!_isInitialized) return;

        ClearAll();
        _layout?.Destroy();
        _layout = null;
        _prefabFactory = null;
        _isInitialized = false;

        PoolLogger.Log("PoolMgr destroyed.");
    }

    #endregion

    #region GameObject 池 API

    /// <summary>
    /// 从池中获取一个 GameObject。
    /// 如果是首次请求该预设体，会自动创建对应的池抽屉。
    /// </summary>
    /// <param name="prefabName">预设体名称（对应 Resources 路径或工厂使用的标识）</param>
    /// <returns>激活的 GameObject</returns>
    public static GameObject GetObj(string prefabName)
    {
        EnsureInitialized();

        if (!_pools.TryGetValue(prefabName, out var pool))
        {
            // 首次请求该预设体：实例化第一个对象并创建池
            return CreateFirstAndPool(prefabName);
        }

        if (pool.IdleCount > 0)
        {
            // 有空闲对象，直接从策略中弹出
            return pool.Pop(_layout);
        }

        if (pool.NeedCreate)
        {
            // 未达容量上限，实例化新对象
            GameObject newObj = InstantiatePrefab(prefabName);
            pool.RegisterActive(newObj, _layout);
            return newObj;
        }

        // 已达容量上限且无空闲对象 —— 通过策略驱逐
        return pool.Pop(_layout);
    }

    /// <summary>
    /// 将一个 GameObject 归还给池。
    /// 对象会通过其 name 匹配对应的池抽屉。
    /// </summary>
    /// <param name="obj">要归还的对象</param>
    public static void PushObj(GameObject obj)
    {
        EnsureInitialized();

        if (obj == null)
        {
            PoolLogger.Warning("PushObj: 传入的对象为 null，已忽略。");
            return;
        }

        if (_pools.TryGetValue(obj.name, out var pool))
        {
            pool.Push(obj, _layout);
        }
        else
        {
            PoolLogger.Warning($"未找到 '{obj.name}' 对应的池抽屉，对象将被销毁。");
            GameObject.Destroy(obj);
        }
    }

    /// <summary>
    /// 清除所有池（保留初始化状态和布局）
    /// 使用场景：切场景时
    /// </summary>
    public static void ClearAll()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();
        _pools.Clear();

        PoolLogger.Log("PoolMgr: 所有池已清除。");
    }

    #endregion

    #region 内部实现

    private static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            // 自动初始化：保持与旧版 BaseManager 懒加载单例的兼容性
            Initialize(enableLayout: true);
        }
    }

    /// <summary>
    /// 首次请求某个预设体时：实例化→验证 PoolObj→创建池抽屉→注册
    /// </summary>
    private static GameObject CreateFirstAndPool(string prefabName)
    {
        // 实例化第一个对象
        GameObject firstObj = InstantiatePrefab(prefabName);

        // 验证 PoolObj 组件（池容量上限由预设体上的 PoolObj 脚本决定）
        PoolObj poolObj = firstObj.GetComponent<PoolObj>();
        if (poolObj == null)
        {
            GameObject.Destroy(firstObj);
            throw new InvalidOperationException(
                $"预设体 '{prefabName}' 缺少 PoolObj 组件，无法确定池容量上限。" +
                "请在预设体上挂载 PoolObj 脚本并设置 maxNum。");
        }

        // 创建策略和池抽屉
        var strategy = CreateStrategy(DefaultStrategy);
        var pool = new GameObjectPool(prefabName, poolObj.maxNum, strategy, _layout);

        // 注册第一个对象
        pool.RegisterActive(firstObj, _layout);
        _pools.Add(prefabName, pool);

        return firstObj;
    }

    private static GameObject InstantiatePrefab(string prefabName)
    {
        GameObject obj = _prefabFactory(prefabName);
        if (obj == null)
            throw new InvalidOperationException($"无法加载预设体: '{prefabName}'");

        // 去掉 Unity 自动添加的 "(Clone)" 后缀，确保名字与池键匹配
        obj.name = prefabName;
        return obj;
    }

    private static IPoolStrategy CreateStrategy(PoolStrategyType type)
    {
        return type switch
        {
            PoolStrategyType.Stack => new StackPoolStrategy(),
            PoolStrategyType.LRU => new LRUPoolStrategy(),
            PoolStrategyType.FIFO => new FIFOPoolStrategy(),
            _ => new LRUPoolStrategy(),
        };
    }

    /// <summary>
    /// 默认预设体工厂：使用 Resources.Load
    /// </summary>
    private static GameObject DefaultPrefabFactory(string name)
    {
        var prefab = Resources.Load<GameObject>(name);
        if (prefab == null)
            throw new InvalidOperationException($"无法从 Resources 加载预设体: '{name}'");
        return GameObject.Instantiate(prefab);
    }

    #endregion
}
