using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFallCheck : MonoBehaviour
{
    public string gameOverSceneName = "GameOver";

    void Update()
    {
        if (transform.position.y <= -1.5f)
        {
            int finalScore = 0;
            if (PulpitSpawner.Instance != null)
                finalScore = PulpitSpawner.Instance.score;

            PlayerPrefs.SetInt("LastScore", finalScore);

            int best = PlayerPrefs.GetInt("BestScore", 0);
            if (finalScore > best)
            {
                PlayerPrefs.SetInt("BestScore", finalScore);
            }

            PlayerPrefs.Save();

            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
