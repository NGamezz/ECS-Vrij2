using UnityEngine;

public enum RandomItemType
{
    Default = 0,
    CollectableItems = 1,
}

public class RandomItemManager : MonoBehaviour
{
    [SerializeField] private GameObject[] randomObjectsPrefabs;

    [SerializeField] private GameObject[] randomCollectableItems;

    public static RandomItemManager Instance { get; private set; }

    public GameObject RequestRandomObject ( RandomItemType type )
    {
        switch ( type )
        {
            case RandomItemType.CollectableItems:
                {
                    if ( randomCollectableItems.Length < 1 )
                    {
                        return null;
                    }

                    return randomCollectableItems[Random.Range(0, randomCollectableItems.Length)];
                }
            case RandomItemType.Default:
                {
                    if ( randomObjectsPrefabs.Length < 1 )
                    {
                        return null;
                    }

                    return randomObjectsPrefabs[Random.Range(0, randomObjectsPrefabs.Length)];
                }
            default:
                {
                    return null;
                }
        }
    }

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
}