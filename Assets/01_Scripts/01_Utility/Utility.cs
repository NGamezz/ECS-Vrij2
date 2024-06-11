using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace Utility
{
    public static class Yielders
    {
        private static readonly Dictionary<float, WaitForSeconds> timeInterval = new();

        private static readonly WaitForEndOfFrame endOfFrame = new();

        public static WaitForEndOfFrame EndOfFrame
        {
            get { return endOfFrame; }
        }

        private static readonly WaitForFixedUpdate fixedUpdate = new();
        public static WaitForFixedUpdate FixedUpdate
        {
            get { return fixedUpdate; }
        }

        public static WaitForSeconds Get ( float seconds )
        {
            if ( !timeInterval.ContainsKey(seconds) )
                timeInterval.Add(seconds, new WaitForSeconds(seconds));
            return timeInterval[seconds];
        }
    }

    public static class Utility
    {
        public static void SetArrayValuesToDefault<T> ( ref T[] array )
        {
            for ( int i = 0; i < array.Length; i++ )
            {
                array[i] = default;
            }
        }

        public static T GetOrAddComponent<T> ( GameObject gameObject ) where T : Component
        {
            if ( !gameObject.TryGetComponent<T>(out var component) )
            {
                return gameObject.AddComponent<T>();
            }

            return component;
        }

        public static T GetOrAddComponentI<T> ( this GameObject gameObject ) where T : Component
        {
            if ( !gameObject.TryGetComponent<T>(out var component) )
            {
                return gameObject.AddComponent<T>();
            }

            return component;
        }

        public static void QuickFor ( int start, int iterations, Action<int> actions )
        {
            Queue<Exception> exceptions = new();

            if ( start > iterations )
                return;

            for ( int i = start; i < iterations; i++ )
            {
                try
                {
                    actions(i);
                }
                catch ( Exception e )
                {
                    exceptions.Enqueue(e);
                }
            }

            if ( exceptions.Count > 0 )
            {
                for ( int i = 0; i < exceptions.Count; i++ )
                {
                    var e = exceptions.Dequeue();
                    LogException(e);
                }
            }
        }

        public static IEnumerable<float> Timer ( float duration )
        {
            float t = 0f;
            while ( t < duration )
            {
                yield return t;
            }
        }

        public static void MultiAddElementToList<T> ( ref List<T> list, params T[] elements )
        {
            list.AddRange(elements);
        }

        public static void MultiAddNonDuplicateElementsToList<T> ( ref List<T> list, params T[] elements )
        {
            list.AddRange(elements.Distinct());
        }

        [Conditional("ENABLE_LOGS")]
        public static void Log ( object msg )
        {
            UnityEngine.Debug.Log(msg);
        }

        [Conditional("ENABLE_LOGS")]
        public static void LogException ( Exception e )
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    //Might make a custom one, using structs and a linkded list design.
    [DisallowMultipleComponent]
    public class AsyncGameObjectDeactivationTrigger : MonoBehaviour
    {
        private bool isDeactivated;
        private Action delegates;

        private void OnDisable ()
        {
            isDeactivated = true;
            delegates?.Invoke();
        }

        public void Subscribe ( Action action )
        {
            if ( isDeactivated )
                return;

            delegates += action;
        }
    }

    public static class Async
    {
        public static async UniTask ChangeValueAfterSeconds<T> ( float duration, Action<T> callBack, T endState, CancellationToken token = default )
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            callBack?.Invoke(endState);
        }

        public static AsyncGameObjectDeactivationTrigger GetAsyncGameObjectDeactivationTrigger ( this GameObject component )
        {
            return Utility.GetOrAddComponent<AsyncGameObjectDeactivationTrigger>(component);
        }

        public static AsyncGameObjectDeactivationTrigger GetAsyncGameObjectDeactivationTrigger ( this Component component )
        {
            return component.gameObject.GetAsyncGameObjectDeactivationTrigger();
        }

        public static async UniTaskVoid StreamedTimerAsync ( Action<float> stream, Action finishCallback, float duration )
        {
            float t = 0;
            await foreach ( var _ in UniTaskAsyncEnumerable.EveryUpdate() )
            {
                t += Time.deltaTime;
                stream?.Invoke(t);

                if ( t >= duration )
                {
                    break;
                }
            }
            finishCallback?.Invoke();
        }

        public static async UniTask WaitWhileAsync ( CancellationToken ct, Func<bool> condition, int pollDelay = 25 )
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                while ( condition() )
                {
                    await UniTask.Delay(pollDelay, cancellationToken: ct);
                }
            }
            catch ( TaskCanceledException )
            {
            }
        }
    }
}