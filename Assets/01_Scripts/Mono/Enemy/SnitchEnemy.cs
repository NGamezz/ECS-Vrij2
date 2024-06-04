using System;
using System.Threading.Tasks;
using UnityEngine;

public class SnitchEnemy : Enemy, IAbilityOwner, ILockOnAble
{
    [SerializeField] private Ability ability = new SnitchingAbility();

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager )
    {
        EnemyType = EnemyType.SnitchEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);
        shooting.recoilMultiplier = 0;
    }

    private async void Attack ()
    {
        if ( !canShoot )
            return;
        canShoot = false;

        shooting.ShootSingle();
        await Task.Delay(TimeSpan.FromSeconds(shooting.currentGun.attackSpeed));
        canShoot = true;
    }

    private async void UseAbility ()
    {
        if ( !canUseAbility )
            return;
        canUseAbility = false;

        ability.Execute(characterData);
        await Task.Delay(TimeSpan.FromSeconds(ability.ActivationCooldown));
        canUseAbility = true;
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

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return ability;
    }

    public void AcquireAbility ( Ability ability ) { }

    public void OnExecuteAbility ( AbilityType type ) { }
}