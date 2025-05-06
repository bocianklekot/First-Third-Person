using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class WeaponSpawner : NetworkBehaviour
{
    bool inputReady = true;
    public GameObject spawnedObject;
    Camera Pcamera;

    void Start()
    {
        Pcamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!inputReady | !IsLocalPlayer)
            return;

        inputReady = false;
        StartCoroutine(mainLoop());
        IEnumerator mainLoop()
        {

            if (Input.GetKey(KeyCode.Mouse2))
            {
                RaycastHit hit;
                Physics.Raycast(Pcamera.transform.position, Pcamera.transform.forward, out hit);
                GameObject go = Instantiate(spawnedObject, hit.point, Quaternion.identity);
                go.GetComponent<NetworkObject>().Spawn();
            }

            

            yield return new WaitForSeconds(.1f);
            inputReady = true;
        }

    }
}
