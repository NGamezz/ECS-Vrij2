using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public void Enqueue ( IEnumerator action )
    {
        lock ( actionQueue )
        {
            actionQueue.Enqueue(() =>
            {
                StartCoroutine(action);
            });
        }
    }

    private IEnumerator ActionWrapper ( Action action )
    {
        yield return null;
        action();
    }
}