using System.Collections.Generic;
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

    [SerializeField] private HashSet<int2> cellPositions = new();

    [SerializeField] private int amountToTrigger = 10;

    private Transform playerTransform;

    [SerializeField] private UnityEvent eventToTrigger;

    private Vector3 ownPosition;

    private async void CalculateOnDeath ( object entity )
    {
        await Awaitable.BackgroundThreadAsync();

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
    }

    private void StandardCollection ( object entity )
    {
        if ( entity is not Vector3 pos )
            return;

        var lenght = math.length(pos - ownPosition);

        if ( lenght > range )
        {
            MainThreadQueue.Instance.Enqueue(() =>
            {
                EventManagerGeneric<DoubleVector3>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, playerTransform.position));
            });
            EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            return;
        }

        EventManagerGeneric<DoubleVector3>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, ownPosition));

        AddSoul(1);
    }

    public void OnStart ( Transform playerTransform )
    {
        ownPosition = transform.position;

        this.playerTransform = playerTransform;

        BackgroundQueue.Instance.Enqueue(() =>
        {
            cellPositions = WorldManager.AddGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
        });
    }

    private void OnDisable ()
    {
        WorldManager.RemoveGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);

        souls = 0;
        cellPositions.Clear();
    }

    private void AddSoul ( int amount )
    {
        if ( amount < 0 )
            return;

        souls += amount;

        if ( souls >= amountToTrigger )
        {
            if ( eventToTrigger != null )
            {
                MainThreadQueue.Instance.Enqueue(() =>
                {
                    eventToTrigger?.Invoke();
                });
            }

            WorldManager.RemoveGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
            EventManager.InvokeEvent(EventType.UponDesiredSoulsAmount);
        }
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