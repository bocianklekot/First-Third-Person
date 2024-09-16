using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyConceptAI : EnemyAIBase
{
    
    void Update()
    {
        if (!NetworkManager.IsServer)
        {
            return;
        }

        BaseUpdate();

        if (passiveState.Value != EnemyPassiveStates.alive)
            return;

        //delete later
        if (activeState.Value == EnemyActiveState.alerted && distantanceToTarget < 3)
        {
            Attack();
        }
        else if (activeState.Value == EnemyActiveState.alerted | activeState.Value == EnemyActiveState.attacking && distantanceToTarget < 1)
        {
            agent.destination = agent.transform.position;
        }
        else if (activeState.Value == EnemyActiveState.alerted | activeState.Value == EnemyActiveState.attacking && distantanceToTarget > 2)
        {
            agent.destination = targetPlayer.transform.position;
        }
    }
}
