using UnityEngine;

public class PauzeToggle : MonoBehaviour
{
    public void Pause ()
    {
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Pauzed);
    }

    public void Resume ()
    {
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Running);
    }

    public void QuitApplication ()
    {
        Application.Quit();
    }
}