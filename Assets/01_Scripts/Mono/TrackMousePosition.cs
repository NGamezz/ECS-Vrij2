using UnityEngine;
using UnityEngine.Events;

public class TrackMousePosition : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector3> uponMouseSelection;

    private Camera mainCamera;
    private Plane plane = new(Vector3.up, 0);

    private void FixedUpdate ()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if ( !plane.Raycast(ray, out float distance) )
        {
            return;
        }

        uponMouseSelection?.Invoke(ray.GetPoint(distance));
    }

    void Start ()
    {
        mainCamera = Camera.main;
    }
}