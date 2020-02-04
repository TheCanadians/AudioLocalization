using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TestAudio : MonoBehaviour
{

    private AudioListener listener;
    public AudioSource source;
    GameObject dirObj;
    float[] volumeRight;
    float[] volumeLeft;

    float[] spectrumRight;
    float[] spectrumLeft;

    // Start is called before the first frame update
    void Start()
    {
        listener = transform.GetComponent<AudioListener>();
        if (listener == null)
            Debug.Log("No Listener");
        volumeRight = new float[256];
        volumeLeft = new float[256];
        spectrumRight = new float[1024];
        spectrumLeft = new float[1024];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        try
        {
            // Channel 1 = right channel, Channel 0 = left channel
            AudioListener.GetOutputData(volumeRight, 1);
            AudioListener.GetOutputData(volumeLeft, 0);

            AudioListener.GetSpectrumData(spectrumRight, 1, FFTWindow.BlackmanHarris);
            AudioListener.GetSpectrumData(spectrumLeft, 0, FFTWindow.BlackmanHarris);

            DrawArray(spectrumRight, spectrumLeft, 0);
            //DrawArray(volumeRight, volumeLeft, 20);

            /*
            foreach (float var in volumeRight) {
                Debug.Log("Right: " + var);
            }
            foreach (float var2 in volumeLeft)
            {
                Debug.Log("Left: " + var2);
            }
            

            for (int i = 0; i < spectrumRight.Length; i++)
            {
                Debug.Log("Right: " + spectrumRight[i] + "   Left: " + spectrumLeft[i]);
            }
            */
            //CorrelationFunction(volumeRight, volumeLeft);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void DrawArray(float[] a, float[] b, int startZ)
    {
        for (int i = 1; i < a.Length - 1; i++)
        {
            Debug.DrawLine(new Vector3(i - 1, a[i] + 10, 0), new Vector3(i, a[i + 1] + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(a[i - 1]) + 10, 0), new Vector3(i, Mathf.Log(a[i]) + 10, 0), Color.cyan);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), a[i - 1] - 10, 0), new Vector3(Mathf.Log(i), a[i] - 10, 0), Color.green);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(a[i - 1]), 0), new Vector3(Mathf.Log(i), Mathf.Log(a[i]), 0), Color.blue);

            Debug.DrawLine(new Vector3(i - 1, b[i] + 10, 0), new Vector3(i, b[i + 1] + 10, 0), Color.black);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(b[i - 1]) + 10, 0), new Vector3(i, Mathf.Log(b[i]) + 10, 0), Color.gray);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), b[i - 1] - 10, 0), new Vector3(Mathf.Log(i), b[i] - 10, 0), Color.magenta);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(b[i - 1]), 0), new Vector3(Mathf.Log(i), Mathf.Log(b[i]), 0), Color.white);
        }
    }

    private void CorrelationFunction(float[] x, float[] y)
    {
        float xSum = x.Sum();
        float ySum = y.Sum();
        float xSumN = xSum / x.Length;
        float ySumN = ySum / y.Length;

        float sumTop = 0;
        float sumBotX = 0;
        float sumBotY = 0;

        float r = 0;

        for (int i = 0; i < x.Length; i++)
        {
            sumTop += ((x[i] - xSumN) * (y[i] - ySumN));
            sumBotX += Mathf.Pow((x[i] - xSumN), 2);
            sumBotY += Mathf.Pow((y[i] - ySumN), 2);
        }

        r = sumTop / (Mathf.Sqrt(sumBotX) * Mathf.Sqrt(sumBotY));

        Debug.Log(r);
        Debug.Log((Mathf.Acos(r) / Mathf.PI) * 180.0f);
    }
}
