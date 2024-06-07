using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class VicinityPopup : MonoBehaviour
{
    [SerializeField] private UnityEvent OnEnter;
    [SerializeField] private UnityEvent OnExit;

    [SerializeField] private int targetLayer = 6;

    private Collider storedCollider;

    private void OnTriggerEnter ( Collider other )
    {
        if ( other.gameObject.layer != targetLayer )
            return;

        OnEnter?.Invoke();
        storedCollider = other;
    }

    private void OnTriggerExit ( Collider other )
    {
        if ( storedCollider != other )
            return;

        OnExit?.Invoke();
    }
}