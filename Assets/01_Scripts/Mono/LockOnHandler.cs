using UnityEngine;
using UnityEngine.Events;

public class LockOnHandler : MonoBehaviour
{
    private const int regularEnemyLayer = 9;
    private const int specialEnemyLayer = 8;

    [SerializeField] private UnityEvent<Transform> UponTargetSelection;
    [SerializeField] private UnityEvent<Transform> UponSpecialTargetSelection;

    [SerializeField] private CharacterData characterData;

    [SerializeField] private UnityEvent UponTargetDeselection;

    private Camera mainCamera;

    private void Start ()
    {
        mainCamera = Camera.main;
    }

    public void OnActivate ()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        var hasHit = Physics.Raycast(ray, out var hits, 5000.0f);

        if ( !hasHit )
        {
            UponTargetDeselection?.Invoke();
            return;
        }

        var transform = hits.transform;
        var layer = transform.gameObject.layer;

        switch ( layer )
        {
            case specialEnemyLayer:
                {
                    characterData.TargetedTransform = transform;
                    UponSpecialTargetSelection?.Invoke(transform);
                    break;
                }
            case regularEnemyLayer:
                {
                    characterData.TargetedTransform = transform;
                    UponTargetSelection?.Invoke(transform);
                    break;
                }
            default:
                {
                    characterData.TargetedTransform = null;
                    UponTargetDeselection?.Invoke();
                    return;
                }
        }
    }
}