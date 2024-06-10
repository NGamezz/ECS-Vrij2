using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ReapEnemy : Enemy, IAbilityOwner
{
    private Ability ability = new ReapAbility();
    private bool canUseAbility = true;

  
    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager, bool inAnimate = false )
    {
        EnemyType = EnemyType.ReapEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);

        if ( inAnimate )
            return;
        ability.Initialize(this, this.characterData);
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
        return ability;
    }

    public void AcquireAbility ( Ability ability ) { }

    private async UniTaskVoid MoveAway ()
    {
        overrideChase = true;

        Vector3 newPos = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(10, 20);
        agent.SetDestination(newPos);

        await UniTask.WaitUntil(() => agent.remainingDistance <= 2.0f || agent.isPathStale);

        overrideChase = false;
    }

    public void OnExecuteAbility ( AbilityType type )
    {
        MoveAway().Forget();
    }
}