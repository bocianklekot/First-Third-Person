using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
public class AddNetScriptStart : NetworkBehaviour
{

    private void Start()
    {
        if (!IsLocalPlayer)
            return;

        AddComponentRpc(GetComponent<NetworkObject>());
    }

    [Rpc(SendTo.Server)]
    void AddComponentRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject obj);
        Debug.Log(GetComponent<NetworkObject>().NetworkObjectId);
        GameObject go = Instantiate(Resources.Load("message") as GameObject);
        NetworkObject goInstance = go.GetComponent<NetworkObject>();
        goInstance.Spawn();
        go.transform.parent = obj.transform;
        //ParentObjRpc(goInstance, obj);

    }

    [Rpc(SendTo.Everyone)]
    void ParentObjRpc(NetworkObjectReference message,NetworkObjectReference player)
    {
        player.TryGet(out NetworkObject obj);
        message.TryGet(out NetworkObject go);

        go.transform.parent = obj.transform;
    }
}
