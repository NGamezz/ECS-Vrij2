using System;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IState
{
    public bool IsActive { get; set; }

    public MoveTarget moveTarget;
    public Transform meshTransform;

    public NavMeshAgent agent;
    public EnemyStats enemyStats;

    public Func<bool> overrideChase;

    public Func<bool> EnterCondition
    {
        get => EnterCond;
        set => Debug.Log("");
    }

    private bool EnterCond ()
    {
        if ( moveTarget.target == null )
            return false;

        return Vector3.Distance(meshTransform.position, moveTarget.target.position) > enemyStats.attackRange;
    }

    public Blackboard blackBoard;

    public void OnEnter ()
    {
    }

    public void OnExit ()
    {
    }

    public void OnFixedUpdate ()
    {
        var over = overrideChase?.Invoke();

        if ( over.Value )
        {
            Debug.Log("Override Chase.");
            return;
        }

        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
        {
            return;
        }

        if ( moveTarget.target != null && (agent.hasPath == false || Vector3.Distance(agent.pathEndPosition, moveTarget.target.position) > 5.0f) )
        {
            agent.SetDestination(moveTarget.target.position);
        }
        agent.isStopped = false;
    }
}