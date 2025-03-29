using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VisualiseBox
{
    public static void DisplayBox(Vector3 center, Vector3 HalfExtend, Quaternion rotation, Color LineColor, float Duration = 0)
    {
        Vector3[] Vertices = new Vector3[8];
        int i = 0;
        for (int x = -1; x < 2; x += 2)
        {
            for (int y = -1; y < 2; y += 2)
            {
                for (int z = -1; z < 2; z += 2)
                {
                    Vertices[i] = center + new Vector3(HalfExtend.x * x, HalfExtend.y * y, HalfExtend.z * z);
                    i++;
                }
            }
        }

        Vertices = RotateObject(Vertices, rotation.eulerAngles, center);

        Debug.DrawLine(Vertices[0], Vertices[1], LineColor, Duration);
        Debug.DrawLine(Vertices[1], Vertices[3], LineColor, Duration);
        Debug.DrawLine(Vertices[2], Vertices[3], LineColor, Duration);
        Debug.DrawLine(Vertices[2], Vertices[0], LineColor, Duration);
        Debug.DrawLine(Vertices[4], Vertices[0], LineColor, Duration);
        Debug.DrawLine(Vertices[4], Vertices[6], LineColor, Duration);
        Debug.DrawLine(Vertices[2], Vertices[6], LineColor, Duration);
        Debug.DrawLine(Vertices[7], Vertices[6], LineColor, Duration);
        Debug.DrawLine(Vertices[7], Vertices[3], LineColor, Duration);
        Debug.DrawLine(Vertices[7], Vertices[5], LineColor, Duration);
        Debug.DrawLine(Vertices[1], Vertices[5], LineColor, Duration);
        Debug.DrawLine(Vertices[4], Vertices[5], LineColor, Duration);
    }
    public static void DisplayBoxCast(Vector3 center, Vector3 halfExtend, Vector3 direction, Quaternion rotation, float maxDistance, Color lineColor, float duration = 0)
    {
        Vector3[] baseVertices = new Vector3[8]; // Base vertices for the box
        int i = 0;
        for (int x = -1; x < 2; x += 2)
        {
            for (int y = -1; y < 2; y += 2)
            {
                for (int z = -1; z < 2; z += 2)
                {
                    baseVertices[i] = center + rotation * new Vector3(halfExtend.x * x, halfExtend.y * y, halfExtend.z * z);
                    i++;
                }
            }
        }

        // Rotate the direction vector to align with the box's rotation
        Vector3 rotatedDirection = rotation * direction.normalized * maxDistance;

        // Create vertices for the extended box
        Vector3[] extendedVertices = new Vector3[8];
        for (i = 0; i < 8; i++)
        {
            // Extend vertices along the direction vector
            extendedVertices[i] = baseVertices[i] + rotatedDirection;
        }

        // Draw lines for the base box
        DrawBoxEdges(baseVertices, lineColor, duration);

        // Draw lines for the extended box
        DrawBoxEdges(extendedVertices, lineColor, duration);

        // Connect the corners of the base box to the extended box
        for (i = 0; i < 8; i++)
        {
            Debug.DrawLine(baseVertices[i], extendedVertices[i], lineColor, duration);
        }
    }

    private static void DrawBoxEdges(Vector3[] vertices, Color lineColor, float duration)
    {
        Debug.DrawLine(vertices[0], vertices[1], lineColor, duration);
        Debug.DrawLine(vertices[1], vertices[3], lineColor, duration);
        Debug.DrawLine(vertices[2], vertices[3], lineColor, duration);
        Debug.DrawLine(vertices[2], vertices[0], lineColor, duration);
        Debug.DrawLine(vertices[4], vertices[0], lineColor, duration);
        Debug.DrawLine(vertices[4], vertices[6], lineColor, duration);
        Debug.DrawLine(vertices[2], vertices[6], lineColor, duration);
        Debug.DrawLine(vertices[7], vertices[6], lineColor, duration);
        Debug.DrawLine(vertices[7], vertices[3], lineColor, duration);
        Debug.DrawLine(vertices[7], vertices[5], lineColor, duration);
        Debug.DrawLine(vertices[1], vertices[5], lineColor, duration);
        Debug.DrawLine(vertices[4], vertices[5], lineColor, duration);
    }
    static Vector3[] RotateObject(Vector3[] ObjToRotate, Vector3 DegreesToRotate, Vector3 Around)//rotates a set of dots counterclockwise
    {
        for (int i = 0; i < ObjToRotate.Length; i++)
        {
            ObjToRotate[i] -= Around;
        }
        DegreesToRotate.z = Mathf.Deg2Rad * DegreesToRotate.z;
        DegreesToRotate.x = Mathf.Deg2Rad * DegreesToRotate.x;
        DegreesToRotate.y = -Mathf.Deg2Rad * DegreesToRotate.y;

        for (int i = 0; i < ObjToRotate.Length; i++)
        {
            float H = Vector3.Distance(Vector3.zero, ObjToRotate[i]);
            if (H != 0)
            {
                float CosA = ObjToRotate[i].x / H;
                float SinA = ObjToRotate[i].y / H;
                float cosB = Mathf.Cos(DegreesToRotate.z);
                float SinB = Mathf.Sin(DegreesToRotate.z);
                ObjToRotate[i] = new Vector3(H * (CosA * cosB - SinA * SinB), H * (SinA * cosB + CosA * SinB), ObjToRotate[i].z);
            }
        }

        for (int i = 0; i < ObjToRotate.Length; i++)
        {
            float H = Vector3.Distance(Vector3.zero, ObjToRotate[i]);
            if (H != 0)
            {
                float CosA = ObjToRotate[i].y / H;
                float SinA = ObjToRotate[i].z / H;
                float cosB = Mathf.Cos(DegreesToRotate.x);
                float SinB = Mathf.Sin(DegreesToRotate.x);
                ObjToRotate[i] = new Vector3(ObjToRotate[i].x, H * (CosA * cosB - SinA * SinB), H * (SinA * cosB + CosA * SinB));
            }
        }

        for (int i = 0; i < ObjToRotate.Length; i++)
        {
            float H = Vector3.Distance(Vector3.zero, ObjToRotate[i]);
            if (H != 0)
            {
                float CosA = ObjToRotate[i].x / H;
                float SinA = ObjToRotate[i].z / H;
                float cosB = Mathf.Cos(DegreesToRotate.y);
                float SinB = Mathf.Sin(DegreesToRotate.y);
                ObjToRotate[i] = new Vector3((CosA * cosB - SinA * SinB) * H, ObjToRotate[i].y, H * (SinA * cosB + CosA * SinB));
            }
        }

        for (int i = 0; i < ObjToRotate.Length; i++)
        {
            ObjToRotate[i] += Around;
        }



        return ObjToRotate;
    }
}
