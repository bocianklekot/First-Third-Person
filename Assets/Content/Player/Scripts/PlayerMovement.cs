using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerMovement : NetworkBehaviour
{
    CharacterController characterController;
    public Animator playerAnimator;

    Vector3 previous;
    Vector3 velocity;

    float moveSpeed = 10;
    bool isMovementOn = true,
        isGrounded = false;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMovementOn || !IsLocalPlayer)
            return;

        velocity = ((transform.position - previous)) / Time.deltaTime;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        previous = transform.position;

        playerAnimator.SetFloat("X", localVelocity.x);
        playerAnimator.SetFloat("Z", localVelocity.z);

        characterController.Move(transform.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);

        characterController.Move(transform.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime);

        if (!isGrounded)
            characterController.Move(-transform.up * moveSpeed * Time.deltaTime);
        //moveRpc(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        
    }

    [Rpc(SendTo.Server)]
    void MoveRpc(float vert, float hor)
    {
        characterController.Move(transform.forward * vert * moveSpeed * Time.deltaTime);

        characterController.Move(transform.right * hor * moveSpeed * Time.deltaTime);

        if (!isGrounded)
            characterController.Move(-transform.up * moveSpeed * Time.deltaTime);
    }

}
