using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyConceptAI : EnemyAIBase
{

    
    void Update()
    {
        if (healthComponent.health <= 0 && state != EnemyStates.dead)
            Die();

    }
}
