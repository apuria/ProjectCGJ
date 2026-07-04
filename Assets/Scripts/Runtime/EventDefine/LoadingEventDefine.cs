using UniFramework.Event;

public class LoadingEventDefine
{
    /// <summary>
    /// 显示加载文本
    /// </summary>
    public class ShowLoadingText : IEventMessage
    {
        public string loadText;

        public static void SendEventMessage(string loadText)
        {
            var msg = new ShowLoadingText();
            msg.loadText = loadText;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 加载完毕，通知 LoadingPanel 可以开始收尾动效
    /// </summary>
    public class LoadComplete : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new LoadComplete();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// LoadingPanel 动效结束，通知 LoadingState 切换到目标状态
    /// </summary>
    public class AnimationEnd : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new AnimationEnd();
            UniEvent.SendMessage(msg);
        }
    }
}
