using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    [SerializeField] private UnityEvent uponEnablePortal;

    [SerializeField] private int playerLayer;

    [SerializeField] private UnityEvent uponPortalActivation;
    [SerializeField] private KeyCode activationKey;

    private bool active = true;

    private void ActivatePortal ()
    {
        uponEnablePortal?.Invoke();
    }

    private void OnTriggerStay ( Collider other )
    {
        if ( !active || other.gameObject.layer != playerLayer )
            return;

        if ( Input.GetKeyDown(activationKey) )
        {
            uponEnablePortal?.Invoke();
            active = false;
        }
    }

    void OnEnable ()
    {
        EventManager.AddListener(EventType.PortalActivation, ActivatePortal);
    }

    void OnDisable ()
    {
        EventManager.RemoveListener(EventType.PortalActivation, ActivatePortal);
    }
}