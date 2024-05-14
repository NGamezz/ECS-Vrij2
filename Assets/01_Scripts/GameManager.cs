using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    private static readonly Queue<Action> actionQueue = new();

    private void Awake ()
    {
        if ( Instance != null )
            Destroy(Instance);

        Instance = this;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

    private void OnDisable ()
    {
        if ( actionQueue.Count > 0 )
        {
            actionQueue.Clear();
        }

        WorldManager.ClearAllEvents();
        
        if ( Instance == this )
        {
            var instance = Instance;
            Instance = null;
            Destroy(instance);
        }
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