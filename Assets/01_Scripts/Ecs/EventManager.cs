using System;
using System.Collections.Generic;

public enum EventType
{
    UponRequestPlayerPosition = 0,
    UponRequestPlayerRotation = 1,
    UponDesiredSoulsAmount = 2,
}

public static class EventManager
{
    private static Dictionary<EventType, Action> events = new();

    public static void AddListener ( EventType type, Action action )
    {
        if ( !events.ContainsKey(type) )
        {
            events.Add(type, action);
        }
        else if ( events.ContainsKey(type) )
        {
            events[type] += action;
        }
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
            events[(EventType)i] = null;
        }

        events.Clear();
    }
}


public static class EventManagerGeneric<T>
{
    private static Dictionary<EventType, Action<T>> events = new();

    public static void AddListener ( EventType type, Action<T> action )
    {
        if ( !events.ContainsKey(type) )
        {
            events.Add(type, action);
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
        int amount = System.Enum.GetValues(typeof(EventType)).Length;

        for ( int i = amount; i > 0; i-- )
        {
            events[(EventType)i] = null;
        }

        events.Clear();
    }
}