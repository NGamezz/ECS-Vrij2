using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class LieEnemy : Enemy, IAbilityOwner
{
    private Ability ability = new LieAbility();
    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager )
    {
        EnemyType = EnemyType.LieEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);
        ability.Initialize(this, characterData());
    }

    private void UseAbility ()
    {
        if ( !canUseAbility )
            return;
        canUseAbility = false;

        ability.Execute(characterData);

        Utility.Async.ChangeValueAfterSeconds(ability.ActivationCooldown, ( x ) => canUseAbility = x, true, this.GetCancellationTokenOnDestroy()).Forget();
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
        return null;
    }

    public void AcquireAbility ( Ability ability ) { }

    public void OnExecuteAbility ( AbilityType type ) { }
}