using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerPickUpObjects : MonoBehaviour
{
    [SerializeField] private Transform Camera;
    [SerializeField] private PlayerToolSpawn PTS;

    [SerializeField] private InputActionReference inputPickup, inputThrow;

    bool hasPickup = false;
    /*
    private void OnEnable()
    {
        inputPickup.action.performed +=
        inputThrow.action.performed +=
    }
    private void OnDisable()
    {
        inputPickup.action.performed -=
        inputThrow.action.performed -=
    }
    private void OnDestroy()
    {
        inputPickup.action.performed -=
        inputThrow.action.performed -=
    }

    private void FixedUpdate()
    {
        
    }
    */
}