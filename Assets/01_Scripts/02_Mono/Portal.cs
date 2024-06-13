using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Portal : MonoBehaviour
{
    [SerializeField] private UnityEvent uponEnablePortal;

    [SerializeField] private UnityEvent uponPortalActivation;

    [SerializeField] private InputAction activationAction;

    private bool active = true;

    private void Start ()
    {
        activationAction.performed += Activate;
    }

    private void ActivatePortal ()
    {
        uponEnablePortal?.Invoke();
        activationAction.Enable();
    }

    private void Activate ( InputAction.CallbackContext ctx )
    {
        if ( !active )
            return;

        uponPortalActivation?.Invoke();
        active = false;
    }

    void OnEnable ()
    {
        EventManager.AddListener(EventType.PortalActivation, ActivatePortal, this);
    }
}