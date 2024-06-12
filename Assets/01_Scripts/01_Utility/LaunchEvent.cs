using NaughtyAttributes;
using UnityEngine;

public class LaunchEvent : MonoBehaviour
{
    [SerializeField] private EventType type;

    [Button]
    public void Activate ()
    {
        EventManager.InvokeEvent(type);
    }
}