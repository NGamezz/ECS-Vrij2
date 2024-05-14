using System;

[Serializable]
public abstract class Ability
{
    public float ActivationCost { get; protected set; }
    public float ActivationCooldown { get; protected set; }
    public IAbilityOwner Owner { get; protected set; }
    public abstract void Execute ( object context );
    public abstract void Initialize ( IAbilityOwner owner, CharacterData context );
    public Func<bool> Trigger;
}

public interface IAbilityOwner
{
    public void AcquireAbility ( Ability ability );
    public Ability HarvestAbility ();
}