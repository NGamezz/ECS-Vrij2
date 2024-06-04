using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BackgroundQueue : MonoBehaviour
{
    public static BackgroundQueue Instance;

    private static readonly ConcurrentQueue<Action> actionQueue = new();

    private int queueCount = 0;

    private Thread queueThread;

    private bool applicationRunning = false;

    private void Awake ()
    {
        if ( Instance != null )
            Destroy(Instance);

        Instance = this;
        applicationRunning = true;

        queueThread = new(OnUpdate)
        {
            IsBackground = true
        };
        queueThread.Start();
    }

    private void OnDisable ()
    {
        actionQueue.Clear();

        applicationRunning = false;
        queueThread.Join();

        if ( Instance == this )
        {
            var instance = Instance;
            Instance = null;
            Destroy(instance);
        }
    }

    private void OnUpdate ()
    {
        while ( applicationRunning )
        {
            while ( queueCount > 0 )
            {
                if ( actionQueue.TryDequeue(out var func) )
                {
                    func.Invoke();
                    Interlocked.Decrement(ref queueCount);
                }
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
        Interlocked.Increment(ref queueCount);
        actionQueue.Enqueue(action);
    }
}