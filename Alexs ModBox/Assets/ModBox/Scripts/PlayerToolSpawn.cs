using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerToolSpawn : MonoBehaviour
{
    private Transform playerCamera;
    private int localItemId = 0, surfaceTypeId = 0;
    [SerializeField] private float MaxDistance = 20;
    [SerializeField] private InputActionReference inputSpawn,inputUndo;
    [SerializeField] private List<GameObject> objectList,spawnedObjectList;
    [SerializeField] private List<Material> materialList;
    private Color matColor = new Color(1,1,1,1);
    [SerializeField] private TMP_Text ToolText;

    private void Awake()
    {
        ToolText.text = objectList[localItemId].name.ToString();
        playerCamera = GetComponentInParent<PlayerMovement>().Camera;
        inputSpawn.action.performed += ToolSpawnInput;
        inputUndo.action.performed += UndoLastObject;
    }
    public void ToolSpawnObject(int ItemId) // called either locally or by spawnMenu
    {
        if(GetComponentInParent<PlayerMovement>().CantShoot) return; // cant spawn items if menu is on

        float distance = MaxDistance;// local instance of maxDistance
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hitI, MaxDistance))//we check if we hit something along the way
            distance = hitI.distance;

        Vector3 spawnPos = playerCamera.position + (playerCamera.forward * distance)+ Vector3.up /2;

        GameObject tempObj = Instantiate(objectList[ItemId], spawnPos, Quaternion.identity);
        tempObj.GetComponent<Renderer>().material = materialList[surfaceTypeId];
        tempObj.GetComponent<Renderer>().material.color = matColor;
        spawnedObjectList.Add(tempObj);
    }
    public void ChangeSpawnObject(int ItemId) // this is called inside the spawnMenu to change the spawned object
    {
        ToolText.text = objectList[ItemId].name.ToString();
        localItemId = ItemId;
    }
    void ToolSpawnInput(InputAction.CallbackContext obj) //on click
    {
        ToolSpawnObject(localItemId);
    }
    public void MaterialChangeSurfaceType(bool transparent)
    {
        surfaceTypeId = transparent ? 1 : 0;
    }
    public void MaterialChangeColor(Color col)
    {
        matColor = col;
    }
    void UndoLastObject(InputAction.CallbackContext obj)
    {
        if (spawnedObjectList.Count < 1)
            return;
        GameObject Obj = spawnedObjectList[spawnedObjectList.Count - 1];
        Debug.Log(Obj.name.Substring(0, Obj.name.Length - 7).ToString());
        spawnedObjectList.Remove(Obj);
        Destroy(Obj);
    }
    
    public void DeleteEverything()
    {
        foreach (var obj in spawnedObjectList)
        {
            Debug.Log(obj.name.Substring(0, obj.name.Length - 7).ToString());
            Destroy(obj);
        }
        spawnedObjectList.Clear();

    }
    public void KillZoneDestroy(GameObject obj)
    {
        Debug.Log(obj.name.Substring(0, obj.name.Length - 7).ToString());
        spawnedObjectList.Remove(obj);
    }
}
