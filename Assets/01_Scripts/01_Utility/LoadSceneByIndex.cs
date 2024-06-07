using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneByIndex : MonoBehaviour
{
    [SerializeField] private int sceneIndex;
    [SerializeField] private LoadSceneMode mode;

    public void Load ()
    {
        StartLoad().Forget();
    }

    public void LoadRandomScene ()
    {
        bool succes = GameManager.SharedInstance.TryGetRandomLevelIndex(out var index);

        if ( succes )
        {
            sceneIndex = index;
            Load();
        }
    }

    public void DeLoad ()
    {
        StartDeLoad().Forget();
    }

    private async UniTaskVoid StartLoad ()
    {
        await LoadScene.LoadSceneByIndex(sceneIndex, mode);

        gameObject.SetActive(false);
    }

    private async UniTask StartDeLoad ()
    {
        await LoadScene.UnLoadScene(sceneIndex);

        gameObject.SetActive(false);
    }
}