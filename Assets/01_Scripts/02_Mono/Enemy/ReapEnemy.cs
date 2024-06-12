using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ReapEnemy : Enemy, IAbilityOwner
{
    private Ability ability;
    private ReapAbility reapAbil = new ReapAbility();

    private bool canUseAbility = true;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager, bool inAnimate = false )
    {
        EnemyType = EnemyType.ReapEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);

        if ( inAnimate )
            return;
        reapAbil.Initialize(this, this.characterData);
    }

    private void UseAbility ()
    {
        if ( !canUseAbility )
            return;
        canUseAbility = false;

        reapAbil.Execute(characterData);

        Utility.Async.ChangeValueAfterSeconds(reapAbil.ActivationCooldown, ( x ) => canUseAbility = x, true, this.GetCancellationTokenOnDestroy()).Forget();
    }

    protected override void Chasing ()
    {
        var over = blackBoard.GetVariable<bool>("OverrideChase");

        if ( over )
        {
            Debug.Log(over);
            return;
        }

        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
        {
            return;
        }

        if ( moveTarget.target != null && (agent.hasPath == false || Vector3.Distance(agent.pathEndPosition, moveTarget.target.position) > 5.0f) )
        {
            agent.SetDestination(moveTarget.target.position);
        }
        agent.isStopped = false;
    }

    protected override void Attacking ()
    {
        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
            return;

        if ( !agent.isStopped )
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        MeshTransform.forward = (moveTarget.target.position - MeshTransform.position).normalized;
        UseAbility();
    }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        reapAbil.oneTimeUse = true;
        return reapAbil;
    }

    public void AcquireAbility ( Ability ability ) { }

    private async UniTaskVoid MoveAway ()
    {
        blackBoard.SetVariable("OverrideChase", true);

        Vector3 newPos = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(10, 20);
        var result = agent.SetDestination(newPos);

        await UniTask.WaitUntil(() => MeshTransform == null || Vector3.Distance(MeshTransform.position, newPos) <= 4.0f || !result);

        blackBoard.SetVariable("OverrideChase", false);
    }

    public void OnExecuteAbility ( AbilityType type )
    {
        Debug.Log("On Execute Abil");
        MoveAway().Forget();
    }
}