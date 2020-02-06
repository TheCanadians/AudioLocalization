using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class PrintAudio : MonoBehaviour
{
    public bool auto = true;
    public bool drawCont = true;

    public float RmsValueR;
    public float DbValueR;
    public float PitchValueR;

    public float RmsValueL;
    public float DbValueL;
    public float PitchValueL;

    public Text rms_R;
    public Text dB_R;
    public Text Pitch_R;
    public Text rms_L;
    public Text dB_L;
    public Text Pitch_L;

    public Text Source_Angle;
    public Text Cross_Cor;

    private const int QSamples = 128;
    private const float RefValue = 0.001f;
    private const float Threshold = 0.002f;

    float[] _samplesL;
    private float[] _spectrumL;

    float[] _samplesR;
    float[] _spectrumR;

    private float _fSample;

    private bool rotate = true;
    private bool keyPressed = false;
    private bool drawn = false;

    void Start()
    {
        _samplesR = new float[QSamples];
        _spectrumR = new float[QSamples];
        _samplesL = new float[QSamples];
        _spectrumL = new float[QSamples];

        _fSample = AudioSettings.outputSampleRate;
    }

    private void FixedUpdate()
    {
        if (auto)
        {
            if (rotate)
                StartCoroutine(Rotate(1));
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (!keyPressed)
                {
                    StartCoroutine(Rotate(1));
                    keyPressed = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (!keyPressed)
                {
                    StartCoroutine(Rotate(-1));
                    keyPressed = true;
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                keyPressed = false;
                drawn = false;
            }
        }
        if (_samplesL.Length != 0)
        {
            DrawSpectrum(_spectrumL, _spectrumR);
            DrawVolume(_samplesL, _samplesR);
        }
        if (drawCont)
            GetSoundData();
        else
        {
            if (!drawn)
            {
                GetSoundData();
                drawn = true;
            }
        }
    }

    IEnumerator Rotate(int dir)
    {
        rotate = false;
        yield return new WaitForSeconds(0.2f);
        transform.Rotate(0, dir, 0);
        rotate = true;
    }

    void GetSoundData()
    {
        AudioListener.GetOutputData(_samplesL, 0); // fill array with samples
        AudioListener.GetOutputData(_samplesR, 1); // fill array with samples
        AudioListener.GetSpectrumData(_spectrumL, 0, FFTWindow.BlackmanHarris); // fill array with samples
        AudioListener.GetSpectrumData(_spectrumR, 1, FFTWindow.BlackmanHarris); // fill array with samples

        float[] valuesL = AnalyzeSound(_samplesL, _spectrumL);
        float[] valuesR = AnalyzeSound(_samplesR, _spectrumR);
        float r = CorrelationFunction(_samplesL, _samplesR);

        RmsValueL = valuesL[0];
        DbValueL = valuesL[1];
        PitchValueL = valuesL[2];

        RmsValueR = valuesR[0];
        DbValueR = valuesR[1];
        PitchValueR = valuesR[2];

        rms_L.text = "Rms L: " + valuesL[0].ToString();
        dB_L.text = "dB L: " + valuesL[1].ToString();
        Pitch_L.text = "Pitch L: " + valuesL[2].ToString();

        rms_R.text = "Rms R: " + valuesR[0].ToString();
        dB_R.text = "dB R: " + valuesR[1].ToString();
        Pitch_R.text = "Pitch R: " + valuesR[2].ToString();

        Vector3 targetDir = GameObject.Find("SoundSource").transform.position - transform.position;
        float sAngle = Vector3.Angle(transform.forward, targetDir);
        Source_Angle.text = "Source Angle: " + sAngle.ToString();

        Cross_Cor.text = "Cross Cor.: " + r.ToString();
    }

    private void DrawSpectrum(float[] a, float[] b)
    {
        for (int i = 1; i < a.Length - 1; i++)
        {
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(a[i - 1]) + 10, 0), new Vector3(i, Mathf.Log(a[i]) + 10, 0), Color.cyan);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(b[i - 1]) + 10, 0), new Vector3(i, Mathf.Log(b[i]) + 10, 0), Color.yellow);
        }
    }

    private void DrawVolume(float[] c, float[] d)
    {
        for (int j = 1; j < c.Length - 1; j++)
        {
            Debug.DrawLine(new Vector3(j - 1, c[j] * 20 + 10, 0), new Vector3(j, c[j + 1] * 20 + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(j - 1, d[j] * 20 + 10, 0), new Vector3(j, d[j + 1] * 20 + 10, 0), Color.green);

            Debug.DrawLine(new Vector3(j - 1, (c[j] - d[j]) * 20 + 10, 0), new Vector3(j, (c[j + 1] - d[j + 1]) * 20 + 10, 0), Color.blue);
        }
    }

    float[] AnalyzeSound(float[] a, float[] b)
    {
        float RmsValue;
        float DbValue;
        float PitchValue;
        int i;
        float sum = 0;
        for (i = 0; i < QSamples; i++)
        {
            sum += a[i] * a[i]; // sum squared samples
        }
        RmsValue = Mathf.Sqrt(sum / QSamples); // rms = square root of average
        DbValue = 20 * Mathf.Log10(RmsValue / RefValue); // calculate dB
        if (DbValue < -160) DbValue = -160; // clamp it to -160dB min
                                            // get sound spectrum
        float maxV = 0;
        var maxN = 0;
        for (i = 0; i < QSamples; i++)
        { // find max 
            if (!(b[i] > maxV) || !(b[i] > Threshold))
                continue;

            maxV = b[i];
            maxN = i; // maxN is the index of max
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < QSamples - 1)
        { // interpolate index using neighbours
            var dL = b[maxN - 1] / b[maxN];
            var dR = b[maxN + 1] / b[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        PitchValue = freqN * (_fSample / 2) / QSamples; // convert index to frequency

        float[] test = { RmsValue, DbValue, PitchValue };

        return test;
    }

    private float CorrelationFunction(float[] x, float[] y)
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

        return r;
        //Debug.Log(r);
        //Debug.Log((Mathf.Acos(r) / Mathf.PI) * 180.0f);
    }
}
