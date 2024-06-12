using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueCheckPoint : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;

    //[SerializeField] private InputAction actionToContinue;
    private bool isActive = false;

    private void OnDisable ()
    {
        //actionToContinue.performed -= Continue;
        //actionToContinue.Disable();
    }

    public void Continue ( InputAction.CallbackContext ctx )
    {
        if ( ctx.phase != InputActionPhase.Performed )
            return;

        if ( !isActive )
            return;

        //actionToContinue.Disable();
        objectToActivate.SetActive(false);
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Running);
        gameObject.SetActive(false);
    }

    public void Activate ()
    {
        isActive = true;
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Pauzed);
        objectToActivate.SetActive(true);

        //actionToContinue.performed += Continue;
        //actionToContinue.Enable();
    }
}