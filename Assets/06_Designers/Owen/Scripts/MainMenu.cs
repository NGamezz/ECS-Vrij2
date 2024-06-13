using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnPlayButton()
    {
        StartCoroutine(DelayBeforeAction());
    }

    public void OnCreditButton()
    {
        SceneManager.LoadScene("CreditScene");
    }

    public void OnSettingsButton()
    {
        //SceneManager.LoadScene("Settings");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }


    private IEnumerator DelayBeforeAction()
    {
        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene("CreditScene");
    }
}
