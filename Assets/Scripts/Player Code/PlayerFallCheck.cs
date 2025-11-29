using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFallCheck : MonoBehaviour
{
    public string gameOverSceneName = "GameOver"; 

    void Update()
    {
        if (transform.position.y <= -1.5f)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
