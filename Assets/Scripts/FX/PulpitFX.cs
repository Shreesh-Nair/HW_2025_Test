using UnityEngine;
using System.Collections;

public class PulpitFX : MonoBehaviour
{
    public float spawnDuration = 0.15f;
    public float destroyDuration = 0.15f;

    // Call this right after Instantiate
    public void PlaySpawnEffect()
    {
        StartCoroutine(ScaleRoutine(0f, 1f, spawnDuration));
    }

    // Call this before Destroy()
    public IEnumerator PlayDestroyEffect()
    {
        yield return ScaleRoutine(1f, 0f, destroyDuration);
    }

    IEnumerator ScaleRoutine(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            float s = Mathf.Lerp(from, to, t / duration);
            transform.localScale = new Vector3(s * 9f, s * 1f, s * 9f); // keep original proportions
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(to * 9f, to * 1f, to * 9f);
    }
}
