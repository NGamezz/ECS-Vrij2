using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void onPlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void onCreditButton()
    {
        SceneManager.LoadScene("Credits");
    }

    public void onSettingsButton()
    {
        SceneManager.LoadScene("Settings");
    }

    public void onQuitButton()
    {
        Application.Quit();
    }
}
