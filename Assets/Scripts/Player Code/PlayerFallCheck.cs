using UnityEngine;

public class PlayerFallCheck : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y <= -1.5f)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
