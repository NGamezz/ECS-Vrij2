using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Enemy : Soulable, IDamageable
{
    public bool Dead = false;

    public bool IsDead => Dead;

    private Transform cachedTransform;

    public Transform Transform { get => cachedTransform; }

    private MoveToTarget moveToTarget;

    private bool canAttack = true;

    private float health;

    private EnemyStats enemyStats;

    public void OnStart ( EnemyStats stats, MoveTarget moveTarget )
    {
        enemyStats = stats;
        cachedTransform = transform;
        moveToTarget = new();

        moveToTarget.OnStart(moveTarget, cachedTransform);
        UpdateStats(stats);
        moveToTarget.Enable();
    }

    public void UpdateStats ( EnemyStats stats )
    {
        health = stats.maxHealth;
        moveToTarget.SetStats(stats);
    }

    public void OnUpdate ()
    {

    }

    private void OnDisable ()
    {
        moveToTarget?.OnDisable();
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
        yield return new WaitForSeconds(enemyStats.attackSpeed);
        canAttack = true;
    }

    public void OnFixedUpdate ()
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