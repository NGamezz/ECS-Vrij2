using System;

[Serializable]
public abstract class Ability
{
    public float ActivationCost { get; protected set; }

    public float ActivationCooldown { get; protected set; }

    public IAbilityOwner Owner { get; protected set; }

    public abstract bool Execute ( object context );

    public AbilityType Type { get; protected set; }

    public abstract void Initialize ( IAbilityOwner owner, CharacterData context );

    public Func<bool> Trigger;

    protected CharacterData ownerData;
}

public enum AbilityType
{
    None = 0,
    LieAbility = 1,
    AttackAbility = 2,
    ShockWaveAbility = 3,
    SnitchAbility = 4,
    ReapAbility = 5,
}

public interface IAbilityOwner
{
    public void AcquireAbility ( Ability ability );
    public void OnExecuteAbility ( AbilityType type );
    public Ability HarvestAbility ();
}