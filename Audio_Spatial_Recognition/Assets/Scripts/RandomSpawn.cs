using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSpawn : MonoBehaviour
{

    public AudioListener listener;
    public float radius = 5f;
    private Vector3 center;

    // Start is called before the first frame update
    void Start()
    {
        center = listener.transform.position;
        Vector3 pos = RandomCircle(center, radius);
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, center - pos);
        transform.rotation = rot;
        transform.position = pos;
        //transform.position = new Vector3(Random.Range(0, 100f), Random.Range(0, 100f), Random.Range(0, 100f));
    }

    Vector3 RandomCircle(Vector3 center, float radius)
    {
        float angle = Random.value * 360;
        Vector3 pos = new Vector3(0, 0, 0);
        pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.y = center.y;
        return pos;
    }
}
