using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShakeManager : MonoBehaviour
{
    public static ShakeManager instance;
    // from https://www.youtube.com/watch?v=O2Pg8e2xwzg&t=77s, im too lazy to code my own
    // altho I might do so if this one is kinda weird;
    public float ShakeDuration = 0.3f;          // Time the Camera Shake effect will last
    public float ShakeAmplitude = 1.2f;         // Cinemachine Noise Profile Parameter
    public float ShakeFrequency = 2.0f;         // Cinemachine Noise Profile Parameter

    private float ShakeElapsedTime = 0f;

    // Cinemachine Shake
    public CinemachineVirtualCamera VirtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    // Use this for initialization
    void Awake()
    {
        if (!instance) instance = this;
        // Get Virtual Camera Noise Profile
        if (VirtualCamera != null)
            virtualCameraNoise = VirtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float duration, float amplitude, float frequency)
    {
        ShakeAmplitude = amplitude;
        ShakeFrequency = frequency;
        ShakeElapsedTime = duration;
    }

    // Update is called once per frame
    void Update()
    {
        // If the Cinemachine componet is not set, avoid update
        if (VirtualCamera != null && virtualCameraNoise != null)
        {
            // If Camera Shake effect is still playing
            if (ShakeElapsedTime > 0)
            {
                // Set Cinemachine Camera Noise parameters
                virtualCameraNoise.m_AmplitudeGain = ShakeAmplitude;
                virtualCameraNoise.m_FrequencyGain = ShakeFrequency;

                // Update Shake Timer
                ShakeElapsedTime -= Time.deltaTime;
            }
            else
            {
                // If Camera Shake effect is over, reset variables
                virtualCameraNoise.m_AmplitudeGain = 0f;
                ShakeElapsedTime = 0f;
            }
        }
    }
}
