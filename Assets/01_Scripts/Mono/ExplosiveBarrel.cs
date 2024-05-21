using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ExplosiveBarrel : MonoBehaviour
{
    [SerializeField] private float radius;

    [SerializeField] private float damage;

    private Collider[] results = new Collider[10];

    private void OnTriggerEnter ( Collider other )
    {
        if ( !other.TryGetComponent<IDamageable>(out var damageable) )
        {
            return;
        }

        var count = Physics.OverlapSphereNonAlloc(other.transform.position, radius, results);

        for(int i = 0; i < count; i++ )
        {
            var dam  = (IDamageable)results[i].GetComponent(typeof(IDamageable));

            if ( dam == null )
                continue;

            dam.AfflictDamage(damage);
        }

        gameObject.SetActive(false);
    }
}