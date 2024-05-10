using System;
using Unity.Mathematics;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Burst;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using System.Threading;

[BurstCompile]
public static class WorldManager
{
    private static int cellSize = 15;
    public static int CellSize { get => cellSize; }

    private static readonly ConcurrentDictionary<int2, Cell> grid = new();

    public static void AddGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int2 cellPos = CalculateCellPos(fPosition, cellSize);

        if ( !grid.ContainsKey(cellPos) )
        {
            grid.TryAdd(cellPos, new Cell(type, action));
            return;
        }

        AddListener(cellPos, action, type);
    }

    public static List<int2> AddGridListener ( float3 fPosition, float radius, Action<object> action, CellEventType type )
    {
        List<int2> cellPositions = new();
        CalculateCellPositions(fPosition, radius, cellSize, ref cellPositions);

        foreach ( var cellPos in cellPositions )
        {
            if ( !grid.ContainsKey(cellPos) )
            {
                grid.TryAdd(cellPos, new Cell(type, action));
                continue;
            }

            AddListener(cellPos, action, type);
        }

        return cellPositions;
    }

    public static bool InvokeCellEvent ( CellEventType type, Vector3 fPosition, object input )
    {
        int2 cellPos = CalculateCellPos(fPosition, cellSize);

        if ( !grid.ContainsKey(cellPos) )
            return false;

        InvokeEvent(cellPos, type, input);
        return true;
    }

    public static void RemoveGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int2 cellPos = CalculateCellPos(fPosition, cellSize);

        if ( !grid.ContainsKey(cellPos) )
            return;

        RemoveListener(cellPos, action, type);
    }

    public static void RemoveGridListener ( float3 fPosition, float radius, Action<object> action, CellEventType type )
    {
        List<int2> cellPositions = new();
        CalculateCellPositions(fPosition, radius, cellSize, ref cellPositions);

        foreach ( var cellPos in cellPositions )
        {
            if ( !grid.ContainsKey(cellPos) )
            {
                continue;
            }

            RemoveListener(cellPos, action, type);
        }
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

    //Probably not needed.
    private async static void AddListener ( int2 position, Action<object> action, CellEventType type )
    {
        while ( !_AddListener(position, action, type) )
        {
            await Task.Yield();
        }
    }

    private static bool _AddListener ( int2 position, Action<object> action, CellEventType type )
    {
        try
        {
            grid[position].AddListener(type, action);
        }
        catch ( Exception e)
        {
            UnityEngine.Debug.LogException(e);
            return false;
        }

        return true;
    }

    private async static void RemoveListener ( int2 position, Action<object> action, CellEventType type )
    {
        while ( !_RemoveListener(position, action, type) )
        {
            await Task.Yield();
        }
    }

    private static bool _RemoveListener ( int2 position, Action<object> action, CellEventType type )
    {
        try
        {
            grid[position].RemoveListener(type, action);
        }
        catch ( Exception )
        {
            return false;
        }
        return true;
    }

    private async static void InvokeEvent ( int2 position, CellEventType type, object input )
    {
        while ( !_RemoveListener(position, type, input) )
        {
            await Task.Yield();
        }
    }

    private static bool _RemoveListener ( int2 position, CellEventType type, object input )
    {
        try
        {
            grid[position].InvokeEvent(type, input);
        }
        catch ( Exception )
        {
            return false;
        }
        return true;
    }

    private static int2 CalculateCellPos ( float3 position, int cellSize )
    {
        int x = SnapFloatToGrid(position.x, cellSize);
        int y = SnapFloatToGrid(position.z, cellSize);
        int2 result = new(x, y);

        return result;
    }

    //Should probably be improved. Maybe with a job and the burst compiler.
    private static void CalculateCellPositions ( float3 position, float radius, int cellSize, ref List<int2> positions )
    {
        positions.Clear();

        var halfCelSize = cellSize / 2;
        var radiusSquared = (radius + halfCelSize) * (radius + halfCelSize);

        int posX = (int)math.round(position.x);
        int posY = (int)math.round(position.z);

        for ( int i = -3; i < 4; i++ )
        {
            for ( int t = -3; t < 4; t++ )
            {
                int x = SnapFloatToGrid(position.x + (i * cellSize), cellSize);
                int y = SnapFloatToGrid(position.z + (t * cellSize), cellSize);
                int2 result = new(x, y);

                if ( positions.Contains(result) )
                    continue;

                int dx = x - posX;
                int dy = y - posY;

                if ( dx * dx + dy * dy <= radiusSquared )
                {
                    positions.Add(result);
                }
            }
        }
    }

    //Work in Progress.
    private static async Task<List<int2>> CalculeCellPositionsJobbed ( float3 position, float radius, int cellSize)
    {
        NativeList<int2> jobList = new(Allocator.TempJob);

        CalculateIntersectingCellPositionsJob job = new()
        {
            position = position,
            radius = radius,
            cellSize = cellSize,
            positions = jobList
        };

        JobHandle handle = job.Schedule();

        await Utility.Async.WaitWhileAsync(CancellationToken.None, ()=> { return handle.IsCompleted == false; });

        List<int2> positions = new();

        for ( int i = 0; i < job.positions.Length; i++ )
        {
            positions.Add(job.positions[i]);
        }

        job.positions.Dispose();

        return positions;
    }

    private static int SnapFloatToGrid ( float value, int cellSize )
    {
        return (int)(math.round(value / cellSize) * cellSize);
    }
}

[BurstCompile]
public struct CalculateIntersectingCellPositionsJob : IJob
{
    [ReadOnly] public int cellSize;
    [ReadOnly] public float radius;
    [ReadOnly] public float3 position;

    public NativeList<int2> positions;

    [BurstCompile]
    public void Execute ()
    {
        var halfCelSize = cellSize / 2;
        var radiusSquared = (radius + halfCelSize) * (radius + halfCelSize);

        int posX = (int)math.round(position.x);
        int posY = (int)math.round(position.z);

        for ( int i = -3; i < 4; i++ )
        {
            for ( int t = -3; t < 4; t++ )
            {
                int x = SnapFloatToGrid(position.x + (i * cellSize), cellSize);
                int y = SnapFloatToGrid(position.z + (t * cellSize), cellSize);
                int2 result = new(x, y);

                if ( positions.Contains(result) )
                    continue;

                int dx = x - posX;
                int dy = y - posY;

                if ( dx * dx + dy * dy <= radiusSquared )
                {
                    positions.Add(result);
                }
            }
        }
    }

    [BurstCompile]
    private int SnapFloatToGrid ( float value, int cellSize )
    {
        return (int)(math.round(value / cellSize) * cellSize);
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