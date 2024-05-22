using System;
using UnityEngine;

public class SnitchEnemy : Enemy, IAbilityOwner
{
    [SerializeField] private Ability ability = new SnitchingAbility();
    private BTBaseNode abilityTree;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        EnemyType = EnemyType.SnitchEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData);
        shooting.recoilMultiplier = 0;
    }

    public override void SetupBehaviourTrees ()
    {
        var currentGun = shooting.currentGun;

        blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, moveTarget.target);
        blackBoard.SetVariable(VariableNames.TARGET_POSITION, moveTarget.target.position);

        moveTree =
            new BTSequence(

                new BTConditionNode(() => !gameOver),
                new BTCancelIfFalse(() => Vector3.Distance(MeshTransform.position, moveTarget.target.position) > enemyStats.attackRange,

                        new BTGetPosition(VariableNames.PLAYER_TRANSFORM, blackBoard),
                        new BTCancelIfFalse(() => Vector3.Distance(blackBoard.GetVariable<Vector3>(VariableNames.TARGET_POSITION), moveTarget.target.position) < 1.0f,

                            new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, moveTarget.target)),
                            new BTGetPosition(VariableNames.PLAYER_TRANSFORM, blackBoard),
                            new BTMoveToPosition(agent, enemyStats.moveSpeed, VariableNames.TARGET_POSITION, enemyStats.attackRange)
        )),
        new BTAlwaysFalse()
                        );

        attackTree =
            new BTSequence(
                new BTConditionNode(() => !gameOver),
                new BTSequence(
                    new BTRepeatWhile(() => Vector3.Distance(MeshTransform.position, moveTarget.target.position) < enemyStats.attackRange,
                           new BTSequence(
                                new BTAlwaysSuccesTask(() => MeshTransform.forward = (moveTarget.target.position - MeshTransform.position).normalized),
                                new BTAlwaysSuccesTask(() => shooting.ShootSingle()),
                                new BTWaitFor(currentGun.attackSpeed)
                                         )),
                    new BTAlwaysFalse()
                             )
                         );

        abilityTree = new BTSequence(
            new BTConditionNode(() => !gameOver),
            new BTSequence(
                new BTRepeatWhile(() => Vector3.Distance(MeshTransform.position, moveTarget.target.position) < enemyStats.attackRange,
                    new BTSequence(
                        new BTAlwaysSuccesTask(() => ability.Execute(characterData)),
                        new BTWaitFor(ability.ActivationCooldown)
                                  )
                        )
                )
            );

        attackTree.SetupBlackboard(blackBoard);
        moveTree.SetupBlackboard(blackBoard);
        abilityTree.SetupBlackboard(blackBoard);
    }

    public override void OnFixedUpdate ()
    {
        base.OnFixedUpdate();
        attackTree?.Tick();
    }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return ability;
    }

    public void AcquireAbility ( Ability ability, bool singleUse = true )
    {
    }
}