using UnityEngine;
using TMPro;

public class CountDownHandler : MonoBehaviour
{
    /// 当前是否正在倒计时？
    public virtual bool IsCounting
    {
        get;
        set;
    } = false;

    private static CountDownHandler _instance;
    public static CountDownHandler Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<CountDownHandler>(); }
            return _instance;
        }
    }

    public GameObject Content;
    public TextMeshProUGUI CountDownText;
    public AudioClip CountAudio;

    private Animator CountAnim;
    private AudioSource ASource;
    private int countDown = 5;

    private void OnEnable()
    {
        NetworkWrapper.AddNetworkCallback(NetworkKeys.CountdownEvent, OnNetworkEvent);
    }

    private void OnDisable()
    {
        NetworkWrapper.RemoveNetworkCallback(OnNetworkEvent);
    }

    void OnNetworkEvent(ExitGames.Client.Photon.Hashtable data)
    {
        OnReceiveCount((int)data["c"]);
    }

    public void StartCountDown(bool overrideIfStarted = false)
    {
        if (!NetworkWrapper.IsMasterClient) return;
        if (IsCounting && !overrideIfStarted)
        {
            return;
        }

        countDown = GameConfig.Instance.prematchTime;
        MatchTimeManager.Instance.SetTimeState(RoomTimeState.Countdown, true);
        GameController.Instance.SetGameState(MatchState.Starting);
        IsCounting = true;

        InvokeRepeating(nameof(SetCountDown), 1, 1);
    }

    public void SetCount(int count)
    {
        OnCountChanged(count);
    }

    void SetCountDown()
    {
        countDown--;
        if (countDown <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            IsCounting = false;
        }

        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("c", countDown);
        NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.CountdownEvent, data);
    }

    void OnReceiveCount(int count)
    {
        if (!NetworkWrapper.IsMasterClient)
        {
            countDown = count;
        }
        if (countDown <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            MatchTimeManager.Instance.InitAfterCountdown();
        }
        else
        {
            MatchTimeManager.Instance.SetTimeState(RoomTimeState.Countdown);
        }
        OnCountChanged(countDown);
    }

    private void OnCountChanged(int count)
    {
        if (CountAudio != null)
        {
            if (ASource == null) { ASource = GetComponent<AudioSource>(); }
            ASource.clip = CountAudio;
            ASource.Play();
        }

        CountDownText.text = count.ToString();
        if(count > 0)
        {
            Content.SetActive(true);

            CountAnim = Content.GetComponent<Animator>();
            CountAnim.Play("count", 0, 0);
        }
        else
        {
            Content.SetActive(false);
        }
    }
}
