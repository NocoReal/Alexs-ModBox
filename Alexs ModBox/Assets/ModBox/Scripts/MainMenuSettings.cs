using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
public class MainMenuSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider, sensititySlider;
    [SerializeField] private TMP_Text volumeValueText, sensitityValueText;
    private float volumeValue, sensitivityValue;
    bool cooldown = true;

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 240;
        LoadSettings(true);
        StartCoroutine(SettingsCooldown());
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public void Quit()
    {
        Application.Quit();
    }
    IEnumerator SettingsCooldown()
    {
        yield return new WaitForSeconds(0.3f);
        cooldown = false;
    }
    public void UpdateSettings()
    {
        if (cooldown)
            return;

        sensitivityValue = sensititySlider.value;
        volumeValue = volumeSlider.value;

        PlayerPrefs.SetFloat("SensitivityValue", sensitivityValue);
        PlayerPrefs.SetFloat("VolumeValue", volumeValue);

        LoadSettings(false);
    }

    void LoadSettings(bool onLoad)
    {
        if (onLoad)
        {
            sensitivityValue = PlayerPrefs.GetFloat("SensitivityValue");
            volumeValue = PlayerPrefs.GetFloat("VolumeValue");
        }
        
        AudioListener.volume = volumeValue/100f;
        volumeSlider.value = volumeValue;
        sensititySlider.value = sensitivityValue;

        volumeValueText.text = volumeValue.ToString("F0");
        sensitityValueText.text = sensitivityValue.ToString("F2");
    }
}
