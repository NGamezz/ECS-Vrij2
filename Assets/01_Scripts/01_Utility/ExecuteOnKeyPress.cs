using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ExecuteOnKeyPress : MonoBehaviour
{
    [SerializeField] private UnityEvent action;

    [EnableIf(nameof(toggle)), ShowIf(nameof(toggle))]
    [SerializeField] private UnityEvent action2;

    [SerializeField] private InputAction inputAction;

    [SerializeField] private bool toggle = false;

    private bool pressed = false;

    private void Awake ()
    {
        inputAction.performed += Input;
        inputAction.Enable();
    }

    [Button]
    public void ResetPressed ()
    {
        pressed = false;
    }

    private void Input ( InputAction.CallbackContext ctx )
    {
        Execute();
    }

    public void Execute ()
    {
        if ( toggle )
        {
            if ( pressed )
            {
                action2?.Invoke();
                pressed = false;
            }
            else if ( !pressed )
            {
                action?.Invoke();
                pressed = true;
            }
        }
        else
        {
            action?.Invoke();
        }
    }
}
