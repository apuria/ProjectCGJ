using UnityEngine;

/// <summary>
/// 缓存池布局管理器（可选功能）
/// 用于在场景层级中组织缓存池对象的父子关系，便于在编辑器中查看
/// </summary>
public class PoolLayout
{
    private readonly GameObject _root;
    private readonly bool _enabled;

    /// <summary>
    /// 布局是否启用
    /// </summary>
    public bool Enabled => _enabled;

    /// <summary>
    /// 根对象
    /// </summary>
    public GameObject Root => _root;

    /// <summary>
    /// 创建布局管理器
    /// </summary>
    /// <param name="enabled">是否启用布局。禁用时不会创建任何 GameObject。</param>
    public PoolLayout(bool enabled)
    {
        _enabled = enabled;
        if (_enabled)
        {
            _root = new GameObject("Pool");
        }
    }

    /// <summary>
    /// 为某个池创建一个子节点（抽屉）
    /// </summary>
    public GameObject CreateDrawerRoot(string drawerName)
    {
        if (!_enabled) return null;

        var child = new GameObject(drawerName);
        child.transform.SetParent(_root.transform);
        return child;
    }

    /// <summary>
    /// 将对象挂到指定父节点下
    /// </summary>
    public void Attach(GameObject obj, GameObject parent)
    {
        if (!_enabled || parent == null) return;
        obj.transform.SetParent(parent.transform);
    }

    /// <summary>
    /// 将对象从父节点断开
    /// </summary>
    public void Detach(GameObject obj)
    {
        if (!_enabled) return;
        obj.transform.SetParent(null);
    }

    /// <summary>
    /// 销毁布局根对象
    /// </summary>
    public void Destroy()
    {
        if (_root != null)
            GameObject.Destroy(_root);
    }
}
