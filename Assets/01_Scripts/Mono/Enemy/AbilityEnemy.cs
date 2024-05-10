using System.Collections;
using UnityEngine;

public class AbilityEnemy : Enemy, IAbilityOwner
{
    [SerializeField] private Ability ability = new SnitchingAbility();

    private bool canUseAbility = true;

    public override void OnFixedUpdate ()
    {
        if ( !GameObject.activeInHierarchy )
            return;

        base.OnFixedUpdate();

        if ( canUseAbility )
        {
            ability.Execute(null);
            canUseAbility = false;
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