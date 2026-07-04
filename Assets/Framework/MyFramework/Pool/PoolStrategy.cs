using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池获取策略类型
/// </summary>
public enum PoolStrategyType
{
    /// <summary>LIFO: 后进先出 — 总是返回最近归还的对象</summary>
    Stack,
    /// <summary>LRU: 最近最少使用 — 缓存为空时淘汰最旧活跃对象（保留当前默认行为）</summary>
    LRU,
    /// <summary>FIFO: 先进先出 — 总是返回最早归还的对象</summary>
    FIFO,
}

#region 策略实现

/// <summary>
/// LIFO 栈策略。最简单，无活跃跟踪。纯缓存，不处理超上限情况。
/// </summary>
internal class StackPoolStrategy : IPoolStrategy
{
    private readonly Stack<GameObject> _idleStack = new Stack<GameObject>();

    public int Count => _idleStack.Count;

    public GameObject TryPop()
    {
        return _idleStack.Count > 0 ? _idleStack.Pop() : null;
    }

    public void Push(GameObject obj)
    {
        _idleStack.Push(obj);
    }

    public void RegisterActive(GameObject obj)
    {
        // 栈策略不跟踪活跃对象
    }

    public void Clear()
    {
        _idleStack.Clear();
    }
}

/// <summary>
/// LRU 最近最少使用策略。缓存为空时淘汰最旧活跃对象。
/// </summary>
internal class LRUPoolStrategy : IPoolStrategy
{
    private readonly Stack<GameObject> _idleStack = new Stack<GameObject>();
    private readonly List<GameObject> _activeList = new List<GameObject>();

    public int Count => _idleStack.Count;
    public int ActiveCount => _activeList.Count;

    public GameObject TryPop()
    {
        if (_idleStack.Count > 0)
        {
            return _idleStack.Pop();
        }

        // 缓存为空，淘汰最旧的活跃对象（LRU 驱逐）
        if (_activeList.Count > 0)
        {
            GameObject obj = _activeList[0];
            _activeList.RemoveAt(0);
            return obj;
        }

        return null;
    }

    public void Push(GameObject obj)
    {
        _idleStack.Push(obj);
        _activeList.Remove(obj);
    }

    public void RegisterActive(GameObject obj)
    {
        _activeList.Add(obj);
    }

    public void Clear()
    {
        _idleStack.Clear();
        _activeList.Clear();
    }
}

/// <summary>
/// FIFO 先进先出策略。使用队列管理空闲对象。
/// </summary>
internal class FIFOPoolStrategy : IPoolStrategy
{
    private readonly Queue<GameObject> _idleQueue = new Queue<GameObject>();
    private readonly List<GameObject> _activeList = new List<GameObject>();

    public int Count => _idleQueue.Count;
    public int ActiveCount => _activeList.Count;

    public GameObject TryPop()
    {
        if (_idleQueue.Count > 0)
        {
            return _idleQueue.Dequeue();
        }

        // 缓存为空，淘汰最旧的活跃对象
        if (_activeList.Count > 0)
        {
            GameObject obj = _activeList[0];
            _activeList.RemoveAt(0);
            return obj;
        }

        return null;
    }

    public void Push(GameObject obj)
    {
        _idleQueue.Enqueue(obj);
        _activeList.Remove(obj);
    }

    public void RegisterActive(GameObject obj)
    {
        _activeList.Add(obj);
    }

    public void Clear()
    {
        _idleQueue.Clear();
        _activeList.Clear();
    }
}

#endregion
