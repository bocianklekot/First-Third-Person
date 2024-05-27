using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerLook : NetworkBehaviour
{
    float 
        lookSensivity = 3,
        mouseY,
        mouseX,
        lookXLimit = 90;
    public bool mouseLookEnabled = true;
    Camera playerCamera;
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (!IsLocalPlayer)
            playerCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mouseLookEnabled || !IsLocalPlayer)
            return;

        mouseX += Input.GetAxis("Mouse X") * lookSensivity;
        mouseY += -Input.GetAxis("Mouse Y") * lookSensivity;
        mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);
        playerCamera.transform.rotation = Quaternion.Euler(mouseY, playerCamera.transform.rotation.eulerAngles.y, 0);
        transform.root.rotation = Quaternion.Euler(0, mouseX, 0); ;
        //mouseLookRpc(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    [Rpc(SendTo.Server)]
    void MouseLookRpc(float mX, float mY)
    {
        mouseX += mX * lookSensivity;
        mouseY += -mY * lookSensivity;
        mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);
        playerCamera.transform.rotation = Quaternion.Euler(mouseY, playerCamera.transform.rotation.eulerAngles.y, 0);
        transform.root.rotation = Quaternion.Euler(0, mouseX, 0); ;
    }
}
