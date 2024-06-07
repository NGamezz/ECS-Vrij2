using UnityEngine;

[CreateAssetMenu()]
public class EnemyStats : ScriptableObject
{
    public EnemyType EnemyType;

    public float statMultiplier = 1;

    private float _maxHealth;
    public float MaxHealth {
        get
        {
            return _maxHealth * statMultiplier;
        }
        set => _maxHealth = value;
    }

    private float _moveSpeed;
    public float MoveSpeed
    {
        get
        {
            return _moveSpeed * statMultiplier;
        }
        set => _moveSpeed = value;
    }

    private float _damage;
    public float Damage
    {
        get
        {
            return _damage * statMultiplier;
        }
        set => _damage = value;
    }

    public float attackRange;
    public float spawnSpeed;

    private float _attackSpeed;
    public float AttackSpeed
    {
        get
        {
            return _attackSpeed * statMultiplier;
        }
        set => _attackSpeed = value;
    }

    public float enemiesPerBatch;
}