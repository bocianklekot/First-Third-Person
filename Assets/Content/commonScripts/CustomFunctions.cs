using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFunctions : MonoBehaviour
{
    public static float[] Vector3ToFloat(Vector3 vec)
    {
        float[] newVec = new float[3];
        newVec[0] = vec.x;
        newVec[1] = vec.y;
        newVec[2] = vec.z;

        return newVec;
    }

    public static Vector3 FloatToVector3(float[] fl)
    {
        Vector3 vec;
        vec.x = fl[0];
        vec.y = fl[1];
        vec.z = fl[2];

        return vec;
    }
}
