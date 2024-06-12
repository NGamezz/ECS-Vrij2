using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Pauzed = 0,
    Running = 1,
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private int[] levelIndexes;

    [SerializeField] private GameObject gameOverScreen;

    [SerializeField] private float returnToMainMenuDelay = 2;

    public static GameManager SharedInstance;

    private void Awake ()
    {
        if ( SharedInstance != null )
            Destroy(SharedInstance);

        DontDestroyOnLoad(gameObject);

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

    private void OnEnable ()
    {
        EventManager.AddListener(EventType.OnSceneChange, OnSceneChange, this);
        EventManager.AddListener(EventType.GameOver, OnGameOver, this);
    }

    private void OnGameOver ()
    {
        GameOver().Forget();
    }

    private async UniTaskVoid GameOver ()
    {
        gameOverScreen.SetActive(true);

        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Pauzed);

        await UniTask.Delay(TimeSpan.FromSeconds(returnToMainMenuDelay));

        gameOverScreen.SetActive(false);

        EventManager.InvokeEvent(EventType.PostGameOverWait);
        await SceneManager.LoadSceneAsync(0);

        Destroy(SharedInstance);
        Destroy(gameObject);
    }

    private void OnSceneChange ()
    {
        WorldManager.ClearAllEvents();
    }

    private void OnDisable ()
    {
        EventManager.ClearListeners();
        EventManagerGeneric<bool>.ClearListeners();
        WorldManager.ClearAllEvents();
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