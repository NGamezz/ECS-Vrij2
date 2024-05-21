using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerMovement
{
    [SerializeField] private float dashCooldown = 0.2f;

    [SerializeField] private GameObject meshObject;
    private Transform cachedMeshTransform;

    [SerializeField] private int groundLayerMask = 4;

    [SerializeField] private float sprintStaminaCost = 2.0f;
    [SerializeField] private float staminaRegenSpeed = 1.0f;

    public CharacterData characterData;

    private bool isRunning = false;
    private bool pressedKey = false;

    private Vector2 inputVector;

    private bool canDash = true;
    private Rigidbody rb;

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        inputVector = ctx.ReadValue<Vector2>();
    }

    public void OnDash ()
    {
        float halfStamina = characterData.Stamina / 2;
        if ( !canDash || characterData.Stamina < halfStamina || !Physics.CheckSphere(cachedMeshTransform.position - (Vector3.one * 0.5f), 0.5f, 1 << groundLayerMask) )
            return;

        var direction = inputVector.magnitude == 0 ? rb.transform.forward : new(inputVector.x, 0.0f, inputVector.y);
        characterData.Stamina -= halfStamina;

        canDash = false;
        rb.AddForce((characterData.Speed * 1.5f) * direction, ForceMode.Impulse);

        ResetDashCooldown();
    }
    
    private async void ResetDashCooldown ()
    {
        canDash = false;
        await Awaitable.WaitForSecondsAsync(dashCooldown);
        canDash = true;
    }

    public void OnStart ()
    {
        rb = meshObject.GetComponent<Rigidbody>();
        rb = rb != null ? rb : meshObject.AddComponent<Rigidbody>();
        characterData.Stamina = characterData.MaxStamina;
        cachedMeshTransform = meshObject.transform;
    }

    private void HandleInput ()
    {
        if ( Input.GetKeyDown(KeyCode.LeftShift) && characterData.Stamina - sprintStaminaCost >= 0 )
        {
            SetSprint(true);
            Utility.Utility.Log("Activate Sprint.");
        }
        if ( Input.GetKeyUp(KeyCode.LeftShift) && pressedKey )
        {
            SetSprint(false);
            Utility.Utility.Log("Activate Sprint.");
        }
    }

    private void CheckStamina ()
    {
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
            SetSprint(false);
        }
    }

    private void SetSprint ( bool state )
    {
        pressedKey = state;
        var speed = characterData.Speed;
        characterData.Speed = state ? speed + characterData.SpeedMultiplier : speed - characterData.SpeedMultiplier;
        isRunning = state;
    }

    private void VelocityLimiting ()
    {
        var vel = rb.velocity;
        var flatVel = new Vector3(vel.x, 0.0f, vel.z);
        if ( flatVel.magnitude > characterData.Speed )
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
        CheckStamina();
    }

    public void OnFixedUpdate ()
    {
        ApplyForce();
        VelocityLimiting();
    }
}