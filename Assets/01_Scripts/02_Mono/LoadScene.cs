using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public static class LoadScene
{
    public static UniTask LoadSceneByIndex(int sceneIndex, LoadSceneMode loadMode)
    {
        return SceneManager.LoadSceneAsync(sceneIndex, loadMode).ToUniTask();
    }

    public static UniTask UnLoadScene (int sceneIndex)
    {
        return SceneManager.UnloadSceneAsync(sceneIndex).ToUniTask();
    }
}