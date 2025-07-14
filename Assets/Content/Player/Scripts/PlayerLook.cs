using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerLook : NetworkBehaviour
{
    public float
        lookSensivity = 3,
        headLookLimit = 20,
        spineLookLimit = 75,
        mouseXHead,
        mouseXSpine,
        mouseXBody,
        mouseXTop,
        mouseXBottom,
        mouseY,
        lookXLimit = 90;

    public bool 
        mouseLookEnabled = true;

    public GameObject 
        neck, 
        spine1, 
        spine2, 
        spine3;

    Camera 
        playerCamera;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (!IsLocalPlayer)
            playerCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!mouseLookEnabled || !IsLocalPlayer)
            return;

        mouseY += -Input.GetAxis("Mouse Y") * lookSensivity;
        mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);

        mouseXHead += Input.GetAxis("Mouse X") * lookSensivity;
        mouseXHead = Mathf.Clamp(mouseXHead, -headLookLimit, headLookLimit);

        if (mouseXHead == headLookLimit || mouseXHead == -headLookLimit)
            mouseXSpine += Input.GetAxis("Mouse X") * lookSensivity;

        mouseXSpine = Mathf.Clamp(mouseXSpine, -spineLookLimit, spineLookLimit);

        if (mouseXSpine == spineLookLimit || mouseXSpine == -spineLookLimit)
            mouseXBottom += Input.GetAxis("Mouse X") * lookSensivity;
 
        Quaternion fakeCameraRotationHolder = Quaternion.Euler(mouseY, mouseXHead, 0);
        Vector3 neckRotation = fakeCameraRotationHolder.eulerAngles + neck.transform.localRotation.eulerAngles;

        neck.transform.localRotation = 
            Quaternion.Euler(neck.transform.localRotation.eulerAngles + fakeCameraRotationHolder.eulerAngles);

        spine2.transform.localRotation =
            Quaternion.Euler(spine2.transform.rotation.eulerAngles.x, mouseXSpine, spine2.transform.rotation.eulerAngles.z);

        transform.root.rotation = Quaternion.Euler(0, mouseXBottom, 0);

        //mouseLookRpc(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    [Rpc(SendTo.Server)]
    void MouseLookRpc(float mX, float mY)
    {
        //mouseX += mX * lookSensivity;
        mouseY += -mY * lookSensivity;
        mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);
        playerCamera.transform.rotation = Quaternion.Euler(mouseY, playerCamera.transform.rotation.eulerAngles.y, 0);
        //transform.root.rotation = Quaternion.Euler(0, mouseX, 0); ;
    }

}
