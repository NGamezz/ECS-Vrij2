using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour, ISoulCollector, IAbilityOwner, IUpgradable, ICharacterDataHolder
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

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] TMP_Text soulsUiText;

    [SerializeField] private ParticleSystem walkEffects;

    [SerializeField] private UnityEvent OnReloadEvent;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private MoveTarget enemyMoveTarget;

    public Transform meshTransform;

    private UpgradeHolder upgradeHolder;

    [SerializeField] private float abilityCooldown = 0.2f;

    [SerializeField] private Shooting playerShooting = new();

    [Space(2)]

    private IAbilityHolder abilityHolder = new PlayerAbilityHolder();

    private bool canUseAbility = true;

    public void OnShoot ( InputAction.CallbackContext context )
    {
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

        var isPlaying = walkEffects.isPlaying;

        if ( state && !isPlaying )
        {
            walkEffects.Play();
            var main = walkEffects.main;
            main.loop = true;
        }
        else if ( !state && isPlaying )
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

        abilityHolder.UseAbility(characterData);

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
        upgradeHolder.RemoveAll();
        StopAllCoroutines();
    }

    //Intialization
    private void Start ()
    {
        characterData.CharacterTransform = transform;

        characterData.Player = true;

        upgradeHolder = new(characterData);

        characterData.MoveTarget = enemyMoveTarget;
        characterData.Initialize(UpdateSoulsUI);
        
        playerMovement.characterData = characterData;
        playerMovement.OnStart();

        playerShooting.ownerData = characterData;
        playerShooting.OnStart(transform, this);

        Souls = 0;
        UpdateSoulsUI();

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
        if(characterData.Souls + amount > characterData.soulBankLimit)
        {
            Souls = characterData.soulBankLimit;
            return;
        }

        Souls += amount;
    }

    //Returns if the player already owns the ability, otherwise adds it.
    public void AcquireAbility ( Ability ability )
    {
        ability.Initialize(this, characterData);

        EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(2.0f, $"Acquired : {ability.GetType()}"));
        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);

        abilityHolder.AddAbility(ability);
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

    public void Upgrade ( IUpgrade upgrade )
    {
        upgradeHolder.AcquireUpgrade(upgrade);
    }

    public void SetCharacterData ( CharacterData characterData )
    {
        playerMovement.characterData = characterData;
        playerShooting.ownerData = characterData;
        this.characterData = characterData;
    }

    public void RemoveRandomUpgrade ()
    {
        upgradeHolder.UndoRandomAction();
    }
}

//To abstract the usage of the abilities a bit.
public interface IAbilityHolder
{
    public void AddAbility ( Ability ability );
    public void UseAbility ( object data );
}

public class PlayerAbilityHolder : IAbilityHolder
{
    private List<Ability> abilities = new(2);

    public void AddAbility ( Ability ability )
    {
        if ( abilities.Contains(ability) )
            return;

        abilities.Add(ability);
    }

    public void UseAbility ( object data )
    {
        CharacterData characterData;
        try
        {
            characterData = data as CharacterData;
        }
        catch ( Exception )
        {
            return;
        }

        var ability = abilities[^1];
        if ( ability.Trigger() )
        {
            if ( ability.Execute(characterData) )
            {
                abilities.Remove(ability);
                return;
            }
        }
        else
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"Not Enough Souls. Requires : {ability.ActivationCost}"));
        }
    }
}

public interface IUpgradeHolder
{
    public void AcquireUpgrade ( IUpgrade upgrade );
    public void RemoveAll ();
    public void UndoRandomAction ();
}

public interface IUpgrade
{
    public void Add ( object ctx );
    public void Remove ( object ctx );
}

public interface IUpgradable
{
    public void Upgrade ( IUpgrade upgrade );

    public void RemoveRandomUpgrade ();
}

public class UpgradeHolder : IUpgradeHolder
{
    private CharacterData data;

    public UpgradeHolder ( CharacterData data )
    {
        this.data = data;
    }

    private List<Action> undoActions = new();

    public void AcquireUpgrade ( IUpgrade upgrade )
    {
        upgrade.Add(data);
        undoActions.Add(() => upgrade.Remove(data));
    }

    public void RemoveAll ()
    {
        undoActions.Clear();
    }

    public void UndoRandomAction ()
    {
        var randomIndex = UnityEngine.Random.Range(0, undoActions.Count);
        var upgrade = undoActions[randomIndex];
        upgrade();

        undoActions.RemoveAt(randomIndex);
    }
}