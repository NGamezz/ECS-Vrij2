using UnityEngine;

public enum GameState
{
    Pauzed = 0,
    Running = 1,
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private int[] levelIndexes;

    public static GameManager SharedInstance;

    private void Awake ()
    {
        if ( SharedInstance != null )
            Destroy(SharedInstance);

        SharedInstance = this;
    }

    private void Start ()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
    }

    private void OnDisable ()
    {
        WorldManager.ClearAllEvents();
        Destroy(SharedInstance);
    }

    public bool TryGetRandomLevelIndex ( out int index )
    {
        if ( levelIndexes == null )
        {
            index = 0;
            return false;
        }

        index = levelIndexes[UnityEngine.Random.Range(0, levelIndexes.Length)];
        return true;
    }
}