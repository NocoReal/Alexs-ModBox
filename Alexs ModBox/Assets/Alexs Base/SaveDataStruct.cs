using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SaveData //we create this structure so we can properly save using json
{
    public List<int> objectIntList;
    public List<int> surfaceIntList;
    public List<Vector3> objectPositions;
    public List<Vector3> objectVelocities;
    public List<Quaternion> objectRotations;
    public List<Vector4> objectColors; // Use Vector4 for Color (r,g,b,a)

    public Vector3 playerPosition;
    public Vector3 playerVelocity;
    public Quaternion cameraRotation;
}