using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Vector3 = UnityEngine.Vector3;

public static class WorldManager
{
    public static int2 cellSize = new(30, 30);
    private static Dictionary<int2, Cell> grid = new();

    public static void AddGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int x = (int)SnapFloatToGrid(fPosition.x, cellSize.x);
        int y = (int)SnapFloatToGrid(fPosition.z, cellSize.y);

        int2 position = new(x, y);

        if ( !grid.ContainsKey(position) )
        {
            var newCell = new Cell();
            newCell.AddListener(type, action);
            grid.Add(position, newCell);
            return;
        }

        var cell = grid[position];
        cell.AddListener(type, action);
    }

    public static void InvokeCellEvent ( CellEventType type, Vector3 fPosition, object input )
    {
        int x = (int)SnapFloatToGrid(fPosition.x, cellSize.x);
        int y = (int)SnapFloatToGrid(fPosition.z, cellSize.y);

        int2 position = new(x, y);

        if ( !grid.ContainsKey(position) )
            return;

        var cell = grid[position];
        cell.InvokeEvent(type, input);
    }

    public static void RemoveGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int x = (int)SnapFloatToGrid(fPosition.x, cellSize.x);
        int y = (int)SnapFloatToGrid(fPosition.z, cellSize.y);

        int2 position = new(x, y);

        if ( !grid.ContainsKey(position) )
            return;

        var cell = grid[position];
        cell.RemoveListener(type, action);
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

    private static float SnapFloatToGrid ( float value, float interval )
    {
        float rem = value % interval;

        float halfInterval = interval / 2f;

        if ( rem > halfInterval )
        {
            float remDif = interval - rem;
            value += remDif;
        }
        else
        {
            value -= rem;
        }

        return value;
    }
}

public enum CellEventType
{
    OnEntityDeath = 0,
}

public class Cell
{
    private Dictionary<CellEventType, Action<object>> events = new();

    public void AddListener ( CellEventType type, Action<object> action )
    {
        if ( events.ContainsKey(type) )
        {
            events[type] += action;
            return;
        }
        events.Add(type, action);
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