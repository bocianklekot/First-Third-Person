using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
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
        if (!isMovementOn)
            return;

        characterController.Move(transform.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);

        characterController.Move(transform.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime);

        if (!isGrounded)
            characterController.Move(-transform.up * moveSpeed * Time.deltaTime);
    }

}
