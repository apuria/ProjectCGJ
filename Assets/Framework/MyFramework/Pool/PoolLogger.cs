using System.Diagnostics;

/// <summary>
/// 缓存池模块内部日志
/// </summary>
internal static class PoolLogger
{
    [Conditional("DEBUG")]
    public static void Log(string info)
    {
        UnityEngine.Debug.Log(info);
    }

    public static void Warning(string info)
    {
        UnityEngine.Debug.LogWarning(info);
    }

    public static void Error(string info)
    {
        UnityEngine.Debug.LogError(info);
    }
}
