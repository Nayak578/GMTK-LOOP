using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;

    public void OnLevelsButton()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
    }

    public void OnSettingsButton()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnSettingsBack()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public void ExitGame()
    {
        Debug.Log("Exiting game...");

        Application.Quit();  // Quits the game in a build

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // Stops play mode in editor
#endif
    }


    public void LoadLevel0()
    {
        SceneManager.LoadScene("Level0");
    }

}
