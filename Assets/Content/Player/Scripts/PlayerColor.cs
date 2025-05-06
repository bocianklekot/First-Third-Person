using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerColor : NetworkBehaviour
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
            ChangeValueRpc(hue_);
        }
        ChangeColorRpc();
    }

    

    [Rpc(SendTo.Everyone)]
    void ChangeColorRpc()
    {
        material.color = Color.HSVToRGB(hue.Value, 1, 1);
    }

    [Rpc(SendTo.Server)]
    void ChangeValueRpc(float hue_)
    {
        hue.Value = hue_;
    }
}
