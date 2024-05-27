using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class playerMovement : NetworkBehaviour
{
    CharacterController characterController;
    float moveSpeed = 10;
    bool isMovementOn = true,
        isGrounded = false;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMovementOn || !IsLocalPlayer)
            return;

        characterController.Move(transform.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);

        characterController.Move(transform.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime);

        if (!isGrounded)
            characterController.Move(-transform.up * moveSpeed * Time.deltaTime);
        //moveRpc(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        
    }

    [Rpc(SendTo.Server)]
    void moveRpc(float vert, float hor)
    {
        characterController.Move(transform.forward * vert * moveSpeed * Time.deltaTime);

        characterController.Move(transform.right * hor * moveSpeed * Time.deltaTime);

        if (!isGrounded)
            characterController.Move(-transform.up * moveSpeed * Time.deltaTime);
    }

}
