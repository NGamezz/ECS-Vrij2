using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private int startSceneIndex = 1;

    public void OnPlayButton()
    {
        SceneManager.LoadScene(startSceneIndex);
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void OnCreditButton()
    {
        SceneManager.LoadScene("CreditScene");
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void OnSettingsButton()
    {
        //SceneManager.LoadScene("Settings");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}