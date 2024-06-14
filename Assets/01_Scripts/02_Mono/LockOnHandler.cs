using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnHandler : MonoBehaviour
{
    [SerializeField] private float radius = 10.0f;

    [SerializeField] private Transform meshTransform;

    private Collider[] hits = new Collider[50];

    public void SwitchLock ( InputAction.CallbackContext ctx )
    {
        if ( ctx.phase != InputActionPhase.Performed )
            return;

        var targetCount = Physics.OverlapSphereNonAlloc(meshTransform.position, radius, hits);

        if ( targetCount < 1 )
            return;

        var collider = GetNearest(meshTransform.position, targetCount, hits);

        if ( collider == null )
        {
            return;
        }

        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, collider.transform);
    }

    private Collider GetNearest ( Vector3 startPosition, int count, params Collider[] colliders )
    {
        int ownLayer = gameObject.layer;

        Collider closest = null;
        float closestDist = float.MaxValue;

        for ( int i = 0; i < count; ++i )
        {
            var coll = colliders[i];

            if ( coll == null || coll.gameObject.layer == ownLayer )
                continue;

            if ( (coll.GetComponentInParent(typeof(LockOnAble)) == null && coll.GetComponent(typeof(LockOnAble)) == null) )
                continue;

            var dist = Vector3.Distance(coll.transform.position, startPosition);
            if ( dist < closestDist )
            {
                closestDist = dist;
                closest = coll;
            }
        }

        return closest;
    }
}

public interface ILockOnAble { }
