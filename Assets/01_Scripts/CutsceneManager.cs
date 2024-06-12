using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private string[] Dialogues;

    [SerializeField] private float cutSceneDuration = 5.0f;

    [SerializeField] private float textDelay = 0.01f;

    [SerializeField] private GameObject cutSceneObject;
    [SerializeField] private TMP_Text cutSceneText;

    public static CutsceneManager SharedInstance;

    private void Awake ()
    {
        if ( SharedInstance != null )
        { Destroy(SharedInstance); }

        DontDestroyOnLoad(gameObject);
        SharedInstance = this;
    }

    private void OnEnable ()
    {
        EventManager.AddListener(EventType.GameOver, OnGameOver, this);
    }

    private void OnGameOver ()
    {
        Destroy(SharedInstance);
        Destroy(gameObject);
    }

    private void OnDisable ()
    {
        Destroy(SharedInstance);
    }

    private async UniTask SetDialogue ()
    {
        string text = Dialogues[UnityEngine.Random.Range(0, Dialogues.Length)];
        cutSceneText.text = "";

        foreach ( char c in text )
        {
            cutSceneText.text += c;
            await UniTask.Delay(TimeSpan.FromSeconds(textDelay));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
    }

    public async UniTask StartCutScene ( int sceneIndex )
    {
        Stopwatch sw = Stopwatch.StartNew();

        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Pauzed);

        cutSceneObject.SetActive(true);
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        var playerMesh = FindAnyObjectByType<PlayerMesh>().GetTransform();

        await SetDialogue();

        await LoadScene.LoadSceneByIndex(sceneIndex, LoadSceneMode.Additive);

        var spawnPoint = FindAnyObjectByType<StartSpawnLocation>();

        UnityEngine.Debug.Log(spawnPoint);

        playerMesh.position = spawnPoint.transform.position;

        Destroy(spawnPoint.gameObject);

        await LoadScene.UnLoadScene(currentSceneIndex);

        EventManager.InvokeEvent(EventType.OnSceneChange);

        var elapsed = sw.Elapsed.TotalSeconds;

        if ( elapsed < cutSceneDuration )
        {
            await UniTask.Delay(TimeSpan.FromSeconds(cutSceneDuration - elapsed));
        }

        cutSceneObject.SetActive(false);
        EventManagerGeneric<GameState>.InvokeEvent(EventType.OnGameStateChange, GameState.Running);
    }
}