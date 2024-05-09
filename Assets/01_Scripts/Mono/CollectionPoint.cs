using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public enum CollectionPointMode
{
    Standard = 0,
    Manual = 1,
}

public class CollectionPoint : MonoBehaviour
{
    [SerializeField] private int range = 15;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private CollectionPointMode mode = CollectionPointMode.Standard;

    [SerializeField] private int souls = 0;

    [SerializeField] private List<int2> cellPositions = new();

    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private Vector3 ownPosition;

    Vector3 currentEntityPosition = Vector3.zero;

    private void CalculateOnDeath ( object entity )
    {
        Task.Run(() =>
        {
            var mode = this.mode;

            switch ( mode )
            {
                case CollectionPointMode.Standard:
                    {
                        StandardCollection(entity);
                        break;
                    }
                case CollectionPointMode.Manual:
                    {
                        EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
                        break;
                    }
            }
        }).ConfigureAwait(false);
    }

    private void StandardCollection ( object entity )
    {
        currentEntityPosition = (Vector3)entity;

        var lenght = math.length(currentEntityPosition - ownPosition);

        if ( lenght < range )
            AddSoul(1);

        if ( lenght > range )
        {
            EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
        }

        if ( souls >= amountToTrigger )
        {
            WorldManager.RemoveGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);

            if ( eventToTrigger != null )
            {
                GameManager.Instance.Enqueue(() =>
                {
                    eventToTrigger?.Invoke();
                });
            }
        }
    }

    private void Start ()
    {
        ownPosition = transform.position;

        Task.Run(() =>
        {
            cellPositions = WorldManager.AddGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
        }).ConfigureAwait(false);
    }

    private void OnDisable ()
    {
        Task.Run(() =>
        {
            WorldManager.RemoveGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
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

        Gizmos.DrawWireSphere(ownPosition, range);

        if ( cellPositions.Count < 1 )
            return;

        foreach ( var cellPos in cellPositions )
        {
            Gizmos.DrawWireCube(new(cellPos.x, 0.0f, cellPos.y), Vector3.one * WorldManager.CellSize);
        }
    }
}