using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour, IDamageable, ISoulCollector
{
    public int Souls => souls;

    public bool IsDead => dead;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerShooting playerShooting = new();

    [Space(2)]

    [SerializeField] private int souls = 0;

    [SerializeField] private float health;

    private bool dead;

    [SerializeField] private PlayerStats playerStats;

    public void AfflictDamage ( float amount )
    {
        health -= amount;

        if ( health <= 0 )
            gameObject.SetActive(false);
    }

    public void OnShoot ()
    {
        playerShooting.OnShoot();
    }

    public void OnMove ( InputAction.CallbackContext ctx )
    {
        playerMovement.OnMove(ctx);
    }

    public void OnDash ()
    {
        playerMovement.OnDash();
    }

    private void OnEnable ()
    {
        EventManagerGeneric<int>.AddListener(EventType.UponHarvestSoul, Collect);
    }

    private void OnDisable ()
    {
        EventManagerGeneric<int>.RemoveListener(EventType.UponHarvestSoul, Collect);
    }

    private void Start ()
    {
        playerMovement.playerStats = playerStats;
        playerMovement.OnStart();
        playerShooting.OnStart(transform, this);

        health = playerStats.maxHealth;
    }

    private void Update ()
    {
        playerMovement.OnUpdate();
    }

    private void FixedUpdate ()
    {
        playerMovement.OnFixedUpdate();
    }

    public void Collect ( int amount )
    {
        souls += amount;
    }
}