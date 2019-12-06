using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioListener))]
public class Audio : MonoBehaviour
{

    private AudioListener listener;
    public AudioSource source;
    GameObject dirObj;
    float[] volumeRight;
    float[] volumeLeft;
    float yRot = 0f;
    float minRight = 1;
    float minLeft = 1;
    float maxRight = -1;
    float maxLeft = -1;

    float globalMax = -1;
    float globalMaxYRot = 0;
    float globalMin = 1;
    float globalMinYRot = 0;

    float stepCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        listener = transform.GetComponent<AudioListener>();
        if (listener == null)
            Debug.Log("No Listener");
        volumeRight = new float[64];
        volumeLeft = new float[64];
    }

    private void FixedUpdate()
    {
        if (stepCount <= 360)
        {
            transform.Rotate(0, 1, 0);
            ScanAngle();
        }   
        else
        {
            Debug.Log("Global Max: " + globalMax + "   " + globalMaxYRot);
            Debug.Log("Global Min: " + globalMin + "   " + globalMinYRot);
            if (dirObj == null)
            {
                InstantiateDirectionObj();

                // Print Estimated angle and real angle
                Debug.Log("Estimated Angle (+- 180°): " + dirObj.transform.rotation.eulerAngles.y);
                transform.LookAt(GameObject.Find("SoundSource").transform);
                Debug.Log("Real Angle: " + transform.rotation.eulerAngles.y);

                StartCoroutine(CheckDirectionAngle());
            }
        }
        stepCount++;
    }

    private void ScanAngle()
    {
            // if the channels are not available catch the error without soft locking the programm
            try
            {
            // Channel 1 = right channel, Channel 0 = left channel
                AudioListener.GetOutputData(volumeRight, 1);
                AudioListener.GetOutputData(volumeLeft, 0);

            GetLocalSampleExtremums();
            GetGlobalSampleExtremums();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
    }

    private void GetLocalSampleExtremums()
    {
        // variables to store the maximum and minimum value in this sample array (right and left)
        minRight = 1;
        minLeft = 1;
        maxRight = -1;
        maxLeft = -1;

        for (int i = 0; i < volumeRight.Length; i++)
        {
            if (volumeRight[i] > maxRight)
                maxRight = volumeRight[i];
            if (volumeRight[i] < minRight)
                minRight = volumeRight[i];
            if (volumeLeft[i] > maxLeft)
                maxLeft = volumeLeft[i];
            if (volumeLeft[i] < minLeft)
                minLeft = volumeLeft[i];
        }


    }

    private void GetGlobalSampleExtremums()
    {
        // Calculate max amplitude
        float maxR = (maxRight - minRight);
        float maxL = (maxLeft - minLeft);

        if (maxR / maxL > globalMin)
        {
            globalMin = maxR / maxL;
            if (maxR > maxL)
                globalMinYRot = transform.rotation.eulerAngles.y;
            else
                globalMinYRot = transform.rotation.eulerAngles.y;
        }

        if (maxR > globalMax)
        {
            globalMax = maxR;
            globalMaxYRot = transform.rotation.eulerAngles.y;
        }

        if (maxL > globalMax)
        {
            globalMax = maxL;
            globalMaxYRot = transform.rotation.eulerAngles.y;
        }
    }

    private void InstantiateDirectionObj()
    {
        // Instantiate Primitive to visually show sound angle
        dirObj = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), transform.position, Quaternion.identity);
        // Calculate mean of the minimum sound angle and maximum sound angle (both should be k*180 degrees apart)
        float middle;
        if (Mathf.Abs(globalMaxYRot - globalMinYRot) > 100)
            middle = (globalMaxYRot + globalMinYRot) / 2;
        else
            middle = ((globalMaxYRot + globalMinYRot) / 2) - 90;
        // Scale and rotate visualizer to show angle
        dirObj.transform.localScale = new Vector3(3, 0.1f, 100);
        dirObj.transform.rotation = Quaternion.Euler(0, middle, 90);
        // Destroy the duplicate cube (I don't know why this gets called twice
        Destroy(GameObject.Find("Cube"));
    }

    private IEnumerator CheckDirectionAngle()
    {

        float currentAngle = dirObj.transform.rotation.eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, currentAngle, 0);
        this.transform.Rotate(0, 90, 0);

        yield return new WaitForSeconds(0.5f);

        // if the channels are not available catch the error without soft locking the programm
        try
        {
            // Channel 1 = right channel, Channel 0 = left channel
            AudioListener.GetOutputData(volumeRight, 1);
            AudioListener.GetOutputData(volumeLeft, 0);

            GetLocalSampleExtremums();

            DebugOutputArrays(1);

            if ((maxRight-minRight) > (maxLeft-minLeft))
            {
                Debug.Log("Estimated directed angle (R): " + ((this.transform.rotation.eulerAngles.y + 90) % 360));
                this.transform.Rotate(0, 90, 0);
            }
            else
            {
                Debug.Log("Estimated directed angle (L): " + ((this.transform.rotation.eulerAngles.y - 90) % 360));
                this.transform.Rotate(0, -90, 0);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void DebugOutputArrays(int option)
    {
        switch(option)
        {
            case 0:
                for (int i = 0; i < volumeLeft.Length; i++)
                {
                    Debug.Log("Step " + i + ": " + " Right: " + volumeRight[i] + "   Left: " + volumeLeft[i]);
                }
                break;
            case 1:
                Debug.Log("Max Right: " + maxRight + " Min Right: " + minRight + "   Max Left: " + maxLeft + " Min Left: " + minLeft);
                break;
            default:
                Debug.Log("Wrong argument given");
                break;
        }

    }
}
