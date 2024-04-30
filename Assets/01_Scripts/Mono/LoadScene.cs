using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadScene
{
    public static AsyncOperation LoadSceneByIndex(int sceneIndex, LoadSceneMode loadMode)
    {
        return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
    }

    public static AsyncOperation UnLoadScene (int sceneIndex)
    {
        return SceneManager.UnloadSceneAsync(sceneIndex);
    }
}