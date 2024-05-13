using System.Collections.Generic;
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
        set
        {
            characterData.Souls = value;
            soulsUiText.SetText($"Amount of Souls = {characterData.Souls}");
        }
    }

    public bool Dead { get; private set; }

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField] TMP_Text soulsUiText;

    [SerializeField] private CharacterData characterData;

    [SerializeField]
    private PlayerShooting playerShooting = new();

    [Space(2)]

    [SerializeField] private float health;

    private List<Ability> abilities = new();

    public void AfflictDamage ( float amount )
    {
        health -= amount;

        if ( health <= 0 )
            gameObject.SetActive(false);
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
        playerMovement.characterData = characterData;
        playerMovement.OnStart();
        playerShooting.OnStart(transform, this);

        Souls = 0;

        ReapAbility reapAbility = new();
        reapAbility.Initialize(this, characterData);
        abilities.Add(reapAbility);

        health = characterData.MaxHealth;
    }

    private void CheckAbilityTriggers ()
    {
        for ( int i = 0; i < abilities.Count; i++ )
        {
            var ability = abilities[i];
            if ( ability == null )
                continue;

            if ( ability.Trigger() )
            {
                ability.Execute(characterData);
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
        if ( characterData.OwnedAbilityTypes.Contains(ability.GetType()) )
        {
            return;
        }

        characterData.OwnedAbilityTypes.Add(ability.GetType());
        ability.Initialize(this, characterData);
        abilities.Add(ability);
    }

    public Ability HarvestAbility ()
    {
        return null;
    }
}