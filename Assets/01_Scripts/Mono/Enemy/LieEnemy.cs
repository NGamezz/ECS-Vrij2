using System;
using UnityEngine;

public class LieEnemy : Enemy, IAbilityOwner
{
    private Ability ability = new LieAbility();
    private BTBaseNode abilityTree;

    public override void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager )
    {
        EnemyType = EnemyType.LieEnemy;
        base.OnStart(stats, moveTarget, startPosition, characterData, manager);
        ability.Initialize(this, characterData());
    }

    //Sets up the behaviour Tree.
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
                        new BTCancelIfFalse(() => Vector3.Distance(blackBoard.GetVariable<Vector3>(VariableNames.TARGET_POSITION), blackBoard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position) < 1.0f,

                            new BTAlwaysSuccesTask(() => blackBoard.SetVariable(VariableNames.PLAYER_TRANSFORM, moveTarget.target)),
                            new BTGetPosition(VariableNames.PLAYER_TRANSFORM, blackBoard),
                            new BTMoveToPosition(agent, enemyStats.moveSpeed, VariableNames.TARGET_POSITION, enemyStats.attackRange)
        )),
        new BTAlwaysFalse()
                        );

        //Make it so it doesn't grab a copy of the attack speed at the start, but the current value.
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
        abilityTree?.Tick();
    }

    public Ability HarvestAbility ()
    {
        gameObject.SetActive(false);
        return ability;
    }

    public void AcquireAbility ( Ability ability ) { }
}