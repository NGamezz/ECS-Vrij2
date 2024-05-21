using UnityEngine;
using UnityEngine.Events;

public class IDamageablePassthrough : MonoBehaviour, IDamageable
{
    [SerializeField] private UnityEvent<float> uponDamageAffliction;

    public bool Dead => false;

    public void AfflictDamage ( float amount )
    {
        uponDamageAffliction?.Invoke(amount);
    }
}