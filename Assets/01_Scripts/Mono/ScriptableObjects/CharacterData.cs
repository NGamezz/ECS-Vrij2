using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    public Transform CharacterTransform { get; set; }
    public Transform TargetedTransform { get; set; }
    public Rigidbody Rigidbody { get; set; }

    private List<Type> ownedAbilityTypes;
    public List<Type> OwnedAbilityTypes
    {
        get
        {
            return ownedAbilityTypes ??= new();
        }
    }

    public MoveTarget MoveTarget { get; set; }

    [SerializeField] private int souls;
    public int Souls { get => souls; set => souls = value; }

    public float Health { get; set; }
    public float Stamina { get; set; }

    [SerializeField] private float maxStamina;
    public float MaxStamina { get => maxStamina; set => maxStamina = value; }

    [SerializeField] private float maxHealth;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    [SerializeField] private float speed;
    public float Speed { get => speed; set => speed = value; }

    [SerializeField] private float damageMultiplier;
    public float DamageMultiplier { get => damageMultiplier; set => damageMultiplier = value; }

    [SerializeField] private float speedMultiplier;
    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }

    [SerializeField] private float armour;
    public float Armour { get => armour; set => armour = value; }

    public bool Stunned { get; set; }

    public event Action OnStunned
    {
        add => OnStunned += value;
        remove => OnStunned -= value;
    }

    public void Reset ()
    {
        ownedAbilityTypes?.Clear();

        Stamina = MaxStamina;
        Health = MaxHealth;
        Stunned = false;
        Souls = 0;
    }
}