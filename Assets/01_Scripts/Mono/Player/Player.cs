using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour, IDamageable, ISoulCollector, IAbilityOwner
{
    public int Souls
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] TMP_Text soulsUiText;

    [SerializeField] private ParticleSystem walkEffects;

    [SerializeField] private UnityEvent OnReloadEvent;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private MoveTarget enemyMoveTarget;

    public Transform meshTransform;

    [SerializeField] private float abilityCooldown = 0.2f;

    [SerializeField] private Shooting playerShooting = new();

    [Space(2)]

    [SerializeField] private List<Ability> abilities = new(2);

    private bool canUseAbility = true;

    public void AfflictDamage ( float amount )
    {
        return;

        characterData.Health -= amount;

        if ( characterData.Health <= 0 )
        {
            Debug.Log("You Died.");
            gameObject.SetActive(false);
        }
    }

    public void OnShoot ( InputAction.CallbackContext context )
    {
        if ( context.phase != InputActionPhase.Performed )
            return;

        playerShooting.OnShoot(context);
    }

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        var inputVector = ctx.ReadValue<Vector2>();
        ApplyWalkEffects(inputVector != Vector2.zero);
        playerMovement.OnMove(ctx);
    }

    //The walking Effects.
    private void ApplyWalkEffects ( bool state )
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

    //Gets Activated when the dash key gets pressed, passes it onto the actual logic.
    public void OnDash ( InputAction.CallbackContext ctx )
    {
        if ( ctx.phase != InputActionPhase.Performed )
            return;

        playerMovement.OnDash();
    }

    //Use the ability, if it fails due to not having enough souls, re-add it.
    public async void OnUseAbility ( InputAction.CallbackContext ctx )
    {
        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;

        canUseAbility = false;

        var ability = abilities[^1];
        if ( ability.Trigger() )
        {
            if ( ability.Execute(characterData) )
            {
                abilities.Remove(ability);
            }
        }
        else
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"Not Enough Souls. Requires : {ability.ActivationCost}"));
        }

        await Task.Delay(TimeSpan.FromSeconds(abilityCooldown));
        canUseAbility = true;
    }

    public void OnReload ( InputAction.CallbackContext ctx )
    {
        if ( ctx.phase != InputActionPhase.Performed )
            return;

        OnReloadEvent?.Invoke();
        playerShooting.OnReload();
    }

    private void OnEnable ()
    {
        EventManagerGeneric<int>.AddListener(EventType.UponHarvestSoul, Collect);
        EventManagerGeneric<Transform>.AddListener(EventType.TargetSelection, ( transform ) => characterData.TargetedTransform = transform);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<int>.RemoveListener(EventType.UponHarvestSoul, Collect);
        EventManagerGeneric<Transform>.RemoveListener(EventType.TargetSelection, ( transform ) => characterData.TargetedTransform = transform);
        characterData.Reset();
        abilities.Clear();
        StopAllCoroutines();
    }

    //Intialization
    private void Start ()
    {
        characterData.Reset();
        characterData.CharacterTransform = transform;

        characterData.Player = true;

        playerMovement.characterData = characterData;
        playerMovement.OnStart();

        playerShooting.ownerData = characterData;
        playerShooting.OnStart(transform, this);

        Souls = 0;
        UpdateSoulsUI();

        characterData.MoveTarget = enemyMoveTarget;

        characterData.Initialize(UpdateSoulsUI);

        AcquireAbility(new ReapAbility());

        characterData.Health = characterData.MaxHealth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateSoulsUI ()
    {
        if ( MainThreadQueue.Instance == null )
            return;

        //Ensures it is executed on the main thread.
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

    //This gets called when you kill an enemy and you're not near a collection point.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Collect ( int amount )
    {
        Souls += amount;
    }

    //Returns if the player already owns the ability, otherwise adds it.
    public void AcquireAbility ( Ability ability )
    {
        Debug.Log(ability);

        ability.Initialize(this, characterData);

        EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(2.0f, $"Acquired : {ability.GetType()}"));
        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);
        abilities.Add(ability);
    }

    public Ability HarvestAbility ()
    {
        //Not Applicable.
        return null;
    }

    //The visual effects of the ability.
    public void OnExecuteAbility ( AbilityType type )
    {
    }
}