using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueCheckPoint : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate;

    [SerializeField] private InputAction actionToContinue;
    private bool isActive = false;

    private void OnDisable ()
    {
        actionToContinue.performed -= Continue;
        actionToContinue.Disable();
    }

    private void Continue ( InputAction.CallbackContext ctx )
    {
        if ( !isActive )
            return;

        objectToActivate.SetActive(false);
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Running);
        actionToContinue.Disable();
        gameObject.SetActive(false);
    }

    public void Activate ()
    {
        isActive = true;
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Pauzed);
        objectToActivate.SetActive(true);

        actionToContinue.performed += Continue;
        actionToContinue.Enable();
    }
}