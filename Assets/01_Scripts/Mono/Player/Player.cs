using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField] private ParticleSystem walkEffects;

    [SerializeField] private UnityEvent OnReloadEvent;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private MoveTarget enemyMoveTarget;

    [SerializeField] private GameObject decoyPrefab;

    [SerializeField] private Transform meshTransform;

    [SerializeField]
    private Shooting playerShooting = new();

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
        var inputVector = ctx.ReadValue<Vector2>();
        OnMoving(inputVector != Vector2.zero);
        playerMovement.OnMove(ctx);
    }

    private void OnMoving ( bool state )
    {
        if ( walkEffects == null )
            return;

        if ( state && !walkEffects.isPlaying )
        {
            walkEffects.Play();
            var main = walkEffects.main;
            main.loop = true;
        }
        else if ( !state && walkEffects.isPlaying )
        {
            walkEffects.Stop();
            var main = walkEffects.main;
            main.loop = false;
        }
    }

    public void OnDash ()
    {
        playerMovement.OnDash();
    }

    public void OnReload ()
    {
        OnReloadEvent?.Invoke();
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
        AcquireAbility(reapAbility);

        ShockWaveAbility shockWaveAbility = new();
        AcquireAbility(shockWaveAbility);

        characterData.Health = characterData.MaxHealth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateSoulsUI ()
    {
        if ( MainThreadQueue.Instance == null )
            return;

        MainThreadQueue.Instance.Enqueue(() =>
        {
            soulsUiText.SetText($"Amount of Souls = {characterData.Souls}");
        });
    }

    private void Update ()
    {
        playerMovement.OnUpdate();
    }

    private void FixedUpdate ()
    {
        playerMovement.OnFixedUpdate();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        ability.Initialize(this, characterData);
        InputHandler.Instance.BindCommand(ability.Trigger, () =>
        {
            if ( ability.Execute(characterData) )
            {
                abilities.Remove(ability);
                characterData.OwnedAbilitiesHash.Remove(ability.GetType());
            }
        }, false, true);

        characterData.OwnedAbilitiesHash.Add(ability.GetType());
        characterData.TargetedTransform = null;

        abilities.Add(ability);
    }

    public Ability HarvestAbility ()
    {
        return null;
    }
}