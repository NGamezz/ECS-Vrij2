using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static readonly ConcurrentQueue<Action> MainThreadActionQueue = new();
    private Coroutine currentDequeueRoutine = null;

    private void OnDisable ()
    {
        if ( MainThreadActionQueue.Count > 0 )
        {
            MainThreadActionQueue.Clear();
        }

        WorldManager.ClearAllEvents();
    }

    private void FixedUpdate ()
    {
        if(currentDequeueRoutine == null && MainThreadActionQueue.Count > 0)
        {
            currentDequeueRoutine = StartCoroutine(WorkOnActionQueue());
        }
    }

    private IEnumerator WorkOnActionQueue ()
    {
        while ( MainThreadActionQueue.Count > 0 )
        {
            MainThreadActionQueue.TryDequeue(out var action);
            Debug.Log("Perform Queue Item.");
            action?.Invoke();
            yield return null;
        }

        currentDequeueRoutine = null;
    }
}