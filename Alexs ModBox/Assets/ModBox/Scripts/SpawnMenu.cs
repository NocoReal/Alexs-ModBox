using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SpawnMenu : MonoBehaviour
{
    [SerializeField] private GameObject Player,Menu;
    [SerializeField] private InputActionReference TurnOnMenuButton;
    private PlayerMovement PM;
    private PlayerToolSpawn PTS;
    [SerializeField] private TMP_Text alphaValueText;
    [SerializeField] private Slider alphaSlider;

    private void Awake()
    {
        alphaValueText.text = alphaSlider.value.ToString("F1");
        PM = Player.GetComponent<PlayerMovement>();
        PTS = Player.GetComponent<PlayerToolSpawn>();
        Menu.gameObject.SetActive(false);
        TurnOnMenuButton.action.performed += ToggleMenu;
        TurnOnMenuButton.action.canceled += ToggleMenu;
    }
    private void ToggleMenu(InputAction.CallbackContext obj)
    {
        if (Menu.activeSelf)//if on
        {
            PM.CanMoveCamera = true;
            PTS.MenuIsOn = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Menu.gameObject.SetActive(false);// set off
        }
        else
        {
            PM.CanMoveCamera = false;
            PTS.MenuIsOn = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Menu.gameObject.SetActive(true);// else set on
        }
    }
    public void UpdatedAlpha()
    {
        alphaValueText.text = alphaSlider.value.ToString("F1");
        PTS.MaterialChangeAlpha(alphaSlider.value);
    }

    public void MenuSpawnObject(int ItemId)
    {
        //PTS.ToolSpawnObject(ItemId);
        PTS.ChangeSpawnObject(ItemId);
    }
}
