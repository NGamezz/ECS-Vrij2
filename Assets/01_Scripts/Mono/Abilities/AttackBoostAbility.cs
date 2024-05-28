using System.Threading.Tasks;

public class AttackBoostAbility : Ability
{
    //In ms.
    private int boostDuration = 2000;
    private float damageMultiplierBoost = 1;

    //Likely not neccesary.
    private bool isActive = false;

    public override bool Execute ( object context )
    {
        ActivateBoost();
        return true;
    }

    private async void ActivateBoost ()
    {
        if ( isActive )
        { return; }

        isActive = true;
        ownerData.DamageMultiplier += damageMultiplierBoost;

        await Task.Delay(boostDuration);

        ownerData.DamageMultiplier -= damageMultiplierBoost;
        isActive = false;
    }

    public override void Initialize ( IAbilityOwner owner, CharacterData context )
    {
        ownerData = context;
        Owner = owner;

        ActivationCooldown = 3;

        Trigger = () => InputHandler.IsKeyPressed(VirtualKeys.KeyB);

        if ( !ownerData.Player )
            ownerData.DamageMultiplier += damageMultiplierBoost;
    }
}