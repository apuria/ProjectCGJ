using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池代理 —— 绑定到一个 owner，管理池对象的获取与生命周期。
///
/// 每个需要使用对象池的类创建自己的 PoolProxy 实例，
/// 通过代理获取/归还对象，owner 销毁时一次性归还所有被追踪的对象。
///
/// <example>
/// 典型用法：
/// <code>
/// public class BulletHell : MonoBehaviour
/// {
///     private PoolProxy _proxy;
///
///     void Awake()     => _proxy = new PoolProxy(this);
///     void OnDestroy() => _proxy.Dispose();  // 归还所有 destroyWithOwner=true 的对象
///
///     void Fire()
///     {
///         // 子弹跟随 owner 自动回池
///         GameObject bullet = _proxy.Get("Bullet", destroyWithOwner: true);
///         // 特效独立管理，手动归还
///         GameObject vfx    = _proxy.Get("MuzzleFlash", destroyWithOwner: false);
///     }
/// }
/// </code>
/// </example>
/// </summary>
public class PoolProxy
{
    private readonly MonoBehaviour _owner;
    private readonly List<GameObject> _trackedList = new List<GameObject>();
    private bool _isDisposed;

    /// <summary>
    /// 代理是否已释放。释放后 Get 返回 null，Push/PushAll 静默忽略。
    /// </summary>
    public bool IsDisposed => _isDisposed;

    /// <summary>
    /// 当前被代理追踪的对象数量（仅 destroyWithOwner=true 的对象）
    /// </summary>
    public int TrackedCount => _trackedList.Count;

    /// <summary>
    /// 创建对象池代理。
    /// </summary>
    /// <param name="owner">
    /// 拥有此代理的 MonoBehaviour。可为 null（纯 C# 类场景），
    /// 为 null 时请务必手动调用 Dispose。
    /// </param>
    public PoolProxy(MonoBehaviour owner = null)
    {
        _owner = owner;
    }

    /// <summary>
    /// 从对象池获取一个 GameObject。
    /// </summary>
    /// <param name="prefabName">预设体名称（对应 Resources 路径或工厂标识）</param>
    /// <param name="destroyWithOwner">
    /// <b>true</b> — 对象被代理追踪，owner 销毁（调用 Dispose）时自动归还到池。<br/>
    /// <b>false</b> — 对象不受代理生命周期管理，调用方通过 Push 手动归还。
    /// </param>
    /// <returns>激活的 GameObject；代理已释放时返回 null</returns>
    public GameObject Get(string prefabName, bool destroyWithOwner = false)
    {
        if (_isDisposed)
        {
            PoolLogger.Error($"PoolProxy: 代理已释放，无法获取对象。请求预设体: '{prefabName}'");
            return null;
        }

        GameObject obj = PoolMgr.GetObj(prefabName);

        if (destroyWithOwner && obj != null)
        {
            _trackedList.Add(obj);
        }

        return obj;
    }

    /// <summary>
    /// 将一个对象归还到池中，并解除代理对其的追踪。
    /// 即使对象未通过 destroyWithOwner=true 获取，也可以调用此方法归还。
    /// </summary>
    /// <param name="obj">要归还的对象</param>
    public void Push(GameObject obj)
    {
        if (_isDisposed)
            return;

        if (obj == null)
        {
            PoolLogger.Warning("PoolProxy.Push: 传入的对象为 null，已忽略。");
            return;
        }

        _trackedList.Remove(obj);
        PoolMgr.PushObj(obj);
    }

    /// <summary>
    /// 归还所有被代理追踪的对象到池中，并清空追踪列表。
    /// 通常在场景切换或需要清空时手动调用。
    /// </summary>
    public void PushAll()
    {
        if (_isDisposed)
            return;

        // 倒序遍历 — 即使 SetActive(false) 触发回调导致列表变化，也能正确遍历
        for (int i = _trackedList.Count - 1; i >= 0; i--)
        {
            GameObject obj = _trackedList[i];
            if (obj != null)
            {
                PoolMgr.PushObj(obj);
            }
            // obj 为 null（已被第三方 Destroy）的情况：静默跳过
        }

        _trackedList.Clear();
    }

    /// <summary>
    /// 释放代理，归还全部被追踪对象。
    /// <b>owner 的 OnDestroy 中必须调用此方法。</b>
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        PushAll();
        _isDisposed = true;
    }

    /// <summary>
    /// 析构函数 —— 用于开发期检测：如果 owner 忘记调用 Dispose，
    /// 当代理被 GC 回收时打印错误，帮助排查对象泄漏。
    /// </summary>
    ~PoolProxy()
    {
        if (!_isDisposed && _trackedList.Count > 0)
        {
            PoolLogger.Error(
                $"PoolProxy 未被 Dispose！还有 {_trackedList.Count} 个对象未归还。" +
                $"如果 owner 是 MonoBehaviour，请在其 OnDestroy 中调用 proxy.Dispose()。");
        }
    }
}
