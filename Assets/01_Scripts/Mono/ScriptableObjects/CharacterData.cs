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

    public HashSet<Type> OwnedAbilitiesHash = new();

    [SerializeField] private int souls;
    public int Souls
    {
        get => souls;
        set
        {
            souls = value;
            UponSoulValueChanged?.Invoke();
        }
    }

    [NonSerialized] public Action UponSoulValueChanged;

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

    public void ApplyStatusEffect ( CharacterStatusEffect effect, float duration )
    {
        TimedStatusEffect(duration, effect);
    }

    private async void TimedStatusEffect ( float duration, CharacterStatusEffect effect )
    {
        StatusEffects |= effect;
        await Awaitable.WaitForSecondsAsync(duration);
        StatusEffects &= ~(effect);
    }

    public void Initialize ( Action uponSoulChanged )
    {
        Reset();
        UponSoulValueChanged = uponSoulChanged;
    }

    public void Reset ()
    {
        Souls = 0;
        OwnedAbilitiesHash?.Clear();

        UponSoulValueChanged = null;

        StatusEffects = CharacterStatusEffect.Default;

        Stamina = MaxStamina;
        Health = MaxHealth;
        Souls = 0;
    }
}