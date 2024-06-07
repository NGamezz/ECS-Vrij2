using System;
using System.Collections.Concurrent;

public enum EventType
{
    UponDesiredSoulsAmount = 0,
    UponHarvestSoul = 1,
    TargetSelection = 2,
    OnTextPopupQueue = 3,
    PortalActivation = 4,
    ActivateSoulEffect = 5,
    GameOver = 6,
    OnGameStateChange = 7,
}

public static class EventManager
{
    private static ConcurrentDictionary<EventType, Action> events = new();

    public static void AddListener ( EventType type, Action action )
    {
        if ( !events.ContainsKey(type) )
        {
            events.TryAdd(type, action);
        }
        else if ( events.ContainsKey(type) )
        {
            lock ( events[type] )
            {
                events[type] += action;
            }
        }
    }

    public static void RemoveListener ( EventType type, Action action )
    {
        if ( !events.ContainsKey(type) )
            return;

        if ( events.ContainsKey(type) )
        {
            lock ( events[type] )
            {
                events[type] -= action;
            }
        }
    }

    public static void InvokeEvent ( EventType type )
    {
        if ( !events.ContainsKey(type) )
            return;

        lock ( events[type] )
        {
            events[type]?.Invoke();
        }
    }

    public static void ClearListeners ()
    {
        int amount = Enum.GetValues(typeof(EventType)).Length;

        for ( int i = amount; i > 0; i-- )
        {
            lock ( events[(EventType)i] )
            {
                events[(EventType)i] = null;
            }
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
            lock ( events[type] )
            {
                events[type] += action;
            }
        }
    }

    public static void RemoveListener ( EventType type, Action<T> action )
    {
        if ( !events.ContainsKey(type) )
            return;

        if ( events.ContainsKey(type) )
        {
            lock ( events[type] )
            {
                events[type] -= action;
            }
        }
    }

    public static void InvokeEvent ( EventType type, T input )
    {
        if ( !events.ContainsKey(type) )
            return;

        lock ( events[type] )
        {
            events[type]?.Invoke(input);
        }
    }

    public static void ClearListeners ()
    {
        int amount = Enum.GetValues(typeof(EventType)).Length;

        for ( int i = amount; i > 0; i-- )
        {
            lock ( events[(EventType)i] )
            {
                events[(EventType)i] = null;
            }
        }

        events.Clear();
    }
}