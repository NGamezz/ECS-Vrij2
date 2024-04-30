using UnityEngine;

public class Enemy : Soulable, IDamageable
{
    public bool Dead = false;

    private Transform cachedTransform;
    public Transform Transform { get => cachedTransform; }

    private MoveToTarget moveToTarget;

    private float health;

    public bool IsDead () => Dead;

    public void OnStart ( EnemyStats stats, MoveTarget moveTarget )
    {
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