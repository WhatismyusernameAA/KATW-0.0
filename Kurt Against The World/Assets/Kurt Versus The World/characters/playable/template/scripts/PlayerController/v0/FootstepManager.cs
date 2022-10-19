using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public LayerMask[] materials;
    public soundplayer[] soundPlayers;

    public Vector3 groundCheckOffset;
    public float groundCheckRadius;

    Dictionary<LayerMask, soundplayer> footsteps;

    private void Awake()
    {
        footsteps = new Dictionary<LayerMask, soundplayer>();

        if (materials.Length != soundPlayers.Length)
            Debug.LogWarning("Number of Materials is less or more than the number of sounds. This can lead to some materials not being used.");
        for (int i = 0; i < soundPlayers.Length; i++)
        {
            if (i <= materials.Length - 1) footsteps.Add(materials[i], soundPlayers[i]);
            else Debug.Log(materials[i] + "was not used!");
        }
    }

    public void PlayFootstep()
    {
        if (soundPlayers.Length == 0) return;
        soundPlayers[0].PlaySound();
    }
}
