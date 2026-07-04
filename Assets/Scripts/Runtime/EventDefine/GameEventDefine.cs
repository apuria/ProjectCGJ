using UniFramework.Event;

public class GameEventDefine
{
    /// <summary>
    /// 继续游戏事件
    /// 加载玩家存档数据，恢复到上次的进度
    /// </summary>
    public class ContinueGame : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ContinueGame();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 开始新游戏事件
    /// 创建新的玩家数据，从 StartNode 开始游戏
    /// </summary>
    public class NewGame : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new NewGame();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 保存玩家进度事件
    /// 将当前玩家数据序列化保存到本地
    /// </summary>
    public class SaveProgress : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new SaveProgress();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 返回主菜单事件
    /// 退出当前游戏流程，返回主菜单界面
    /// </summary>
    public class ReturnToMainMenu : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ReturnToMainMenu();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 退出游戏事件
    /// 关闭游戏应用程序
    /// </summary>
    public class QuitGame : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new QuitGame();
            UniEvent.SendMessage(msg);
        }
    }

    public class SaveInGameData : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new SaveInGameData();
            UniEvent.SendMessage(msg);
        }
    }
}
