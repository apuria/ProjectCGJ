using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniFramework.Machine
{
    /// <summary>
    /// 状态机管理器：编排 IStateNode 的创建、切换、挂起、销毁
    /// </summary>
    public class StateMachine
    {
        private readonly Dictionary<string, IStateNode> _suspendedNodes = new();
        private readonly Dictionary<string, object> _blackboard = new();

        /// <summary>
        /// 当前活跃的节点
        /// </summary>
        private IStateNode _curNode;

        /// <summary>
        /// 当前活跃节点的 Tag
        /// </summary>
        private string _curNodeTag;

        /// <summary>
        /// 导航栈：记录状态切换路径，支持多层 BackToPrevState
        /// 栈顶为最近一次 forward 切换前所在的节点 Tag
        /// </summary>
        private readonly Stack<string> _navStack = new();

        /// <summary>
        /// 当前运行的节点 Tag
        /// </summary>
        public string CurrentNodeTag => _curNodeTag ?? string.Empty;

        /// <summary>
        /// 之前运行的节点 Tag（导航栈顶，用于 BackToPrevState）
        /// </summary>
        public string PreviousNodeTag => _navStack.Count > 0 ? _navStack.Peek() : string.Empty;

        public StateMachine() { }

        // =========================================================
        // Update
        // =========================================================

        /// <summary>
        /// 更新当前活跃的节点
        /// </summary>
        public void Update()
        {
            _curNode?.OnUpdate();
        }

        // =========================================================
        // 启动
        // =========================================================

        /// <summary>
        /// 启动状态机，进入入口节点
        /// </summary>
        /// <typeparam name="TState">实现 IStateNode 且有 new() 的状态类型</typeparam>
        /// <param name="tag">状态实例的唯一标识，用于切换和恢复</param>
        /// <param name="data">绑定到此状态的数据（可选）</param>
        public void Run<TState>(string tag, IStateData data = null) where TState : IStateNode, new()
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            if (_curNode != null)
            {
                UniLogger.Warning($"State machine already running at '{_curNodeTag}', fallback to SwitchTo.");
                SwitchTo<TState>(tag, data);
                return;
            }

            // 如果挂起池中已有同名 tag 的节点，先销毁
            if (_suspendedNodes.TryGetValue(tag, out var oldSuspended))
            {
                UniLogger.Log($"Destroy suspended node with same tag: {tag}");
                oldSuspended.OnDispose();
                _suspendedNodes.Remove(tag);
            }

            var node = new TState();
            node.OnCreate(this, data);

            _curNode = node;
            _curNodeTag = tag;
            _navStack.Clear();

            UniLogger.Log($"Start state machine: {tag}");
            _curNode.OnEnter();
        }

        // =========================================================
        // 切换
        // =========================================================

        /// <summary>
        /// 切换状态：退出当前节点，进入目标节点
        /// </summary>
        /// <typeparam name="TState">实现 IStateNode 且有 new() 的状态类型</typeparam>
        /// <param name="tag">状态实例的唯一标识，用于切换和恢复</param>
        /// <param name="data">绑定到此状态的数据（可选）</param>
        /// <param name="destroy">是否销毁前一个状态，默认为 true（销毁）；false 保留到挂起池可恢复</param>
        public void SwitchTo<TState>(string tag, IStateData data = null, bool destroy = true) where TState : IStateNode, new()
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            if (_curNode == null)
            {
                Run<TState>(tag, data);
                return;
            }

            var node = new TState();
            node.OnCreate(this, data);

            string mode = destroy ? "destroy" : "suspend";
            string savedInfo = destroy ? "" : $", saved '{_curNodeTag}'";
            UniLogger.Log($"{_curNodeTag} --> {tag} ({mode} mode{savedInfo})");

            // 退出当前节点
            _curNode.OnExit();

            // 如果目标在挂起池中，先销毁旧的挂起节点（同名事件触发时销毁旧节点）
            if (_suspendedNodes.TryGetValue(tag, out var oldSuspended))
            {
                UniLogger.Log($"Destroy suspended node with same tag: {tag}");
                oldSuspended.OnDispose();
                _suspendedNodes.Remove(tag);
            }

            if (!destroy)
            {
                // 挂起当前节点，存入字典以备恢复
                _suspendedNodes[_curNodeTag] = _curNode;
            }
            else
            {
                // 销毁当前节点
                _curNode.OnDispose();
            }

            // 将当前节点 Tag 推入导航栈，供后续 BackToPrevState 回溯
            _navStack.Push(_curNodeTag);
            _curNode = node;
            _curNodeTag = tag;

            _curNode.OnEnter();
        }

        /// <summary>
        /// 切换至挂起池中指定 tag 的节点（非泛型重载，用于恢复已挂起的状态）
        /// </summary>
        /// <param name="tag">挂起节点的 Tag</param>
        /// <param name="destroy">是否销毁当前状态，默认为 true（销毁）；false 保留到挂起池可恢复</param>
        public void SwitchTo(string tag, bool destroy = true)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            if (!_suspendedNodes.TryGetValue(tag, out var targetNode))
            {
                UniLogger.Error($"Can not found suspended state node: {tag}");
                return;
            }

            if (_curNode == null)
            {
                // 没有当前节点，直接恢复
                _suspendedNodes.Remove(tag);
                _navStack.Clear();
                _curNode = targetNode;
                _curNodeTag = tag;

                UniLogger.Log($"Restore state: {tag}");
                _curNode.OnEnter();
                return;
            }

            string mode = destroy ? "destroy" : "suspend";
            string savedInfo = destroy ? "" : $", saved '{_curNodeTag}'";
            UniLogger.Log($"{_curNodeTag} --> {tag} ({mode} mode{savedInfo})");

            _curNode.OnExit();

            // 如果目标在挂起池中，先移除（即将被激活）
            _suspendedNodes.Remove(tag);

            if (!destroy)
            {
                _suspendedNodes[_curNodeTag] = _curNode;
            }
            else
            {
                _curNode.OnDispose();
            }

            // 从导航栈弹出目标 Tag（返回路径），不推入当前节点
            if (_navStack.Count > 0 && _navStack.Peek() == tag)
            {
                _navStack.Pop();
            }
            _curNode = targetNode;
            _curNodeTag = tag;

            _curNode.OnEnter();
        }

        // =========================================================
        // 通过 System.Type 切换（供事件系统等反射场景使用）
        // =========================================================

        private static readonly MethodInfo _switchToGenericMethod;

        static StateMachine()
        {
            foreach (var m in typeof(StateMachine).GetMethods())
            {
                if (m.Name == "SwitchTo" && m.IsGenericMethod)
                {
                    _switchToGenericMethod = m;
                    break;
                }
            }
        }

        /// <summary>
        /// 通过 System.Type 切换状态（非泛型重载，供事件系统调用）
        /// </summary>
        /// <param name="stateType">实现 IStateNode 且有 new() 的状态类型</param>
        /// <param name="tag">状态实例的唯一标识</param>
        /// <param name="data">绑定到此状态的数据（可选）</param>
        /// <param name="destroy">是否销毁前一个状态，默认为 true</param>
        public void SwitchTo(System.Type stateType, string tag, IStateData data = null, bool destroy = true)
        {
            if (stateType == null)
                throw new ArgumentNullException(nameof(stateType));

            try
            {
                var generic = _switchToGenericMethod.MakeGenericMethod(stateType);
                generic.Invoke(this, new object[] { tag, data, destroy });
            }
            catch (Exception ex)
            {
                UniLogger.Error($"Failed to switch to state type {stateType.Name}: {ex.Message}");
            }
        }

        // =========================================================
        // 查询
        // =========================================================

        /// <summary>
        /// 查询指定 Tag 的节点是否被挂起
        /// </summary>
        public bool IsSuspended(string tag)
        {
            return _suspendedNodes.ContainsKey(tag);
        }

        /// <summary>
        /// 获取挂起池中指定 Tag 的节点（不会恢复该节点）
        /// </summary>
        public IStateNode GetSuspendedNode(string tag)
        {
            _suspendedNodes.TryGetValue(tag, out var node);
            return node;
        }

        // =========================================================
        // 清理
        // =========================================================

        /// <summary>
        /// 清空所有挂起的状态节点，并执行每个节点的 OnDispose
        /// </summary>
        public void ClearSuspendedNodes()
        {
            if (_suspendedNodes.Count == 0)
                return;

            UniLogger.Log($"Clearing all suspended nodes, count: {_suspendedNodes.Count}");

            foreach (var node in _suspendedNodes.Values)
            {
                node.OnDispose();
            }

            _suspendedNodes.Clear();
        }

        // =========================================================
        // 黑板
        // =========================================================

        /// <summary>
        /// 设置黑板数据（跨节点共享）
        /// </summary>
        public void SetBlackboardValue(string key, object value)
        {
            if (_blackboard.ContainsKey(key) == false)
                _blackboard.Add(key, value);
            else
                _blackboard[key] = value;
        }

        /// <summary>
        /// 获取黑板数据
        /// </summary>
        public object GetBlackboardValue(string key)
        {
            if (_blackboard.TryGetValue(key, out object value))
                return value;

            UniLogger.Warning($"Not found blackboard value: {key}");
            return null;
        }
    }
}
