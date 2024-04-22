using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget
{
    private float distanceToPlayer = 5.0f;

    private Transform target;
    private Transform ownTransform;

    private NavMeshAgent agent;

    private float3 cachedPosition;

    public void SetStats ( EnemyStats stats )
    {
        distanceToPlayer = stats.attackRange;
        agent.speed = stats.moveSpeed;
    }

    public void OnDisable ()
    {
        if ( agent == null || !agent.isActiveAndEnabled )
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
        if ( Vector3.Distance(ownTransform.position, target.position) < distanceToPlayer || Vector3.Distance(cachedPosition, target.position) > 5.0f)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void UpdatePath ()
    {
        if ( agent == null || target == null )
        { return; }

        cachedPosition = target.position;
        agent.SetDestination(target.position);
    }
}