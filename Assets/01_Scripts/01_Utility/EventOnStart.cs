using UnityEngine;
using UnityEngine.Events;

public class EventOnStart : MonoBehaviour
{
    [SerializeField] private UnityEvent eventToTrigger;

    void Start ()
    {
        eventToTrigger?.Invoke();
    }
}