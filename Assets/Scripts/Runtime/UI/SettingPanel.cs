using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Toggle musicToggle;
    public bool isMusicOn => musicToggle.isOn;
    public Toggle soundToggle;
    public bool isSoundOn => soundToggle.isOn;
    public Slider musicSlider;
    public float musicVolume => musicSlider.value;
    public Slider soundSlider;
    public float soundVolume => soundSlider.value;

    public Button btnChangeSettingPanel;

    protected void Start()
    {
        // Toggle 和 Slider 值变化时，立即应用音量到 MusicMgr
        musicToggle.onValueChanged.AddListener(_ => ApplyToMusicMgr());
        soundToggle.onValueChanged.AddListener(_ => ApplyToMusicMgr());
        musicSlider.onValueChanged.AddListener(_ => ApplyToMusicMgr());
        soundSlider.onValueChanged.AddListener(_ => ApplyToMusicMgr());

        btnChangeSettingPanel.onClick.AddListener(() =>
        {
            SaveSettings();
            this.gameObject.SetActive(false);
        });
    }

    public override void ShowMe()
    {
        LoadSettings();
    }

    public override void HideMe()
    {

    }

    /// <summary>
    /// 从 GameManager.InGameData 读取音频设置并初始化 UI
    /// </summary>
    private void LoadSettings()
    {
        var data = GameManager.Instance.inGameData;
        if (data == null) return;

        musicToggle.isOn = data.MusicOn;
        soundToggle.isOn = data.SfxOn;
        musicSlider.value = data.MusicVolume;
        soundSlider.value = data.SfxVolume;
    }

    /// <summary>
    /// 将 UI 上的音频设置写回 InGameData、持久化，并实时应用到 MusicMgr
    /// </summary>
    private void SaveSettings()
    {
        var data = GameManager.Instance.inGameData;
        if (data == null) return;

        data.MusicOn = musicToggle.isOn;
        data.SfxOn = soundToggle.isOn;
        data.MusicVolume = musicSlider.value;
        data.SfxVolume = soundSlider.value;

        GameManager.Instance.SaveSettingData();

        // 将设置实时应用到 MusicMgr
        ApplyToMusicMgr();
    }

    /// <summary>
    /// 将当前的音频设置实时应用到 MusicMgr
    /// </summary>
    private void ApplyToMusicMgr()
    {
        // 应用背景音乐设置：关闭时音量为 0，否则使用设置的音量
        float actualMusicVolume = isMusicOn ? musicVolume : 0f;
        MusicMgr.Instance.ChangeBKMusicValue(actualMusicVolume);

        // 如果音乐被关闭，同时停止当前正在播放的背景音乐
        if (!isMusicOn)
        {
            MusicMgr.Instance.StopBKMusic();
        }

        // 应用音效设置：关闭时音量为 0，否则使用设置的音量
        float actualSoundVolume = isSoundOn ? soundVolume : 0f;
        MusicMgr.Instance.ChangeSoundValue(actualSoundVolume);

        // 如果音效被关闭，同时停止当前所有正在播放的音效
        if (!isSoundOn)
        {
            MusicMgr.Instance.ClearSound();
        }
    }
}
