using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class CollectionPoint : MonoBehaviour
{
    [SerializeField] private float range = 5.0f;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private Vector3 ownPosition;

    Vector3 currentEntityPosition = Vector3.zero;
    private void CalculateOnDeath ( object entity )
    {
        Task.Run(async () =>
        {
            currentEntityPosition = (Vector3)entity;
            OnEnemyDeath(currentEntityPosition);

            if ( souls >= amountToTrigger )
            {
                WorldManager.RemoveGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);

                if ( eventToTrigger != null )
                {
                    await Awaitable.MainThreadAsync();
                    eventToTrigger?.Invoke();
                }
            }
        });
    }

    private void OnEnemyDeath ( Vector3 position )
    {
        var lenght = math.length(position - ownPosition);

        if ( lenght < range )
            AddSoul(1);
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