using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class SnitchEnemy : Enemy, IAbilityOwner
{
    [SerializeField] private Ability ability = new SnitchingAbility();

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        EnemyType = EnemyType.SnitchEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData);
        shooting.recoilMultiplier = 0;
    }

    public override void CheckAttackRange ( MoveTarget target, Vector3 targetPos )
    {
        if ( !GameObject.activeInHierarchy )
            return;

        var distanceToTarget = math.length(targetPos - Transform.position);

        if ( distanceToTarget > enemyStats.attackRange )
            return;

        if ( canUseAbility )
        {
            var damagable = (IDamageable)target.target.GetComponentInParent(typeof(IDamageable));
            if ( damagable == null )
            {
                return;
            }

            canUseAbility = false;
            ability.Execute(characterData);
            StartCoroutine(ResetAbilityCooldown());
        }

        if ( canAttack )
        {
            shooting.ShootSingle();

            canAttack = false;
            StartCoroutine(ResetAttack());
        }

        Transform.forward = target.target.position - Transform.position;
    }

    private IEnumerator ResetAbilityCooldown ()
    {
        yield return Utility.Yielders.Get(ability.ActivationCooldown);
        canUseAbility = true;
    }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return ability;
    }

    public void AcquireAbility ( Ability ability, bool singleUse = true )
    {
    }
}