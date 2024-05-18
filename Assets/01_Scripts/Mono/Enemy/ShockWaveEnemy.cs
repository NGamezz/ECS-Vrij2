using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ShockWaveEnemy : Enemy, IAbilityOwner
{
    private readonly Ability ability = new ShockWaveAbility();

    private bool canUseAbility = true;

    private float abilityCoolDown = 3;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        EnemyType = EnemyType.ShockWaveEnemy;
        this.characterData = characterData();
        base.OnStart(stats, moveTarget, startPosition, characterData);
        ability.Initialize(this, this.characterData);
    }

    public override void CheckAttackRange ( MoveTarget target, Vector3 targetPos )
    {
        if ( !canUseAbility || !GameObject.activeInHierarchy )
            return;

        var distanceToTarget = math.length(targetPos - Transform.position);

        if ( distanceToTarget < enemyStats.attackRange )
        {
            Debug.Log(target);

            var damagable = (IDamageable)target.target.GetComponentInParent(typeof(IDamageable));
            if ( damagable == null )
            {
                return;
            }

            canUseAbility = false;
            ability.Execute(characterData);
            StartCoroutine(ResetAbilityCooldown());
        }
    }

    private IEnumerator ResetAbilityCooldown()
    {
        yield return Utility.Yielders.Get(abilityCoolDown);
        canUseAbility = true;
    }

    public void AcquireAbility ( Ability ability )
    {
    }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return ability;
    }
}