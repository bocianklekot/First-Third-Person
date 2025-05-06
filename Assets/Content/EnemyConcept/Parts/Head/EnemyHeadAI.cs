using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadAI : EnemyAIBase
{
    void Update()
    {
        if (!agent.isActiveAndEnabled | !NetworkManager.IsServer)
        {
            return;
        }

        BaseUpdate();

        if (passiveState.Value != EnemyPassiveStates.alive)
            return;

        //delete later //later: idk if actually tho
        if (activeState.Value == EnemyActiveState.alerted && distantanceToTarget < 1.5f)
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

    public override void Attack()
    {
        StartCoroutine(DecideAttack());
        StartCoroutine(ContinueAttack(() =>
            {
                Die();
            }));
    }

    IEnumerator ContinueAttack(System.Action callback)
    {
        yield return new WaitUntil(() => decideAttackFinished); // Czeka, a¿ isReady bêdzie true
        callback?.Invoke(); // Wywo³uje przekazan¹ funkcjê
    }
}
