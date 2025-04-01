using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SpawnMenu : MonoBehaviour
{
    [SerializeField] private GameObject Player, Menu, Crosshair, ColorRep,HSVPanel,RGBPanel;
    [SerializeField] private InputActionReference TurnOnMenuButton;
    private PlayerMovement PM;
    private PlayerToolSpawn PTS;
    [SerializeField] private TMP_Text alphaValueText, hueValueText, saturationValueText, valueValueText,redValueText,greenValueText,blueValueText;
    [SerializeField] private Slider alphaSlider, hueSlider, saturationSlider, valueSlider,redSlider,greenSlider,blueSlider;
    [SerializeField] private Toggle alphaToggle,hsvToggle;
    private Image ColorImage;
    private Color finalColor;
    private Vector2 CursorPos;
    private bool hsvOn = false;
    [HideInInspector]
    public bool escapeMenuOn = false,spawnMenu = false;

    private void Awake()
    {
        PM = Player.GetComponent<PlayerMovement>();
        PTS = Player.GetComponent<PlayerToolSpawn>();

        alphaValueText.text = alphaSlider.value.ToString("F2");
        hueValueText.text = hueSlider.value.ToString("F2");
        saturationValueText.text = saturationSlider.value.ToString("F2");
        valueValueText.text = valueSlider.value.ToString("F2");
        redValueText.text = redSlider.value.ToString("F2");
        greenValueText.text = greenSlider.value.ToString("F2");
        blueValueText.text = blueSlider.value.ToString("F2");

        finalColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        finalColor.a = alphaSlider.value;

        ColorImage = ColorRep.GetComponent<Image>();
        ColorImage.color = finalColor;
        PTS.MaterialChangeColor(finalColor);

        Menu.gameObject.SetActive(false);

        HSVPanel.gameObject.SetActive(hsvOn);
        RGBPanel.gameObject.SetActive(!hsvOn);

        TurnOnMenuButton.action.performed += ToggleMenuInput;
        TurnOnMenuButton.action.canceled += ToggleMenuInput;

        CursorPos = new Vector2(Screen.width / 2, Screen.height / 2);
    }
    void ToggleMenuInput(InputAction.CallbackContext obj)
    {
        ToggleMenu();
    }
    public void ToggleMenu()
    {
        if (escapeMenuOn)
            return;
        spawnMenu = !spawnMenu;
        if (!spawnMenu)//if on
        {
            CursorPos = Input.mousePosition;
            PM.CantAccel = false;
            PM.CantRotateCam = false;
            PM.CantShoot = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Menu.gameObject.SetActive(false);// set off
            Crosshair.gameObject.SetActive(true);
        }
        else
        {
            PM.CantAccel = true;
            PM.CantRotateCam = true;
            PM.CantShoot = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Mouse.current.WarpCursorPosition(CursorPos);
            Menu.gameObject.SetActive(true);// else set on
            Crosshair.gameObject.SetActive(false);
        }
    }
    public void UpdatedColor()
    {
        if (hsvOn)
        {
            hueValueText.text = hueSlider.value.ToString("F2");
            saturationValueText.text = saturationSlider.value.ToString("F2");
            valueValueText.text = valueSlider.value.ToString("F2");
            // convert to rgb
            finalColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
            // set slider value
            redSlider.value = finalColor.r;
            greenSlider.value = finalColor.g;
            blueSlider.value = finalColor.b;
            // change text value
            redValueText.text = redSlider.value.ToString("F2");
            greenValueText.text = greenSlider.value.ToString("F2");
            blueValueText.text = blueSlider.value.ToString("F2");
        }
        else
        {
            redValueText.text = redSlider.value.ToString("F2");
            greenValueText.text = greenSlider.value.ToString("F2");
            blueValueText.text = blueSlider.value.ToString("F2");
            //set the final color
            finalColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
            // convert rgb to hsv in a temp vector3
            Vector3 temp; Color.RGBToHSV(finalColor, out temp.x, out temp.y, out temp.z);
            // set slider value
            hueSlider.value = temp.x;
            saturationSlider.value = temp.y;
            valueSlider.value = temp.z;
            // change text value
            hueValueText.text = hueSlider.value.ToString("F2");
            saturationValueText.text = saturationSlider.value.ToString("F2");
            valueValueText.text = valueSlider.value.ToString("F2");

        }
        // set alpha
        alphaValueText.text = alphaSlider.value.ToString("F2");
        finalColor.a = alphaSlider.value;
        // apply color to image and pass it to material
        ColorImage.color = finalColor;
        PTS.MaterialChangeColor(finalColor);
    }

    public void MenuSpawnObject(int ItemId)
    {
        PTS.ChangeSpawnObject(ItemId);
    }

    public void toggleAlpha()
    {
        PTS.MaterialChangeSurfaceType(alphaToggle.isOn);
    }
    public void toggleHsv()
    {
        hsvOn = hsvOn ? false : true;
        HSVPanel.gameObject.SetActive(hsvOn);
        RGBPanel.gameObject.SetActive(!hsvOn);
    }
}
