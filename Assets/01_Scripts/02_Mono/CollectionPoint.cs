using UnityEngine;
using UnityEngine.Events;

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

    private Vector3 ownPos;

    private void Start ()
    {
        ownPos = new(transform.position.x, 0.0f, transform.position.z);
    }

    public override bool CalculateOnDeath ( Vector3 entity )
    {
        var lenght = Vector3.Distance(new(entity.x, 0.0f, entity.z), ownPos);

        if ( lenght > range )
        {
            return false;
        }

        EventManagerGeneric<VectorAndTransformAndCallBack>.InvokeEvent(EventType.ActivateSoulEffect, new(entity, transform, () =>
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

        if ( souls >= amountToTrigger )
        {
            if ( eventToTrigger != null )
            {
                eventToTrigger?.Invoke();
            }

            EventManager.InvokeEvent(EventType.UponDesiredSoulsAmount);
        }
    }
}