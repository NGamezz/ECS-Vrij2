using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

    public BlackBoardObject blackBoardObject;

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

    protected bool gameOver = false;

    private float health;

    protected MoveTarget moveTarget;

    protected EnemyStats enemyStats;

    protected StateManager stateManager = new();

    protected NavMeshAgent agent;

    public virtual void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager )
    {
        enemyStats = stats;
        cachedGameObject = gameObject;
        Dead = false;

        agent = (NavMeshAgent)MeshTransform.GetComponent(typeof(NavMeshAgent));
        agent.Warp(startPosition);

        this.characterData = characterData();
        this.characterData.CharacterTransform = MeshTransform;
        shooting.ownerData = this.characterData;

        shooting.OnStart(manager, this);
        health = stats.MaxHealth;

        this.moveTarget = moveTarget;

        var chasingState = new BaseState(() =>
        {
            if ( moveTarget.target == null )
                return false;

            return Vector3.Distance(MeshTransform.position, moveTarget.target.position) > enemyStats.attackRange;
        }, null, null, Chasing);
        var attackState = new BaseState(() =>
        {
            if ( moveTarget.target == null )
                return false;

            return Vector3.Distance(MeshTransform.position, moveTarget.target.position) < enemyStats.attackRange;
        }, () =>
        {
            if ( !agent.isStopped )
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }, null, Attacking);

        stateManager.AddState(chasingState, attackState);
    }

    public void OnReuse ( EnemyStats stats, Vector3 startPosition )
    {
        GameObject.SetActive(true);
        Dead = false;
        enemyStats = stats;
        agent.Warp(startPosition);
        gameOver = false;
        shooting.SelectGun(shooting.currentGun);
        health = stats.MaxHealth;
    }

    protected bool canShoot = true;
    private async void Attack ()
    {
        if ( !canShoot )
            return;
        canShoot = false;

        shooting.ShootSingle();
        await Task.Delay(TimeSpan.FromSeconds(shooting.currentGun.attackSpeed));
        canShoot = true;
    }

    public void OnFixedUpdate ()
    {
        if ( gameOver )
            return;

        stateManager?.OnFixedUpdate();
    }

    protected virtual void Attacking ()
    {
        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
            return;

        MeshTransform.forward = (moveTarget.target.position - MeshTransform.position).normalized;
        Attack();
    }

    protected virtual void Chasing ()
    {
        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
        {
            return;
        }

        if ( moveTarget.target != null && agent.hasPath == false || Vector3.Distance(agent.pathEndPosition, moveTarget.target.position) > 5.0f )
        {
            agent.SetDestination(moveTarget.target.position);
        }
        agent.isStopped = false;
    }

    public void UpdateStats ( EnemyStats stats )
    {
        health = stats.MaxHealth;
    }

    protected void OnDisable ()
    {
        gameOver = true;
        StopAllCoroutines();
        OnDisabled?.Invoke(this);
    }

    public void AfflictDamage ( float amount )
    {
        if ( Dead )
            return;

        health -= amount;

        if ( health <= 0 )
        {
            Dead = true;
            OnDeath?.Invoke(this);
            OnDeath = null;
            OnDisabled = null;
        }
    }
}