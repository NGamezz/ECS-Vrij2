using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static readonly Queue<Action> actionQueue = new Queue<Action>();

    private void Awake ()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
        else if ( Instance != null && Instance != this )
        {
            Destroy(this);
        }
    }

    private void OnDisable ()
    {
        if ( Instance == this )
        {
            Instance = null;
        }

        if ( actionQueue.Count > 0 )
        {
            actionQueue.Clear();
        }

        WorldManager.ClearAllEvents();
    }

    private void Update ()
    {
        lock ( actionQueue )
        {
            while ( actionQueue.Count > 0 )
            {
                actionQueue.Dequeue().Invoke();
            }
        }
    }

    //Testing Purposes.
    private IEnumerator HandleJobData ()
    {
        NativeArray<int> result = new(10000, Allocator.Persistent);
        TestJob testJob = new()
        {
            results = result,
        };

        JobHandle jobHandle = testJob.Schedule(10000, 64);

        yield return new WaitUntil(() => { return jobHandle.IsCompleted; });

        jobHandle.Complete();

        Debug.Log(testJob.results.Length);

        testJob.results.Dispose();
    }

    /// <summary>
    /// Returns a Task which will hold the result of the func, can be used to request an object from the main thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public Task<T> AsyncEnqueueFunc<T> ( Func<T> action )
    {
        var src = new TaskCompletionSource<T>();

        void Action ()
        {
            try
            {
                src.SetResult(action());
            }
            catch ( Exception e )
            {
                src.TrySetException(e);
            }
        }

        Enqueue(ActionWrapper(Action));
        return src.Task;
    }

    /// <summary>
    /// Returns a Task, which will return true when the queued action succeeded, or false when it got an exception.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Task AsyncEnqueue ( Action action )
    {
        var src = new TaskCompletionSource<bool>();

        void Action ()
        {
            try
            {
                action();
                src.TrySetResult(true);
            }
            catch ( Exception e )
            {
                src.TrySetException(e);
            }
        }

        Enqueue(ActionWrapper(Action));
        return src.Task;
    }

    public void Enqueue ( Action action )
    {
        Enqueue(ActionWrapper(action));
    }

    public async void Enqueue ( IEnumerator action )
    {
        while ( !_Enqueue(action) )
        {
            await Task.Yield();
        }
    }

    private bool _Enqueue ( IEnumerator action )
    {
        try
        {
            lock ( actionQueue )
            {
                actionQueue.Enqueue(() =>
                {
                    StartCoroutine(action);
                });
            }
        }
        catch ( Exception )
        {
            return false;
        }
        return true;
    }

    private IEnumerator ActionWrapper ( Action action )
    {
        yield return null;
        action();
    }
}

[BurstCompile]
public struct TestJob : IJobParallelFor
{
    public NativeArray<int> results;

    public void Execute ( int index )
    {
        int value = 0;
        for ( int x = 0; x < 10000; x++ )
        {
            int result = x * index;
            int newResult = result * x;
            int newResultB = newResult * x;
            value = newResult * newResultB;
        }
        results[index] = value;
    }
}