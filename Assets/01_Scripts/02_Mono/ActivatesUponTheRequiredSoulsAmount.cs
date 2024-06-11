using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class ActivatesUponTheRequiredSoulsAmount : MonoBehaviour
{
    [SerializeField] private int range = 10;

    [SerializeField] private int requiredSoulsAmount = 10;

    [SerializeField] private UnityEvent uponComepletion;

    private Transform playerMeshTransform;

    private HashSet<int2> cellPositions;
    private int souls = 0;
    private Vector3 ownPosition;

    private void Start ()
    {
        ownPosition = transform.position;
        playerMeshTransform = ((PlayerMesh)FindAnyObjectByType(typeof(PlayerMesh))).GetTransform();
        SetupPositions().Forget();
    }

    private void CalculateOnDeathWrapper ( object entity )
    {
        CalculateOnDeath(entity).Forget();
    }

    private async UniTaskVoid SetupPositions ()
    {
        cellPositions = await WorldManager.AddGridListener(ownPosition, range, CalculateOnDeathWrapper, CellEventType.OnEntityDeath);
    }

    private async UniTaskVoid CalculateOnDeath ( object entity )
    {
        if ( entity is not Vector3 pos )
            return;

        await UniTask.SwitchToThreadPool();

        var lenght = math.length(pos - ownPosition);

        if ( lenght > range )
        {
            await UniTask.SwitchToMainThread();

            EventManagerGeneric<VectorAndTransform>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, playerMeshTransform));
            EventManagerGeneric<int>.InvokeEvent(EventType.UponHarvestSoul, 1);
            return;
        }

        EventManagerGeneric<DoubleVector3>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, ownPosition));
        AddSoul(1);
    }

    private void AddSoul ( int amount )
    {
        if ( amount < 0 )
            return;

        souls += amount;

        if ( souls >= requiredSoulsAmount )
        {
            if ( uponComepletion != null )
            {
                MainThreadQueue.Instance.Enqueue(() =>
                {
                    uponComepletion?.Invoke();
                });
            }

            WorldManager.RemoveGridListeners(cellPositions, CalculateOnDeathWrapper, CellEventType.OnEntityDeath);
        }
    }

    private void OnDisable ()
    {
        cellPositions.Clear();
        WorldManager.RemoveGridListeners(cellPositions, CalculateOnDeathWrapper, CellEventType.OnEntityDeath);
    }

    [Conditional("ENABLE_LOGS")]
    private void OnDrawGizmos ()
    {
        if ( cellPositions == null )
            return;

        Gizmos.DrawWireSphere(ownPosition, range);

        foreach ( var cell in cellPositions )
        {
            Gizmos.DrawWireCube(new(cell.x, 0.0f, cell.y), Vector3.one * WorldManager.CellSize);
        }
    }
}
