using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashStretchHandler : MonoBehaviour
{
    [HideInInspector]
    public float InitialX;
    [HideInInspector]
    public float InitialY;
    public float StretchSpeed;

    public bool lockX;
    public bool lockY;


    //public Collider2D ObjectCollider;
    //public float BounceModifier;

    private void Awake()
    {
        Scale(transform.localScale.x,transform.localScale.y);
    }

    private void FixedUpdate()
    {
        //Handle scaling back into place
        float scaleOffsetx = Mathf.Abs(InitialX - transform.localScale.x);
        float scaleOffsety = Mathf.Abs(InitialY - transform.localScale.y);

        if (scaleOffsetx != 0 && !lockX)
        {
            transform.localScale = new Vector3(Mathf.Lerp(transform.localScale.x, InitialX, Time.deltaTime * StretchSpeed), transform.localScale.y, transform.localScale.z);
        }

        if (scaleOffsety != 0 && !lockY)
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, InitialY, Time.deltaTime * StretchSpeed), transform.localScale.z);
        }

        //Handle Collisions
        // or maybe not
        
    }

    public void Scale(float x, float y)
    {
        InitialX = x;
        InitialY = y;
    }

    public void ScaleX(float value)
    {
        InitialX = value;
    }

    public void ScaleY(float value)
    {
        InitialY = value;
    }

    public void UnphysicsScale(float x, float y)
    {
        InitialX = x;
        InitialY = y;
        transform.localScale = new Vector3(InitialX, InitialY, transform.localScale.z);
    }
}
