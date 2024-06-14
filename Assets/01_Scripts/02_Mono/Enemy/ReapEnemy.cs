using System;
using UnityEngine;

public class ReapEnemy : Enemy, IAbilityOwner
{
    private Ability ability;
    private ReapAbility reapAbil = new();

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager, bool inAnimate = false )
    {
        EnemyType = EnemyType.ReapEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);

        if ( inAnimate )
            return;

        reapAbil.oneTimeUse = true;
        reapAbil.Initialize(this, this.characterData);
    }

    private void UseAbility ()
    {
        if ( !canUseAbility )
            return;

        canUseAbility = false;

        reapAbil.Execute(characterData);
    }

    protected override void Chasing ()
    {
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

    protected override void Attacking ()
    {
        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
            return;

        if ( !agent.isStopped )
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        MeshTransform.forward = (moveTarget.target.position - MeshTransform.position).normalized;
        UseAbility();
    }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return reapAbil;
    }

    public void AcquireAbility ( Ability ability ) { }

    public void OnExecuteAbility ( AbilityType type ) { }
}