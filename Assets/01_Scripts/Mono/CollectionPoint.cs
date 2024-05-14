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
        if ( entity is not Vector3 pos )
            return;

        var lenght = math.length(pos - ownPosition);

        if ( lenght > range )
        {
            EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            return;
        }

        AddSoul(1);
    }

    public void OnStart ()
    {
        ownPosition = transform.position;

        Task.Run(async () =>
        {
            cellPositions = await WorldManager.AddGridListenerParallelJob(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
        }).ConfigureAwait(false);
    }

    private void OnDisable ()
    {
        Task.Run(() =>
        {
            WorldManager.RemoveGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
        }).Wait();

        souls = 0;
        cellPositions.Clear();
    }

    private void AddSoul ( int amount )
    {
        if ( amount < 0 )
            return;

        souls += amount;

        Task.Run(() =>
        {
            if ( souls >= amountToTrigger )
            {
                if ( eventToTrigger != null )
                {
                    GameManager.Instance.Enqueue(() =>
                    {
                        eventToTrigger?.Invoke();
                    });
                }

                WorldManager.RemoveGridListener(ownPosition, range, CalculateOnDeath, CellEventType.OnEntityDeath);
                EventManager.InvokeEvent(EventType.UponDesiredSoulsAmount);
            }
        }).ConfigureAwait(false);
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