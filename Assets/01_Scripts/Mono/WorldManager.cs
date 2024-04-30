using System;
using Unity.Mathematics;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Concurrent;

public static class WorldManager
{
    public static int cellSize = 20;
    private static readonly ConcurrentDictionary<int2, Cell> grid = new();

    public static void AddGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int x = (int)SnapFloatToGrid(fPosition.x, cellSize);
        int y = (int)SnapFloatToGrid(fPosition.z, cellSize);

        int2 position = new(x, y);

        if ( !grid.ContainsKey(position) )
        {
            grid.TryAdd(position, new Cell(type, action));
            return;
        }

        grid[position].AddListener(type, action);
    }

    public static bool InvokeCellEvent ( CellEventType type, Vector3 fPosition, object input )
    {
        int x = (int)SnapFloatToGrid(fPosition.x, cellSize);
        int y = (int)SnapFloatToGrid(fPosition.z, cellSize);

        int2 position = new(x, y);

        if ( !grid.ContainsKey(position) )
            return false;

        grid[position].InvokeEvent(type, input);
        return true;
    }

    public static void RemoveGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int x = (int)SnapFloatToGrid(fPosition.x, cellSize);
        int y = (int)SnapFloatToGrid(fPosition.z, cellSize);

        int2 position = new(x, y);

        if ( !grid.ContainsKey(position) )
            return;

        grid[position].RemoveListener(type, action);
    }

    public static void ClearListeners ( int2 position )
    {
        grid[position].ClearEvents();
    }

    public static void ClearAllEvents ()
    {
        foreach ( var cell in grid.Values )
        {
            if ( cell == null )
                continue;

            cell.ClearEvents();
        }
        grid.Clear();
    }

    private static float SnapFloatToGrid ( float value, float cellSize )
    {
        return (float)Math.Round(value / cellSize) * cellSize;
    }
}

public enum CellEventType
{
    OnEntityDeath = 0,
}

public class Cell
{
    private readonly ConcurrentDictionary<CellEventType, Action<object>> events = new();

    public Cell ( CellEventType type, Action<object> action )
    {
        if ( events.ContainsKey(type) )
        {
            events[type] += action;
            return;
        }
        events.TryAdd(type, action);
    }

    public void AddListener ( CellEventType type, Action<object> action )
    {
        if ( events.ContainsKey(type) )
        {
            events[type] += action;
            return;
        }
        events.TryAdd(type, action);
    }

    public void RemoveListener ( CellEventType type, Action<object> action )
    {
        if ( !events.ContainsKey(type) )
            return;

        events[type] -= action;
    }

    public void ClearEvents ()
    {
        int amount = Enum.GetValues(typeof(CellEventType)).Length;

        for ( int i = amount - 1; i >= 0; i-- )
        {
            events[(CellEventType)i] = null;
        }

        events.Clear();
    }

    public void InvokeEvent ( CellEventType type, object input )
    {
        if ( !events.ContainsKey(type) )
            return;

        events[type]?.Invoke(input);
    }
}