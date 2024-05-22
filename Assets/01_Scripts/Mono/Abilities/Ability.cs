using System;
using System.Threading.Tasks;

[Serializable]
public abstract class Ability
{
    public float ActivationCost { get; protected set; }
    public float ActivationCooldown { get; protected set; }
    public IAbilityOwner Owner { get; protected set; }
    public abstract bool Execute ( object context );
    public abstract void Initialize ( IAbilityOwner owner, CharacterData context );
    public Func<bool> Trigger;

    protected CharacterData ownerData;
}

public interface IAbilityOwner
{
    public void AcquireAbility ( Ability ability, bool oneTimeUse = true );
    public Ability HarvestAbility ();
}