using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            }
        }
    }

    [SerializeField] private Image soulsBar;
    [SerializeField] private Image reloadBar;
    [SerializeField] private Image dashBar;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private ParticleSystem walkEffects;

    [SerializeField] private UnityEvent OnReloadEvent;

    [SerializeField] private Image[] abilityCooldownBars;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private MoveTarget enemyMoveTarget;

    [SerializeField] private UnityEvent OnReapUse;
    [SerializeField] private UnityEvent OnStolenReap;
    [SerializeField] private UnityEvent OnLie;

    [SerializeField] private UnityEvent PreOnShockWave;
    [SerializeField] private UnityEvent PostOnShockWave;

    public Transform meshTransform;

    private UpgradeHolder upgradeHolder;

    [SerializeField] private float abilityCooldown = 0.2f;

    [SerializeField] private Shooting playerShooting = new();

    private GameState gameState = GameState.Running;

    [Space(2)]

    private IAbilityHolder abilityHolder = new PlayerAbilityHolder();

    private bool canUseAbility = true;

    public void OnShoot ( InputAction.CallbackContext context )
    {
        if ( gameState == GameState.Pauzed )
        {
            return;
        }

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

        playerMovement.OnDash(( count ) => dashBar.fillAmount = count / playerMovement.dashCooldown, () => dashBar.fillAmount = 0);
    }

    #region abilityTriggers
    public void OnUseReapAbility ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        const int index = 0;

        var succes = abilityHolder.UseAbility(index, characterData, () => abilityCooldownBars[index].gameObject.SetActive(false));

        if ( succes )
            OnReapUse?.Invoke();

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => { canUseAbility = x; Debug.Log("FinishCallBack"); }, true).Forget();
    }

    public void OnUseAbilityA ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        const int index = 1;

        var succes = abilityHolder.UseAbility(index, characterData, () => abilityCooldownBars[index - 1].gameObject.SetActive(false));
        if ( succes )
            OnStolenReap?.Invoke();

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => canUseAbility = x, true).Forget();
    }

    public void OnUseAbilityB ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        const int index = 2;

        var succes = abilityHolder.UseAbility(index, characterData, () => abilityCooldownBars[index - 1].gameObject.SetActive(false));
        if ( succes )
            OnLie?.Invoke();

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => canUseAbility = x, true).Forget();
    }

    public void OnUseAbilityC ( InputAction.CallbackContext ctx )
    {
        if ( gameState == GameState.Pauzed )
            return;

        if ( !canUseAbility || ctx.phase != InputActionPhase.Performed )
            return;
        canUseAbility = false;

        const int index = 3;

        var succes = abilityHolder.UseAbility(index, characterData, () => abilityCooldownBars[index - 1].gameObject.SetActive(false), () => PreOnShockWave?.Invoke());
        if ( succes )
            PostOnShockWave?.Invoke();

        Utility.Async.ChangeValueAfterSeconds(abilityCooldown, ( x ) => { canUseAbility = x; }, true).Forget();
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
        EventManager.AddListener(EventType.PostGameOverWait, OnGameOver, this);
    }

    private void OnGameOver ()
    {
        Destroy(gameObject);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<int>.RemoveListener(EventType.UponHarvestSoul, Collect);
        EventManagerGeneric<Transform>.RemoveListener(EventType.TargetSelection, ( transform ) => characterData.TargetedTransform = transform);
        EventManagerGeneric<GameState>.RemoveListener(EventType.OnGameStateChange, SetGameState);

        characterData.Reset();
        upgradeHolder?.RemoveAll();
        StopAllCoroutines();
    }

    //Intialization
    private void Start ()
    {
        characterData.CharacterTransform = meshTransform;

        characterData.Player = true;

        upgradeHolder = new(characterData);

        characterData.MoveTarget = enemyMoveTarget;
        characterData.Initialize(UpdateSoulsUI);

        playerMovement.characterData = characterData;
        playerMovement.OnStart();

        playerShooting.ownerData = characterData;
        playerShooting.OnStart(transform, this);

        playerShooting.onFinishReload = () => reloadBar.fillAmount = 0;
        playerShooting.reloadTimeStream = ( amount ) => reloadBar.fillAmount = amount / playerShooting.currentGun.ReloadSpeed;

        DontDestroyOnLoad(gameObject);

        Souls = 0;
        UpdateSoulsUI();

        AcquireAbility(new ReapAbility());
        var reap = new ReapAbility
        {
            oneTimeUse = true
        };
        AcquireAbility(reap);
        AcquireAbility(new ShockWaveAbility());
        AcquireAbility(new LieAbility());

        characterData.Health = characterData.MaxHealth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateSoulsUI ()
    {
        MainThreadQueue.Instance.Enqueue(SetSoulsUI);
    }

    private void SetSoulsUI ()
    {
        var value = characterData.Souls / (float)characterData.soulBankLimit;
        soulsBar.fillAmount = value;
        //soulsUiText.SetText($"Amount of Souls = {characterData.Souls}");
    }

    private void CheckSlider ( int index, Ability abil, int count )
    {
        if ( index + 1 >= count )
            return;

        //Normalize the value with the max value, if it exceeds it, make it 1.
        var amount = characterData.Souls / abil.ActivationCost;

        if ( amount > abil.ActivationCost )
            amount = 1;

        if ( index >= abilityCooldownBars.Length )
            return;

        abilityCooldownBars[index].fillAmount = amount;
    }

    private void Update ()
    {
        if ( gameState == GameState.Pauzed )
            return;

        abilityHolder.ForeachAbility(CheckSlider);
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

    //Returns if the player already owns the ability, otherwise adds it. Not the greatest Function.
    public void AcquireAbility ( Ability ability )
    {
        if ( ability == null )
            return;

        ability.Initialize(this, characterData);

        EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"Acquired : {ability.GetType()}"));
        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);

        int index;
        var type = ability.GetType();

        index = GetAbilityIndex(type, ability);

        if ( index == -1 )
            return;

        abilityHolder.AddAbility(ability, index, out var unique);

        if ( index == 0 )
            return;

        if ( unique && type == typeof(ShockWaveAbility) )
        {
            index = 2;
        }
        if ( unique && type == typeof(LieAbility) )
        {
            index = 1;
        }
        if ( unique && ability is ReapAbility reap && reap.oneTimeUse )
        {
            index = 0;
        }

        abilityCooldownBars[index].gameObject.SetActive(true);
        abilityCooldownBars[index].fillAmount = 0;

        Debug.Log(abilityCooldownBars[index]);
        Debug.Log(index);
    }

    private int GetAbilityIndex ( Type type, Ability ability )
    {
        if ( type == typeof(ShockWaveAbility) )
        {
            return 3;
        }
        if ( type == typeof(LieAbility) )
        {
            return 2;
        }
        if ( ability is ReapAbility reap )
        {
            if ( reap.oneTimeUse )
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        return -1;
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
        abilityHolder.RemoveRandomAbility();
    }
}

//To abstract the usage of the abilities a bit.
public interface IAbilityHolder
{
    public void AddAbility ( Ability ability, int index, out bool unique );
    public bool HasAbilityAt ( int index );
    public bool HasAbilityOfType ( Type type );
    public void ForeachAbility ( Action<int, Ability, int> body );
    public bool UseAbility ( int index, CharacterData data, Action succesCallBack, Action preActivationCallBack = null );
    public void RemoveRandomAbility ();
}

public class PlayerAbilityHolder : IAbilityHolder
{
    private Ability[] abilities = new Ability[4];

    public bool HasAbilityOfType ( Type type )
    {
        for ( int i = 0; i < abilities.Length; ++i )
        {
            if ( abilities[i].GetType() == type )
                return true;
        }
        return false;
    }

    public void AddAbility ( Ability ability, int index, out bool unique )
    {
        if ( abilities.Length > 4 )
        {
            unique = false;
            return;
        }

        if ( abilities.Contains(ability) )
            unique = false;
        else
            unique = true;

        abilities[index] = ability;
    }

    public bool HasAbilityAt ( int index )
    {
        return index < abilities.Length && abilities[index] != null;
    }

    public void ForeachAbility ( Action<int, Ability, int> body )
    {
        for ( int i = 0; i < abilities.Length; ++i )
        {
            if ( abilities[i] == null )
                continue;

            body(i, abilities[i], abilities.Length);
        }
    }

    public void RemoveRandomAbility ()
    {
        var nonNullAbilities = abilities.Where(x => x != null && x.GetType() != typeof(ReapAbility)).ToArray();

        if ( nonNullAbilities.Length < 1 )
            return;

        var ability = nonNullAbilities[UnityEngine.Random.Range(0, nonNullAbilities.Length)];

        if ( ability != null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"{ability} was stolen."));

            var index = Array.IndexOf(abilities, ability);
            abilities[index] = null;
        }
    }

    public bool UseAbility ( int index, CharacterData data, Action succesCallBack, Action preActivationCallBack )
    {
        if ( index >= abilities.Length || abilities[index] == null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"No ability at position : {index}"));
            return false;
        }

        var abilty = abilities[index];

        if ( abilty == null )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"No ability at position : {index}"));
            return false;
        }

        if ( !abilty.Trigger() )
        {
            EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"Not Enough Souls. Requires : {abilty.ActivationCost}"));
            return false;
        }

        preActivationCallBack?.Invoke();

        if ( !abilty.Execute(data) )
            return false;

        succesCallBack?.Invoke();
        abilities[index] = null;
        return true;
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

        Debug.Log("Upgrade");

        if ( randomIndex >= undoActions.Count )
        {
            return;
        }

        var upgrade = undoActions[randomIndex];
        upgrade();
        EventManagerGeneric<TextPopup>.InvokeEvent(EventType.OnTextPopupQueue, new(1.0f, $"An ability was stolen."));

        undoActions.RemoveAt(randomIndex);
    }
}