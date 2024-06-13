using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class SoulCollectionPointManager : MonoBehaviour
{
    private ISoulCollectionArea[] collectors;

    private void Start ()
    {
        DelayedStart().Forget();
    }

    private async UniTaskVoid DelayedStart ()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2));
        collectors = FindObjectsByType<ISoulCollectionArea>(FindObjectsSortMode.None);
    }

    public bool CheckCollections ( Vector3 position )
    {
        for ( int i = 0; i < collectors.Length; ++i )
        {
            if ( collectors[i] == null || !collectors[i].gameObject.activeInHierarchy )
                continue;

            if ( collectors[i].CalculateOnDeath(position) )
            {
                Debug.Log("Collection Point in Range.");
                return true;
            }
        }
        return false;
    }
}

public abstract class ISoulCollectionArea : MonoBehaviour
{
    public abstract bool CalculateOnDeath ( Vector3 pos );
}