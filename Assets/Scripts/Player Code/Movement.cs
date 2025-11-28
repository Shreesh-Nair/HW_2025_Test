using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData { public float speed; }

[System.Serializable]
public class RootData { public PlayerData player_data; }

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    RootData data;
    float speed = 3f;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        string path = Path.Combine(Application.dataPath, "Scripts/JSON Files/doofus_diary.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<RootData>(json);
            if (data != null && data.player_data != null) speed = data.player_data.speed;
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        Vector3 horizontalVelocity = input.normalized * speed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }
}
