using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using System.Diagnostics;

public enum CollectionPointMode
{
    Standard = 0,
    Manual = 1,
}

public class CollectionPoint : ISoulCollectionArea
{
    [SerializeField] private int range = 15;

    [SerializeField] private bool gizmos = true;

    [SerializeField] private int souls = 0;
    [SerializeField] private int amountToTrigger = 10;

    [SerializeField] private UnityEvent eventToTrigger;

    private Vector3 ownPosition;

    public override bool CalculateOnDeath ( object entity )
    {
        return StandardCollection(entity);
    }

    private bool StandardCollection ( object entity )
    {
        if ( entity is not Vector3 pos )
            return false;

        var lenght = math.length(pos - ownPosition);

        if ( lenght > range )
        {
            return false;
        }

        EventManagerGeneric<VectorAndTransformAndCallBack>.InvokeEvent(EventType.ActivateSoulEffect, new(pos, transform, () =>
        {
            AddSoul(1);
        }));

        return true;
    }

    public void OnStart ()
    {
        ownPosition = transform.position;
    }

    private void OnDisable ()
    {
        souls = 0;
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
                eventToTrigger?.Invoke();
            }

            EventManager.InvokeEvent(EventType.UponDesiredSoulsAmount);
        }
    }

    [Conditional("ENABLE_LOGS")]
    private void OnDrawGizmos ()
    {
        if ( !gizmos )
            return;

        Gizmos.DrawWireSphere(ownPosition, range);
    }
}