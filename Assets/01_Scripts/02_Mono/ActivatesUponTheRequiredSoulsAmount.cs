using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class ActivatesUponTheRequiredSoulsAmount : ISoulCollectionArea
{
    [SerializeField] private int range = 10;

    [SerializeField] private int requiredSoulsAmount = 10;

    [SerializeField] private UnityEvent uponComepletion;

    private int souls = 0;
    private Vector3 ownPosition;

    private void Start ()
    {
        ownPosition = transform.position;
    }

    public override bool CalculateOnDeath ( Vector3 pos )
    {
        var lenght = Vector3.Distance(pos, ownPosition);

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

    private void AddSoul ( int amount )
    {
        if ( amount < 0 )
            return;

        souls += amount;

        if ( souls >= requiredSoulsAmount )
        {
            if ( uponComepletion != null )
            {
                uponComepletion?.Invoke();
            }
        }
    }

    [Conditional("ENABLE_LOGS")]
    private void OnDrawGizmos ()
    {
        Gizmos.DrawWireSphere(ownPosition, range);
    }
}