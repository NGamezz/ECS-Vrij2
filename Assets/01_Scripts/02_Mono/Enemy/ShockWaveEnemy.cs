using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;

public class ShockWaveEnemy : Enemy, IAbilityOwner, ILockOnAble
{
    private readonly Ability ability = new ShockWaveAbility();
    private ShockWaveAbility shockAbil;

    public UnityEvent OnAttackEvent;
    public UnityEvent OnFinishAttack;

    public Animator animator;

    const string animBaseLayer = "Base Layer";
    readonly int animAttackHash = Animator.StringToHash(animBaseLayer + ".Attack");

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager, bool inAnimate = false )
    {
        EnemyType = EnemyType.ShockWaveEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);

        if ( inAnimate )
            return;
        shockAbil = ability as ShockWaveAbility;

        shockAbil.Initialize(this, characterData());
    }

    private async UniTaskVoid UseAbility ()
    {
        if ( !canUseAbility )
            return;
        canUseAbility = false;

        OnAttackEvent?.Invoke();

        animator.CrossFadeInFixedTime("Attack", 0.6f);

        while ( animator != null && animator.GetCurrentAnimatorStateInfo(0).fullPathHash != animAttackHash )
        {
            await UniTask.NextFrame();
        }

        var time = animator.GetCurrentAnimatorStateInfo(0).length / 3.0f;

        await UniTask.Delay(TimeSpan.FromSeconds(time));
        OnFinishAttack?.Invoke();

        shockAbil.Execute(characterData);
        Utility.Async.ChangeValueAfterSeconds(shockAbil.ActivationCooldown, ( x ) => canUseAbility = x, true, this.GetCancellationTokenOnDestroy()).Forget();
    }

    protected override void Attacking ()
    {
        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
        {
            Debug.Log("Agent is not on a navMesh or is inactive.");
            return;
        }

        if ( !agent.isStopped )
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        MeshTransform.forward = (moveTarget.target.position - MeshTransform.position).normalized;
        UseAbility().Forget();
    }

    public void AcquireAbility ( Ability ability ) { }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return shockAbil;
    }

    public void OnExecuteAbility ( AbilityType type ) { }
}