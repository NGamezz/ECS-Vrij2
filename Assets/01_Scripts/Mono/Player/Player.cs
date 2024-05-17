using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour, IDamageable, ISoulCollector, IAbilityOwner
{
    public int Souls
    {
        get
        {
            return characterData.Souls;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if ( characterData.Souls != value )
            {
                characterData.Souls = value;

                //Kind of dirty but oh well.
                UpdateSoulsUI();
            }
        }
    }

    public bool Dead { get; private set; }

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField] TMP_Text soulsUiText;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private MoveTarget enemyMoveTarget;

    [SerializeField] private GameObject decoyPrefab;

    [SerializeField]
    private PlayerShooting playerShooting = new();

    [Space(2)]

    [SerializeField] private List<Ability> abilities = new();

    public void AfflictDamage ( float amount )
    {
        characterData.Health -= amount;

        if ( characterData.Health <= 0 )
        {
            Debug.Log("You Died.");
            gameObject.SetActive(false);
        }
    }

    public void OnShoot ( InputAction.CallbackContext context )
    {
        playerShooting.OnShoot(context);
    }

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        playerMovement.OnMove(ctx);
    }

    public void OnDash ()
    {
        playerMovement.OnDash();
    }

    public void OnReload ()
    {
        playerShooting.OnReload();
    }

    private void OnEnable ()
    {
        EventManagerGeneric<int>.AddListener(EventType.UponHarvestSoul, Collect);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<int>.RemoveListener(EventType.UponHarvestSoul, Collect);
        characterData.Reset();
        abilities.Clear();
        StopAllCoroutines();
    }

    private void Start ()
    {
        characterData.Reset();
        characterData.CharacterTransform = transform;

        characterData.Player = true;

        playerMovement.characterData = characterData;
        playerMovement.OnStart();
        playerShooting.OnStart(transform, this);

        Souls = 0;

        characterData.decoyPrefab = decoyPrefab;
        characterData.MoveTarget = enemyMoveTarget;

        characterData.Initialize(UpdateSoulsUI);

        ReapAbility reapAbility = new();
        reapAbility.Initialize(this, characterData);
        abilities.Add(reapAbility);

        LieAbility lieAbility = new();
        lieAbility.Initialize(this, characterData);
        abilities.Add(lieAbility);

        characterData.Health = characterData.MaxHealth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateSoulsUI ()
    {
        if ( GameManager.Instance == null )
            return;

        GameManager.Instance.Enqueue(() =>
        {
            soulsUiText.SetText($"Amount of Souls = {characterData.Souls}");
        });
    }

    private void CheckAbilityTriggers ()
    {
        for ( int i = 0; i < abilities.Count; i++ )
        {
            var ability = abilities[i];
            if ( ability == null )
                continue;

            if ( ability.Trigger != null && ability.Trigger() )
            {
                if ( ability.Execute(characterData) )
                {
                    abilities.Remove(ability);
                    characterData.OwnedAbilitiesHash.Remove(ability.GetType());
                }
            }
        }
    }

    private void Update ()
    {
        CheckAbilityTriggers();
        playerMovement.OnUpdate();
    }

    private void FixedUpdate ()
    {
        playerMovement.OnFixedUpdate();
    }

    public void Collect ( int amount )
    {
        Souls += amount;
    }

    public void AcquireAbility ( Ability ability )
    {
        if ( characterData.OwnedAbilitiesHash.Contains(ability.GetType()) )
        {
            return;
        }

        characterData.OwnedAbilitiesHash.Add(ability.GetType());
        characterData.TargetedTransform = null;
        ability.Initialize(this, characterData);
        abilities.Add(ability);
    }

    public Ability HarvestAbility ()
    {
        return null;
    }
}