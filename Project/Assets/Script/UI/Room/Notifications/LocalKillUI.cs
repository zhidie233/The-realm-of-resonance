using UnityEngine;
using System.Collections;
using TMPro;

public class LocalKillUI : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI NameText = null;
    [SerializeField]private TextMeshProUGUI ValueText = null;
    [SerializeField] private TextMeshProUGUI ExtraText = null;
    [SerializeField] private Animator CircleAnim = null;
    private CanvasGroup Alpha;
    private Animator Anim;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="headShot"></param>
    public void InitMultiple(KillInfo info, bool headShot)
    {
        if (!headShot)
        {
            NameText.text = info.Killed;
            ValueText.text = GameConfig.Instance.ScoreReward.ScorePerKill.ToString();
        }
        else
        {
            NameText.text = GameTexts.HeatShotBonus;
            ValueText.text = GameConfig.Instance.ScoreReward.ScorePerHeadShot.ToString();
        }
        Alpha = GetComponent<CanvasGroup>();
        StartCoroutine(Hide(true));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    public void InitIndividual(KillInfo info)
    {
        if(Anim == null) { Anim = GetComponent<Animator>(); }
        NameText.text = info.Killed;
        ValueText.text = (info.byHeadShot) ? GameTexts.HeadShot.ToUpper() : GameTexts.KillingInAction.ToUpper();
        int spk = GameConfig.Instance.ScoreReward.ScorePerKill;
        if (info.byHeadShot)
        {
            ExtraText.text = string.Format("{0} <b>+{1}</b>\n{2} +{3}", info.KillMethod.ToUpper(), spk, GameTexts.HeadShot.ToUpper(), GameConfig.Instance.ScoreReward.ScorePerHeadShot);
        }
        else
        {
            ExtraText.text = string.Format("{0} <b>+{1}</b>", info.KillMethod.ToUpper(), spk);
        }
        gameObject.SetActive(true);
        if (CircleAnim != null) { CircleAnim.Play("play", 0, 0); }
        Anim.SetBool("show", true);
        Anim.Play("show", 0, 0);
        if (Alpha == null) Alpha = GetComponent<CanvasGroup>();

        StartCoroutine(HideAnimated());
    }

    IEnumerator Hide(bool destroy)
    {
        yield return new WaitForSeconds(7);
        while(Alpha.alpha > 0)
        {
            Alpha.alpha -= Time.deltaTime;
            yield return null;
        }
        if (destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            LocalKillNotifier.Instance.LocalDisplayDone();
        }
    }

    IEnumerator HideAnimated()
    {
        yield return new WaitForSeconds(LocalKillNotifier.Instance.IndividualShowTime);
        Anim.SetBool("show", false);
        yield return new WaitForSeconds(Anim.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        LocalKillNotifier.Instance.LocalDisplayDone();
    }

}