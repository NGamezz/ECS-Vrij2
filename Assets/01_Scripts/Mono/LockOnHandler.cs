using System.Threading.Tasks;
using UnityEngine;

public class LockOnHandler : MonoBehaviour
{
    [SerializeField] private float activationCoolDown = 0.2f;

    private Camera mainCamera;

    private void Start ()
    {
        mainCamera = Camera.main;
    }

    private RaycastHit[] hits = new RaycastHit[5];
    private bool active = false;
    public async void OnActivate ()
    {
        if ( active )
        {
            Debug.Log("On Cooldown");
            return;
        }

        active = true;

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var hitCount = Physics.RaycastNonAlloc(ray, hits, 5000.0f);

        if ( hitCount == 0 )
        {
            EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, null);
            return;
        }

        for ( int i = 0; i < hitCount; ++i )
        {
            if ( CheckHit(hits[i]) )
                break;
        }

        await Task.Delay(System.TimeSpan.FromSeconds(activationCoolDown));

        active = false;
    }

    private bool CheckHit ( RaycastHit hit )
    {
        var transform = hit.transform;

        bool hasLockOnProperty;
        if ( transform.root == transform )
            hasLockOnProperty = (ILockOnAble)transform.GetComponent(typeof(ILockOnAble)) != null;
        else
            hasLockOnProperty = (ILockOnAble)transform.GetComponentInParent(typeof(ILockOnAble)) != null;

        Transform target = null;
        if ( hasLockOnProperty )
        {
            target = transform;
        }
        EventManagerGeneric<Transform>.InvokeEvent(EventType.TargetSelection, target);

        return target != null;
    }
}

public interface ILockOnAble { }