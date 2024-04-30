using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class CollectionPoint : MonoBehaviour
{
    [SerializeField] private int range = 15;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private Vector3 ownPosition;

    Vector3 currentEntityPosition = Vector3.zero;

    private void CalculateOnDeath ( object entity )
    {
        Task.Run(() =>
        {
            currentEntityPosition = (Vector3)entity;

            var lenght = math.length(currentEntityPosition - ownPosition);

            if ( lenght < range )
                AddSoul(1);

            if ( souls >= amountToTrigger )
            {
                WorldManager.RemoveGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);

                if ( eventToTrigger != null )
                {
                    GameManager.MainThreadActionQueue.Enqueue(() =>
                    {
                        eventToTrigger?.Invoke();
                    });
                }
            }
        }).ConfigureAwait(false);
    }

    private void Start ()
    {
        ownPosition = transform.position;

        Task.Run(() =>
            {
                WorldManager.AddGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);
            }).ConfigureAwait(false);
    }

    private void OnDisable ()
    {
        Task.Run(() =>
        {
            WorldManager.RemoveGridListener(ownPosition, CalculateOnDeath, CellEventType.OnEntityDeath);
        }).Wait();
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