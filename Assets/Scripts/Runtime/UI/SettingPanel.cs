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
    /// 将 UI 上的音频设置写回 InGameData 并持久化
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
    }
}
