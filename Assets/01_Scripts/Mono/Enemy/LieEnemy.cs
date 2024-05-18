using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LieEnemy : Enemy, IAbilityOwner
{
    private Ability ability = new LieAbility();

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        EnemyType = EnemyType.LieEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData);
        ability.Initialize(this, characterData());
    }

    public override void CheckAttackRange ( MoveTarget target, Vector3 targetPos )
    {
        if ( !canUseAbility || !GameObject.activeInHierarchy )
            return;

        var distanceToTarget = math.length(targetPos - Transform.position);

        if ( distanceToTarget < enemyStats.attackRange )
        {
            var transform = target.target;

            IDamageable damageable;
            if ( transform.root == transform )
            {
                damageable = (IDamageable)transform.GetComponent(typeof(IDamageable));
            }
            else
            {
                damageable = (IDamageable)target.target.GetComponentInParent(typeof(IDamageable));
            }

            if ( damageable == null )
            {
                return;
            }

            canUseAbility = false;
            ability.Execute(characterData);
            StartCoroutine(ResetAbilityCooldown());
        }
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

    public void AcquireAbility ( Ability ability )
    {
    }
}