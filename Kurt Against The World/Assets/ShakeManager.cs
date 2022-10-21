using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

[Serializable]
public class Shake
{
    [SerializeField] public float shakeDuration = 0.3f;          // Time the Camera Shake effect will last
    [SerializeField] public float shakeAmplitude = 1.2f;         // Cinemachine Noise Profile Parameter
    [SerializeField] public float shakeFrequency = 2.0f;         // Cinemachine Noise Profile Parameter
}

public class ShakeManager : MonoBehaviour
{
    // from https://www.youtube.com/watch?v=O2Pg8e2xwzg&t=77s, im too lazy to code my own
    // altho I might do so if this one is kinda weird;

    public static ShakeManager instance;

    [Header("Dependencies")]
    public CinemachineVirtualCamera VirtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    [Space]
    [SerializeField] public Shake cameraSettings;


    float ShakeElapsedTime = 0f;
    float currentMagnitude = 0f;
    float currentFrequency = 0f;

    

    // Use this for initialization
    void Awake()
    {
        if (!instance) instance = this;
        // Get Virtual Camera Noise Profile
        if (VirtualCamera != null)
            virtualCameraNoise = VirtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(Shake setCamera)
    {
        cameraSettings = setCamera;
        ShakeElapsedTime = setCamera.shakeDuration;
    }

    public void SetNoiseProperties(Shake setCamera)
    {
        cameraSettings = setCamera;
        currentMagnitude = setCamera.shakeAmplitude;
        virtualCameraNoise.m_FrequencyGain = setCamera.shakeFrequency;
    }

    // Update is called once per frame
    void Update()
    {
        // if the Cinemachine component is not set, avoid update
        if (VirtualCamera != null && virtualCameraNoise != null)
        {
            // if Camera Shake effect is still playing
            if (ShakeElapsedTime > 0)
            {
                // Set Cinemachine Camera Noise parameters
                virtualCameraNoise.m_AmplitudeGain = cameraSettings.shakeAmplitude;
                virtualCameraNoise.m_FrequencyGain = cameraSettings.shakeFrequency;

                // Update Shake Timer
                ShakeElapsedTime -= Time.deltaTime;

                currentMagnitude = 0f;
            }
            else
            {
                // If Camera Shake effect is over, reset variables
                virtualCameraNoise.m_AmplitudeGain = currentMagnitude;
                ShakeElapsedTime = 0f;
            }
        }
    }
}
