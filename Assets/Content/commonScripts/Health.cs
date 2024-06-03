using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 100;
    public bool invincible, complex;
    public BodyPart[] bodyParts;

    private void Start()
    {
        if(complex)
            AutoBodyPartsHealth();
    }

    public enum BodyPartTypes
    {
        torso,
        head,
        leg,
        arm
    }
    
    public void TakeDamage(float damage, RaycastHit hit)
    {
        if (invincible)
            return;

        if (!complex)
        {
            health -= damage;
            return;
        }
        for (int i = 0; i < bodyParts.Length; i++)
        {
            foreach(Collider collider in bodyParts[i].colliders)
            {
                if (collider == hit.collider)
                {
                    bodyParts[i].partHealth -= damage;

                    if (bodyParts[i].partHealth <= 0)
                    {
                        GetComponent<EnemyAIBase>().DetachPartRpc(i);
                        Debug.Log("health");
                    }
                       
                }
                    
            }
        }

    }

    void AutoBodyPartsHealth()
    {
        for(int i = 0; i < bodyParts.Length; i++)
        {
            if (bodyParts[i].partHealth != 0)
                return;

            if (bodyParts[i].type == BodyPartTypes.torso)
                bodyParts[i].partHealth = health * 0.5f;
            else if(bodyParts[i].type == BodyPartTypes.head)
                bodyParts[i].partHealth = health * 0.1f;
            else if (bodyParts[i].type == BodyPartTypes.arm)
                bodyParts[i].partHealth = health * 0.25f;
            else if (bodyParts[i].type == BodyPartTypes.leg)
                bodyParts[i].partHealth = health * 0.35f;
        }
    }

    [System.Serializable]
    public struct BodyPart
    {
        public float partHealth;
        public Transform[] parts;
        public Collider[] colliders;
        public BodyPartTypes type;
        public string childNpcName;
    }

    
    
}
