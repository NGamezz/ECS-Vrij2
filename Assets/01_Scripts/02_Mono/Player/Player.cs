using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
                UpdateSoulsUI().Forget();
            }
        }
    }

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] TMP_Text soulsUiText;

    [SerializeField] private ParticleSystem walkEffects;

    [SerializeField] private UnityEvent OnReloadEvent;

    [SerializeField] private Slider[] sliders;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private MoveTarget enemyMoveTarget;

    [SerializeField] private UnityEvent<AbilityType> OnReapUse;
    [SerializeField] private UnityEvent<AbilityType> OnAbility1Use;
    [SerializeField] private UnityEvent<AbilityType> OnAbility2Use;
    [SerializeField] private UnityEvent<AbilityType> OnAbility3Use;

    public Transform meshTransform;

    private UpgradeHolder upgradeHolder;

    [SerializeField] private float abilityCooldown = 0.2f;

    [SerializeField] private Shooting playerShooting = new();

    private GameState gameState;

    [Space(2)]

    private IAbilityHolder abilityHolder = new PlayerAbilityHolder();

    private bool canUseAbility = true;

    public void OnShoot ( InputAction.CallbackContext context )
    {
        if ( context.phase != InputActionPhase.Performed )
            return;

        if ( gameState == GameState.Pauzed )
            return;

        playerShooting.OnShoot(context);
    }

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        var inputVector = ctx.ReadValue<Vector2>();
        ApplyWalkEffects(inputVector != Vector2.zero);
        playerMovement.OnMove(ctx);
    }

    private void SetGameState ( GameState state )
    {
        gameState = state;
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
        if ( gameState == GameState.Pauzed )
            return;

        if ( ctx.phase != InputActionPhase.Performed )
            return;

        playerMovement.OnDash();
    }

    #region abilityTriggers
    public void OnUseReapAbility ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;

        canUseAbility = false;

        var ability = abilityHolder.GetAbility(0);

        abilityHolder.UseAbility(0, characterData, null);

        OnReapUse?.Invoke(ability.Type);

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => canUseAbility = x, true).Forget();
    }

    //Use the ability, if it fails due to not having enough souls, re-add it.
    public void OnUseAbilityA ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        int index = 1;

        var ability = abilityHolder.GetAbility(index);

        OnReapUse?.Invoke(ability.Type);

        abilityHolder.UseAbility(index, characterData, () => sliders[index].gameObject.SetActive(false));

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => canUseAbility = x, true).Forget();
    }

    //Use the ability, if it fails due to not having enough souls, re-add it.
    public void OnUseAbilityB ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        int index = 2;
        var ability = abilityHolder.GetAbility(index);

        OnReapUse?.Invoke(ability.Type);

        abilityHolder.UseAbility(index, characterData, () => sliders[index].gameObject.SetActive(false));

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => canUseAbility = x, true).Forget();
    }

    //Use the ability, if it fails due to not having enough souls, re-add it.
    public void OnUseAbilityC ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        int index = 3;

        var ability = abilityHolder.GetAbility(index);

        OnReapUse?.Invoke(ability.Type);

        abilityHolder.UseAbility(index, characterData, () => sliders[index].gameObject.SetActive(false));

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => canUseAbility = x, true).Forget();
    }
    #endregion

    public void OnReload ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( ctx.phase != InputActionPhase.Performed )
            return;

        OnReloadEvent?.Invoke();
        playerShooting.OnReload();
    }

    private void OnEnable ()
    {
        EventManagerGeneric<int>.AddListener(EventType.UponHarvestSoul, Collect);
        EventManagerGeneric<Transform>.AddListener(EventType.TargetSelection, ( transform ) => characterData.TargetedTransform = transform);
        EventManagerGeneric<GameState>.AddListener(EventType.OnGameStateChange, SetGameState);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<int>.RemoveListener(EventType.UponHarvestSoul, Collect);
        EventManagerGeneric<Transform>.RemoveListener(EventType.TargetSelection, ( transform ) => characterData.TargetedTransform = transform);
        EventManagerGeneric<GameState>.RemoveListener(EventType.OnGameStateChange, SetGameState);
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
        characterData.Initialize(() => UpdateSoulsUI().Forget());

        playerMovement.characterData = characterData;
        playerMovement.OnStart();

        playerShooting.ownerData = characterData;
        playerShooting.OnStart(transform, this);

        Souls = 0;
        UpdateSoulsUI().Forget();

        AcquireAbility(new ReapAbility());

        characterData.Health = characterData.MaxHealth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async UniTaskVoid UpdateSoulsUI ()
    {
        await UniTask.SwitchToMainThread();

        soulsUiText.SetText($"Amount of Souls = {characterData.Souls}");
    }

    private void Update ()
    {
        if ( gameState == GameState.Pauzed )
            return;

        abilityHolder.ForeachAbility(( Index, abil, count ) =>
        {
            if ( Index + 1 >= count )
                return;

            sliders[Index + 1].value = characterData.Souls;
        });

        playerMovement.OnUpdate();
    }

    private void FixedUpdate ()
    {
        if ( gameState == GameState.Pauzed )
            return;

        playerMovement.OnFixedUpdate();
    }

    //This gets called when you kill an enemy and you're not near a collection point.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Collect ( int amount )
    {
        if ( characterData.Souls + amount > characterData.soulBankLimit )
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

        var index = abilityHolder.AddAbility(ability);
        sliders[index].gameObject.SetActive(true);
        sliders[index].maxValue = ability.ActivationCost;
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
    public Ability GetAbility ( int index );
    public int AddAbility ( Ability ability );
    public void ForeachAbility ( Action<int, Ability, int> body );
    public void UseAbility ( int index, object data, Action succesCallBack );
    public void RemoveRandomAbility ();
}

public class PlayerAbilityHolder : IAbilityHolder
{
    private List<Ability> abilities = new();

    public int AddAbility ( Ability ability )
    {
        if ( abilities.Count >= 4 || abilities.Contains(ability) )
            return -1;

        abilities.Add(ability);
        return abilities.Count - 1;
    }

    public Ability GetAbility ( int index )
    {
        return abilities[index] ?? null;
    }

    public void ForeachAbility ( Action<int, Ability, int> body )
    {
        for ( int i = 0; i < abilities.Count; ++i )
        {
            body(i, abilities[i], abilities.Count);
        }
    }

    public void RemoveRandomAbility ()
    {
        var nonNullAbilities = abilities.Where(x => x != null && x.GetType() != typeof(ReapAbility)).ToArray();

        var ability = nonNullAbilities[UnityEngine.Random.Range(0, nonNullAbilities.Length)];

        if ( ability != null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"{ability} was stolen."));
            abilities.Remove(ability);
        }
    }

    public void UseAbility ( int index, object data, Action succesCallBack )
    {
        if ( data is not CharacterData characterData )
            return;

        if ( index >= abilities.Count )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"No ability at position : {index}"));
            return;
        }

        var abilty = abilities[index];

        if ( abilty == null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"No ability at position : {index}"));
            return;
        }

        if ( !abilty.Trigger() )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"Not Enough Souls. Requires : {abilty.ActivationCost}"));
            return;
        }

        if ( !abilty.Execute(characterData) )
            return;

        succesCallBack?.Invoke();
        abilities.RemoveAt(index);
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