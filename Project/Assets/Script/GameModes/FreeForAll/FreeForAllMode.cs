using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Game.GameModes.FreeForAll;

public class FreeForAllMode : MonoBehaviour, IGameMode
{

    private bool isSub = false;
    [HideInInspector] public List<GamePlayer> FFAPlayerSort = new List<GamePlayer>();

    void Awake()
    {
        if (!PhotonNetworkWrapper.IsConnected)
            return;

        Initialize();
    }

    void OnDisable()
    {
        if (isSub)
        {
            PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        }
    }

    public void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PropertiesKeys.KillsKey))
        {
            CheckScore();
        }
    }

    void CheckScore()
    {
        if (!RoomSettings.Instance.RoomInfoFetched) return;

        FFAPlayerSort.Clear();
        FFAPlayerSort.AddRange(GameController.Instance.OthersActorsInScene);
        FFAPlayerSort.Add(GameController.Instance.LocalActor);

        GamePlayer player = null;
        if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
        {
            FFAPlayerSort.Sort(UtilityHelper.GetSortPlayerByKills);
            player = FFAPlayerSort[0];
        }
        else
        {
            player = GameController.Instance.LocalActor;
        }
        FreeForAllUI.Instance.SetScores(player);
        //check if the best player reach the max kills
        if((int)player.GetPlayerPropertie(PropertiesKeys.KillsKey) >= RoomSettings.Instance.GameGoal && !PhotonNetworkWrapper.OfflineMode)
        {
            MatchTimeManagerBase.Instance.FinishRound();
            return;
        }
        //check if bots have not reach max kills
        if (AIMananger.Instance != null && AIMananger.Instance.BotsActive && AIMananger.Instance.BotsStatistics.Count > 0)
        {
            if (AIMananger.Instance.GetBotWithMoreKills().Kills >= RoomSettings.Instance.GameGoal)
            {
                MatchTimeManagerBase.Instance.FinishRound();
            }
        }
    }

    public GamePlayer GetBestPlayer()
    {
        if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
        {
            return FFAPlayerSort[0];
        }
        else
        {
            return GameController.Instance.LocalActor;
        }
    }

    #region Interface
    public bool isLocalPlayerWinner
    {
        get
        {
            string winner = GetBestPlayer().Name;
            if (AIMananger.Instance != null && AIMananger.Instance.GetBotWithMoreKills().Kills >= RoomSettings.Instance.GameGoal)
            {
                winner = AIMananger.Instance.GetBotWithMoreKills().Name;
            }
            return winner == PhotonNetworkWrapper.LocalPlayer.NickName;

        }
    }

    public void Initialize()
    {
        //check if this is the game mode of this room
        if (GameController.Instance.IsGameMode(GameMode.FFA, this))
        {
            GameController.Instance.SetGameState(MatchState.Starting);
            PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            FreeForAllUI.Instance.ShowUp();
            isSub = true;
        }
        else
        {
            FreeForAllUI.Instance.Hide();
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        RoundFinishScreenBase.Instance?.Show(GetBestPlayer().Name);
    }

    public void OnLocalPlayerDeath()
    {

    }

    public void OnLocalPlayerKill()
    {

    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
        CheckScore();
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {

    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {

    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

    } 
    #endregion
}