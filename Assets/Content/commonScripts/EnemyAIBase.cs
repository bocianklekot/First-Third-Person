using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class EnemyAIBase : NetworkBehaviour
{
    public EnemyStates state;
    Rigidbody[] ragdollRigidbodies;
    public Health healthComponent;
    Animator animator;

    public enum EnemyStates
    {
        alive,
        dead,
        knockedOut,
    }

    private void Start()
    {
        state = EnemyStates.alive;
        healthComponent = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
        ragdollRigidbodies = animator.GetComponentsInChildren<Rigidbody>();
    }

    [Rpc(SendTo.Server)]
    void AddForceRpc(NetworkObjectReference reference, Vector3 force)
    {
        reference.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
    }

    void AddForceCl(GameObject go, Vector3 force)
    {
        Debug.Log("force");
        go.GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
    }

    void EnableRagdoll()
    {
        foreach(var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
            animator.enabled = false;
        }
        Debug.Log("ragdollsOn");
    }

    void DisableRagdoll()
    {
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    public IEnumerator Knockout(GameObject rb, float time, Vector3 force)
    {
        state = EnemyStates.knockedOut;
        animator.enabled = false;
        EnableRagdoll();
        AddForceCl(rb, force);
        yield return new WaitForSeconds(time);
        WakeUp();
    }

    public void WakeUp()
    {
        state = EnemyStates.alive;
        animator.enabled = true;
        DisableRagdoll();
    }

    public void Die()
    {
        state = EnemyStates.dead;
        EnableRagdoll();
    }

    private void DisableTransforms(Transform[] transforms)
    {
        foreach(Transform transform in transforms)
        {
            transform.gameObject.SetActive(false);
        }
    }

    private void DisableColliders(Collider[] colliders)
    {
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    [Rpc(SendTo.Server)]
    public void DetachPartRpc(int i)
    {
        Transform tr = healthComponent.bodyParts[i].parts[0].transform;

        if (healthComponent.bodyParts[i].childNpcName != "torso")
            Instantiate(Resources.Load(healthComponent.bodyParts[i].childNpcName) as GameObject, tr.GetComponent<SkinnedMeshRenderer>().bounds.center, Quaternion.identity);
        else
        {
            Instantiate(Resources.Load(healthComponent.bodyParts[i].childNpcName + "upper") as GameObject, tr.GetComponent<SkinnedMeshRenderer>().bounds.center, Quaternion.identity);
            Instantiate(Resources.Load(healthComponent.bodyParts[i].childNpcName + "lower") as GameObject, tr.GetComponent<SkinnedMeshRenderer>().bounds.center, Quaternion.identity);
        }
        
        DisableTransforms(healthComponent.bodyParts[i].parts);
        DisableColliders(healthComponent.bodyParts[i].colliders);
        if (healthComponent.bodyParts[i].type == Health.BodyPartTypes.torso)
        {
            bool continue_ = true;
            foreach(Health.BodyPart part in healthComponent.bodyParts)
            {
                if (part.partHealth <= 0 && part.type != Health.BodyPartTypes.torso)
                {
                    continue_ = false;
                    break;
                }
                    
            }

            if (continue_)
            {
                Destroy(gameObject);
            }
            else
            {
                foreach (Health.BodyPart part in healthComponent.bodyParts)
                {
                    if (part.partHealth <= 0 && part.type != Health.BodyPartTypes.torso)
                    {
                        GameObject go2 = Instantiate(Resources.Load(part.childNpcName) as GameObject, tr.GetComponent<SkinnedMeshRenderer>().bounds.center, Quaternion.identity);
                    }

                }
                Destroy(gameObject);
            }
                
        }

        

        //spawnNewEnemyPartRpc(CustomFunctions.VectorToFloat(transform.TransformPoint(healthComponent.bodyParts[i].parts[0].localPosition)), healthComponent.name);

    }
    [Rpc(SendTo.Server)]
    void spawnNewEnemyPartRpc(float[] pos, string name)
    {
        GameObject go = Instantiate(Resources.Load("name") as GameObject, new Vector3(pos[0], pos[1], pos[2]), Quaternion.identity);
        NetworkObject netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();
    }
}
