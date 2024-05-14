using UnityEngine;

[CreateAssetMenu()]
public class EnemyStats : ScriptableObject
{
    public EnemyType EnemyType;

    public float maxHealth;
    public float moveSpeed;
    public float damage;
    public float attackRange;
    public float spawnSpeed;
    public float attackSpeed;

    public float enemiesPerBatch;
}