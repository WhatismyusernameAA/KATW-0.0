using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundplayer : MonoBehaviour
{
    public List<AudioSource> audioSources;

    [Header("Alternate Pitch Settings")]
    public bool alternatePitch;
    [Range(0.0f,2.0f)]
    public float minAltPitch;
    [Range(0.0f,2.0f)]
    public float maxAltPitch;

    [Header("Animation Triggers")]
    public Animator animator;

    [Header("Extras")]
    public ParticleSystem acompanyFx;


    public void PlaySound()
    {
        if (audioSources.Count > 1)
        {
            int randomSourceIndex = Mathf.RoundToInt(Random.Range(0, audioSources.Count));
            if(alternatePitch) audioSources[randomSourceIndex].pitch = Random.Range(minAltPitch, maxAltPitch);
            audioSources[randomSourceIndex].Play();
        }
        else
        {
            audioSources[0].pitch = Random.Range(minAltPitch, maxAltPitch);
            audioSources[0].Play();
        }

        if(acompanyFx) acompanyFx.Play();
    }

    public void StopAllSounds()
    {
        for(int i = 0; i < audioSources.Count; i++)
        {
            audioSources[i].Stop();
        }

        if (acompanyFx) acompanyFx.Stop();
    }
}
