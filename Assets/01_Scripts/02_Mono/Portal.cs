using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Portal : MonoBehaviour
{
    [SerializeField] private UnityEvent uponEnablePortal;

    [SerializeField] private float range = 10.0f;

    [SerializeField] private UnityEvent uponPortalActivation;

    [SerializeField] private InputAction activationAction;

    private Transform playerTransform;

    private bool active = true;

    private void Start ()
    {
        playerTransform = FindAnyObjectByType<PlayerMesh>().GetTransform();
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

        var playerPos = playerTransform.position;
        var ownPos = transform.position;

        playerPos.y = 0.0f;
        ownPos.y = 0.0f;

        if ( Vector3.Distance(playerPos, ownPos) > range )
            return;

        uponPortalActivation?.Invoke();
        active = false;
    }

    void OnEnable ()
    {
        EventManager.AddListener(EventType.PortalActivation, ActivatePortal, this);
    }
}