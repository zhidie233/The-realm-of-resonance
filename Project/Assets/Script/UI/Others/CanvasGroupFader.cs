using System;
using System.Collections;
using UnityEngine;

public class CanvasGroupFader : MonoBehaviour
{
    [Range(0.1f, 3)] public float duration = 0.75f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup m_canvasGroup;
    private CanvasGroup canvasGroup
    {
        get
        {
            if (m_canvasGroup == null) m_canvasGroup = GetComponent<CanvasGroup>();
            return m_canvasGroup;
        }
    }

    public void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha; if(alpha > 0) gameObject.SetActive(true);
    }

    public Coroutine FadeIn(float length = 0, Action onFinish = null)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        return StartCoroutine(DoFadeIn(length, onFinish));
    }

    public Coroutine FadeOut(float length = 0, Action onFinish = null)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        return StartCoroutine(DoFadeOut(length, onFinish));
    }

    IEnumerator DoFadeIn(float length = 0, Action onFinish = null)
    {
        float d = 0;
        float t = 0;
        float dur = length <= 0 ? duration : length;
        while (d < 1)
        {
            d += Time.deltaTime / dur;
            t = fadeCurve.Evaluate(d);
            canvasGroup.alpha = t;
            yield return null;
        }
        onFinish?.Invoke();
    }

    IEnumerator DoFadeOut(float length = 0, Action onFinish = null)
    {
        float d = 0;
        float t = 0;
        float dur = length <= 0 ? duration : length;
        while (d < 1)
        {
            d += Time.deltaTime / dur;
            t = fadeCurve.Evaluate(d);
            canvasGroup.alpha = 1 - t;
            yield return null;
        }
        onFinish?.Invoke();
        gameObject.SetActive(false);
    }
}