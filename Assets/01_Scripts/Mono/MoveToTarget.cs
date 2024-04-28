using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget
{
    private float distanceToPlayer = 5.0f;

    private MoveTarget moveTarget;
    private Transform ownTransform;

    private NavMeshAgent agent;

    private float3 cachedPosition;

    public void SetStats ( EnemyStats stats)
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

    public void OnStart ( MoveTarget target, Transform ownTransform )
    {
        if ( !ownTransform.TryGetComponent(out agent) )
        {
            agent = ownTransform.AddComponent<NavMeshAgent>();
        }

        moveTarget = target;
        this.ownTransform = ownTransform;
    }

    public void OnFixedUpdate ()
    {
        if ( !agent.hasPath && Vector3.Distance(ownTransform.position, moveTarget.target.position) > distanceToPlayer )
        {
            UpdatePath();
        }

        CheckForCancelPath();
    }

    private void CheckForCancelPath ()
    {
        Vector3 targetPos = moveTarget.target.position;
        if ( Vector3.Distance(ownTransform.position, targetPos) < distanceToPlayer || Vector3.Distance(cachedPosition, targetPos) > 5.0f )
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void UpdatePath ()
    {
        if ( agent == null || moveTarget.target == null )
        { return; }

        cachedPosition = moveTarget.target.position;
        agent.SetDestination(cachedPosition);
    }
}