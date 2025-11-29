using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public Text scoreNumberText; // assign ScoreNumber Text

    void Start()
    {
        if (scoreNumberText == null)
            Debug.LogWarning("ScoreUI: scoreNumberText not assigned in inspector.");
    }

    void Update()
    {
        if (PulpitSpawner.Instance != null && scoreNumberText != null)
        {
            scoreNumberText.text = PulpitSpawner.Instance.score.ToString();
        }
    }
}
