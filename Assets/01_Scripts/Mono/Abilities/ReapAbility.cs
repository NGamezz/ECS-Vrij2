using UnityEngine;

//To be improved.
public class ReapAbility : Ability
{
    public override bool Execute ( object context )
    {
        if ( ownerData.TargetedTransform == null )
            return false;

        var enemyTransform = ownerData.TargetedTransform;
        if ( enemyTransform.gameObject.activeInHierarchy == false )
            return false;

        var abilityOwner = (IAbilityOwner)enemyTransform.gameObject.GetComponent(typeof(IAbilityOwner));
        if ( abilityOwner == null )
        {
            return false;
        }

        var ability = abilityOwner.HarvestAbility();
        ownerData.Souls -= (int)ActivationCost;

        if ( ownerData.OwnedAbilitiesHash.Contains(ability.GetType()) )
        {
            return false;
        }

        Owner.AcquireAbility(ability);
        return false;
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Owner = owner;
        ownerData = context;

        ActivationCooldown = 3;
        //Set the values, the trigger condition and the cost.
        ActivationCost = 5;
        Trigger = () => { return InputHandler.IsKeyPressed(VirtualKeys.KeyE) && ownerData.Souls > ActivationCost; };
    }
}