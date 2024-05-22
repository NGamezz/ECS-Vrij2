using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void OnCreditButton()
    {
        //SceneManager.LoadScene("Credits");
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
