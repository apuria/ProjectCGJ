using UnityEngine;

public class InGameData
{
    /// <summary>
    /// 音乐大小 (0.0 ~ 1.0)
    /// </summary>
    public float MusicVolume
    {
        get => _musicVolume;
        set => _musicVolume = Mathf.Clamp01(value);
    }
    private float _musicVolume = 1f;

    /// <summary>
    /// 音乐开关
    /// </summary>
    public bool MusicOn { get; set; } = true;

    /// <summary>
    /// 音效大小 (0.0 ~ 1.0)
    /// </summary>
    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = Mathf.Clamp01(value);
    }
    private float _sfxVolume = 1f;

    /// <summary>
    /// 音效开关
    /// </summary>
    public bool SfxOn { get; set; } = true;
}
