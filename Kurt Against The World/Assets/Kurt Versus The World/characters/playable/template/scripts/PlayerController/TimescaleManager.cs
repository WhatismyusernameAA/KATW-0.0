using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static TimescaleManager instance;

    private void Awake()
    {
        if (!instance) instance = this;
    }

    public void SetTimeScale(float scale = 1.0f)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;
    }

    public void LerpTimeScale(float duration, float scale = 1.0f)
    {
        StartCoroutine(LerpTimeScaleCoroutine(duration,scale));
    }

    IEnumerator LerpTimeScaleCoroutine(float duration, float scale = 1.0f)
    {
        float currentTimeScale = Time.timeScale;
        float currentTime = duration;
        while(currentTime > 0)
        {
            currentTime -= Time.unscaledDeltaTime;
            SetTimeScale(Mathf.Lerp(currentTimeScale,scale, currentTime / duration));
            yield return null;
        }
        SetTimeScale(scale);
    }

    public void HaltTime(float scale = 0.0f, float duration = 1.0f, bool EaseIn = false, bool EaseOut = false)
    {
        if(duration > 0)
            StartCoroutine(HaltTimeCoroutine(scale,duration,EaseIn,EaseOut));
    }

    IEnumerator HaltTimeCoroutine(float scale, float duration, bool EaseIn, bool EaseOut)
    {
        if (EaseIn) LerpTimeScale(duration * 0.5f, scale);
        else SetTimeScale(scale);
        yield return new WaitForSecondsRealtime(duration);
        if (EaseOut) LerpTimeScale(1f);
        else SetTimeScale(1.0f);
    }
}
