using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRagdollMassChild : MonoBehaviour
{
    float mass;
    Armatures type;

    private void Start()
    {
        mass = transform.root.GetComponent<AutoRagdollMassParent>().mass;
        type = transform.root.GetComponent<AutoRagdollMassParent>().type;
        Rigidbody rb = GetComponent<Rigidbody>(); 
        if(type == Armatures.Humanoid01)
        {
            if (name == "head")
                rb.mass = mass * 0.08f;
            else if (name == "spine01" || name == "spine02" || name == "spine03")
                rb.mass = mass * (.50f / 3);
            else if (name == "l_upperArm" | name == "r_upperArm")
                rb.mass = mass * 0.027f;
            else if (name == "l_lowerArm" | name == "r_lowerArm")
                rb.mass = mass * 0.016f;
            else if (name == "l_hand" | name == "r_hand")
                rb.mass = mass * 0.007f;
            else if (name == "l_thigh" | name == "r_thigh")
                rb.mass = mass * 0.101f;
            else if (name == "l_calf" | name == "r_calf")
                rb.mass = mass * 0.044f;
            else if (name == "l_foot" | name == "r_foot")
                rb.mass = mass * 0.015f;
        }
    }
}
