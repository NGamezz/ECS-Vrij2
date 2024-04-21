using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.0f;

    [SerializeField] private float dashForce = 10.0f;

    [SerializeField] private float dashCooldown = 0.2f;

    [SerializeField] private GameObject meshObject;

    [SerializeField] private int groundLayerMask = 4;

    [SerializeField] private float maxStamina = 100.0f;

    [SerializeField] private float sprintSpeedIncrease = 2.0f;

    [SerializeField] private float sprintStaminaCost = 2.0f;

    [SerializeField] private float stamina;

    [SerializeField] private float staminaRegenSpeed = 1.0f;

    private bool isRunning = false;
    private bool pressedKey = false;

    private Vector2 inputVector;

    private bool canJump = true;

    private Rigidbody rb;

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        inputVector = ctx.ReadValue<Vector2>();
    }

    public void OnJump ( InputAction.CallbackContext ctx )
    {
        if ( !canJump || !Physics.SphereCast(meshObject.transform.position, 0.5f, -meshObject.transform.up, out _, 2.0f, 1 << groundLayerMask) || stamina < maxStamina / 2.0f )
            return;

        var direction = inputVector.magnitude == 0 ? rb.transform.forward : new(inputVector.x, 0.0f, inputVector.y);

        stamina -= maxStamina / 2.0f;

        Debug.Log("Dash");

        canJump = false;
        rb.AddForce(dashForce * direction, ForceMode.Impulse);

        StartCoroutine(ResetDashCooldown());
    }

    private IEnumerator ResetDashCooldown ()
    {
        canJump = false;
        yield return new WaitForSeconds(dashCooldown);
        canJump = true;
    }

    void Start ()
    {
        rb = meshObject.GetComponent<Rigidbody>();
        rb ??= meshObject.AddComponent<Rigidbody>();

        stamina = maxStamina;
    }

    private void HandleInput ()
    {
        if ( Input.GetKeyDown(KeyCode.LeftShift) && stamina - sprintStaminaCost > 0 )
        {
            moveSpeed += sprintSpeedIncrease;
            isRunning = true;
            pressedKey = true;
            Debug.Log("Activate Sprint.");
        }
        if ( Input.GetKeyUp(KeyCode.LeftShift) && pressedKey)
        {
            moveSpeed -= sprintSpeedIncrease;
            pressedKey = false;
            isRunning = false;
            Debug.Log("Deactivate Sprint.");
        }

        if ( !isRunning && stamina + staminaRegenSpeed <= maxStamina )
        {
            stamina += staminaRegenSpeed * Time.deltaTime;
        }
        if(isRunning && stamina - sprintStaminaCost > 0)
        {
            stamina -= sprintStaminaCost * Time.deltaTime;
        }
        else if(isRunning && stamina - sprintStaminaCost <= 0)
        {
            pressedKey = false;
            moveSpeed -= sprintSpeedIncrease;
            isRunning = false;
        }
    }

    private void ApplyForce ()
    {
        rb.AddForce(new float3(inputVector.x, 0.0f, inputVector.y) * moveSpeed, ForceMode.Force);
    }

    private void Update ()
    {
        HandleInput();
    }

    private void FixedUpdate ()
    {
        ApplyForce();
    }
}