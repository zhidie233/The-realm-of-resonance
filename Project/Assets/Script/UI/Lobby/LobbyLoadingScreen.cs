using System.Collections;
using TMPro;
using UnityEngine;
/// 默认的大厅连接加载界面
public class LobbyLoadingScreen : MonoBehaviour
{
    private static LobbyLoadingScreen _instance;
    public static LobbyLoadingScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LobbyLoadingScreen>();
            }
            return _instance;
        }
    }

    public GameObject content;
    public TextMeshProUGUI loadingText;
    public CanvasGroup canvasGroup;
    public Animator bottomAnimator;

    public LobbyLoadingScreen HideIn(float delay, bool faded)
    {
        StopAllCoroutines();
        StartCoroutine(Hide(delay, faded));
        return this;
    }

    public LobbyLoadingScreen SetActive(bool active)
    {
        content.SetActive(active);
        if (active)
        {
            if (bottomAnimator != null)
            {
                bottomAnimator.SetBool("show", true);
            }
            if (canvasGroup != null) canvasGroup.alpha = 1;
        }
        return this;
    }

    public LobbyLoadingScreen SetText(string text)
    {
        if (loadingText != null) loadingText.text = text;
        return this;
    }

    IEnumerator Hide(float delay, bool faded)
    {
        yield return new WaitForSeconds(delay);
        if(faded)
        {
            float d = 1;
            bottomAnimator.SetBool("show", false);
            while (d > 0)
            {
                d -= Time.deltaTime / 0.5f;
                canvasGroup.alpha = LobbyUI.Instance.blackScreenFader.fadeCurve.Evaluate(d);
                yield return null;
            }
        }
        SetActive(false);
    }
}
