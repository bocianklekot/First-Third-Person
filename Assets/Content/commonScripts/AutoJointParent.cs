using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoJointParent : MonoBehaviour
{
    CharacterJoint joint;

    private void Start()
    {
        joint = GetComponent<CharacterJoint>();
        joint.connectedBody = transform.parent.GetComponent<Rigidbody>();
    }

}
