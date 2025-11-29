using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public string retrySceneName = "SampleScene"; // gameplay scene
    public string startMenuSceneName = "StartMenu";

    public void Retry()
    {
        SceneManager.LoadScene(retrySceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void QuitToStart()
    {
        SceneManager.LoadScene(startMenuSceneName);
    }
}
