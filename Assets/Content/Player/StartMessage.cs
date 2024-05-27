using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class StartMessage : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("added and working");
        sendRpc();
    }

    [Rpc(SendTo.Everyone)]
    void sendRpc()
    {
        Debug.Log("working everywhere");
    }

}
