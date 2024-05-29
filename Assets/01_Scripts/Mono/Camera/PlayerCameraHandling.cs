using UnityEngine;

public class PlayerCameraHandling : MonoBehaviour
{
    [SerializeField] private Transform meshTransform;

    private Camera mainCamera;

    Plane plane = new(Vector3.up, 0);

    //Should be improved.
    private void ApplyLookDirection ()
    {
        Vector3 direction;

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if ( !plane.Raycast(ray, out float distance) )
        {
            return;
        }
        var position = ray.GetPoint(distance);

        direction = position - meshTransform.position;
        direction.y = 0.0f;

        meshTransform.forward = direction.normalized;
    }

    private void Start ()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate ()
    {
        ApplyLookDirection();
    }
}