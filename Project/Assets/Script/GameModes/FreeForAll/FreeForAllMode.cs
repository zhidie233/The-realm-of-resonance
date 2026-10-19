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
        if (!NetworkWrapper.IsConnected)
            return;

        Initialize();
    }

    void OnDisable()
    {
        if (isSub)
        {
            NetworkCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        }
    }

    public void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(NetworkKeys.KillsKey))
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
            FFAPlayerSort.Sort(CommonUtil.GetSortPlayerByKills);
            player = FFAPlayerSort[0];
        }
        else
        {
            player = GameController.Instance.LocalActor;
        }
        FreeForAllUI.Instance.SetScores(player);
        //检查最佳玩家是否达到最大击杀数
        if((int)player.GetPlayerPropertie(NetworkKeys.KillsKey) >= RoomSettings.Instance.GameGoal && !NetworkWrapper.OfflineMode)
        {
            MatchTimeManagerBase.Instance.FinishRound();
            return;
        }
        //检查机器人是否还没达到最大击杀数
        if (AIManager.Instance != null && AIManager.Instance.BotsActive && AIManager.Instance.BotsStatistics.Count > 0)
        {
            if (AIManager.Instance.GetBotWithMoreKills().Kills >= RoomSettings.Instance.GameGoal)
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
            if (AIManager.Instance != null && AIManager.Instance.GetBotWithMoreKills().Kills >= RoomSettings.Instance.GameGoal)
            {
                winner = AIManager.Instance.GetBotWithMoreKills().Name;
            }
            return winner == NetworkWrapper.LocalPlayer.NickName;

        }
    }

    public void Initialize()
    {
        //检查这是否是当前房间的游戏模式
        if (GameController.Instance.IsGameMode(GameMode.FFA, this))
        {
            GameController.Instance.SetGameState(MatchState.Starting);
            NetworkCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
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