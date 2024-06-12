using UnityEngine;

public class LockTrack : MonoBehaviour
{
    [SerializeField] private Transform trackingIdentifier;

    private Transform trackingTarget;
    private bool tracking;

    private void StartTracking ( Transform target )
    {
        if ( target == null || target.gameObject.activeInHierarchy == false )
        {
            CancelTracking();
            return;
        }

        tracking = true;
        trackingTarget = target;

        var pos = trackingTarget.position;
        pos.y = 1.0f;
        trackingIdentifier.position = pos;

        trackingIdentifier.gameObject.SetActive(true);
    }

    private void OnEnable ()
    {
        EventManagerGeneric<Transform>.AddListener(EventType.TargetSelection, StartTracking);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<Transform>.RemoveListener(EventType.TargetSelection, StartTracking);
    }

    private void CancelTracking ()
    {
        trackingIdentifier.gameObject.SetActive(false);
        tracking = false;
        trackingTarget = null;
    }

    private void FixedUpdate ()
    {
        if ( !tracking || trackingTarget == null || trackingTarget.gameObject.activeInHierarchy == false )
        {
            if ( trackingIdentifier.gameObject.activeInHierarchy )
            {
                CancelTracking();
            }
            return;
        }
        if ( trackingTarget != null && trackingTarget.gameObject.activeInHierarchy == false )
        {
            EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);
        }

        var position = trackingTarget.position;
        position.y = 1.0f;
        trackingIdentifier.position = position;
    }
}