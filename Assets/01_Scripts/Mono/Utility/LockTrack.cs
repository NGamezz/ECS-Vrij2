using UnityEngine;

public class LockTrack : MonoBehaviour
{
    public bool Tracking { get; }

    [SerializeField] private bool tracking;

    [SerializeField] private Transform trackingIdentifier;

    private Transform trackingTarget;

    private Vector3 ownPos;

    public void StartTracking ( Transform target )
    {
        tracking = true;
        trackingTarget = target;
    }

    private void Start ()
    {
        ownPos = transform.position;
    }

    public void CancelTracking ()
    {
        tracking = false;
        trackingTarget = null;
    }

    private void FixedUpdate ()
    {
        if ( !tracking || trackingTarget == null || trackingTarget == null )
        { return; }

        var position = trackingTarget.position;
        position.y = ownPos.y;
        trackingIdentifier.position = position;
    }
}
