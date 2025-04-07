using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private InputActionReference inputEscape;
    [SerializeField] private GameObject SettingsParent, EscapeMenuParent,EscapeMenuUI, Player, Crosshair;
    [SerializeField] private Slider volumeSlider, sensititySlider;
    [SerializeField] private TMP_Text volumeValueText, sensitityValueText;
    private float volumeValue,sensitivityValue;

    private bool inSettings = false, inEscape = false, cooldown = true;
    private PlayerMovement PM;
    private SpawnMenu SM;

    private void OnEnable() // we subscribe to the inputs onEnable
    {
        inputEscape.action.performed += EscapeButtonPressed;

    }
    private void OnDestroy() // unsubscribe from them if we're destroyed
    {
        inputEscape.action.performed -= EscapeButtonPressed;

    }
    private void OnDisable() // unsubscribe from them if we're disabled
    {
        inputEscape.action.performed -= EscapeButtonPressed;

    }

    void Awake()
    {
        EscapeMenuParent.SetActive(false);
        SettingsParent.SetActive(false);

        PM = Player.GetComponent<PlayerMovement>();
        SM = GetComponentInParent<SpawnMenu>();

        LoadSettings(true);
        StartCoroutine(SettingsCooldown());

    }
    void EscapeButtonPressed(InputAction.CallbackContext obj)
    {
        ToggleEscapeMenu();
    }
    public void ToggleEscapeMenu()
    {
        if (inSettings)
        {
            ToggleSettingsMenu();
            return;
        }
        inEscape = !inEscape;
        if (SM.spawnMenu)
            SM.ToggleMenu();
        EscapeMenuParent.SetActive(inEscape);
        if (!inEscape)//if on
        {
            Time.timeScale = 1.0f;
            PM.CantAccel = false;
            PM.CantRotateCam = false;
            PM.CantShoot = false;
            SM.escapeMenuOn = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EscapeMenuParent.SetActive(false);// set off
            Crosshair.SetActive(true);
        }
        else
        {
            Time.timeScale = 0;
            PM.CantAccel = true;
            PM.CantRotateCam = true;
            PM.CantShoot = true;
            SM.escapeMenuOn = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EscapeMenuParent.SetActive(true);// set off
            Crosshair.SetActive(false);
        }
    }

    public void ToggleSettingsMenu()
    {
        inSettings = !inSettings;
        SettingsParent.SetActive(inSettings);
        EscapeMenuUI.SetActive(!inSettings);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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

        PlayerPrefs.SetFloat("SensitivityValue",sensitivityValue);
        PlayerPrefs.SetFloat("VolumeValue",volumeValue);

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
        PM.mouseSensitivity = sensitivityValue;
        volumeSlider.value = volumeValue;
        sensititySlider.value = sensitivityValue;

        volumeValueText.text = volumeValue.ToString("F0");
        sensitityValueText.text = sensitivityValue.ToString("F2");
    }
}
