using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType
{
    Default = 0,
    SnitchEnemy = 1,
    ShockWaveEnemy = 2,
    LieEnemy = 3,
    AngryEnemy = 4,
}

public class Enemy : Soulable, IDamageable
{
    public bool Dead { get; private set; }

    public event Action<Enemy> OnDisabled;

    public EnemyType EnemyType;

    public Transform MeshTransform;

    public Shooting shooting = new();

    private GameObject cachedGameObject;

    protected CharacterData characterData;

    public GameObject GameObject
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => cachedGameObject;
    }

    protected Blackboard blackBoard = new();

    protected BTBaseNode moveTree;
    protected BTBaseNode attackTree;

    protected bool gameOver = false;

    private float health;

    protected MoveTarget moveTarget;

    protected EnemyStats enemyStats;

    protected NavMeshAgent agent;

    public virtual void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        enemyStats = stats;
        cachedGameObject = gameObject;
        Dead = false;

        agent = (NavMeshAgent)MeshTransform.GetComponent(typeof(NavMeshAgent));
        agent.Warp(startPosition);

        this.characterData = characterData();
        this.characterData.CharacterTransform = MeshTransform;
        shooting.ownerData = this.characterData;

        shooting.OnStart(transform, this);
        UpdateStats(stats);

        this.moveTarget = moveTarget;
    }

    //Need to add the disposal when it's game over.
    public virtual void SetupBehaviourTrees ()
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

        moveTree.SetupBlackboard(blackBoard);
    }

    public void OnReuse ( EnemyStats stats, Vector3 startPosition )
    {
        Dead = false;
        enemyStats = stats;
        agent.Warp(startPosition);
        gameOver = false;
        shooting.SelectGun(shooting.currentGun);
        UpdateStats(stats);
    }

    public void UpdateStats ( EnemyStats stats )
    {
        health = stats.maxHealth;
    }

    protected void OnDisable ()
    {
        gameOver = true;
        StopAllCoroutines();
        OnDisabled?.Invoke(this);
    }

    public virtual void OnFixedUpdate ()
    {
        if ( gameOver )
            return;

        moveTree?.Tick();
        attackTree?.Tick();
    }

    public void AfflictDamage ( float amount )
    {
        if ( Dead )
            return;

        health -= amount;

        if ( health <= 0 )
        {
            Debug.Log("EnemyDied");

            Dead = true;
            OnDeath?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}