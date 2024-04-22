using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class CollectionPoint : MonoBehaviour
{
    [SerializeField] private float range = 5.0f;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private Vector3 ownPosition;

    private async void CalculateOnDeath ( object entity )
    {
        Vector3 position = (Vector3)entity;

        await Task.Run(() =>
        {
            OnEnemyDeath(position);
        });

        if ( souls >= amountToTrigger )
        {
            await Task.Run(() =>
            {
                WorldManager.RemoveGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);
            });
            eventToTrigger?.Invoke();
        }
    }

    private Task OnEnemyDeath ( Vector3 position )
    {
        var direction = position - ownPosition;

        var lenght = math.length(direction);

        if ( lenght < range )
            AddSoul(1);

        return Task.CompletedTask;
    }

    private void Start ()
    {
        ownPosition = transform.position;

        Task.Run(() =>
        {
            WorldManager.AddGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);
        });
    }

    private void OnDisable ()
    {
        Task.Run(() =>
        {
            WorldManager.RemoveGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);
        });
    }

    private void AddSoul ( int amount )
    {
        souls += amount;
    }

    private void OnDrawGizmos ()
    {
        if ( !gizmos )
            return;

        Gizmos.DrawWireSphere(ownPosition, range / 2.0f);
    }
}