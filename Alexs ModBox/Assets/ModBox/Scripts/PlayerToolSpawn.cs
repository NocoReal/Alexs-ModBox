using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerToolSpawn : MonoBehaviour
{
    private Transform playerCamera;
    private int localItemId = 0, materialId = 0;
    private float alpha = 1.0f;
    [SerializeField] private float MaxDistance = 20;
    [SerializeField] private InputActionReference inputSpawn;
    [SerializeField] private List<GameObject> objectList;
    [SerializeField] private List<Material> materialList;

    [HideInInspector] public bool MenuIsOn = false;
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }
    public enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }
    private void Awake()
    {
        playerCamera = GetComponentInParent<PlayerMovement>().Camera;
        inputSpawn.action.performed += ToolSpawnInput;
    }
    public void ToolSpawnObject(int ItemId) // called either locally or by spawnMenu
    {
        if (MenuIsOn) return; // cant spawn items if menu is on

        float distance = MaxDistance;// local instance of maxDistance
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hitI, MaxDistance))//we check if we hit something along the way
            distance = hitI.distance;

        Vector3 spawnPos = playerCamera.position + (playerCamera.forward * distance)+ Vector3.up /2;

        GameObject tempObj = Instantiate(objectList[ItemId], spawnPos, Quaternion.identity);
        tempObj.GetComponent<Renderer>().material = materialList[materialId];
        Color Col = tempObj.GetComponent<Renderer>().material.color;
        Col = new Color(Col.r, Col.g, Col.b, alpha);
        tempObj.GetComponent<Renderer>().material.color = Col;
        tempObj = null;
    }
    public void ChangeSpawnObject(int ItemId) // this is called inside the spawnMenu to change the spawned object
    {
        localItemId = ItemId;
    }
    void ToolSpawnInput(InputAction.CallbackContext obj) //on click
    {
        ToolSpawnObject(localItemId);
    }
    public void MaterialIdChange(int matId)
    {
        materialId = matId;
    }
    public void MaterialChangeAlpha(float matAlpha)
    {
        alpha = matAlpha;
    }
}
