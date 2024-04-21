using UnityEngine;

public class Enemy : Soulable, IDamageable
{
    private MoveToTarget moveToTarget;

    private float health;

    public Vector3 Position => transform.position;

    public void OnStart ( Transform playerTransform, EnemyStats stats)
    {
        moveToTarget = new();

        moveToTarget.OnStart(playerTransform, gameObject.transform);

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
        health -= amount;

        if ( health <= 0 )
            OnDeath?.Invoke(this, new());
    }
}