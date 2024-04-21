using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget
{
    private float distanceToPlayer = 5.0f;

    private Transform target;
    private Transform ownTransform;

    private NavMeshAgent agent;

    public void SetStats ( EnemyStats stats )
    {
        distanceToPlayer = stats.attackRange;
        agent.speed = stats.moveSpeed;
    }

    public void OnDisable ()
    {
        if ( agent == null )
            return;

        agent.ResetPath();
        agent.isStopped = true;
    }

    public void OnStart ( Transform target, Transform ownTransform )
    {
        if ( !ownTransform.TryGetComponent(out agent) )
        {
            agent = ownTransform.AddComponent<NavMeshAgent>();
        }

        this.target = target;
        this.ownTransform = ownTransform;
    }

    public void OnFixedUpdate ()
    {
        if ( !agent.hasPath && Vector3.Distance(ownTransform.position, target.position) > distanceToPlayer )
        {
            UpdatePath();
        }

        CheckForCancelPath();
    }

    private void CheckForCancelPath ()
    {
        if ( Vector3.Distance(ownTransform.position, target.position) < distanceToPlayer )
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void UpdatePath ()
    {
        if ( agent == null || target == null )
        { return; }

        agent.SetDestination(target.position);
    }
}