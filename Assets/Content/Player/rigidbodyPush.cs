using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class RigidbodyPush : NetworkBehaviour
{
    Camera camera;
    public float forceAmount = 1;
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis("Fire1") > 0)
        {
            RaycastHit hit;
            Physics.Raycast(camera.transform.position, camera.transform.forward, out hit);

            if (!hit.collider.GetComponent<Rigidbody>())
                return;

            if (hit.collider.GetComponent<NetworkObject>())
                AddForceRpc(hit.collider.GetComponent<NetworkObject>(), camera.transform.forward.normalized);
            else
                AddForceCl(hit.collider.gameObject, camera.transform.forward.normalized);


        }
    }

    void AddForceCl(GameObject go, Vector3 force)
    {
        go.GetComponent<Rigidbody>().AddForce(force * forceAmount, ForceMode.VelocityChange);
    }

    [Rpc(SendTo.Server)]
    void AddForceRpc(NetworkObjectReference reference, Vector3 force)
    {
        reference.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<Rigidbody>().AddForce(force * forceAmount, ForceMode.VelocityChange);
    }
}
