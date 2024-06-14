using UnityEngine;

public class DeactivateOnDamage : MonoBehaviour, IDamageable
{
    public bool Dead => false;

    public void AfflictDamage ( float amount, bool silent )
    {
        Destroy(gameObject);
    }
}