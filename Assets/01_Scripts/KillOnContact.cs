using UnityEngine;

public class KillOnContact : MonoBehaviour
{
    private void OnTriggerEnter ( Collider other )
    {
        var damagable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
        Debug.Log("Death Barrier.");
        damagable.AfflictDamage(float.MaxValue, true);
    }
}