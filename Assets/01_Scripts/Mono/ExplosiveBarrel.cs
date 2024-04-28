using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ExplosiveBarrel : MonoBehaviour
{
    [SerializeField] private float radius;

    [SerializeField] private float damage;

    private void OnTriggerEnter ( Collider other )
    {
        if ( !other.TryGetComponent<IDamageable>(out var damageable) )
        {
            return;
        }

        var colliders = Physics.OverlapSphere(other.transform.position, radius);

        foreach ( var coll in colliders )
        {
            if ( !coll.TryGetComponent<IDamageable>(out var dam) )
                continue;

            dam.AfflictDamage(damage);
        }

        gameObject.SetActive(false);
    }
}