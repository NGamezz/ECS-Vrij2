using UnityEngine;

public class Enemy : Soulable, IDamageable
{
    public bool Dead = false;

    public Vector3 Position => transform.position;

    private MoveToTarget moveToTarget;

    private float health;

    public void OnStart ( EnemyStats stats , MoveTarget moveTarget)
    {
        moveToTarget = new();
        moveToTarget.OnStart(moveTarget, gameObject.transform);

        UpdateStats(stats);
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