using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BackgroundQueue : MonoBehaviour
{
    public static BackgroundQueue Instance;

    private static readonly ConcurrentQueue<Action> actionQueue = new();
    private static readonly ConcurrentQueue<Action> priorityQueue = new();

    private int queueCount = 0;
    private int priorityQueueCount = 0;

    private Thread queueThread;
    private Thread priorityQueueThread;

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

        priorityQueueThread = new(OnPriorityUpdate)
        {
            IsBackground = true,
            Priority = System.Threading.ThreadPriority.AboveNormal
        };
        priorityQueueThread.Start();

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

    private void OnDisable ()
    {
        actionQueue.Clear();
        priorityQueue.Clear();

        applicationRunning = false;

        queueThread.Join();
        priorityQueueThread.Join();

        if ( Instance == this )
        {
            var instance = Instance;
            Instance = null;
            Destroy(instance);
        }
    }

    private void OnPriorityUpdate ()
    {
        while ( applicationRunning )
        {
            while ( priorityQueueCount > 0 )
            {
                if ( priorityQueue.TryDequeue(out var func) )
                {
                    func.Invoke();
                    Interlocked.Decrement(ref priorityQueueCount);
                }
            }
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
    public Task<T> AsyncEnqueueFunc<T> ( Func<T> action, bool priority = false )
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

        Enqueue(Action, priority);
        return src.Task;
    }

    /// <summary>
    /// Returns a Task, which will return true when the queued action succeeded, or false when it got an exception.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Task AsyncEnqueue ( Action action, bool priority = false )
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

        Enqueue(Action, priority);
        return src.Task;
    }

    public void Enqueue ( Action action, bool priority = false )
    {
        if ( priority )
        {
            Interlocked.Increment(ref priorityQueueCount);
            priorityQueue.Enqueue(action);
        }
        else
        {
            Interlocked.Increment(ref queueCount);
            actionQueue.Enqueue(action);
        }
    }
}