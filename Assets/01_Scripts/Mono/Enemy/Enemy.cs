using System;
using System.Collections;
using Unity.Mathematics;
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

    private Transform cachedTransform;

    private GameObject cachedGameObject;

    protected CharacterData characterData;

    public bool Decoy = false;

    public Transform Transform { get => cachedTransform; }
    public GameObject GameObject { get => cachedGameObject; }

    private MoveToTarget moveToTarget;

    private bool canAttack = true;

    private float health;

    protected EnemyStats enemyStats;

    public virtual void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, Func<CharacterData> characterData )
    {
        enemyStats = stats;
        cachedTransform = transform;
        cachedGameObject = gameObject;
        moveToTarget = new();

        Debug.Log(moveToTarget);

        Dead = false;

        moveToTarget.OnStart(moveTarget, cachedTransform, startPosition);
        UpdateStats(stats);
        moveToTarget.Enable();

        this.characterData = characterData();
        this.characterData.CharacterTransform = cachedTransform;
    }

    public void OnReuse ( EnemyStats stats, Vector3 startPosition )
    {
        Dead = false;
        enemyStats = stats;

        moveToTarget.OnUpdate(startPosition);
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
        if ( !canAttack || !GameObject.activeInHierarchy)
            return;

        var distanceToTarget = math.length(targetPos - cachedTransform.position);

        if ( distanceToTarget < enemyStats.attackRange )
        {
            if ( target == null )
                return;

            var damagable = (IDamageable)target.target.GetComponentInParent(typeof(IDamageable));
            if ( damagable == null )
            {
                return;
            }

            canAttack = false;
            damagable.AfflictDamage(enemyStats.damage);
            StartCoroutine(ResetAttack());
        }
    }

    private IEnumerator ResetAttack ()
    {
        yield return Utility.Yielders.Get(enemyStats.attackSpeed);
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
        }
    }
}