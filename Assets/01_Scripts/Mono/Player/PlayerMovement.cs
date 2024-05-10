using System;
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

    [SerializeField] private float staminaRegenSpeed = 1.0f;

    //public PlayerStats playerStats;

    public CharacterData characterData;

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
        if ( !canJump || !Physics.SphereCast(meshObject.transform.position, 0.5f, -meshObject.transform.up, out _, 2.0f, (1 << groundLayerMask)) || characterData.Stamina < characterData.MaxStamina / 2.0f )
            return;

        var direction = inputVector.magnitude == 0 ? rb.transform.forward : new(inputVector.x, 0.0f, inputVector.y);

        characterData.Stamina -= characterData.MaxStamina / 2.0f;

        Debug.Log("Dash");

        canJump = false;
        rb.AddForce((characterData.Speed * 1.5f) * direction, ForceMode.Impulse);

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
        characterData.Stamina = characterData.MaxStamina;
    }

    private void HandleInput ()
    {
        if ( Input.GetKeyDown(KeyCode.LeftShift) && characterData.Stamina - sprintStaminaCost > 0 )
        {
            characterData.Speed += characterData.SpeedMultiplier;
            isRunning = true;
            pressedKey = true;
            Debug.Log("Activate Sprint.");
        }
        if ( Input.GetKeyUp(KeyCode.LeftShift) && pressedKey )
        {
            characterData.Speed -= characterData.SpeedMultiplier;
            pressedKey = false;
            isRunning = false;
            Debug.Log("Deactivate Sprint.");
        }

        if ( !isRunning && characterData.Stamina + staminaRegenSpeed <= characterData.MaxStamina )
        {
            characterData.Stamina += staminaRegenSpeed * Time.deltaTime;
        }
        if ( isRunning && characterData.Stamina - sprintStaminaCost > 0 )
        {
            characterData.Stamina -= sprintStaminaCost * Time.deltaTime;
        }
        else if ( isRunning && characterData.Stamina - sprintStaminaCost <= 0 )
        {
            pressedKey = false;
            characterData.Speed -= characterData.SpeedMultiplier;
            isRunning = false;
        }
    }

    private void VelocityLimiting ()
    {
        var vel = rb.velocity;
        var flatVel = new Vector3(vel.x, 0.0f, vel.z);
        if ( flatVel.magnitude > characterData.Speed)
        {
            flatVel.Normalize();
            rb.velocity = flatVel * characterData.Speed;
        }
    }

    private void ApplyForce ()
    {
        rb.AddForce(new float3(inputVector.x, 0.0f, inputVector.y) * characterData.Speed, ForceMode.Force);
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