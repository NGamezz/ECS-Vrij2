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
            IDamageable hit;
            if ( results[i].transform.root == results[i].transform )
                hit = (IDamageable)results[i].GetComponent(typeof(IDamageable));
            else
                hit = (IDamageable)results[i].GetComponentInParent(typeof(IDamageable));

            if ( hit == null )
                continue;

            hit.AfflictDamage(damage);
        }

        gameObject.SetActive(false);
    }
}