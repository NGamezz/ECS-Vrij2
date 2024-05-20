using System;
using UnityEngine;

public class AngryEnemy : Enemy, IAbilityOwner
{
    private Ability ability = new AttackBoostAbility();

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        base.OnStart(stats, moveTarget, startPosition, characterData);
        ability.Initialize(this, this.characterData);
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