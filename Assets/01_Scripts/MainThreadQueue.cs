using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MainThreadQueue : MonoBehaviour
{
    public static MainThreadQueue Instance { get; set; }

    private static readonly ConcurrentQueue<Action> actionQueue = new();

    private int queueCount = 0;

    private void Awake ()
    {
        if ( Instance != null )
            Destroy(Instance);

        Instance = this;
    }

    private void OnDisable ()
    {
        if ( queueCount > 0 )
        {
            actionQueue.Clear();
        }

        if ( Instance == this )
        {
            var instance = Instance;
            Instance = null;
            Destroy(instance);
        }
    }

    private void Update ()
    {
        while ( queueCount > 0 )
        {
            if ( actionQueue.TryDequeue(out var action) )
            {
                action.Invoke();
                Interlocked.Decrement(ref queueCount);
            }
        }
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

        Enqueue(Action);
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

        Enqueue(Action);
        return src.Task;
    }

    public void Enqueue ( Action action )
    {
        actionQueue.Enqueue(action);
        Interlocked.Increment(ref queueCount);
    }
}