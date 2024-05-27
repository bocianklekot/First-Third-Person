using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class playerColor : NetworkBehaviour
{
    public Material material;
    public NetworkVariable<float> hue;
    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;

        if(IsLocalPlayer)
        {
            float hue_ = Random.Range(0, 100);
            hue_ /= 100;
            changeValueRpc(hue_);
        }
        changeColorRpc();
    }

    

    [Rpc(SendTo.Everyone)]
    void changeColorRpc()
    {
        material.color = Color.HSVToRGB(hue.Value, 1, 1);
    }

    [Rpc(SendTo.Server)]
    void changeValueRpc(float hue_)
    {
        Debug.Log(hue_);
        hue.Value = hue_;
    }
}
