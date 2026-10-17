using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif

public class PlayerVoice : MonoBehaviourBase
{
    private GameObject RecorderIcon;
#if !UNITY_WEBGL && PVOICE
    private Recorder VoiceRecorder;
    private Speaker Speaker;
    private bool PushToTalk = false;
#endif
    private PhotonView View;

    protected override void Awake()
    {
        base.Awake();
        View = photonView;
        if (!PhotonNetwork.InRoom || PhotonNetwork.OfflineMode)
            return;

        RecorderIcon = UIReferences.Instance.PlayerUI.SpeakerIcon;
#if !UNITY_WEBGL && PVOICE
        VoiceRecorder = FindObjectOfType<Recorder>();
        Speaker = GetComponent<Speaker>();
#endif
#if !UNITY_WEBGL && PVOICE
        if (View.IsMine)
        {
            if(VoiceRecorder != null)
            VoiceRecorder.enabled = true;
            PushToTalk = (bool)GameCore.Settings.GetSettingOf("Voice Chat");
            if (Speaker != null)
            Speaker.enabled = (bool)GameCore.Settings.GetSettingOf("PushToTalk");
        }
        else
        {
            if (GetGameMode != GameMode.FFA)
            {
                if (Speaker != null)
                    Speaker.enabled = photonView.Owner.GetPlayerTeam() == PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam();
            }
        }
#else
        RecorderIcon.SetActive(false);
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public void OnPushToTalkMobile(bool transmit)
    {
#if !UNITY_WEBGL && PVOICE
        if (VoiceRecorder != null)
            VoiceRecorder.TransmitEnabled = transmit;
#endif       
    }

#if !UNITY_WEBGL && PVOICE
    public override void OnUpdate()
    {
        if (VoiceRecorder == null) return;
        if (!PhotonNetwork.InRoom || PhotonNetwork.OfflineMode)
            return;

        if (View.IsMine)
        {
            RecorderIcon.SetActive(VoiceRecorder.TransmitEnabled && VoiceRecorder.IsCurrentlyTransmitting && VoiceRecorder.LevelMeter.CurrentPeakAmp > 0.1f);
            if (PushToTalk && !UtilityHelper.isMobile)
            {
                VoiceRecorder.TransmitEnabled = GameInput.Talk();
            }
        }
    }
#endif

    public void OnMuteChange(bool b)
    {
        if (View.IsMine)
        {
#if !UNITY_WEBGL && PVOICE
            if (Speaker != null)
                Speaker.enabled = !b;
#endif
        }
    }

    /// <summary>
    /// Called when the player push to talk option change.
    /// </summary>
    /// <param name="b"></param>
    public void OnPushToTalkChange(bool b)
    {
        if (View.IsMine)
        {
#if !UNITY_WEBGL && PVOICE
            PushToTalk = b;
#endif
        }
    }
}