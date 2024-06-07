using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Diagnostics;

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

    private void CalculateOnDeathWrapper ( object entity )
    {
        CalculateOnDeath(entity).Forget();
    }

    private async UniTaskVoid CalculateOnDeath ( object entity )
    {
        await UniTask.SwitchToThreadPool();

        var mode = this.mode;

        switch ( mode )
        {
            case CollectionPointMode.Standard:
                {
                    StandardCollection(entity).Forget();
                    break;
                }
            case CollectionPointMode.Manual:
                {
                    EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
                    break;
                }
        }
    }

    private async UniTaskVoid StandardCollection ( object entity )
    {
        if ( entity is not Vector3 pos )
            return;

        var lenght = math.length(pos - ownPosition);

        if ( lenght > range )
        {
            await UniTask.SwitchToMainThread();

            EventManagerGeneric<VectorAndTransformAndCallBack>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, playerTransform, () =>
            {
                EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            }));
            return;
        }

        EventManagerGeneric<VectorAndTransformAndCallBack>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, transform, () =>
        {
            AddSoul(1).Forget();
        }));
    }

    public async UniTaskVoid OnStart ( Transform playerTransform )
    {
        ownPosition = transform.position;

        this.playerTransform = playerTransform;

        await UniTask.SwitchToThreadPool();

        cellPositions = await WorldManager.AddGridListener(ownPosition, range, CalculateOnDeathWrapper, CellEventType.OnEntityDeath);
    }

    private void OnDisable ()
    {
        WorldManager.RemoveGridListeners(cellPositions, CalculateOnDeathWrapper, CellEventType.OnEntityDeath);

        souls = 0;
        cellPositions.Clear();
    }

    private async UniTaskVoid AddSoul ( int amount )
    {
        if ( amount < 0 )
            return;

        await UniTask.SwitchToThreadPool();

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

            WorldManager.RemoveGridListeners(cellPositions, CalculateOnDeathWrapper, CellEventType.OnEntityDeath);
            EventManager.InvokeEvent(EventType.UponDesiredSoulsAmount);
        }
    }

    [Conditional("ENABLE_LOGS")]
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