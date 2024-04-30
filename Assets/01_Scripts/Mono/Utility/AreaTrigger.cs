using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class AreaTrigger : MonoBehaviour
{
    [Layer]
    [SerializeField] private int triggerLayer;

    [SerializeField] private UnityEvent eventToTrigger;

    private void OnTriggerEnter ( Collider other )
    {
        if(other.gameObject.layer == triggerLayer)
        {
            eventToTrigger?.Invoke();
            this.enabled = false;
        }
    }
}