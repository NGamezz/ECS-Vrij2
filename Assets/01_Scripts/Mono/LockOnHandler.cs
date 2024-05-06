using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class LockOnHandler : MonoBehaviour
{
    [Layer]
    [SerializeField] private int targetLayer;

    [SerializeField] private UnityEvent<Transform> UponTargetSelection;

    RaycastHit[] hits = new RaycastHit[1];

    private Camera mainCamera;

    private void Start ()
    {
        mainCamera = Camera.main;
    }

    void Update ()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if ( Physics.RaycastNonAlloc(ray, hits, float.MaxValue, (1 << targetLayer)) == 0 )
        {
            return;
        }

        UponTargetSelection?.Invoke(hits[0].collider.transform);

        ClearArray(ref hits);
    }

    private void ClearArray<T> ( ref T[] array )
    {
        for ( int i = 0; i < array.Length; i++ )
        {
            array[i] = default;
        }
    }
}