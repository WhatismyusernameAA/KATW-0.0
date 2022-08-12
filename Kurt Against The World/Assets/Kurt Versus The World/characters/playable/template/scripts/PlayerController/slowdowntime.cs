using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slowdowntime : MonoBehaviour
{
    [Range(0.1f,1.0f)]
    public float setTime;
    void Start()
    {
        Time.timeScale = setTime;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }
}
