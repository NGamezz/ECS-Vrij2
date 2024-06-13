using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum EnemyType
{
    Default = 0,
    SnitchEnemy = 1,
    ShockWaveEnemy = 2,
    LieEnemy = 3,
    AngryEnemy = 4,
    ReapEnemy = 5,
}

public class Enemy : Soulable, IDamageable
{
    public bool Dead { get; private set; }

    public EnemyType EnemyType;

    public Transform MeshTransform;

    public Shooting shooting = new();

    private GameObject cachedGameObject;

    protected CharacterData characterData;

    public UnityEvent OnAttack;

    public GameObject GameObject
    {
        get => cachedGameObject;
    }

    private float health;

    [SerializeField] private UnityEvent onDeath;
    [SerializeField] private UnityEvent<Vector3> PositionOnDeath;

    public UnityEvent<float, Transform> OnHit;

    protected MoveTarget moveTarget;

    protected EnemyStats enemyStats;

    protected StateManager stateManager = new();

    protected bool overrideChase = false;

    public NavMeshAgent agent { get; protected set; }

    public virtual void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData, Transform manager, bool inAnimate = false )
    {
        enemyStats = stats;
        cachedGameObject = gameObject;
        Dead = false;
        health = stats.MaxHealth;

        if ( inAnimate )
            return;

        agent = (NavMeshAgent)MeshTransform.GetComponent(typeof(NavMeshAgent));
        agent.Warp(startPosition);

        this.characterData = characterData();
        this.characterData.CharacterTransform = MeshTransform;
        shooting.ownerData = this.characterData;

        shooting.OnStart(manager, this);

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
            if ( agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped )
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }, null, Attacking);

        stateManager.AddState(chasingState, attackState);
    }

    public void OnReuse ( EnemyStats stats, Vector3 startPosition )
    {
        if ( GameObject == null )
            return;

        GameObject.SetActive(true);
        Dead = false;
        enemyStats = stats;
        agent.Warp(startPosition);
        shooting.SelectGun(shooting.currentGun);

        health = stats.MaxHealth;
    }

    protected bool canShoot = true;
    private void Attack ()
    {
        if ( !canShoot )
            return;
        canShoot = false;

        shooting.ShootSingle();

        Utility.Async.ChangeValueAfterSeconds(shooting.currentGun.attackSpeed, ( x ) => canShoot = x, true, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void OnFixedUpdate ()
    {
        stateManager?.OnFixedUpdate();
    }

    protected virtual void Attacking ()
    {
        if ( agent.isActiveAndEnabled == false || agent.isOnNavMesh == false )
            return;

        MeshTransform.forward = (moveTarget.target.position - MeshTransform.position).normalized;
        OnAttack?.Invoke();
        Attack();
    }

    protected virtual void Chasing ()
    {
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

    public void UpdateStats ( EnemyStats stats )
    {
        health = stats.MaxHealth;
    }

    protected void OnDisable ()
    {
        StopAllCoroutines();

        if ( !Dead )
            shooting.OnDisable();
    }

    public void AfflictDamage ( float amount, bool silent )
    {
        if ( Dead )
            return;

        health -= amount;

        if ( !silent )
            OnHit?.Invoke(amount, MeshTransform);

        if ( health > 0 )
            return;

        Debug.Log("Enemy Died.");

        if ( !silent )
        {
            onDeath?.Invoke();
            PositionOnDeath?.Invoke(MeshTransform.position);
        }

        OnDeath?.Invoke(this, silent);
        Dead = true;

        if ( GameObject != null )
            GameObject.SetActive(false);
    }
}