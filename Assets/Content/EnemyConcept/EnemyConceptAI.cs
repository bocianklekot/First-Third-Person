using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyConceptAI : EnemyAIBase
{
    
    void Update()
    {

        BaseUpdate();
        if (passiveState != EnemyPassiveStates.alive)
            return;

        //delete later
        if (activeState == EnemyActiveState.alerted && distantanceToTarget < 3)
        {
            Attack();
        }
        else if (activeState == EnemyActiveState.alerted | activeState == EnemyActiveState.attacking && distantanceToTarget < 1)
        {
            agent.destination = agent.transform.position;
        }
        else if (activeState == EnemyActiveState.alerted | activeState == EnemyActiveState.attacking && distantanceToTarget > 2)
        {
            agent.destination = targetPlayer.transform.position;
        }
    }
}
