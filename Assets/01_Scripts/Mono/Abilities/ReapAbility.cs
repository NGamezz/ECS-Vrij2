using UnityEngine;

//To be improved.
public class ReapAbility : Ability
{
    private CharacterData characterData;

    public override void Execute ( object context )
    {
        if ( context == null || characterData.TargetedTransform == null )
            return;

        var enemyTransform = characterData.TargetedTransform;

        if ( enemyTransform.gameObject.activeInHierarchy == false )
            return;

        var isAbilityOwner = enemyTransform.gameObject.TryGetComponent(out IAbilityOwner iAbilityOwner);

        if ( !isAbilityOwner )
        {
            return;
        }

        var ability = iAbilityOwner.HarvestAbility();

        if ( characterData.OwnedAbilityTypes.Contains(ability.GetType()) )
        {
            return;
        }

        Owner.AcquireAbility(ability);
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        Owner = owner;
        characterData = context;
        ActivationCost = 10;
    }

    public override bool Trigger ()
    {
        return Input.GetKeyDown(KeyCode.R) && characterData.Souls >= ActivationCost;
    }
}