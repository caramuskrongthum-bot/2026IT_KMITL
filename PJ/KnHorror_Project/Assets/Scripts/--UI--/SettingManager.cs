using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("UI")]
    public GameObject settingPanel;
    public GameObject mainMenu;

    [Header("Audio Sliders")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Other Settings")]
    public Slider sensitivitySlider;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // โหลดค่าที่บันทึกไว้
        LoadSettings();

        // เริ่มเกมด้วย Main Menu
        settingPanel.SetActive(false);
        mainMenu.SetActive(true);
    }


    // =========================================================
    // OPEN SETTING
    // =========================================================

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingPanel.SetActive(true);
    }


    // =========================================================
    // CLOSE SETTING
    // =========================================================

    public void CloseSettings()
    {
        // บันทึกค่าก่อนออกจากหน้า Setting
        SaveSettings();

        // ปิด Setting
        settingPanel.SetActive(false);

        // กลับ Main Menu
        mainMenu.SetActive(true);
    }


    // =========================================================
    // SAVE SETTINGS
    // =========================================================

    public void SaveSettings()
    {
        // -------------------------
        // Audio
        // -------------------------

        PlayerPrefs.SetFloat(
            "MasterVolume",
            masterVolumeSlider.value
        );

        PlayerPrefs.SetFloat(
            "BGMVolume",
            bgmVolumeSlider.value
        );

        PlayerPrefs.SetFloat(
            "SFXVolume",
            sfxVolumeSlider.value
        );


        // -------------------------
        // Sensitivity
        // -------------------------

        if (sensitivitySlider != null)
        {
            PlayerPrefs.SetFloat(
                "Sensitivity",
                sensitivitySlider.value
            );
        }


        // บันทึกลงเครื่องจริง
        PlayerPrefs.Save();
    }


    // =========================================================
    // LOAD SETTINGS
    // =========================================================

    public void LoadSettings()
    {
        // -------------------------
        // Load Audio Values
        // -------------------------

        float masterValue =
            PlayerPrefs.GetFloat(
                "MasterVolume",
                1f
            );

        float bgmValue =
            PlayerPrefs.GetFloat(
                "BGMVolume",
                1f
            );

        float sfxValue =
            PlayerPrefs.GetFloat(
                "SFXVolume",
                1f
            );


        // -------------------------
        // Load Sensitivity
        // -------------------------

        float sensitivityValue =
            PlayerPrefs.GetFloat(
                "Sensitivity",
                1f
            );


        // -------------------------
        // Set Slider Values
        // -------------------------

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterValue;
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = bgmValue;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxValue;
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivityValue;
        }


        // -------------------------
        // Apply Audio Mixer
        // -------------------------

        SetMasterVolume(masterValue);
        SetBGMVolume(bgmValue);
        SetSFXVolume(sfxValue);
    }


    // =========================================================
    // SLIDER VALUE → DECIBEL
    // =========================================================

    private float SliderToDB(float value)
    {
        // ถ้า Slider = 0
        // ให้ลดเสียงลงไปที่ -80 dB
        if (value <= 0.0001f)
        {
            return -80f;
        }

        // แปลงค่า 0-1 เป็น Decibel
        return Mathf.Log10(value) * 20f;
    }


    // =========================================================
    // MASTER VOLUME
    // =========================================================

    public void SetMasterVolume(float value)
    {
        float dB = SliderToDB(value);

        bool success =
            audioMixer.SetFloat(
                "MasterVolume",
                dB
            );

        Debug.Log(
            "MASTER | Slider: "
            + value
            + " | dB: "
            + dB
            + " | SetFloat: "
            + success
        );
    }


    // =========================================================
    // BGM VOLUME
    // =========================================================

    public void SetBGMVolume(float value)
    {
        float dB = SliderToDB(value);

        bool success =
            audioMixer.SetFloat(
                "BGMVolume",
                dB
            );

        Debug.Log(
            "BGM | Slider: "
            + value
            + " | dB: "
            + dB
            + " | SetFloat: "
            + success
        );
    }


    // =========================================================
    // SFX VOLUME
    // =========================================================

    public void SetSFXVolume(float value)
    {
        float dB = SliderToDB(value);

        bool success =
            audioMixer.SetFloat(
                "SFXVolume",
                dB
            );

        Debug.Log(
            "SFX | Slider: "
            + value
            + " | dB: "
            + dB
            + " | SetFloat: "
            + success
        );
    }
}