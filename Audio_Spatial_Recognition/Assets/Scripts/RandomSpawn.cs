using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSpawn : MonoBehaviour
{
    [Tooltip("AudioListener Object which defines the center point of the spawn circle.")]
    public AudioListener listener;
    [Tooltip("Radius to spawn the GameObject around the AudioListener.")]
    public float radius = 5f;

    private Vector3 center;

    // Start is called before the first frame update
    void Start()
    {
        // Set center variable to the AudioListener position
        center = listener.transform.position;
        // Set AudioSource position to a random point in a circle around the center.
        Spawn();
    }

    // Calculates random position on the circle
    Vector3 RandomCircle(Vector3 center, float radius)
    {
        float angle = Random.value * 360;
        Vector3 pos = new Vector3(0, 0, 0);
        pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.y = center.y;
        return pos;
    }

    // Spawns the GameObject in a random position in a circle around the AudioListener GameObject and rotates the GameObject to look at the Listener
    public void Spawn()
    {
        Vector3 pos = RandomCircle(center, radius);
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, center - pos);
        transform.rotation = rot;
        transform.position = pos;
    }
}
