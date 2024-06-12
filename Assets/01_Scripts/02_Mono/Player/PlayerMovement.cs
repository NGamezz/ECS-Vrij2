using Cysharp.Threading.Tasks;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerMovement
{
    public float dashCooldown = 0.2f;

    [SerializeField] private GameObject meshObject;
    private Transform cachedMeshTransform;

    [SerializeField] private int groundLayerMask = 4;

    [SerializeField] private bool inverse = false;

    public CharacterData characterData;
    private Vector2 inputVector;

    private bool canDash = true;
    private Rigidbody rb;

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        if ( inverse )
        {
            inputVector = -ctx.ReadValue<Vector2>();
        }
        else
            inputVector = ctx.ReadValue<Vector2>();
    }

    public void OnDash ( Action<float> coolDownStream, Action completionCallback )
    {
        var ownPos = cachedMeshTransform.position;
        var halfStamina = characterData.MaxStamina / 2.0f;

        if ( !canDash || characterData.Stamina < halfStamina || !Physics.CheckSphere(new Vector3(ownPos.x, ownPos.y + 0.25f, ownPos.z), 0.5f, 1 << groundLayerMask) )
            return;
        canDash = false;

        var direction = inputVector.magnitude == 0 ? rb.transform.forward : new(inputVector.x, 0.0f, inputVector.y);
        characterData.Stamina -= halfStamina;

        rb.AddForce((characterData.Speed) * direction.normalized, ForceMode.Impulse);

        Utility.Async.StreamedTimerAsync(coolDownStream, () => { completionCallback?.Invoke(); canDash = true; }, dashCooldown).Forget();
    }

    public void OnStart ()
    {
        rb = meshObject.GetComponent<Rigidbody>();
        rb = rb != null ? rb : meshObject.AddComponent<Rigidbody>();
        characterData.Stamina = characterData.MaxStamina;
        cachedMeshTransform = meshObject.transform;
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

    private void StaminaRegen ()
    {
        if ( characterData.Stamina < characterData.MaxStamina )
        {
            characterData.Stamina += 10 * Time.fixedDeltaTime;

            if ( characterData.Stamina > characterData.MaxStamina )
            {
                characterData.Stamina = characterData.MaxStamina;
            }
        }
    }

    public void OnFixedUpdate ()
    {
        ApplyForce();
        VelocityLimiting();
        StaminaRegen();
    }
}