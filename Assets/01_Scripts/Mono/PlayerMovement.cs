using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerMovement
{
    [SerializeField] private float dashCooldown = 0.2f;

    [SerializeField] private GameObject meshObject;

    [SerializeField] private int groundLayerMask = 4;

    [SerializeField] private float sprintStaminaCost = 2.0f;

    [SerializeField] private float stamina;

    [SerializeField] private float staminaRegenSpeed = 1.0f;

    public PlayerStats playerStats;

    private bool isRunning = false;
    private bool pressedKey = false;

    private Vector2 inputVector;

    private bool canJump = true;

    private Rigidbody rb;

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        inputVector = ctx.ReadValue<Vector2>();
    }

    public void OnDash ()
    {
        if ( !canJump || !Physics.SphereCast(meshObject.transform.position, 0.5f, -meshObject.transform.up, out _, 2.0f, (1 << groundLayerMask)) || stamina < playerStats.maxStamina / 2.0f )
            return;

        var direction = inputVector.magnitude == 0 ? rb.transform.forward : new(inputVector.x, 0.0f, inputVector.y);

        stamina -= playerStats.maxStamina / 2.0f;

        Debug.Log("Dash");

        canJump = false;
        rb.AddForce(playerStats.dashForce * direction, ForceMode.Impulse);

        ResetDashCooldown();
    }

    private async void ResetDashCooldown ()
    {
        canJump = false;
        await Awaitable.WaitForSecondsAsync(dashCooldown);
        canJump = true;
    }

    public void OnStart ()
    {
        rb = meshObject.GetComponent<Rigidbody>();
        rb = rb != null ? rb : meshObject.AddComponent<Rigidbody>();

        stamina = playerStats.maxStamina;
    }

    private void HandleInput ()
    {
        if ( Input.GetKeyDown(KeyCode.LeftShift) && stamina - sprintStaminaCost > 0 )
        {
            playerStats.moveSpeed += playerStats.sprintSpeedIncrease;
            isRunning = true;
            pressedKey = true;
            Debug.Log("Activate Sprint.");
        }
        if ( Input.GetKeyUp(KeyCode.LeftShift) && pressedKey )
        {
            playerStats.moveSpeed -= playerStats.sprintSpeedIncrease;
            pressedKey = false;
            isRunning = false;
            Debug.Log("Deactivate Sprint.");
        }

        if ( !isRunning && stamina + staminaRegenSpeed <= playerStats.maxStamina )
        {
            stamina += staminaRegenSpeed * Time.deltaTime;
        }
        if ( isRunning && stamina - sprintStaminaCost > 0 )
        {
            stamina -= sprintStaminaCost * Time.deltaTime;
        }
        else if ( isRunning && stamina - sprintStaminaCost <= 0 )
        {
            pressedKey = false;
            playerStats.moveSpeed -= playerStats.sprintSpeedIncrease;
            isRunning = false;
        }
    }

    private void VelocityLimiting ()
    {
        var vel = rb.velocity;
        var flatVel = new Vector3(vel.x, 0.0f, vel.z);
        if ( flatVel.magnitude > playerStats.moveSpeed )
        {
            flatVel.Normalize();
            rb.velocity = flatVel * playerStats.moveSpeed;
        }
    }

    private void ApplyForce ()
    {
        rb.AddForce(new float3(inputVector.x, 0.0f, inputVector.y) * playerStats.moveSpeed, ForceMode.Force);
    }

    public void OnUpdate ()
    {
        HandleInput();
    }

    public void OnFixedUpdate ()
    {
        ApplyForce();
        VelocityLimiting();
    }
}