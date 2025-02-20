using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class EnemyAIBase : NetworkBehaviour
{
    public NetworkVariable<EnemyPassiveStates> passiveState;
    public NetworkVariable<EnemyActiveState> activeState;
    Rigidbody[] ragdollRigidbodies;
    public AttackDef[] attacs;
    [HideInInspector]
    public SphereCollider detectCollider;
    [HideInInspector]
    public NetworkObject targetPlayer;
    [HideInInspector]
    public float distantanceToTarget;
    float idleColliderSize = 5, alertColliderSize = 15;
    [HideInInspector]
    public Health healthComponent;
    Animator animator;
    float legs, startLegs;
    bool startLegsAssigned, startKnockedOut;
    [HideInInspector]
    public NavMeshAgent agent;

    [System.Serializable]
    public struct AttackDef
    {
        public int bodyPartElement;
        public string animationName;
        public int animatorLayer;
        public bool stopWhenAttacking, mustStand;
        public float attackLenght, damageDelay;
    }

    public enum EnemyPassiveStates
    {
        alive,
        dead,
        knockedOut,
    }
    public enum EnemyActiveState
    {
        idle,
        alerted,
        walking,
        attacking
    }

    public void BaseUpdate()
    {
        if(targetPlayer)
            distantanceToTarget = Vector3.Distance(transform.position, targetPlayer.transform.position);

        AnimatorSetFloatRpc("velocity", agent.velocity.magnitude);
    }

    private void Start()
    {
        passiveState.Value = EnemyPassiveStates.alive;
        healthComponent = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
        ragdollRigidbodies = animator.GetComponentsInChildren<Rigidbody>();
        detectCollider = GetComponent<SphereCollider>();
        detectCollider.radius = idleColliderSize;
        agent = GetComponent<NavMeshAgent>();

        if (NetworkManager.IsServer)
            healthComponent.AutoBodyPartsHealth();

        //likelly not clean solution, prev on health component
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        if (startKnockedOut && IsHost)
            StartCoroutine(Knockout(gameObject, 5, transform.forward));

    }

    private void OnClientConnected(ulong clientId)
    {
        healthComponent.SyncBodyPartsHealthRpc();
    }

    private void OnServerStarted()
    {
        if (!NetworkManager.IsServer)
            return;

        healthComponent.AutoBodyPartsHealth();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.IsServer)
            return;

        if(other.CompareTag("Player"))
        {
            activeState.Value = EnemyActiveState.alerted;
            targetPlayer = other.GetComponent<NetworkObject>();
        }
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
        if(NetworkManager.IsServer)
            passiveState.Value = EnemyPassiveStates.knockedOut;
        animator.enabled = false;
        EnableRagdoll();
        AddForce(force * 2, rb);
        agent.enabled = false; // supposed to be false
        yield return new WaitForSeconds(time);
        StartCoroutine(WakeUp(time));
    }

    public IEnumerator WakeUp(float time)
    {
        RaycastHit hit;
        Physics.Raycast(animator.gameObject.GetComponentInChildren<Rigidbody>().position + Vector3.up, Vector3.down, out hit);
        animator.enabled = true;      
        animator.Play("chooseWakeUp");
        DecideStandUpAnimation();
        transform.position = hit.point;
        agent.enabled = true;
        //Debug.Log(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        yield return new WaitForSeconds(time);
        animator.SetTrigger("retarder");
        agent.enabled = true;

        if (NetworkManager.IsServer)
            passiveState.Value = EnemyPassiveStates.alive;

       

       
        
        DisableRagdoll();

        //reset rig parent rotation, no avatar = no rotation reset, dont use it might not work with every model
        /*
        if (!animator.avatar)
        {
            animator.GetComponentInChildren<Rigidbody>().gameObject.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        }*/
            
    }

    public void Die()
    {
        if (NetworkManager.IsServer)
            passiveState.Value = EnemyPassiveStates.dead;
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
        RefreshPartsCount();
        CheckPartToRagdoll(i, Vector3.zero);
        if(IsHost)
        {
            Transform tr = healthComponent.bodyParts[i].parts[0].transform;

            if (healthComponent.bodyParts[i].childNpcName.name != "torso")
                AddForce(CustomFunctions.FloatToVector3(force), SpawnNewEnemyPart(tr.GetComponent<SkinnedMeshRenderer>().bounds.center, healthComponent.bodyParts[i].childNpcName.name));

            if (healthComponent.bodyParts[i].type == Health.BodyPartTypes.torso)
            {
                bool continue_ = true;
                foreach (Health.BodyPart part in healthComponent.bodyParts)
                {
                    if (part.partHealth <= 0 && part.type != Health.BodyPartTypes.torso)
                    {
                        AddForce(CustomFunctions.FloatToVector3(force),
                            SpawnNewEnemyPart(tr.GetComponent<SkinnedMeshRenderer>().bounds.center, healthComponent.bodyParts[i].childNpcName.name));
                        
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
                            SpawnNewEnemyPart(part.parts[0].GetComponent<SkinnedMeshRenderer>().bounds.center, part.childNpcName.name));
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
        AddBleedingFX(healthComponent.bodyParts[i].colliders[0].transform);
        DisableTransforms(healthComponent.bodyParts[i].parts);
        DisableColliders(healthComponent.bodyParts[i].colliders);
    }

    void AddBleedingFX(Transform tr)
    {
        GameObject go = Instantiate(Resources.Load("BloodStreamFX") as GameObject, tr.position, Quaternion.LookRotation(tr.forward), tr);
        go.transform.localScale = Vector3.one;
    }

    void CheckPartToRagdoll(int i, Vector3 force)
    {
        if (healthComponent.bodyParts[i].type == Health.BodyPartTypes.leg || healthComponent.bodyParts[i].type == Health.BodyPartTypes.torso)
            StartCoroutine(Knockout(gameObject, 5, force));
    }
    
    GameObject SpawnNewEnemyPart(Vector3 pos, string name)
    {
        GameObject go = Instantiate(Resources.Load(name) as GameObject, new Vector3(pos[0], pos[1], pos[2]), Quaternion.identity);
        NetworkObject netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();

        if (go.GetComponent<EnemyAIBase>())
            go.GetComponent<EnemyAIBase>().startKnockedOut = true;

        return go;
    }

    [Rpc(SendTo.Server)]
    void SpawnNewEnemyPartRpc(float[] pos, string name)
    {
        GameObject go = Instantiate(Resources.Load("name") as GameObject, new Vector3(pos[0], pos[1], pos[2]), Quaternion.identity);
        NetworkObject netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();
    }

    public void RefreshPartsCount()
    {
        int counter = 0;
        foreach (Health.BodyPart part in healthComponent.bodyParts)
        {           
            if (part.type == Health.BodyPartTypes.leg && part.partHealth > 0)
            {
                counter += 1;
            }
             
        }

        legs = counter;

        if (startLegsAssigned)
            return;

        startLegs = counter;
        startLegsAssigned = true;
    }

    void DecideStandUpAnimation()
    {
        if (legs == startLegs)
            animator.SetInteger("wakeUpDecide", 0);
        else if (legs == 0)
            animator.SetInteger("wakeUpDecide", -1);
        else
            animator.SetInteger("wakeUpDecide", 1);
        
    }

    [Rpc(SendTo.Everyone)]
    void PlayAnimationRpc(string animationName, int layer = 0)
    {
        animator.Play(animationName, layer);
    }

    [Rpc(SendTo.Everyone)]
    void AnimatorSetFloatRpc(string name, float value)
    {
        animator.SetFloat(name, value);
    }

    [Rpc(SendTo.Server)]
    public void DecideTakeDamageAnimationRpc()
    {
        StartCoroutine(DecideTakeDamageAnimation());
    }

    public IEnumerator DecideTakeDamageAnimation()
    {
        passiveState.Value = EnemyPassiveStates.knockedOut;
        agent.SetDestination(agent.transform.position);
        PlayAnimationRpc("decideTakeDamage");
        yield return new WaitForSeconds(2);
        passiveState.Value = EnemyPassiveStates.alive;
    }

    //cos tam z body partami przydzielonymi do animacji paramtr health
    public void Attack()
    {
        StartCoroutine(DecideAttack());
    }

    IEnumerator DecideAttack()
    {
        activeState.Value = EnemyActiveState.attacking;

        foreach(AttackDef attack in attacs)
        {
            if (healthComponent.bodyParts[attack.bodyPartElement].partHealth <= 0)
                continue;

            if (attack.mustStand && startLegs != legs)
                continue;

            if (attack.stopWhenAttacking)
                agent.SetDestination(transform.position);

            PlayAnimationRpc(attack.animationName, attack.animatorLayer);
            agent.transform.LookAt(targetPlayer.transform.position);
            yield return new WaitForSeconds(attack.damageDelay);
            agent.transform.LookAt(targetPlayer.transform.position);
            //do takedamage here        
            yield return new WaitForSeconds(attack.attackLenght - attack.damageDelay);
            agent.transform.LookAt(targetPlayer.transform.position);
            activeState.Value = EnemyActiveState.alerted;
            break;


        }
    }
}
