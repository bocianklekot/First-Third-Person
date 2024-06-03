using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFunctions : MonoBehaviour
{
    public static float[] VectorToFloat(Vector3 vec)
    {
        float[] newVec = new float[3];
        newVec[0] = vec.x;
        newVec[1] = vec.y;
        newVec[2] = vec.z;

        return newVec;
    }
}
