//To be improved.
using System.Diagnostics;

public class ReapAbility : Ability
{
    public override bool Execute ( object context )
    {
        if ( ownerData.TargetedTransform == null )
        {
            UnityEngine.Debug.Log("No Targeted Transform.");
            return false;
        }

        var enemyTransform = ownerData.TargetedTransform;
        if ( enemyTransform.gameObject.activeInHierarchy == false )
            return false;

        IAbilityOwner abilityOwner;

        if ( enemyTransform.root == enemyTransform )
            abilityOwner = (IAbilityOwner)enemyTransform.gameObject.GetComponent(typeof(IAbilityOwner));
        else
            abilityOwner = (IAbilityOwner)enemyTransform.gameObject.GetComponentInParent(typeof(IAbilityOwner));

        if ( abilityOwner == null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, "Enemy Doesn't have an ability."));
            return false;
        }

        var ability = abilityOwner.HarvestAbility();
        ownerData.Souls -= (int)ActivationCost;

        Owner.AcquireAbility(ability);
        return false;
    }

    //Set the values, the trigger condition and the cost.
    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Owner = owner;
        ownerData = context;

        ActivationCooldown = 3;
        ActivationCost = 5;
        Trigger = () => { return ownerData.Souls >= ActivationCost; };
    }
}