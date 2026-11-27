using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Resonance.Foundation;
using Resonance.Domain.Anomaly;
using Resonance.Core;
using Resonance.Multiplayer;
using Resonance.UI;
namespace Resonance.Match
{

    public class FlagCaptureMode : NetworkActorBehaviour, IGameMode
    {
        [Header("Settings")]
        public int scorePerCapture = 500;
        public int scorePerRecover = 100;
        public float captureAreaRange = 3;
        [GameToggle] public bool moveFlagWithCarrierMotion = true;

        [Header("Events (For Local Only)")]
        public UnityEngine.Events.UnityEvent onPickUp;
        public UnityEngine.Events.UnityEvent onCapture;
        public UnityEngine.Events.UnityEvent onRecover;

        [Header("References")]
        public GameObject Content;
        public FlagPoint Team1Flag;
        public FlagPoint Team2Flag;

        private bool isActive = false;
        public int CapturesToWin { get; set; } = 3;

        void Awake()
        {
            if (!NetworkWrapper.IsConnected)
                return;

            Initialize();
        }

        void OnDisable()
        {
            if (isActive)
            {
                NetworkWrapper.RemoveNetworkCallback(OnNetworkMessage);
            }
        }

        void LateUpdate()
        {
            if (Team1Flag) Team1Flag.HandleFlagCapture();
            if (Team2Flag) Team2Flag.HandleFlagCapture();
        }

        void CheckScores()
        {
            if (!RoomSettings.Instance.RoomInfoFetched) return;

            int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);

            if (team1 >= CapturesToWin || team2 >= CapturesToWin)
            {
                MatchTimeManager.Instance.FinishRound();
            }
        }

        void OnNetworkMessage(Hashtable data)
        {
            var command = (int)data["cmd"];
            switch (command)
            {
                case 0:
                    OnFlagStateChanged(data);
                    break;
                case 1:
                    var flag = (Team)data["team"] == Team.Team1 ? Team1Flag : Team2Flag;
                    flag.DropFlag(data);
                    break;
                case 2:
                    flag = (Team)data["team"] == Team.Team1 ? Team1Flag : Team2Flag;
                    flag.SetFlagToOrigin();
                    break;
                case 3:
                    SyncFlags(data);
                    break;
                default:
                    return;
            }
        }

        void OnFlagStateChanged(Hashtable data)
        {
            var flagTeam = (Team)data["team"];
            var newState = (FlagPoint.FlagState)data["state"];
            var flag = flagTeam == Team.Team1 ? Team1Flag : Team2Flag;
            var player = (Player)data["player"];

            flag.State = newState;
            switch (newState)
            {
                case FlagPoint.FlagState.Captured:
                    flag.OnCapture(player);
                    break;
                case FlagPoint.FlagState.PickUp:
                    flag.OnPickup(player, (int)data["viewID"]);
                    break;
                case FlagPoint.FlagState.InHome:
                    flag.Recover(player);
                    break;
            }
            switch (newState)
            {
                case FlagPoint.FlagState.PickUp:
                    EventBus.Publish(new ObjectiveStateChangedEvent(ResonanceObjectiveKind.FlagPickedUp));
                    break;
                case FlagPoint.FlagState.Captured:
                    EventBus.Publish(new ObjectiveStateChangedEvent(ResonanceObjectiveKind.FlagCaptured));
                    break;
                case FlagPoint.FlagState.InHome:
                    EventBus.Publish(new ObjectiveStateChangedEvent(ResonanceObjectiveKind.FlagRecovered));
                    break;
            }
        }

        void SyncFlags(Hashtable data)
        {
            Team1Flag.State = (FlagPoint.FlagState)data["f1s"];
            Team2Flag.State = (FlagPoint.FlagState)data["f2s"];

            if (Team1Flag.State == FlagPoint.FlagState.PickUp)
            {
                var player = GameController.Instance.FindActor((int)data["f1ca"]);
                if (player != null) Team1Flag.SetFlagToCarrier(player.GetComponent<PlayerConfig>());
            }

            if (Team2Flag.State == FlagPoint.FlagState.PickUp)
            {
                var player = GameController.Instance.FindActor((int)data["f2ca"]);
                if (player != null) Team2Flag.SetFlagToCarrier(player.GetComponent<PlayerConfig>());
            }
        }

        public void Initialize()
        {
            //检查这是否是当前房间的游戏模式
            if (GameController.Instance.IsGameMode(GameMode.CaptureMatch, this))
            {
                Content.SetActive(true);
                NetworkWrapper.Instance.AddCallback(NetProtocol.FlagCaptureEvent, OnNetworkMessage);
                GameController.Instance.WaitForPlayers(GameMode.CaptureMatch.GetGameModeInfo().MinPlayersToStart);
                FlagCaptureUI.Instance.ShowUp();
                CapturesToWin = (int)NetworkWrapper.CurrentRoom.CustomProperties[NetProtocol.RoomGoal];
                isActive = true;
            }
            else
            {
                Content.SetActive(false);
                FlagCaptureUI.Instance.Hide();
            }
        }

        public void OnFinishTime(bool gameOver)
        {
            //判定胜者
            string finalText = "";
            if (GetWinnerTeam() != Team.None)
            {
                finalText = GetWinnerTeam().GetTeamName();
            }
            else
            {
                finalText = GameTexts.NoOneWonName;
            }
            RoundFinishScreen.Instance?.Show(finalText);
        }

        public void OnLocalPlayerDeath()
        {

        }

        public void OnLocalPlayerKill()
        {

        }

        public void OnLocalPoint(int points, Team teamToAddPoint)
        {
            NetworkWrapper.CurrentRoom.SetTeamScore(teamToAddPoint);
        }

        public void OnOtherPlayerEnter(Player newPlayer)
        {
            if (NetworkWrapper.IsMasterClient)
            {
                var data = NetPropertyStore.CreateBag();
                data.Add("cmd", 3);
                data.Add("f1s", Team1Flag.State);
                data.Add("f2s", Team2Flag.State);
                if (Team1Flag.State == FlagPoint.FlagState.PickUp)
                {
                    data.Add("f1ca", Team1Flag.carryingPlayer.View.ViewID);
                }
                if (Team2Flag.State == FlagPoint.FlagState.PickUp)
                {
                    data.Add("f2ca", Team1Flag.carryingPlayer.View.ViewID);
                }
                NetworkWrapper.Instance.SendDataOverNetworkToPlayer(NetProtocol.FlagCaptureEvent, data, newPlayer);
            }
        }

        public void OnOtherPlayerLeave(Player otherPlayer)
        {

        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged.ContainsKey(NetProtocol.Team1Score) || propertiesThatChanged.ContainsKey(NetProtocol.Team2Score))
            {
                int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
                int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);
                FlagCaptureUI.Instance.SetScores(team1, team2);
                CheckScores();
            }
        }

        public bool isLocalPlayerWinner
        {
            get
            {
                return (NetworkWrapper.LocalPlayer.GetPlayerTeam() == GetWinnerTeam());
            }
        }

        public FlagPoint GetFlag(Team team)
        {
            if (team == Team.Team1)
            {
                return Team1Flag;
            }

            return Team2Flag;
        }

        public Team GetWinnerTeam()
        {
            int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);

            Team winner = Team.None;
            if (team1 > team2) { winner = Team.Team1; }
            else if (team1 < team2) { winner = Team.Team2; }
            else { winner = Team.None; }
            return winner;
        }

        public static Team GetOppositeTeam(Team team)
        {
            if (team == Team.Team1)
            {
                return Team.Team2;
            }

            return Team.Team1;
        }

        private static FlagCaptureMode _instance;
        public static FlagCaptureMode Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<FlagCaptureMode>(); }
                return _instance;
            }
        }
    }
}
