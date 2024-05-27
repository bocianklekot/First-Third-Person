using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerLook : MonoBehaviour
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mouseLookEnabled)
            return;

        mouseX += Input.GetAxis("Mouse X") * lookSensivity;
        mouseY += -Input.GetAxis("Mouse Y") * lookSensivity;
        mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);
        playerCamera.transform.rotation = Quaternion.Euler(mouseY, playerCamera.transform.rotation.eulerAngles.y, 0);
        transform.root.rotation = Quaternion.Euler(0, mouseX, 0); ;
    }
}
