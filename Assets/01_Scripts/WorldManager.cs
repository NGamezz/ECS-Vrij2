using System;
using Unity.Mathematics;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

[BurstCompile]
public static class WorldManager
{
    private static readonly int cellSize = 5;
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

    public static UniTask<HashSet<int2>> AddGridListener ( float3 position, float radius, Action<object> action, CellEventType type )
    {
        var cellPositions = CalculateCellPositions(position, radius, cellSize);

        foreach ( var cellPos in cellPositions )
        {
            if ( !grid.ContainsKey(cellPos) )
            {
                grid.TryAdd(cellPos, new Cell(type, action));
                continue;
            }

            AddListener(cellPos, action, type);
        }

        return UniTask.FromResult(cellPositions);
    }

    //Work in Progress.
    public static bool InvokeCellEvent ( CellEventType type, float3 position, object input, float radius )
    {
        var positions = CalculateCellPositions(position, radius, cellSize);

        foreach ( var pos in positions )
        {
            if ( !grid.ContainsKey(pos) || grid[pos].IsEventEmpty(type) )
            {
                continue;
            }

            InvokeEvent(pos, type, input);
            return true;
        }
        return false;
    }

    public static bool InvokeCellEvent ( CellEventType type, Vector3 fPosition, object input )
    {
        int2 cellPos = CalculateCellPos(fPosition, cellSize);

        if ( !grid.ContainsKey(cellPos) || grid[cellPos].IsEventEmpty(type) )
            return false;

        InvokeEvent(cellPos, type, input);
        return true;
    }

    private static void InvokeEvent ( int2 cellPos, CellEventType type, object input )
    {
        grid[cellPos].InvokeEvent(type, input);
    }

    public static void RemoveGridListener ( Vector3 fPosition, Action<object> action, CellEventType type )
    {
        int2 cellPos = CalculateCellPos(fPosition, cellSize);

        if ( !grid.ContainsKey(cellPos) )
            return;

        RemoveListener(cellPos, action, type);
    }

    //Improve the grid listener removal.
    public static void RemoveGridListener ( float3 fPosition, float radius, Action<object> action, CellEventType type )
    {
        var cellPositions = CalculateCellPositions(fPosition, radius, cellSize);

        foreach ( var cellPos in cellPositions )
        {
            if ( !grid.ContainsKey(cellPos) )
            {
                continue;
            }

            RemoveListener(cellPos, action, type);
        }
    }

    public static void RemoveGridListeners ( IEnumerable<int2> collection, Action<object> action, CellEventType type )
    {
        foreach ( var pos in collection )
        {
            if ( !grid.ContainsKey(pos) )
                continue;

            RemoveListener(pos, action, type);
        }
    }

    private static void RemoveListener ( int2 cellPos, Action<object> action, CellEventType type )
    {
        grid[cellPos].RemoveListener(type, action);
    }

    private static void AddListener ( int2 cellPos, Action<object> action, CellEventType type )
    {
        grid[cellPos].AddListener(type, action);
    }

    public static void ClearListeners ( int2 position )
    {
        grid[position].ClearEvents();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    private static int2 CalculateCellPos ( float3 position, int cellSize )
    {
        int x = SnapFloatToGrid(position.x, cellSize);
        int y = SnapFloatToGrid(position.z, cellSize);
        int2 result = new(x, y);

        return result;
    }

    //Should probably be improved. Maybe with a job and the burst compiler.
    private static HashSet<int2> CalculateCellPositions ( float3 position, float radius, int cellSize )
    {
        HashSet<int2> positions = new();
        positions.Clear();

        const float divisor = 1;
        var cellMargin = cellSize * divisor;
        var radiusSquared = (radius + cellMargin) * (radius + cellMargin);

        int posX = (int)math.round(position.x);
        int posY = (int)math.round(position.z);

        for ( int i = -3; i < 4; i++ )
        {
            for ( int t = -3; t < 4; t++ )
            {
                int x = SnapFloatToGrid(position.x + (i * cellSize), cellSize);
                int y = SnapFloatToGrid(position.z + (t * cellSize), cellSize);
                int2 result = new(x, y);

                int dx = x - posX;
                int dy = y - posY;

                if ( dx * dx + dy * dy <= radiusSquared )
                {
                    positions.Add(result);
                }
            }
        }
        return positions;
    }

    //Work in progress.
    private static async UniTask<List<int2>> CalculatePositionsParallelJobbed ( float3 position, float radius, int cellSize )
    {
        NativeArray<PositionHolder> jobList = new(49, Allocator.Persistent);
        List<int2> positions = new();

        try
        {
            const int indexOffSet = -3;
            const int iterations = 4;

            CalculateIntersectinCellPositionsParallelJob job = new()
            {
                indexOffset = indexOffSet,
                position = position,
                radius = radius,
                cellSize = cellSize,
                positions = jobList
            };

            JobHandle handle = job.Schedule(-indexOffSet + iterations, 64);

            await UniTask.WaitWhile(() => !handle.IsCompleted);
            handle.Complete();

            for ( int i = 0; i < job.positions.Length; i++ )
            {
                var pos = job.positions[i];

                if ( pos.PositionSet == false )
                    continue;

                if ( positions.Contains(pos.Position) )
                    continue;

                positions.Add(pos.Position);
            }
        }
        catch ( Exception e )
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            jobList.Dispose();
        }

        return positions;
    }

    //Work in Progress.
    private static async UniTask<HashSet<int2>> CalculeCellPositionsJobbed ( float3 position, float radius, int cellSize )
    {
        NativeList<int2> jobList = new(Allocator.Persistent);

        CalculateIntersectingCellPositionsJob job = new()
        {
            position = position,
            radius = radius,
            cellSize = cellSize,
            positions = jobList
        };

        JobHandle handle = job.Schedule();

        await UniTask.WaitWhile(() => { return handle.IsCompleted == false; });
        handle.Complete();

        HashSet<int2> positions = new();

        for ( int i = 0; i < job.positions.Length; i++ )
        {
            positions.Add(job.positions[i]);
        }

        job.positions.Dispose();

        return positions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SnapFloatToGrid ( float value, int cellSize )
    {
        return (int)(math.round(value / cellSize) * cellSize);
    }
}

public struct PositionHolder
{
    public bool PositionSet;
    public int2 Position;

    public PositionHolder ( int2 pos, bool set )
    {
        Position = pos;
        PositionSet = set;
    }
}

[BurstCompile]
public struct CalculateIntersectinCellPositionsParallelJob : IJobParallelFor
{
    [ReadOnly] public int indexOffset;

    [ReadOnly] public int cellSize;
    [ReadOnly] public float radius;
    [ReadOnly] public float3 position;

    [NativeDisableParallelForRestriction]
    [WriteOnly] public NativeArray<PositionHolder> positions;

    [BurstCompile]
    public void Execute ( int index )
    {
        index += indexOffset;

        var halfCelSize = cellSize / 2;
        var radiusSquared = (radius + halfCelSize) * (radius + halfCelSize);

        int posX = (int)math.round(position.x);
        int posY = (int)math.round(position.z);

        for ( int t = -3; t < 4; t++ )
        {
            int x = SnapFloatToGrid(position.x + (index * cellSize), cellSize);
            int y = SnapFloatToGrid(position.z + (t * cellSize), cellSize);
            int2 result = new(x, y);

            int dx = x - posX;
            int dy = y - posY;

            if ( dx * dx + dy * dy <= radiusSquared )
            {
                positions[((index - indexOffset) * 7) + (t + 3)] = new(result, true);
            }
        }
    }

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int SnapFloatToGrid ( float value, int cellSize )
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

    [BurstCompile, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int SnapFloatToGrid ( float value, int cellSize )
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

    public bool IsEventEmpty ( CellEventType type )
    {
        return events[type] == null || events[type].GetInvocationList() == null;
    }

    public void InvokeEvent ( CellEventType type, object input )
    {
        if ( !events.ContainsKey(type) )
            return;

        events[type]?.Invoke(input);
    }
}