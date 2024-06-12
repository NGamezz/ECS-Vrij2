using UnityEngine;

public class HealPickup : MonoBehaviour
{
    [SerializeField] private float healAmount = 10.0f;

    [SerializeField] private int playerLayer = 6;

    private void OnTriggerEnter ( Collider other )
    {
        if ( other.gameObject.layer != playerLayer )
            return;

        other.GetComponentInParent<Health>().Heal(healAmount);

        Destroy(gameObject);
    }
}