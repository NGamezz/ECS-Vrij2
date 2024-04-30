using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerShooting playerShooting = new();

    private bool dead;

    [SerializeField] private PlayerStats playerStats;

    public bool IsDead () => dead; 

    public void AfflictDamage ( float amount )
    {

    }

    public void OnShoot ()
    {
        playerShooting.OnShoot();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        playerMovement.OnMove(ctx);
    }

    public void OnDash()
    {
        playerMovement.OnDash();
    }

    private void Start ()
    {
        playerMovement.playerStats = playerStats;
        playerMovement.OnStart();
        playerShooting.OnStart(transform, this);
    }

    private void Update ()
    {
        playerMovement.OnUpdate();
    }

    private void FixedUpdate ()
    {
        playerMovement.OnFixedUpdate();
    }
}