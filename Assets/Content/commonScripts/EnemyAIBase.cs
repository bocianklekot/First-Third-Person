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

    void AddForce(Vector3 vec, GameObject go)
    {
        if (go.GetComponent<Rigidbody>())
            go.GetComponent<Rigidbody>().AddForce(vec / 10, ForceMode.VelocityChange);
        else
            go.GetComponentInChildren<Rigidbody>().AddForce(vec, ForceMode.VelocityChange);
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
        StopCoroutine(Knockout(rb, time, force));
        state = EnemyStates.knockedOut;
        animator.enabled = false;
        EnableRagdoll();
        AddForce(force * 2, rb);
        yield return new WaitForSeconds(time);
        WakeUp();
    }

    public void WakeUp()
    {
        state = EnemyStates.alive;
        animator.enabled = true;
        animator.Play("");
        animator.Play("humanoid01|wakeUp");
        //animator.SetTrigger("wakeUp");
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

    [Rpc(SendTo.Everyone)]
    public void DetachPartRpc(int i, float[] force)
    {
        if(IsHost)
        {
            Transform tr = healthComponent.bodyParts[i].parts[0].transform;

            if (healthComponent.bodyParts[i].childNpcName != "torso")
                AddForce(CustomFunctions.FloatToVector3(force), SpawnNewEnemyPart(tr.GetComponent<SkinnedMeshRenderer>().bounds.center, healthComponent.bodyParts[i].childNpcName));

            if (healthComponent.bodyParts[i].type == Health.BodyPartTypes.torso)
            {
                bool continue_ = true;
                foreach (Health.BodyPart part in healthComponent.bodyParts)
                {
                    if (part.partHealth <= 0 && part.type != Health.BodyPartTypes.torso)
                    {
                        AddForce(CustomFunctions.FloatToVector3(force),
                            SpawnNewEnemyPart(tr.GetComponent<SkinnedMeshRenderer>().bounds.center, healthComponent.bodyParts[i].childNpcName));
                        
                        continue_ = false;
                        break;
                    }

                }

                if(!continue_)
                {
                    foreach (Health.BodyPart part in healthComponent.bodyParts)
                    {
                        if (part.partHealth > 0 && part.type != Health.BodyPartTypes.torso)
                        {
                            AddForce(CustomFunctions.FloatToVector3(force),
                            SpawnNewEnemyPart(part.parts[0].GetComponent<SkinnedMeshRenderer>().bounds.center, part.childNpcName));
                        }

                    }
                }
                else
                {
                    AddForce(CustomFunctions.FloatToVector3(force),
                    SpawnNewEnemyPart(tr.GetComponent<SkinnedMeshRenderer>().bounds.center, healthComponent.bodyParts[i].childNpcName + "upper"));
                    AddForce(CustomFunctions.FloatToVector3(force),
                    SpawnNewEnemyPart(tr.GetComponent<SkinnedMeshRenderer>().bounds.center, healthComponent.bodyParts[i].childNpcName + "lower"));
                }
                GetComponent<NetworkObject>().Despawn();
            }
        }

        DisableTransforms(healthComponent.bodyParts[i].parts);
        DisableColliders(healthComponent.bodyParts[i].colliders);
    }
    
    GameObject SpawnNewEnemyPart(Vector3 pos, string name)
    {
        GameObject go = Instantiate(Resources.Load(name) as GameObject, new Vector3(pos[0], pos[1], pos[2]), Quaternion.identity);
        NetworkObject netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();

        return go;
    }

    [Rpc(SendTo.Server)]
    void SpawnNewEnemyPartRpc(float[] pos, string name)
    {
        GameObject go = Instantiate(Resources.Load("name") as GameObject, new Vector3(pos[0], pos[1], pos[2]), Quaternion.identity);
        NetworkObject netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();
    }
}
