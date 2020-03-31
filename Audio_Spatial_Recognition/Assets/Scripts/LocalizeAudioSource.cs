using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeAudioSource : MonoBehaviour
{
    private float DbValueR;
    private float DbValueL;

    [SerializeField] private int SampleSize = 128;
    [SerializeField] private float ReferenceValue = 0.0001f;
    [SerializeField] private float Threshold = 0.0002f;

    private float[] _samplesR;
    private float[] _samplesL;

    private string direction;
    private string lastDirection;

    private bool stop = false;

    [SerializeField] private AudioSource source;


    // Start is called before the first frame update
    void Start()
    {
        _samplesR = new float[SampleSize];
        _samplesL = new float[SampleSize];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!stop)
        {
            GetSoundData();

            DbValueR = AnalyzeSound(_samplesR);
            DbValueL = AnalyzeSound(_samplesL);

            if (direction != null)
                lastDirection = direction;

            if (DbValueR > DbValueL)
            {
                direction = "right";
                transform.Rotate(0f, -1f, 0f);
            }
            else if (DbValueR < DbValueL)
            {
                direction = "left";
                transform.Rotate(0f, 1f, 0f);
            }

            if (lastDirection != direction && lastDirection != null && direction != null)
            {
                stop = true;
                Debug.Log("Estimated Angle: " + transform.rotation.eulerAngles.y);
                transform.LookAt(source.transform);
                Debug.Log("Real Angle: " + transform.rotation.eulerAngles.y);
            }
            
        }

    }

    void GetSoundData()
    {
        AudioListener.GetOutputData(_samplesR, 0);
        AudioListener.GetOutputData(_samplesL, 1);
    }

    float AnalyzeSound(float[] array)
    {
        float RmsValue;
        float DbValue;
        float sum = 0;

        for (int i = 0; i < SampleSize; i++)
        {
            sum += array[i] * array[i];
        }

        RmsValue = Mathf.Sqrt(sum / SampleSize);
        DbValue = 20 * Mathf.Log10(RmsValue / ReferenceValue);

        if (DbValue < -160)
            DbValue = -160;

        return DbValue;
    }
}
