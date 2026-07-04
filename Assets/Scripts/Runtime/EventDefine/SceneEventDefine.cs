using UniFramework.Event;

public class SceneEventDefine
{
    /// <summary>
    /// 更新地图UI事件
    /// </summary>
    public class UpdateMapUI : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new UpdateMapUI();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 开始游戏事件
    /// 触发 StartNode 中配置的游戏流程
    /// </summary>
    public class StartGame : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new StartGame();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 结束游戏事件
    /// 触发 EndNode 中配置的游戏流程
    /// </summary>
    public class EndGame : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new EndGame();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 节点游戏事件（中间 6 个节点）
    /// 根据当前 PlayerData.NowFlow 触发对应节点的流程
    /// </summary>
    public class NodeGame : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new NodeGame();
            UniEvent.SendMessage(msg);
        }
    }
}
