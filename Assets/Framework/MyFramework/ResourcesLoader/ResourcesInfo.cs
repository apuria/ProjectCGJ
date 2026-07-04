using System.Threading;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源信息基类 主要用于里式替换原则 父类容器装子类对象
/// </summary>
public abstract class ResInfoBase {
    //引用计数
    public int refCount;
}

/// <summary>
/// 资源信息对象 主要用于存储资源信息 异步加载委托信息 异步加载 取消令牌
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
public class ResInfo<T> : ResInfoBase
{
    //资源
    public T asset;
    //主要用于异步加载结束后 传递资源到外部的委托
    public UnityAction<T> callBack;
    //用于取消正在进行的异步加载（UniTask）
    public CancellationTokenSource cts;
    //决定引用计数为0时 是否真正需要移除
    public bool isDel;


    public void AddRefCount()
    {
        ++refCount;
    }

    public void SubRefCount()
    {
        --refCount;
        if (refCount < 0)
            Debug.LogError("引用计数小于0了，请检查使用和卸载是否配对执行");
    }
}