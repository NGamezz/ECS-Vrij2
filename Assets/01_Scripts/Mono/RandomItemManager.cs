using UnityEngine;

public class RandomItemManager : MonoBehaviour
{
    [SerializeField] private GameObject[] randomObjectsPrefabs;

    public static RandomItemManager Instance { get; private set; }

    public GameObject RequestRandomObject()
    {
        if( randomObjectsPrefabs.Length < 1)
        {
            return null;
        }

        return randomObjectsPrefabs[Random.Range(0, randomObjectsPrefabs.Length)];
    }

    private void Awake ()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
    }
}