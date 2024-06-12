using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoulCollectionPointManager : MonoBehaviour
{
    private ISoulCollectionArea[] collectors;

    private void Start ()
    {
        collectors = FindObjectsByType<ISoulCollectionArea>(FindObjectsSortMode.None);
    }

    public UniTask<bool> CheckCollections ( Vector3 position )
    {
        for ( int i = 0; i < collectors.Length; ++i )
        {
            if ( collectors[i].CalculateOnDeath(position) )
            {
                return UniTask.FromResult(true);
            }
        }
        return UniTask.FromResult(false);
    }
}

public class ISoulCollectionArea : MonoBehaviour
{
    public virtual bool CalculateOnDeath ( object entity ) { return false; }
}