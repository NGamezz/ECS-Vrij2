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
        var action = LoadScene.LoadSceneByIndex(sceneIndex, mode);

        await action;

        gameObject.SetActive(false);
        currentAction = null;
    }

    private async Task StartDeLoad ()
    {
        var action = LoadScene.UnLoadScene(sceneIndex);

        await action;

        gameObject.SetActive(false);
        currentAction = null;
    }
}