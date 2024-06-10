using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadSceneByIndex : MonoBehaviour
{
    [SerializeField] private int sceneIndex;

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
        await CutsceneManager.SharedInstance.StartCutScene(sceneIndex);
    }

    private async UniTask StartDeLoad ()
    {
        await LoadScene.UnLoadScene(sceneIndex);

        gameObject.SetActive(false);
    }
}