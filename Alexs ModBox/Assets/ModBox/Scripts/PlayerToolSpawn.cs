using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerToolSpawn : MonoBehaviour
{
    private Transform playerCamera;
    private int localItemId = 0, surfaceTypeId = 0;
    [SerializeField] private float MaxDistance = 20;
    [SerializeField] private InputActionReference inputSpawn, inputUndo;
    [SerializeField] private List<GameObject> objectList, spawnedObjectList;
    private List<int> objectListInt;
    [SerializeField] private List<Material> materialList;
    private Color matColor = new Color(1, 1, 1, 1);
    [SerializeField] private TMP_Text ToolText;
    private void OnEnable() // we subscribe to the inputs onEnable
    {
        inputUndo.action.performed += UndoLastObject;
        inputSpawn.action.performed += ToolSpawnInput;
    }
    private void OnDisable() // unsubscribe from them if we're destroyed
    {
        inputUndo.action.performed -= UndoLastObject;
        inputSpawn.action.performed -= ToolSpawnInput;

    }
    private void OnDestroy() // unsubscribe from them if we're disabled
    {
        inputUndo.action.performed -= UndoLastObject;
        inputSpawn.action.performed -= ToolSpawnInput;

    }

    private void Awake()
    {
        ToolText.text = objectList[localItemId].name.ToString();
        playerCamera = GetComponentInParent<PlayerMovement>().Camera;
    }
    public void ToolSpawnObject(int ItemId) // called either locally or by spawnMenu
    {
        if (GetComponentInParent<PlayerMovement>().CantShoot) return; // cant spawn items if menu is on

        float distance = MaxDistance;// local instance of maxDistance
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hitI, MaxDistance))//we check if we hit something along the way
            distance = hitI.distance;

        Vector3 spawnPos = playerCamera.position + (playerCamera.forward * distance) + Vector3.up / 2;

        GameObject tempObj = Instantiate(objectList[ItemId], spawnPos, Quaternion.Euler(new Vector3(0, playerCamera.rotation.eulerAngles.y, 0)));
        tempObj.GetComponent<Renderer>().material = materialList[surfaceTypeId];
        tempObj.GetComponent<Renderer>().material.color = matColor;
        spawnedObjectList.Add(tempObj);
        objectListInt.Add(ItemId);
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
        objectListInt.Remove(objectListInt[objectListInt.Count - 1]);
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
        objectListInt.Clear();
    }
    public void KillZoneDestroy(GameObject obj)
    {
        Debug.Log(obj.name.Substring(0, obj.name.Length - 7).ToString());
        objectListInt.RemoveAt(spawnedObjectList.IndexOf(obj));
        spawnedObjectList.Remove(obj);
    }
    public List<GameObject> ReturnObjectList()
    {
        return spawnedObjectList;
    }
    public List<int> ReturnSpawnObjectInt()
    {
        return objectListInt;
    }
    public void SetSpawnedObjectList(int count, List<int> ints, List<int> surface, List<Vector4> col, List<Vector3> pos, List<Quaternion> rot, List<Vector3> vel)
    {
        objectListInt = ints;
        for (int i = 0; i < count; ++i)
        {
            int objectId = ints[i];  // Get the ID from the loaded data
            if (objectId >= 0 && objectId < objectList.Count)
            {
                GameObject tempObj = Instantiate(objectList[objectId], pos[i], rot[i]);
                tempObj.GetComponent<Renderer>().material = materialList[surface[i]];
                tempObj.GetComponent<Renderer>().material.color = new Color(col[i].x, col[i].y, col[i].z, col[i].w);
                // Optional: Set the velocity if needed
                tempObj.GetComponent<Rigidbody>().linearVelocity = vel[i];

                spawnedObjectList.Add(tempObj);
            }
            else
            {
                Debug.LogError("Invalid object ID: " + objectId);  // To help with debugging
            }
        }
    }

}
