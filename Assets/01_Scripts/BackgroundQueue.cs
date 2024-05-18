using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BackgroundQueue : MonoBehaviour
{
    public static BackgroundQueue Instance;

    private static readonly Queue<Action> actionQueue = new();
    private static readonly Queue<Action> priorityQueue = new();

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
        applicationRunning = false;

        priorityQueueThread.Abort();

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
            lock ( priorityQueue )
            {
                while ( priorityQueue.Count > 0 )
                {
                    priorityQueue.Dequeue().Invoke();
                }
            }
        }
    }

    private void OnUpdate ()
    {
        while ( applicationRunning )
        {
            lock ( actionQueue )
            {
                while ( actionQueue.Count > 0 )
                {
                    actionQueue.Dequeue().Invoke();
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
            lock ( priorityQueue )
            {
                priorityQueue.Enqueue(action);
            }
        }
        else
        {
            lock ( actionQueue )
            {
                actionQueue.Enqueue(action);
            }
        }
    }
}