using UnityEngine;

public class SoulCollectionPointManager : MonoBehaviour
{
    private ISoulCollectionArea[] collectors;

    private void Start ()
    {
        collectors = FindObjectsByType<ISoulCollectionArea>(FindObjectsSortMode.None);
    }

    public bool CheckCollections ( Vector3 position )
    {
        for ( int i = 0; i < collectors.Length; ++i )
        {
            if ( !collectors[i].gameObject.activeInHierarchy )
                continue;

            if ( collectors[i].CalculateOnDeath(position) )
            {
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