using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraHandling : MonoBehaviour
{
    [SerializeField] private Transform meshTransform;

    [SerializeField] private bool mouseController = false;

    private Camera mainCamera;

    private Vector2 lookDirection;

    Plane plane = new(Vector3.up, 0);

    public void OnLook ( InputAction.CallbackContext ctx )
    {
        lookDirection = ctx.ReadValue<Vector2>();
    }

    private void ApplyLookDirection ()
    {
        var direction = GetDirection();

        meshTransform.forward = direction.normalized;
    }

    private Vector3 GetDirection ()
    {
        var direction = meshTransform.forward;

        if ( mouseController )
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if ( !plane.Raycast(ray, out float distance) )
            {
                return direction;
            }
            var position = ray.GetPoint(distance);

            direction = position - meshTransform.position;
        }
        else if ( lookDirection != Vector2.zero )
        {
            direction = new(lookDirection.x, direction.y, lookDirection.y);
        }

        direction.y = 0.0f;

        return direction;
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