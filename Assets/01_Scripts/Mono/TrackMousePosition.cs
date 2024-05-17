using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TrackMousePosition : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector3> uponMouseSelection;
    [SerializeField] private int ignoredLayers = 6;

    private bool trackMousePosition = true;

    private Camera mainCamera;

    private readonly RaycastHit[] hits = new RaycastHit[1];
    private IEnumerator StartMouseTracking()
    {
        while(trackMousePosition)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if ( Physics.RaycastNonAlloc(ray, hits, float.MaxValue, ~(1 << ignoredLayers)) == 0 )
            {
                continue;
            }

            var playerLocation = hits[0].point;
            uponMouseSelection?.Invoke(playerLocation);

            yield return Utility.Yielders.FixedUpdate;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;

        StartCoroutine(StartMouseTracking());
    }
}