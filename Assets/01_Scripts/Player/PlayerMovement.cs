using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.0f;

    [SerializeField] private float jumpForce = 10.0f;

    [SerializeField] private float jumpCooldown = 0.2f;

    [SerializeField] private GameObject meshObject;

    [SerializeField] private int groundLayerMask = 4;

    private Vector2 inputVector;

    private bool canJump = true;

    private Rigidbody rb;

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        inputVector = ctx.ReadValue<Vector2>();
    }

    public void OnJump ( InputAction.CallbackContext ctx )
    {
        if ( !canJump || !Physics.SphereCast(meshObject.transform.position, 0.5f, -meshObject.transform.up, out _, 2.0f, 1 << groundLayerMask) )
            return;

        canJump = false;
        rb.AddForce(jumpForce * transform.up, ForceMode.Impulse);

        StartCoroutine(ResetJumpCoolDown());
    }

    private IEnumerator ResetJumpCoolDown ()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    void Start ()
    {
        rb = meshObject.GetComponent<Rigidbody>();
        rb ??= meshObject.AddComponent<Rigidbody>();
    }

    private void HandleInput ()
    {
        ApplyForce();
    }

    private void ApplyForce ()
    {
        rb.AddForce(new float3(inputVector.x, 0.0f, inputVector.y) * moveSpeed, ForceMode.Force);
    }

    private void FixedUpdate ()
    {
        HandleInput();
    }
}