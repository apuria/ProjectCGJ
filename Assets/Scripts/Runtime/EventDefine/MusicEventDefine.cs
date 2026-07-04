using UniFramework.Event;

public class MusicEventDefine
{
    /// <summary>
    /// 播放背景音乐事件
    /// </summary>
    public class PlayBGM : IEventMessage
    {
        /// <summary>
        /// 音乐资源名称
        /// </summary>
        public string musicName;

        /// <summary>
        /// 是否循环播放，默认 true
        /// </summary>
        public bool loop = true;

        /// <summary>
        /// 淡入时间（秒），默认 0
        /// </summary>
        public float fadeInTime;

        public static void SendEventMessage(string musicName, bool loop = true, float fadeInTime = 0f)
        {
            var msg = new PlayBGM
            {
                musicName = musicName,
                loop = loop,
                fadeInTime = fadeInTime
            };
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 播放音效事件
    /// </summary>
    public class PlaySFX : IEventMessage
    {
        /// <summary>
        /// 音效资源名称
        /// </summary>
        public string sfxName;

        /// <summary>
        /// 音量缩放（0.0 ~ 1.0），默认 1.0
        /// </summary>
        public float volumeScale = 1f;

        public static void SendEventMessage(string sfxName, float volumeScale = 1f)
        {
            var msg = new PlaySFX
            {
                sfxName = sfxName,
                volumeScale = volumeScale
            };
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 停止背景音乐事件
    /// </summary>
    public class StopBGM : IEventMessage
    {
        /// <summary>
        /// 淡出时间（秒），默认 0
        /// </summary>
        public float fadeOutTime;

        public static void SendEventMessage(float fadeOutTime = 0f)
        {
            var msg = new StopBGM { fadeOutTime = fadeOutTime };
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 停止音效事件
    /// </summary>
    public class StopSFX : IEventMessage
    {
        /// <summary>
        /// 指定音效名称（为空则停止所有音效）
        /// </summary>
        public string sfxName;

        public static void SendEventMessage(string sfxName = null)
        {
            var msg = new StopSFX { sfxName = sfxName };
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 修改背景音乐音量事件
    /// </summary>
    public class ChangeBGMVolume : IEventMessage
    {
        /// <summary>
        /// 目标音量（0.0 ~ 1.0）
        /// </summary>
        public float volume;

        /// <summary>
        /// 渐变时间（秒），默认 0 即立即生效
        /// </summary>
        public float fadeTime;

        public static void SendEventMessage(float volume, float fadeTime = 0f)
        {
            var msg = new ChangeBGMVolume
            {
                volume = volume,
                fadeTime = fadeTime
            };
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 修改音效音量事件
    /// </summary>
    public class ChangeSFXVolume : IEventMessage
    {
        /// <summary>
        /// 目标音量（0.0 ~ 1.0）
        /// </summary>
        public float volume;

        /// <summary>
        /// 渐变时间（秒），默认 0 即立即生效
        /// </summary>
        public float fadeTime;

        public static void SendEventMessage(float volume, float fadeTime = 0f)
        {
            var msg = new ChangeSFXVolume
            {
                volume = volume,
                fadeTime = fadeTime
            };
            UniEvent.SendMessage(msg);
        }
    }
}
