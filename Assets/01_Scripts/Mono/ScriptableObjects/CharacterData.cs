using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum CharacterStatusEffect
{
    Default = 0,
    Stunned = 1,
}

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [NonSerialized] public Transform CharacterTransform;
    [NonSerialized] public Transform TargetedTransform;
    [NonSerialized] public Rigidbody Rigidbody;
    [NonSerialized] public MoveTarget MoveTarget;

    private List<Type> ownedAbilityTypes;
    public List<Type> OwnedAbilityTypes
    {
        get
        {
            return ownedAbilityTypes ??= new();
        }
    }

    public int Souls;

    public float Health;
    public float Stamina;

    public float MaxStamina;

    public float MaxHealth;

    public float Speed;

    public float DamageMultiplier;

    public float SpeedMultiplier;

    public float Armour;

    public CharacterStatusEffect StatusEffects = CharacterStatusEffect.Default;

    public event Action OnStunned
    {
        add => OnStunned += value;
        remove => OnStunned -= value;
    }

    public void Reset ()
    {
        ownedAbilityTypes?.Clear();

        StatusEffects = CharacterStatusEffect.Default;

        Stamina = MaxStamina;
        Health = MaxHealth;
        Souls = 0;
    }
}