using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ShockWaveEnemy : Enemy, IAbilityOwner, ILockOnAble
{
    private readonly Ability ability = new ShockWaveAbility();

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager, bool inAnimate = false )
    {
        EnemyType = EnemyType.ShockWaveEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);

        if ( inAnimate )
            return;
        ability.Initialize(this, characterData());
    }

    private void Attack ()
    {
        if ( !canShoot )
            return;
        canShoot = false;

        shooting.ShootSingle();

        Utility.Async.ChangeValueAfterSeconds(shooting.currentGun.attackSpeed, ( x ) => canShoot = x, true, this.GetCancellationTokenOnDestroy()).Forget();
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
        Attack();
        UseAbility();
    }

    public void AcquireAbility ( Ability ability ) { }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return ability;
    }

    public void OnExecuteAbility ( AbilityType type ) { }
}