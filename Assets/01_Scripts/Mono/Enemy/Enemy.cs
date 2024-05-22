using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor.Timeline.Actions;
using UnityEngine;

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

    public Transform meshTransform;

    public Shooting shooting = new();

    private GameObject cachedGameObject;

    protected CharacterData characterData;

    protected Action attackAction;

    public bool Decoy = false;

    public Transform Transform { get => meshTransform; }
    public GameObject GameObject { get => cachedGameObject; }

    private MoveToTarget moveToTarget;

    protected bool canAttack = true;

    private float health;

    protected EnemyStats enemyStats;

    public virtual void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        enemyStats = stats;
        cachedGameObject = gameObject;
        moveToTarget = new();

        Dead = false;

        shooting.OnStart(transform, this);

        moveToTarget.OnStart(moveTarget, meshTransform, startPosition);
        UpdateStats(stats);
        moveToTarget.Enable();

        this.characterData = characterData();
        this.characterData.CharacterTransform = meshTransform;
    }

    public void OnReuse ( EnemyStats stats, Vector3 startPosition )
    {
        Dead = false;
        enemyStats = stats;

        moveToTarget.OnUpdate(startPosition);
        moveToTarget.Enable();
        shooting.SelectGun(shooting.currentGun);
        UpdateStats(stats);
    }

    public void UpdateStats ( EnemyStats stats )
    {
        health = stats.maxHealth;
        moveToTarget.SetStats(stats);
    }

    public virtual void OnUpdate ()
    {
    }

    private void OnDisable ()
    {
        StopAllCoroutines();
        moveToTarget?.OnDisable();
        OnDisabled?.Invoke(this);
    }

    //To be improved.
    public virtual void CheckAttackRange ( MoveTarget target, Vector3 targetPos )
    {
        if ( !GameObject.activeInHierarchy )
            return;

        var distanceToTarget = math.length(targetPos - Transform.position);

        if ( distanceToTarget > enemyStats.attackRange )
        {
            moveToTarget.Enable();
            return;
        }

        moveToTarget.OnDisable();

        if ( canAttack )
        {
            shooting.ShootSingle();

            canAttack = false;
            StartCoroutine(ResetAttack());
        }

        Transform.forward = target.target.position - Transform.position;
    }

    protected IEnumerator ResetAttack ()
    {
        yield return Utility.Yielders.Get(shooting.currentGun.ReloadSpeed);
        canAttack = true;
    }

    public virtual void OnFixedUpdate ()
    {
        moveToTarget.OnFixedUpdate();
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
            gameObject.SetActive(false);
        }
    }
}