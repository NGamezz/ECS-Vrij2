using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utility;

public enum EventType
{
    UponDesiredSoulsAmount = 0,
    UponHarvestSoul = 1,
    TargetSelection = 2,
    OnTextPopupQueue = 3,
    PortalActivation = 4,
    ActivateSoulEffect = 5,
    GameOver = 6,
    PostGameOverWait = 7,
    OnGameStateChange = 8,
    OnSceneChange = 9,
    OnEnemyDeath = 10,
}

public struct EventHolder
{
    public EventType type;
    public Action action;

    public EventHolder ( EventType type, Action action )
    {
        this.type = type;
        this.action = action;
    }
}

public struct EventSubscriptions
{
    private List<EventHolder> events;

    public EventSubscriptions ( EventHolder eventHandle )
    {
        events = new()
        {
            eventHandle
        };
    }

    public void Subscribe ( EventHolder eventHandle )
    {
        events ??= new();
        events.Add(eventHandle);
    }

    public readonly void UnsubscribeAll ()
    {
        for ( int i = events.Count - 1; i >= 0; --i )
        {
            EventManager.RemoveListener(events[i].type, events[i].action);
        }
    }
}

public struct EventSubscription
{
    private EventHolder eventHandle;

    public EventSubscription ( EventHolder eventHandle )
    {
        this.eventHandle = eventHandle;
    }

    public readonly void UnSubcribe ()
    {
        EventManager.RemoveListener(eventHandle.type, eventHandle.action);
    }
}

public static class EventManager
{
    private static readonly ConcurrentDictionary<EventType, Action> events = new();

    public static void AddListener ( EventType type, Action action, ref EventSubscriptions sub )
    {
        if ( !events.ContainsKey(type) )
        {
            events.TryAdd(type, action);
        }
        else if ( events.ContainsKey(type) )
        {
            events[type] += action;
        }

        sub.Subscribe(new(type, action));
    }

    //The component is for automatic unsubscribing.
    public static void AddListener ( EventType type, Action action, UnityEngine.Component component )
    {
        if ( !events.ContainsKey(type) )
        {
            events.TryAdd(type, action);
        }
        else if ( events.ContainsKey(type) )
        {
            events[type] += action;
        }

        component.GetAsyncGameObjectDeactivationTrigger().Subscribe(() => RemoveListener(type, action));
    }

    public static void RemoveListener ( EventType type, Action action )
    {
        if ( !events.ContainsKey(type) )
            return;

        if ( events.ContainsKey(type) )
        {
            events[type] -= action;
        }
    }

    public static void InvokeEvent ( EventType type )
    {
        if ( !events.ContainsKey(type) )
            return;

        events[type]?.Invoke();
    }

    public static void ClearListeners ()
    {
        int amount = Enum.GetValues(typeof(EventType)).Length;

        for ( int i = amount; i > 0; i-- )
        {
            if ( !events.ContainsKey((EventType)i) )
                continue;

            events[(EventType)i] = null;
        }

        events.Clear();
    }
}

public static class EventManagerGeneric<T>
{
    private static ConcurrentDictionary<EventType, Action<T>> events = new();

    public static void AddListener ( EventType type, Action<T> action )
    {
        if ( !events.ContainsKey(type) )
        {
            events.TryAdd(type, action);
        }
        else if ( events.ContainsKey(type) )
        {
            events[type] += action;
        }
    }

    public static void RemoveListener ( EventType type, Action<T> action )
    {
        if ( !events.ContainsKey(type) )
            return;

        if ( events.ContainsKey(type) )
        {
            events[type] -= action;
        }
    }

    public static void InvokeEvent ( EventType type, T input )
    {
        if ( !events.ContainsKey(type) )
            return;

        events[type]?.Invoke(input);
    }

    public static void ClearListeners ()
    {
        int amount = Enum.GetValues(typeof(EventType)).Length;

        for ( int i = amount; i > 0; i-- )
        {
            if ( !events.ContainsKey((EventType)i) )
                continue;

            events[(EventType)i] = null;
        }

        events.Clear();
    }
}