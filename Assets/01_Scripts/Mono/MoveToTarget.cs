using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget
{
    private float distanceToPlayer = 5.0f;

    private MoveTarget moveTarget;
    private Transform ownTransform;

    private NavMeshAgent agent;

    private bool started = false;

    private float3 cachedPosition;

    public void SetStats ( EnemyStats stats )
    {
        distanceToPlayer = stats.attackRange;
        agent.speed = stats.moveSpeed;
    }

    public void Enable ()
    {
        started = true;
    }

    public void OnDisable ()
    {
        CancelPath();
        started = false;
    }

    public void OnUpdate ( Vector3 startPosition )
    {
        agent.Warp(startPosition);
    }

    public void OnStart ( MoveTarget target, Transform ownTransform, Vector3 startPosition )
    {
        agent = ownTransform.GetComponentInChildren<NavMeshAgent>();

        agent.Warp(startPosition);

        moveTarget = target;
        this.ownTransform = ownTransform;
    }

    public void OnFixedUpdate ()
    {
        if ( !started )
            return;

        if ( moveTarget.target == null )
            return;

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
            CancelPath();
        }
    }

    private void CancelPath ()
    {
        if ( agent == null || agent.isOnNavMesh == false || agent.isActiveAndEnabled == false )
            return;
        
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void UpdatePath ()
    {
        if ( agent == null || moveTarget.target == null || agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
        { return; }

        cachedPosition = moveTarget.target.position;
        agent.SetDestination(cachedPosition);
    }
}