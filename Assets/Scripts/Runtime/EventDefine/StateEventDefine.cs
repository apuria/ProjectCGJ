using UniFramework.Event;
using UniFramework.Machine;

public class StateEventDefine
{
    /// <summary>
    /// 切换到指定状态的事件消息
    /// </summary>
    public class ChangeState : IEventMessage
    {
        public string tag;
        public IStateData data;
        public bool destroy;
        public System.Type stateType;

        /// <summary>
        /// 发送切换状态事件
        /// </summary>
        /// <typeparam name="TState">实现 IStateNode 且有 new() 的状态类型</typeparam>
        /// <param name="tag">状态实例的唯一标识</param>
        /// <param name="data">绑定到此状态的数据（可选）</param>
        /// <param name="destroy">是否销毁当前状态，默认为 true</param>
        public static void SendEventMessage<TState>(string tag, IStateData data = null, bool destroy = true) where TState : IStateNode, new()
        {
            var msg = new ChangeState
            {
                tag = tag,
                data = data,
                destroy = destroy,
                stateType = typeof(TState)
            };
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 回到上一个状态的事件消息
    /// </summary>
    public class BackToPrevState : IEventMessage
    {
        public bool destroy;

        /// <summary>
        /// 发送回到上一个状态事件
        /// </summary>
        /// <param name="destroy">是否销毁当前状态，默认为 true</param>
        public static void SendEventMessage(bool destroy = true)
        {
            var msg = new BackToPrevState { destroy = destroy };
            UniEvent.SendMessage(msg);
        }
    }
}
