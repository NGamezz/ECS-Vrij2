using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;

public class CollectionPointHybrid : MonoBehaviour
{
    [SerializeField] private int range = 15;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private EntityManager entityManager;

    private async void OnShoot ( object entity )
    {
        Debug.Log("Shoot");

        var localTransform = entityManager.GetComponentData<LocalTransform>((Entity)entity);
        Vector3 entityPos = localTransform.Position;
        var direction = entityPos - transform.position;

        var lenght = math.length(direction);
        Debug.Log(lenght);

        if ( lenght < range )
            AddSoul();

        if ( souls >= amountToTrigger )
        {
            WorldManager.RemoveGridListener(transform.position, OnShoot, CellEventType.OnEntityDeath);

            if ( eventToTrigger != null )
            {
                await Awaitable.MainThreadAsync();
                eventToTrigger?.Invoke();
            }
        }
    }

    private void AddSoul ()
    {
        souls++;
    }

    void Start ()
    {
        Vector3 position = transform.position;

        Task.Run(() =>
        {
            WorldManager.AddGridListener(position, OnShoot, CellEventType.OnEntityDeath);
        }).ConfigureAwait(false);

        var world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;
    }

    private void OnDrawGizmos ()
    {
        if ( !gizmos )
            return;

        Gizmos.DrawWireSphere(transform.position, range / 2.0f);
    }
}