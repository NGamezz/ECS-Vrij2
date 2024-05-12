using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public class Yielders
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

    public class Utility : MonoBehaviour
    {
        public static void BatchPrint<T> ( params T[] items )
        {
            foreach ( var item in items )
            {
                Debug.Log(item);
            }
        }

        public static void SetArrayValuesToDefault<T> ( ref T[] array )
        {
            for ( int i = 0; i < array.Length; i++ )
            {
                array[i] = default;
            }
        }

        public static void QuickFor ( int start, int iterations, Action<int> actions )
        {
            Queue<(int index, Exception e)> exceptions = new();

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
                    exceptions.Enqueue((i, e));
                }
            }

            if ( exceptions.Count > 0 )
            {
                for ( int i = 0; i < exceptions.Count; i++ )
                {
                    var (index, e) = exceptions.Dequeue();

                    Debug.LogWarning($"Exception Thrown At Index : {index}");
                    Debug.LogException(e);
                }
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
    }

    public class Async
    {
        public static async Task WaitWhileAsync ( CancellationToken ct, Func<bool> condition, int pollDelay = 25 )
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                while ( condition() )
                {
                    await Task.Delay(pollDelay, ct).ConfigureAwait(true);
                }
            }
            catch ( TaskCanceledException )
            {
            }
        }
    }
}