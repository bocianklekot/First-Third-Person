using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class RigidbodyPush : NetworkBehaviour
{
    bool inputReady = true;
    float damage = 999;
    Camera camera;
    float forceAmount = 100;
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inputReady)
            return;

        inputReady = false;
        StartCoroutine(mainLoop());
        IEnumerator mainLoop()
        {
            
            if (Input.GetAxis("Fire1") > 0)
            {
                RaycastHit hit;
                Physics.Raycast(camera.transform.position, camera.transform.forward, out hit);

                if (hit.collider.GetComponent<Rigidbody>())
                {

                    if (hit.collider.transform.root.GetComponent<Health>())
                        hit.collider.transform.root.GetComponent<Health>().TakeDamage(damage, hit, camera.transform.forward.normalized*forceAmount);

                    if (hit.collider.GetComponent<NetworkObject>())
                        AddForceRpc(hit.collider.GetComponent<NetworkObject>(), camera.transform.forward.normalized);
                    else
                        AddForceCl(hit.collider.gameObject, camera.transform.forward.normalized);
                }    
            }

            if (Input.GetAxis("Kick") > 0)
            {
                RaycastHit hit;
                Physics.Raycast(camera.transform.position, camera.transform.forward, out hit);

                if (hit.collider.GetComponent<Rigidbody>())
                {
                    if (hit.collider.transform.root.GetComponent<EnemyAIBase>())
                        StartCoroutine(hit.collider.transform.root.GetComponent<EnemyAIBase>().Knockout(hit.collider.gameObject, 5, camera.transform.forward.normalized * forceAmount));

                }
            }

            yield return new WaitForSeconds(.1f);
            inputReady = true;
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
