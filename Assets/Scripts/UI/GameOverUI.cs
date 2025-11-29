using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public Text finalScoreText; // assign in inspector
    public Text bestScoreText;  // assign in inspector

    void Start()
    {
        int last = PlayerPrefs.GetInt("LastScore", 0);
        int best = PlayerPrefs.GetInt("BestScore", 0);

        if (finalScoreText != null) finalScoreText.text = last.ToString();
        if (bestScoreText != null) bestScoreText.text = best.ToString();
    }
}
