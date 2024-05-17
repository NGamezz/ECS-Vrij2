using System.Collections;
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

    [SerializeField] private int2 scalingResolution = new(2560, 1440);

    private Vector3 velocity = Vector3.zero;

    private Vector2 lookVector;

    private bool updateCameraPosition = false;

    private Resolution screenRes;

    private Camera mainCamera;

    private bool running = true;

    [Tooltip("For arrow keys aiming for instance.")]
    public void OnLook ( InputAction.CallbackContext ctx )
    {
        if ( lookAtMouse )
            return;

        lookVector = ctx.ReadValue<Vector2>();
    }

    private readonly RaycastHit[] hits = new RaycastHit[1];

    private Vector3 mousePosition = Vector3.zero;
    public void UpdateMouseWorldPosition ( Vector3 pos )
    {
        mousePosition = pos;
    }

    //Should be improved.
    private void ApplyLookDirection ()
    {
        Vector3 direction;

        if ( lookAtMouse )
        {
            //var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            //if ( Physics.RaycastNonAlloc(ray, hits, float.MaxValue, ~(1 << playerLayerMask)) == 0 )
            //{
            //    return;
            //}

            direction = mousePosition - meshTransform.position;
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

    private void OnDisable ()
    {
        StopAllCoroutines();
        updateCameraPosition = false;
        waitUntilUpdateCam = null;
    }

    private void CheckForPositionChange ()
    {
        var meshPos = meshTransform.position;
        var meshScreenPos = mainCamera.WorldToScreenPoint(meshPos);

        var withinSafeZone = meshScreenPos.x <= screenRes.width - distanceFromBorder.x
            && meshScreenPos.y <= screenRes.height - distanceFromBorder.y
            && meshScreenPos.y >= distanceFromBorder.y
            && meshScreenPos.x >= distanceFromBorder.x;

        if ( withinSafeZone )
            return;

        updateCameraPosition = true;
    }

    private static WaitUntil waitUntilUpdateCam;

    private IEnumerator UpdateCameraPositionIE ()
    {
        while ( running )
        {
            yield return waitUntilUpdateCam;

            while ( updateCameraPosition )
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
                yield return null;
            }
        }
    }

    private void Start ()
    {
        mainCamera = Camera.main;
        waitUntilUpdateCam = new(() => updateCameraPosition);

        StartCoroutine(UpdateCameraPositionIE());
        SetScalingForBorderDeadZone();
    }

    private void SetScalingForBorderDeadZone ()
    {
        var mainDisplay = Display.main;

        int height = mainDisplay.renderingHeight;
        int width = mainDisplay.renderingWidth;

        float scalingA = width / (scalingResolution.x * 1.0f);
        float scalingB = height / (scalingResolution.y * 1.0f);

        screenRes = new()
        {
            width = width,
            height = height
        };

        distanceFromBorder = new((int)(distanceFromBorder.x * scalingA), (int)(distanceFromBorder.y * scalingB));
    }

    private void Update ()
    {
        ApplyLookDirection();
        CheckForPositionChange();
    }
}