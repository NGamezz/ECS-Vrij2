using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneByIndex : MonoBehaviour
{
    [SerializeField] private int sceneIndex;
    [SerializeField] private LoadSceneMode mode;

    private Task currentAction;

    public void Load ()
    {
        if ( currentAction != null )
            return;

        currentAction = StartLoad();
    }

    public void DeLoad ()
    {
        if ( currentAction != null )
            return;

        currentAction = StartDeLoad();
    }

    private async Task StartLoad ()
    {
        await LoadScene.LoadSceneByIndex(sceneIndex, mode);

        gameObject.SetActive(false);
        currentAction = null;
    }

    private async Task StartDeLoad ()
    {
        await LoadScene.UnLoadScene(sceneIndex);

        gameObject.SetActive(false);
        currentAction = null;
    }
}