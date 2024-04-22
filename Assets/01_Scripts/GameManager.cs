using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void OnDisable ()
    {
        WorldManager.ClearAllEvents();
    }
}