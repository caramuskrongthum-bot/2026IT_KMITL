using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingPanel;
    public GameObject mainMenu;

    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider sensitivitySlider;

    private void Start()
    {
        LoadSettings();

        settingPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingPanel.SetActive(false);
        mainMenu.SetActive(true);

        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        masterVolumeSlider.value =
            PlayerPrefs.GetFloat("MasterVolume", 1f);

        bgmVolumeSlider.value =
            PlayerPrefs.GetFloat("BGMVolume", 1f);

        sfxVolumeSlider.value =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        sensitivitySlider.value =
            PlayerPrefs.GetFloat("Sensitivity", 1f);
    }
}