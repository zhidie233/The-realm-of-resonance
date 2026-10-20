using Photon.Realtime;
using UnityEngine;

public class TeamDeathMatchMode : NetworkHelper, IGameMode
{

    void Awake()
    {
        if (!NetworkWrapper.IsConnected)
            return;

        Initialize();
    }

    #region Interface
    public void Initialize()
    {
        //检查这是否是当前房间的游戏模式
        if (GameController.Instance.IsGameMode(GameMode.TDM, this))
        {
            GameController.Instance.SetGameState(MatchState.Starting);
            TeamDeathMatchUI.Instance.ShowUp();
        }
        else
        {
            TeamDeathMatchUI.Instance.Hide();
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        //判定胜者
        string finalText = "";
        if(!NetworkWrapper.OfflineMode && GetWinnerTeam() != Team.None)
        {
            finalText = GetWinnerTeam().GetTeamName();
        }
        else
        {
            finalText = GameTexts.NoOneWonName;
        }

        RoundFinishScreenBase.Instance?.Show(finalText);
    }

    public void OnLocalPlayerKill()
    {
        NetworkWrapper.CurrentRoom.SetTeamScore(NetworkWrapper.LocalPlayer.GetPlayerTeam());
    }

    public void OnLocalPoint(int points, Team team)
    {
        NetworkWrapper.CurrentRoom.SetTeamScore(team);
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(NetworkKeys.Team1Score) || propertiesThatChanged.ContainsKey(NetworkKeys.Team2Score))
        {
            int team1 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = NetworkWrapper.CurrentRoom.GetRoomScore(Team.Team2);
            TeamDeathMatchUI.Instance.SetScores(team1, team2);
            CheckScores(team1, team2);
        }
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {   
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {       
    }

    public void OnLocalPlayerDeath()
    { 
    }

    public bool isLocalPlayerWinner
    {
        get
        {         
            return GetWinnerTeam() == NetworkWrapper.LocalPlayer.GetPlayerTeam();
        }
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
    #endregion

    void CheckScores(int team1, int team2)
    {
        if (NetworkWrapper.OfflineMode || !RoomSettings.Instance.RoomInfoFetched) return;
        
        //检查是否有队伍达到最大击杀数
        if (team1 >= RoomSettings.Instance.GameGoal)
        {
            MatchTimeManagerBase.Instance.FinishRound();
            return;
        }
        if (team2 >= RoomSettings.Instance.GameGoal)
        {
            MatchTimeManagerBase.Instance.FinishRound();
        }
    }
}