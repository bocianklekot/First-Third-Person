using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Health : NetworkBehaviour
{
    public NetworkVariable<float> health = new(100);
    public bool invincible, complex;
    public BodyPart[] bodyParts;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        
        /*
        if (!NetworkManager.IsServer)
            return;

        if (complex)
            AutoBodyPartsHealth();*/
    }
    private void OnClientConnected(ulong clientId)
    {
        SyncBodyPartsHealthRpc();
        Debug.Log("sync");
    }

    private void OnServerStarted()
    {
        Debug.Log("autohealth");
        if (!NetworkManager.IsServer)
            return;

        AutoBodyPartsHealth();
        
    }

    public enum BodyPartTypes
    {
        torso,
        head,
        leg,
        arm
    }
    
    public void TakeDamage(float damage, RaycastHit hit, Vector3 force)
    {
        if (invincible)
            return;

        if (!complex)
        {
            SimpleDamageRpc(damage);
            return;
        }
        for (int i = 0; i < bodyParts.Length; i++)
        {
            foreach(Collider collider in bodyParts[i].colliders)
            {
                if (collider == hit.collider)
                {
                    BodyPartDamageRpc(damage, i);

                    var aiBase = GetComponent<EnemyAIBase>();
                    if (bodyParts[i].partHealth / damage < 3.5 && aiBase.passiveState.Value != EnemyAIBase.EnemyPassiveStates.knockedOut) // 3.5 ma byc
                        aiBase.DecideTakeDamageAnimationRpc();

                    if (bodyParts[i].partHealth <= 0)
                    {
                        aiBase.DetachPartRpc(i, CustomFunctions.Vector3ToFloat(force));
                    }
                       
                }
                    
            }
        }

    }

    [Rpc(SendTo.Server)]
    void SimpleDamageRpc(float dmg)
    {
        health.Value -= dmg;
    }

    [Rpc(SendTo.Server)]
    void BodyPartDamageRpc(float dmg, int i)
    {
        Debug.Log(dmg);
        bodyParts[i].partHealth -= dmg;
    }

    void AutoBodyPartsHealth()
    {
        for(int i = 0; i < bodyParts.Length; i++)
        {
            if (bodyParts[i].partHealth != 0)
                return;

            if (bodyParts[i].type == BodyPartTypes.torso)
                bodyParts[i].partHealth = health.Value * 0.5f;
            else if(bodyParts[i].type == BodyPartTypes.head)
                bodyParts[i].partHealth = health.Value * 0.1f;
            else if (bodyParts[i].type == BodyPartTypes.arm)
                bodyParts[i].partHealth = health.Value * 0.25f;
            else if (bodyParts[i].type == BodyPartTypes.leg)
                bodyParts[i].partHealth = health.Value * 0.35f;
        }

        GetComponent<EnemyAIBase>().RefreshPartsCount();
    }

    [System.Serializable]
    public struct BodyPart
    {
        public float partHealth;

        [Tooltip("parts to hide | optional")]
        public Transform[] parts;

        [Tooltip("colliders of parts hide | required")]
        public Collider[] colliders;
        public BodyPartTypes type;

        [Tooltip("prefab for child npc to instantiate | optional")]
        public Transform childNpcName;
        //public string ;
    }

    [Rpc(SendTo.Server)]
    void SyncBodyPartsHealthRpc()
    {
        float[] partsHealth = new float[bodyParts.Length];
        for(int i = 0; i < bodyParts.Length; i++)
        {
            partsHealth[i] = bodyParts[i].partHealth;
            //Debug.Log(bodyParts[i].partHealth + " " + i);
        }
        SyncBodyPartsHealthToClientRpc(partsHealth);
    }
    [Rpc(SendTo.Everyone)]
    void SyncBodyPartsHealthToClientRpc(float[] array)
    {
        
        for (int i = 0; i < bodyParts.Length; i++)
        {
            bodyParts[i].partHealth = array[i];
            //Debug.Log(array[i]);
            //Debug.Log(bodyParts[i] + " " + i);
        }
    }

    
    
}
