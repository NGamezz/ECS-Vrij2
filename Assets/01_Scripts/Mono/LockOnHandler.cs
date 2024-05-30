using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnHandler : MonoBehaviour
{
    [SerializeField] private float radius = 10.0f;

    [SerializeField] private Transform meshTransform;

    private Collider[] hits = new Collider[50];
    private int targetCount = 0;

    public void SwitchLock (InputAction.CallbackContext ctx)
    {
        if ( ctx.phase != InputActionPhase.Performed )
            return;

        targetCount = Physics.OverlapSphereNonAlloc(meshTransform.position, radius, hits);

        if ( targetCount < 1 )
            return;

        var collider = GetNearest(meshTransform.position, hits);

        if ( collider == null )
        {
            EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);
            return;
        }

        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, collider.transform);
    }

    private Collider GetNearest ( Vector3 startPosition, params Collider[] colliders )
    {
        int ownLayer = gameObject.layer;
        var numerator = colliders.Where(x => x != null && x.gameObject.layer != ownLayer && (x.GetComponentInParent<IAbilityOwner>() != null || x.GetComponent<IAbilityOwner>() != null));
        var nearest = numerator.OrderBy(x => Vector3.Distance(x.transform.position, startPosition)).FirstOrDefault();

        return nearest;
    }
}

public interface ILockOnAble { }