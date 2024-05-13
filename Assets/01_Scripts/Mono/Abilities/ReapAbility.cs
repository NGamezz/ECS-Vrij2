using UnityEngine;

//To be improved.
public class ReapAbility : Ability
{
    private CharacterData characterData;

    public override void Execute ( object context )
    {
        if ( characterData.TargetedTransform == null )
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

        characterData.Souls -= (int)ActivationCost;

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
        return Input.GetKeyDown(KeyCode.E) && characterData.Souls >= ActivationCost;
    }
}