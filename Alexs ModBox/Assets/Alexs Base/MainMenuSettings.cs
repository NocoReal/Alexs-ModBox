using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
 
public class MainMenuSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider, sensititySlider;
    [SerializeField] private TMP_Text volumeValueText, sensitityValueText, Title;
    [SerializeField] private Image Logo;
    private float volumeValue, sensitivityValue;
    [SerializeField] float lowerAlpha;
    bool cooldown = true, inSettings=false;

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 240;
        FirstTimeLoad();
        LoadSettings(true);
        StartCoroutine(SettingsCooldown());
    }
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void toggleLogo()
    {
        inSettings = !inSettings;
        Color col = Title.color;
        col.a = inSettings ? lowerAlpha:1f;
        Title.color = col;
        col = Logo.color;
        col.a = inSettings ? lowerAlpha:1f;
        Logo.color = col;
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

        AudioListener.volume = volumeValue / 100f;
        volumeSlider.value = volumeValue;
        sensititySlider.value = sensitivityValue;

        volumeValueText.text = volumeValue.ToString("F0");
        sensitityValueText.text = sensitivityValue.ToString("F2");
    }
    void FirstTimeLoad()
    {
        if (PlayerPrefs.GetInt("FirstTime") == 0)
        {
            cooldown = false;
            UpdateSettings();
        }
        else
        {
            PlayerPrefs.SetInt("FirstTime", 1);
        }
    }
}
