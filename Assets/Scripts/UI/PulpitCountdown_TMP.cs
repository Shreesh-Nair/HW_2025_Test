using UnityEngine;
using TMPro;
using System.Collections;

public class PulpitCountdown_TMP : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // assign in prefab inspector
    public bool faceCamera = false;       // keep false to keep text fixed
    float life = 5f;

    void Start()
    {
        if (countdownText == null) countdownText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(float lifetime)
    {
        life = Mathf.Max(0.0001f, lifetime);
        StopAllCoroutines();
        StartCoroutine(DoCountdown());
    }

    IEnumerator DoCountdown()
    {
        float remaining = life;
        while (remaining > 0f)
        {
            if (countdownText != null) countdownText.text = remaining.ToString("0.0");
            remaining -= Time.deltaTime;
            yield return null;
        }
        if (countdownText != null) countdownText.text = "0.0";
    }
}
