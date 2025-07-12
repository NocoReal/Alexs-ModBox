using UnityEngine;
using System.Collections.Generic;


public class EnviromentHandler : MonoBehaviour
{
    [SerializeField] GameObject Player,Camera;
    PlayerToolSpawn PTS;
    private List<GameObject> ObjectList;
    private List<int> ObjectIntList, SurfaceIntList;
    List<Vector4> ColorList;
    List<Vector3> PositionList;
    List<Quaternion> RotationList;
    Vector3 PlayerPosition, PlayerVelocity;
    Quaternion CameraRotation;
    Rigidbody rb;

#if !UNITY_EDITOR
    private void OnApplicationQuit()
    {
        SaveObjects();
    }

    private void OnDisable()
    {
        Application.quitting -= SaveObjects;
    }
#endif
    private void OnEnable()
    {
#if !UNITY_EDITOR
        Application.quitting += SaveObjects;
#endif
        PTS = Player.GetComponent<PlayerToolSpawn>();
        rb = Player.GetComponent<Rigidbody>();
        LoadSave();
    }
    public void SaveObjects()
    {
        ObjectList = PTS.ReturnObjectList();
        ObjectIntList = PTS.ReturnSpawnObjectInt(); // so we can later instantiate the correct prefab

        SaveData data = new SaveData // we create data we some new values
        {
            objectIntList = new List<int>(),
            surfaceIntList = new List<int>(),
            objectPositions = new List<Vector3>(),
            objectVelocities = new List<Vector3>(),
            objectRotations = new List<Quaternion>(),
            objectColors = new List<Vector4>()
        };

        foreach (GameObject obj in ObjectList) // loop through the whole list of objects and save each to the savedata lists
        {
            data.objectPositions.Add(obj.transform.position);
            data.objectRotations.Add(obj.transform.rotation);
            string materialName = obj.GetComponent<Renderer>().material.name;
            materialName = materialName.Replace(" (Instance)", "");
            int mat = materialName == "Transparent" ? 1 : 0;
            data.surfaceIntList.Add(mat);
            Color col = obj.GetComponent<Renderer>().material.color;
            data.objectColors.Add(new Vector4(col.r, col.g, col.b, col.a));
            data.objectVelocities.Add(obj.GetComponent<Rigidbody>().linearVelocity);
        }
        data.objectIntList = new List<int>(ObjectIntList);

        data.playerPosition = rb.position;
        data.playerVelocity = rb.linearVelocity;
        data.cameraRotation = Camera.transform.rotation;

        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }
    void LoadSave()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (!System.IO.File.Exists(path)) return;

        string json = System.IO.File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        PTS.SetSpawnedObjectList(data.objectIntList.Count, data.objectIntList, data.surfaceIntList, data.objectColors, data.objectPositions, data.objectRotations, data.objectVelocities);
        rb.position = data.playerPosition;
        rb.linearVelocity = data.playerVelocity;
        Camera.transform.rotation = data.cameraRotation;

        PlayerMovement pm = Player.GetComponent<PlayerMovement>();
        Vector3 euler = data.cameraRotation.eulerAngles;
        pm.xRotation = euler.x;
        pm.yRotation = euler.y;
    }
}
