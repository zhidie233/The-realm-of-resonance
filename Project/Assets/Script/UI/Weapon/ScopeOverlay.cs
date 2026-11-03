using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ScopeOverlay : MonoBehaviour
{
    private static ScopeOverlay _instance;
    public static ScopeOverlay Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScopeOverlay>();
            }
            return _instance;
        }
    }

    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public GameObject content;
    public Image scopeImage;
    public CanvasGroup scopeAlpha;

    public void SetActive(bool active)
    {
        scopeAlpha.alpha = 0;
        content.SetActive(active);
    }

    public void SetupWeapon(SniperScopeOverlay weapon)
    {
        scopeImage.sprite = weapon.ScopeTexture;
        scopeAlpha.alpha = 0;
    }
    /// 淡入淡出显示/隐藏瞄准镜 UI
    public void Crossfade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null)
    {
        StopAllCoroutines();
        StartCoroutine(DoFade(fadeIn, speed, delay, onFinish, onStart));
    }

    IEnumerator DoFade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null)
    {
        float d = 0;
        float t;
        float origin = scopeAlpha.alpha;
        float target = fadeIn ? 1 : 0;
        if (fadeIn) content.SetActive(true);

        if (delay > 0) yield return new WaitForSeconds(delay);
        onStart?.Invoke();
        while (d < 1)
        {
            d += Time.deltaTime / speed;
            t = transitionCurve.Evaluate(d);
            scopeAlpha.alpha = Mathf.Lerp(origin, target, t);
            yield return null;
        }
        onFinish?.Invoke();
        if(!fadeIn) content.SetActive(false);
    }
}
