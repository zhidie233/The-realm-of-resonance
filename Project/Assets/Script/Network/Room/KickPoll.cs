using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class KickPoll : MonoBehaviourBase
{
    [SerializeField] private KeyCode YesKey = KeyCode.F1;
    [SerializeField] private KeyCode NoKey = KeyCode.F2;

    private bool IsOpen = false;
    private int YesCount = 0;
    private int NoCount = 0;
    private KickPollUI UI;
    private Player TargetPlayer;
    private bool isAgainMy = false;
    private bool Voted = false;
    private int AllVoters = 0;

    protected override void Awake()
    {
        base.Awake();
        UI = FindObjectOfType<KickPollUI>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        NetworkCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
        NetworkWrapper.Instance.AddCallback(NetworkKeys.VoteEvent, OnNetworkReceived);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        NetworkCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
        NetworkWrapper.Instance?.RemoveCallback(OnNetworkReceived);
    }

    void OnNetworkReceived(Hashtable data)
    {
        CallType ct = (CallType)data["type"];
        switch(ct)
        {
            case CallType.VoteStart:
                VoteStart(data);
                break;
            case CallType.Vote:
                OnVote(data);
                break;
            case CallType.VoteEnd:
                EndVote(data);
                break;
        }
    }

    public void RequestKick(Player player)
    {
        if (IsOpen || player == null)
            return;
        if(NetworkWrapper.PlayerList.Length < 3)
        {
            Debug.Log("there are not enough players.");
            return;
        }
        if (player.ActorNumber == NetworkWrapper.LocalPlayer.ActorNumber)
        {
            Debug.Log("You can not send a vote for yourself.");
            return;
        }

        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("type", CallType.VoteStart);
        data.Add("player", player);
        data.Add("by", NetworkWrapper.LocalPlayer);
        GameCore.Network.SendNetworkCall(NetworkKeys.VoteEvent, data);
    }

    void ResetVote()
    {
        YesCount = 0;
        NoCount = 0;
        isAgainMy = false;
        Voted = false;
    }

    public override void OnUpdate()
    {
        if (!IsOpen || isAgainMy || Voted)
            return;
        if (TargetPlayer == null)
            return;

        if (Input.GetKeyDown(YesKey))
        {
            SendVote(true);
            Voted = true;
        }
        else if (Input.GetKeyDown(NoKey))
        {
            SendVote(false);
            Voted = true;
        }
    }

    void SendVote(bool yes)
    {
        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("type", CallType.Vote);
        data.Add("vote", yes);
        GameCore.Network.SendNetworkCall(NetworkKeys.VoteEvent, data);

        UI.OnSendLocalVote(yes);
    }

    void OnVote(Hashtable data)
    {
        var vote = (bool)data["vote"];
        if (vote)
        {
            YesCount++;
        }
        else
        {
            NoCount++;
        }
        UI.OnReceiveVote(YesCount, NoCount);
        if (NetworkWrapper.IsMasterClient)//master count all votes to determine if kick or not
        {
            CountVotes();
        }
    }

    void VoteStart(Hashtable data)
    {
        if (IsOpen)
            return;

        var player = (Player)data["player"];
        var by = (Player)data["by"];

        AllVoters = NetworkWrapper.PlayerListOthers.Length;
        TargetPlayer = player;
        ResetVote();
        isAgainMy = (player.ActorNumber == NetworkWrapper.LocalPlayer.ActorNumber);
        UI.OpenVotatation(player, by);
        IsOpen = true;
    }

    void EndVote(Hashtable data)
    {
        var kicked = (bool)data["kick"];

        IsOpen = false;
        Voted = true;
        UI.OnFinish(kicked);
        TargetPlayer = null;
    }

    void CountVotes()
    {
        int half = (AllVoters / 2);
        bool kicked = false;
        var data = CommonUtil.CreatePhotonHashTable();
        data.Add("type", CallType.VoteEnd);

        if (YesCount > half)//kick
        {
            NetworkWrapper.Instance.KickPlayer(TargetPlayer);
            kicked = true;
        }

        data.Add("kick", kicked);
        GameCore.Network.SendNetworkCall(NetworkKeys.VoteEvent, data);
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
        if (TargetPlayer == null)
            return;

       if(otherPlayer.ActorNumber == TargetPlayer.ActorNumber)
        {
            //玩家自己离开房间，取消投票
            UI.OnFinish(true);
        }
    }

    public enum CallType
    {
        VoteStart = 0,
        Vote,
        VoteEnd
    }

    private static KickPoll _instance;
    public static KickPoll Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<KickPoll>(); }
            return _instance;
        }
    }
}