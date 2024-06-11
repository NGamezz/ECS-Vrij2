using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [NonSerialized] public Transform CharacterTransform;
    [NonSerialized] public Transform TargetedTransform;
    [NonSerialized] public Rigidbody Rigidbody;
    [NonSerialized] public MoveTarget MoveTarget;

    public bool Player = false;

    public GunStats currentGun;

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

    public int soulBankLimit = 20;

    public bool hasShield = false;
    [NonSerialized] public bool shieldActive = false;
    public float shieldRechargeSpeed = 5.0f;

    [NonSerialized] public Action UponSoulValueChanged;

    public Action OnHit;

    public bool canTakeDamage = true;

    public float abilityCostMultiplier = 1;

    public float Health;
    public float Stamina;

    public GameObject decoyPrefab;

    public float MaxStamina;
    public float MaxHealth;

    public float Speed;

    public float DamageMultiplier;

    public float Armour;

    private CharacterDataCache _cache;
    private CharacterDataCache cache
    {
        get
        {
            if ( _cache == null )
                return new(this);

            return _cache;
        }
    }

    public void Initialize ( Action uponSoulChanged )
    {
        _cache = new(this);
        UponSoulValueChanged = uponSoulChanged;
    }

    public void Reset ()
    {
        OnHit = null;
        hasShield = false;
        UponSoulValueChanged = null;
        abilityCostMultiplier = cache.abilityCostMultiplier;
        MaxHealth = cache.MaxHealth;
        MaxStamina = cache.MaxStamina;
        Armour = cache.Armour;
        Speed = cache.Speed;
        DamageMultiplier = cache.DamageMultiplier;
        Health = cache.Health;
        decoyPrefab = cache.decoyPrefab;
        Souls = cache.souls;
        Player = cache.Player;
        Stamina = cache.Stamina;
    }
}

class CharacterDataCache
{
    [NonSerialized] public Transform CharacterTransform;
    [NonSerialized] public Transform TargetedTransform;
    [NonSerialized] public Rigidbody Rigidbody;
    [NonSerialized] public MoveTarget MoveTarget;

    public bool Player = false;
    public float3 PlayerMousePosition;
    public GunStats currentGun;

    public int souls;
    [NonSerialized] public Action UponSoulValueChanged;
    public float abilityCostMultiplier = 1;
    public float Health;
    public float Stamina;

    public GameObject decoyPrefab;
    public float MaxStamina;
    public float MaxHealth;
    public float Speed;
    public float DamageMultiplier;
    public float Armour;

    public CharacterDataCache ( CharacterData data )
    {
        abilityCostMultiplier = data.abilityCostMultiplier;
        MaxHealth = data.MaxHealth;
        MaxStamina = data.MaxStamina;
        Armour = data.Armour;
        Speed = data.Speed;
        DamageMultiplier = data.DamageMultiplier;
        Health = data.Health;
        decoyPrefab = data.decoyPrefab;
        data.Souls = souls;
        Player = data.Player;
        Stamina = data.Stamina;
    }
}