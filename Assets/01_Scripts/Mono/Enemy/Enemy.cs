using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public enum EnemyType
{
    Default = 0,
    SnitchEnemy = 1,
}

public class Enemy : Soulable, IDamageable
{
    public bool Dead { get; private set; }

    public event Action<Enemy> OnDisabled;

    private Transform cachedTransform;

    private GameObject cachedGameObject;

    protected CharacterData characterData;

    public Transform Transform { get => cachedTransform; }
    public GameObject GameObject { get => cachedGameObject; }

    private MoveToTarget moveToTarget;

    private bool canAttack = true;

    private float health;

    private EnemyStats enemyStats;

    public void OnStart ( EnemyStats stats, MoveTarget moveTarget, Vector3 startPosition, CharacterData characterData )
    {
        enemyStats = stats;
        cachedTransform = transform;
        cachedGameObject = gameObject;
        moveToTarget = new();

        Dead = false;

        this.characterData = characterData;

        moveToTarget.OnStart(moveTarget, cachedTransform, startPosition);
        UpdateStats(stats);
        moveToTarget.Enable();
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
    public void CheckAttackRange ( Transform target, Vector3 targetPos )
    {
        if ( !canAttack )
            return;

        var distanceToTarget = math.length(targetPos - cachedTransform.position);

        if ( distanceToTarget < enemyStats.attackRange )
        {
            var damagable = target.GetComponentInParent<IDamageable>();
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