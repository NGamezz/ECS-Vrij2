using Cysharp.Threading.Tasks;
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

    public CharacterData characterData;
    private Vector2 inputVector;

    private bool canDash = true;
    private Rigidbody rb;

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        inputVector = ctx.ReadValue<Vector2>();
    }

    public void OnDash ()
    {
        var ownPos = cachedMeshTransform.position;
        var halfStamina = characterData.MaxStamina / 2.0f;
        if ( !canDash || characterData.Stamina < halfStamina || !Physics.CheckSphere(ownPos - new Vector3(ownPos.x, ownPos.y + 0.25f, ownPos.z), 0.5f, 1 << groundLayerMask) )
            return;

        var direction = inputVector.magnitude == 0 ? rb.transform.forward : new(inputVector.x, 0.0f, inputVector.y);
        characterData.Stamina -= halfStamina;

        canDash = false;
        rb.AddForce((characterData.SpeedMultiplier) * direction, ForceMode.Impulse);

        ResetDashCooldown().Forget();
    }

    private async UniTaskVoid ResetDashCooldown ()
    {
        canDash = false;
        await UniTask.Delay(TimeSpan.FromSeconds(dashCooldown));
        canDash = true;
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

    public void OnUpdate ()
    {
    }

    public void OnFixedUpdate ()
    {
        ApplyForce();
        VelocityLimiting();
    }
}