//To be improved.
using System.Diagnostics;
using UnityEngine;

public class ReapAbility : Ability
{
    private void Player ( CharacterData ownerData )
    {
        if ( ownerData.TargetedTransform == null )
        {
            UnityEngine.Debug.Log("No Targeted Transform.");
            return;
        }

        var enemyTransform = ownerData.TargetedTransform;
        if ( enemyTransform.gameObject.activeInHierarchy == false )
            return;

        IAbilityOwner abilityOwner;

        if ( enemyTransform.root == enemyTransform )
            abilityOwner = (IAbilityOwner)enemyTransform.gameObject.GetComponent(typeof(IAbilityOwner));
        else
            abilityOwner = (IAbilityOwner)enemyTransform.gameObject.GetComponentInParent(typeof(IAbilityOwner));

        if ( abilityOwner == null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, "Enemy Doesn't have an ability."));
            return;
        }

        var ability = abilityOwner.HarvestAbility();
        ownerData.Souls -= (int)ActivationCost;

        Owner.AcquireAbility(ability);
        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);

        Owner.OnExecuteAbility(Type);
    }

    private void Enemy ( CharacterData ownerData )
    {
        if ( ownerData.MoveTarget.target == null )
        {
            return;
        }

        var enemyTransform = ownerData.MoveTarget.target;
        if ( enemyTransform.gameObject.activeInHierarchy == false )
            return;

        //Not the right way of doing it but oh well. Better way would be with an interface, both in terms of principle and performance.
        PlayerManager abilityOwner;
        if ( enemyTransform.root == enemyTransform )
            abilityOwner = (PlayerManager)enemyTransform.gameObject.GetComponent(typeof(PlayerManager));
        else
            abilityOwner = (PlayerManager)enemyTransform.gameObject.GetComponentInParent(typeof(PlayerManager));

        if ( abilityOwner == null )
        {
            return;
        }

        abilityOwner.RemoveRandomUpgrade();
        Owner.OnExecuteAbility(AbilityType.None);

    }

    public override bool Execute ( object context )
    {
        if ( context is not CharacterData data )
            return false;

        if ( data.Player )
        {
            Player(data);
        }
        else
        {
            Enemy(data);
        }

        return false;
    }

    //Set the values, the trigger condition and the cost.
    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Owner = owner;
        ownerData = context;

        ActivationCooldown = 3;
        ActivationCost = 5;
        Trigger = () => { return ownerData.Souls >= (ActivationCost * ownerData.abilityCostMultiplier); };
    }
}