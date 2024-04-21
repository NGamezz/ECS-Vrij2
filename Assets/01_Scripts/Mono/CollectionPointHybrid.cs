using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;

public class CollectionPointHybrid : MonoBehaviour
{
    [SerializeField] private float range = 5.0f;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private EntityManager entityManager;

    private PlayerShootingSystem shootingSystem;

    private void OnShoot ( object entity, EventArgs args )
    {
        var localTransform = entityManager.GetComponentData<LocalTransform>((Entity)entity);

        Vector3 entityPos = localTransform.Position;

        var direction = entityPos - transform.position;

        var lenght = math.length(direction);

        if ( lenght < range )
            AddSoul();

        if ( souls >= amountToTrigger )
        {
            eventToTrigger?.Invoke();
            Debug.Log("Triggered Event.");
            shootingSystem.OnShoot -= OnShoot;
            this.enabled = false;
        }
    }

    private void AddSoul ()
    {
        souls++;
    }

    void Start ()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;

        shootingSystem = world.GetExistingSystemManaged<PlayerShootingSystem>();
        shootingSystem.OnShoot += OnShoot;
    }

    private void OnDrawGizmos ()
    {
        if ( !gizmos )
            return;

        Gizmos.DrawWireSphere(transform.position, range / 2.0f);
    }
}