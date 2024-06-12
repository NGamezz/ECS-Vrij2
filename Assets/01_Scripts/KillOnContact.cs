using UnityEngine;

public class KillOnContact : MonoBehaviour
{
    private void OnTriggerEnter ( Collider other )
    {
        var damagable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
        damagable.AfflictDamage(float.MaxValue);
    }
}