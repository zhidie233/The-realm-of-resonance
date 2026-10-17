using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

public class KickVoteUI : MonoBehaviour
{
    [SerializeField] private GameObject Window = null;
    [SerializeField] private TextMeshProUGUI TitleText = null;
    [SerializeField] private TextMeshProUGUI VotingText = null;
    [SerializeField] private TextMeshProUGUI YesText = null;
    [SerializeField] private TextMeshProUGUI NoText = null;
    [SerializeField] private TextMeshProUGUI VoteConfirmation = null;
    [SerializeField] private GameObject KeyInfoUI = null;
    private string CacheTargetName;
    private KickVote KickManager;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        Window.SetActive(false);
        KickManager = FindObjectOfType<KickVote>();
    }

    private void OnEnable()
    {
        ScoreboardPopUpMenuBase.onScoreboardMenuAction += OnScoreboardPopupAction;
    }

    private void OnDisable()
    {
        ScoreboardPopUpMenuBase.onScoreboardMenuAction -= OnScoreboardPopupAction;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="actionID"></param>
    private void OnScoreboardPopupAction(int actionID)
    {
        if(actionID == 0)
        {
            RequestVote();
        }
    }

    public void OpenVotatation(Player again, Player By)
    {
        CancelInvoke("Hide");
        CacheTargetName = again.NickName;
        TitleText.text = string.Format(GameTexts.VoteBy, By.NickName);
        VotingText.text = string.Format(GameTexts.KickQuestion, again.NickName);
        YesText.text = "0";
        NoText.text = "0";
        VoteConfirmation.text = string.Empty;
        if (again.ActorNumber != PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            KeyInfoUI.SetActive(true);
        }
        else
        {
            KeyInfoUI.SetActive(false);
        }
        Window.SetActive(true);
    }

    public void OnSendLocalVote(bool yes)
    {
        KeyInfoUI.SetActive(false);
        string vote = yes ? "<color=green>YES</color>" : "<color=red>NO</color>";
        VoteConfirmation.text = string.Format(GameTexts.YouVote, vote);
    }

    public void OnReceiveVote(int yes, int no)
    {
        YesText.text = yes.ToString();
        NoText.text = no.ToString();
    }

    public void OnFinish(bool yes)
    {
        if (yes)
        {
            VotingText.text = string.Format(GameTexts.KickConfirmation, CacheTargetName);
        }
        else
        {
            VotingText.text = string.Format(GameTexts.KickFailed, CacheTargetName);
        }
        KeyInfoUI.SetActive(false);
        Invoke("Hide", 3);
    }

    /// <summary>
    /// 
    /// </summary>
    public void RequestVote()
    {
        KickManager.RequestKick(ScoreboardPopUpMenuBase.TargetPlayer);
    }

    void Hide()
    {
        Window.SetActive(false);
    }
}