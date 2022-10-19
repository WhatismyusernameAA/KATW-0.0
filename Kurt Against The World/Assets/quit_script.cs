using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit_script : MonoBehaviour
{
    public inputscript inputscript;

    // Update is called once per frame
    void Update()
    {
        if (inputscript.GetInputDown("Exit"))
            Application.Quit();   
    }
}
