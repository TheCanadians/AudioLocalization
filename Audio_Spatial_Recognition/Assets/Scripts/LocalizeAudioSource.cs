using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizeAudioSource : MonoBehaviour
{
    // Db Values of the sound the left and right channel are hearing
    private float DbValueR, DbValueL;

    [Tooltip("Number of Samples, must be a power of 2. Higher values get better results but calculate slower.")]
    [SerializeField] private int SampleSize = 128;
    [Tooltip("Step in degrees the algorithm takes each step to determine the Audio Source angle. Smaller value make better results but take longer.")]
    [SerializeField] private float stepSize = 1f;

    private float[] _samplesR, _samplesL;

    // if set to true stops all Listening and Calculations
    private bool stop = false;

    [Tooltip("Time in seconds to listen to Clip. Ignores if set to 0.")]
    [SerializeField] private float stopTime = 0f;
    private float nextStop;

    [Tooltip("AudioSource from which the Clips get played. Neccessary to calculate the real angle.")]
    [SerializeField] private AudioSource source;
    [Tooltip("Array of Clips to listen to. Listens to every Clip in order if playAllClipsAutomatically is true.")]
    [SerializeField] private AudioClip[] clips;
    [Tooltip("Plays all Clips in clips automatically if set to true.")]
    [SerializeField] private bool playAllClipsAutomatically = false;
    [Tooltip("Number of repetitions of the same Clip the algorithm has to listen to before changing the Clip.")]
    [SerializeField] private int numberOfRepetitions = 5;

    private int currentClip = 0, repetition = 1;

    [Tooltip("Directory to save the CSV File into. If empty saves to Assets/Data")]
    [SerializeField] private string pathToSaveTo;
    [Tooltip("CSV filename. If empty saves as directionValues.csv")]
    [SerializeField] private string fileName;
    private bool firstLine = true;

    [Tooltip("Prints the name of the Audio Clip, the current repetition, the estimated angle and the real angle in the console.")]
    [SerializeField] private bool debug = true;


    // Start is called before the first frame update
    void Start()
    {
        // Initiate array with a size of SampleSize
        _samplesR = new float[SampleSize];
        _samplesL = new float[SampleSize];

        // Checks if clips are available and plays first clip
        if (clips.Length != 0)
        {
            source.clip = clips[currentClip];
            source.Play();
            currentClip++;
        }
        else
        {
            Debug.Log("No Clips provided");
            Stop();
        }

        nextStop = Time.time + stopTime;
            
    }

    // Calculates each frame which direction is louder to rotate into
    void FixedUpdate()
    {
        if (!stop)
        {
            GetSoundData();

            DbValueR = AnalyzeSound(_samplesR);
            DbValueL = AnalyzeSound(_samplesL);

            if (DbValueR > DbValueL)
            {
                transform.Rotate(0f, -stepSize, 0f);
            }
            else if (DbValueR < DbValueL)
            {
                transform.Rotate(0f, stepSize, 0f);
            }
            // if the DbValues are -160 no sound is perceived and the SoundClip has probably ended
            if ((!source.isPlaying) || (stopTime != 0 && Time.time > nextStop))
            {
                stop = true;

                float estimatedAngle = transform.rotation.eulerAngles.y;
                
                transform.LookAt(source.transform);
                float realAngle = transform.rotation.eulerAngles.y;

                if (debug)
                    Debug.Log(source.clip.name + repetition + "  " + estimatedAngle + "   " + realAngle);

                StartCoroutine(WriteToCSV(estimatedAngle, realAngle));

                if (playAllClipsAutomatically)
                {
                    Reset();
                }
            }
        }

    }
    // Fills arrays with the samples from the left and right channel
    void GetSoundData()
    {
        AudioListener.GetOutputData(_samplesR, 0);
        AudioListener.GetOutputData(_samplesL, 1);
    }
    // Recalculates the given array into a Db Value
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
        DbValue = 20 * Mathf.Log10(RmsValue / 1f);

        if (DbValue < -160)
            DbValue = -160;

        return DbValue;
    }
    // Reset all variables and rotations. Spawns the AudioSource at a new random position
    private void Reset()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);

        _samplesR = new float[SampleSize];
        _samplesL = new float[SampleSize];

        nextStop = Time.time + stopTime;

        source.GetComponent<RandomSpawn>().Spawn();
        
        if (currentClip < clips.Length && repetition >= numberOfRepetitions)
        {
            LoadClip();
        }
        else if (repetition < numberOfRepetitions)
        {
            repetition++;
        }
        else
        {
            Stop();
        }

        source.Play();
        stop = false;
    }
    // Loads next AudioClip
    private void LoadClip()
    {
        source.clip = clips[currentClip];
        currentClip++;
        repetition = 1;
    }
    // Write the estimated angle, real angle and angle difference between the two to the directionValues CSV file
    private IEnumerator WriteToCSV(float estAng, float realAng)
    {
        string filePath;

        if (pathToSaveTo != "")
            filePath = pathToSaveTo;
        else
            filePath = Application.dataPath + "/Data/";

        if (fileName != "")
        {
            filePath += fileName;
            if (!fileName.ToUpper().Contains(".CSV"))
                filePath += ".csv";
        }  
        else
            filePath += "directionValues.csv";

        Debug.Log(filePath);
        StreamWriter writer = new StreamWriter(filePath, true);

        if (firstLine)
        {
            writer.WriteLine("Name of Sound;Estimated Angle;Real Angle;Angle Difference");
            firstLine = false;
        }

        float angDif = (estAng - realAng);

        if (angDif > 180)
            angDif -= 360;
        else if (angDif < -180)
            angDif += 360;

        writer.WriteLine(source.clip.name + ";" + estAng + ";" + realAng + ";" + angDif);

        writer.Flush();
        writer.Close();

        yield return null;
    }
    // Stop the Program
    private void Stop()
    {
        Debug.Log("Finished");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
