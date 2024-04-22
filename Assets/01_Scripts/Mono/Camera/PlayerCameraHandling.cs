using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraHandling : MonoBehaviour
{
    [Tooltip("Whether or not the player aims using their mouse or defined Input Action.")]
    [SerializeField] private bool lookAtMouse = true;

    [SerializeField] private Transform meshTransform;

    [SerializeField] private Transform cameraTransform;

    [SerializeField] private int playerLayerMask = 6;

    [SerializeField] private float cameraRecenterDeadzone = 5.0f;

    [SerializeField] private float smoothTime = 0.5f;

    [Tooltip("Distance from the border of the screen before the camera moves.")]
    [SerializeField] private int2 distanceFromBorder = new(200, 200);

    private Vector3 velocity = Vector3.zero;

    private Vector2 lookVector;

    private bool updateCameraPosition = false;

    private Camera mainCamera;

    [Tooltip("For arrow keys aiming for instance.")]
    public void OnLook ( InputAction.CallbackContext ctx )
    {
        if ( lookAtMouse )
            return;

        lookVector = ctx.ReadValue<Vector2>();
    }

    private (bool succes, Vector3 position) GetMousePosition ()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if ( !Physics.Raycast(ray, out var hitInfo, float.MaxValue, ~(1 << playerLayerMask)) )
        {
            return (false, Vector3.zero);
        }

        return (true, hitInfo.point);
    }

    //Should be improved.
    private void ApplyLookDirection ()
    {
        Vector3 direction;

        if ( lookAtMouse )
        {
            var (success, mousePos) = GetMousePosition();

            if ( !success )
                return;

            direction = mousePos - meshTransform.position;
            direction.y = 0.0f;
        }
        else
        {
            direction = new(lookVector.x, 0.0f, lookVector.y);

            if ( lookVector.magnitude == 0 )
                direction = meshTransform.forward;
        }

        meshTransform.forward = direction.normalized;
    }

    private void CheckForPositionChange ()
    {
        var meshPos = meshTransform.position;
        var meshScreenPos = mainCamera.WorldToScreenPoint(meshPos);
        var res = Screen.currentResolution;

        Task.Run(() =>
        {
            var withinSafeZone = meshScreenPos.x <= res.width - distanceFromBorder.x
                && meshScreenPos.y <= res.height - distanceFromBorder.y
                && meshScreenPos.y >= distanceFromBorder.y
                && meshScreenPos.x >= distanceFromBorder.x;

            if ( withinSafeZone )
                return;

            updateCameraPosition = true;
        });
    }

    private void UpdatePosition ()
    {
        var cameraPos = cameraTransform.position;
        var meshPos = meshTransform.position;

        var newPos = new Vector3(meshPos.x, cameraPos.y, meshPos.z);
        cameraPos = Vector3.SmoothDamp(cameraPos, newPos, ref velocity, smoothTime);

        if ( Vector3.Distance(cameraPos, newPos) < cameraRecenterDeadzone )
        {
            updateCameraPosition = false;
        }

        cameraTransform.position = cameraPos;
    }

    private void Start ()
    {
        mainCamera = Camera.main;
        //SetScalingForBorderDeadZone();
    }

    //Needs to be redone.
    private void SetScalingForBorderDeadZone ()
    {
        var scalingA = distanceFromBorder.x / 2560;
        var scalingB = distanceFromBorder.y / 1440;

        var mainDisplay = Display.main;

        distanceFromBorder = new(scalingA * mainDisplay.renderingWidth, scalingB * mainDisplay.renderingHeight);
    }

    private void Update ()
    {
        ApplyLookDirection();
        CheckForPositionChange();
    }

    private void LateUpdate ()
    {
        if ( updateCameraPosition )
            UpdatePosition();
    }
}