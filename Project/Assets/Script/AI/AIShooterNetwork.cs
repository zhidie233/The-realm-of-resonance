using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using Game.Runtime.AI;

public sealed class AIShooterNetwork : MonoBehaviourBase, IPunObservable
{
    public NavMeshAgent Agent { get; set; }
    public Vector3 Velocity { get; set; }
    public Transform AimTarget { get; set; }

    private Transform m_Transform;
    private int receivePackages = 0;
    private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this
    private Vector3 networkLookAtPosition = Vector3.zero;

    private GameBotProperties BotStat = null;

    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        Agent = References.Agent;

        AIMananger.OnMaterStatsReceived += OnMasterStatsReceived;
        AIMananger.OnBotStatUpdate += OnBotStatUpdate;

        GetEssentialData();
        if (photonView.IsMine)
        {
            OnLocalStart();
        }
        else
        {
            OnNetworkStart();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        RegisterPlayerSpawn();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        AIMananger.OnMaterStatsReceived -= OnMasterStatsReceived;
        AIMananger.OnBotStatUpdate -= OnBotStatUpdate;
    }

    void OnLocalStart()
    {

    }

    void OnNetworkStart()
    {
        References.aiShooter.Init();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Transform.localPosition);
            stream.SendNext(m_Transform.localRotation);
            stream.SendNext(Agent.velocity);
            stream.SendNext(References.aiShooter.LookAtPosition);
        }
        else
        {
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            Velocity = (Vector3)stream.ReceiveNext();
            networkLookAtPosition = (Vector3)stream.ReceiveNext();

            //Fix the translation effect on remote clients
            if (receivePackages < 5)
            {
                m_Transform.localPosition = correctPlayerPos;
                m_Transform.localRotation = correctPlayerRot;
                receivePackages++;
            }
        }
    }

    public override void OnUpdate()
    {
        if (!PhotonNetworkWrapper.IsMasterClient)//if not master client, then get position from server
        {
            m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, correctPlayerPos, Time.deltaTime * 7);
            m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, correctPlayerRot, Time.deltaTime * 7);
            References.aiShooter.LookAtPosition = Vector3.Lerp(References.aiShooter.LookAtPosition, networkLookAtPosition, Time.deltaTime * 5);
        }
        else
        {
            Velocity = Agent.velocity;
            if (MatchTimeManagerBase.Instance.IsTimeUp())
            {
                if(Agent.enabled) Agent.isStopped = true;
                return;
            }
        }
    }

    void GetEssentialData()
    {
        object[] data = photonView.InstantiationData;
        AIName = (string)data[0];
        AITeam = (Team)data[1];
        gameObject.name = AIName;
        CheckNamePlate();
        //since Non master client doesn't update the view ID when bots are created, lets do it on Start
        if (!PhotonNetworkWrapper.IsMasterClient)
        {
            AIMananger.UpdateBotView(References.aiShooter, photonView.ViewID);
        }
    }

    void OnMasterStatsReceived(List<GameBotProperties> stats)
    {
        ApplyMasterInfo(stats);
    }

    void ApplyMasterInfo(List<GameBotProperties> stats)
    {
        int viewID = photonView.ViewID;
        GameBotProperties bs = stats.Find(x => x.ViewID == viewID);
        if (bs != null)
        {
            AIName = bs.Name;
            AITeam = bs.Team;
            gameObject.name = AIName;
            BotStat = new GameBotProperties();
            BotStat.Name = AIName;
            BotStat.Score = bs.Score;
            BotStat.Kills = bs.Kills;
            BotStat.Deaths = bs.Deaths;
            BotStat.ViewID = bs.ViewID;

            RegisterPlayerSpawn();

            CheckNamePlate();
        }
    }

    void OnBotStatUpdate(GameBotProperties stat)
    {
        if (stat.ViewID != photonView.ViewID) return;

        BotStat = stat;
        AIName = stat.Name;
        AITeam = BotStat.Team;
        gameObject.name = AIName;

        RegisterPlayerSpawn();
    }

    private void RegisterPlayerSpawn()
    {
        string name = string.IsNullOrEmpty(AIName) ? (string)photonView.InstantiationData[0] : AIName;
        GameEvents.DispatchRemoteActorChange(new GameEvents.PlayerChangeData()
        {
            PlayerName = name,
            GameActor = BuildPlayer(),
            IsAlive = true,
            NetworkView = photonView
        });
    }

    private GamePlayer BuildPlayer(bool isAlive = true)
    {
        GamePlayer player = new GamePlayer()
        {
            Name = AIName,
            ActorView = photonView,
            isRealPlayer = false,
            Actor = transform,
            AimPosition = AimTarget,
            Team = AITeam,
            isAlive = isAlive,
        };
        return player;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEvents.DispatchRemoteActorChange(new GameEvents.PlayerChangeData()
        {
            PlayerName = AIName,
            GameActor = BuildPlayer(false),
            IsAlive = false,
        });
    }

    public void CheckNamePlate()
    {
        References.namePlateDrawer.SetName(AIName);
        if (!isOneTeamMode && GameController.Instance.LocalPlayer != null && !References.aiShooter.isDeath)
        {
            References.namePlateDrawer.SetActive(GameCore.LocalPlayer.Team == AITeam);
        }
        else
        {
            References.namePlateDrawer.SetActive(false);
        }
    }

    private string AIName { get => References.aiShooter.AIName; set => References.aiShooter.AIName = value; }
    private Team AITeam { get => References.aiShooter.AITeam; set => References.aiShooter.AITeam = value; }

    private AIShooterReferences _References;
    public AIShooterReferences References
    {
        get
        {
            if (_References == null) _References = GetComponent<AIShooterReferences>();
            return _References;
        }
    }
}