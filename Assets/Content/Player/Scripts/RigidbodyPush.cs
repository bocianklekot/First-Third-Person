using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class RigidbodyPush : NetworkBehaviour
{
    bool inputReady = true;
    float damage = 25;
    Camera PlayerCamera;
    float forceAmount = 100;
    void Start()
    {
        PlayerCamera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inputReady | !IsLocalPlayer)
            return;

        inputReady = false;
        StartCoroutine(mainLoop());
        IEnumerator mainLoop()
        {

            if (Input.GetAxis("Fire1") > 0/* && Physics.Raycast(camera.transform.position, camera.transform.forward, 5, 0, QueryTriggerInteraction.Collide)*/)
            {
                RaycastHit hit;
                Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit);

                if (hit.collider.GetComponent<Rigidbody>())
                {

                    if (hit.collider.transform.root.GetComponent<Health>())
                    {
                        hit.collider.transform.root.GetComponent<Health>().TakeDamage(damage, hit, PlayerCamera.transform.forward.normalized * forceAmount);
                        GameObject fx = Instantiate(Resources.Load("BloodImpact01FX") as GameObject, hit.point, Quaternion.LookRotation(hit.normal), hit.collider.transform);
                        Destroy(fx, 3);
                    }
                    else
                    {
                        if (hit.collider.GetComponent<NetworkObject>())
                            AddForceRpc(hit.collider.GetComponent<NetworkObject>(), PlayerCamera.transform.forward.normalized);
                        else
                            AddForceCl(hit.collider.gameObject, PlayerCamera.transform.forward.normalized);
                    }


                }
            }

            if (Input.GetAxis("Kick") > 0)
            {
                RaycastHit hit;
                Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit);

                if (hit.collider.GetComponent<Rigidbody>())
                {
                    if (hit.collider.transform.root.GetComponent<EnemyAIBase>())
                        StartCoroutine(hit.collider.transform.root.GetComponent<EnemyAIBase>().Knockout(hit.collider.gameObject, 3.2f, PlayerCamera.transform.forward.normalized * forceAmount));

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
