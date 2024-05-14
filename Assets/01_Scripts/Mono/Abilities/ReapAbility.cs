using UnityEngine;

//To be improved.
public class ReapAbility : Ability
{
    private CharacterData ownerData;

    public override void Execute ( object context )
    {
        if ( ownerData.Souls < ActivationCost )
            return;
        if ( ownerData.TargetedTransform == null )
            return;

        var enemyTransform = ownerData.TargetedTransform;
        if ( enemyTransform.gameObject.activeInHierarchy == false )
            return;

        var isAbilityOwner = enemyTransform.gameObject.TryGetComponent(out IAbilityOwner iAbilityOwner);

        if ( !isAbilityOwner )
        {
            return;
        }

        var ability = iAbilityOwner.HarvestAbility();
        ownerData.Souls -= (int)ActivationCost;

        if ( ownerData.OwnedAbilitiesHash.Contains(ability.GetType()) )
        {
            return;
        }

        Owner.AcquireAbility(ability);
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Owner = owner;
        ownerData = context;
        ActivationCost = 5;
        Trigger = () => { return Input.GetKeyDown(KeyCode.E); };
    }
}