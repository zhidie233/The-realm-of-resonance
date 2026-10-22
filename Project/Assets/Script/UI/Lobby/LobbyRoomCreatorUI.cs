using UnityEngine;
using System.Linq;
using TMPro;

public class LobbyRoomCreatorUI : MonoBehaviour
{
    [Header("References")]
    public SingleSettingsBinding MapSettingsSelector;
    public SingleSettingsBinding MaxPlayersSelector;
    public SingleSettingsBinding GameGoalSelector;
    public SingleSettingsBinding TimeLimitSelector;
    public SingleSettingsBinding GameModeSelector;
    public SingleSettingsBinding MaxPingSelector;
    public SingleSettingsBinding BotsActiveSelector;
    public SingleSettingsBinding FriendlyFireSelector;
    public SingleSettingsBinding PerRoundSelector;
    public SingleSettingsBinding TeamSelectionSelector;
    public TMP_InputField roomInputField;
    public TMP_InputField roomPasswordInputField;
    public GameRoomPreview roomPreview;

    private GameModeSettings[] allModes;
    private string defaultRoomName = "Server";
    private string[] endPoints = new string[]
    {
        "人","分钟"
    };

    public void SetupSelectors()
    {
        defaultRoomName = $"{LobbyRoomCreator.Instance.defaultRoomPrefix} {Random.Range(10, 9999)}";
        roomInputField.text = defaultRoomName;

        MapSettingsSelector.SetOptions(LobbyRoomCreator.Instance.MapList.Select(x => x.ShowName).ToArray());
        var allModes = GetModesForCurrentMap();
        GameModeSelector.SetOptions(allModes.Select(x => x.ModeName).ToArray());
        MaxPingSelector.SetOptions(MatchLobby.Instance.MaxPing.AsStringArray(" ms"));

        SetupGameModeDependentsSelectors();
    }

    void SetupGameModeDependentsSelectors()
    {
        allModes = GetModesForCurrentMap();
        var gameMode = allModes[GameMode];

        MaxPlayersSelector.SetOptions(gameMode.maxPlayers.AsStringArray($" {endPoints[0]}"));
        TimeLimitSelector.SetOptions(gameMode.roundTimes.Select(x => $"{(x / 60)} {endPoints[1]}").ToArray());

        if (gameMode.GoalValueOptions.Length > 0)
            GameGoalSelector.SetOptions(gameMode.GoalValueOptions.AsStringArray($" {gameMode.GoalName}"));
        else GameGoalSelector.SetOptions(new string[1] { gameMode.GoalName });

        BotsActiveSelector.gameObject.SetActive(gameMode.botsEnabled);
        TeamSelectionSelector.SetActive(!gameMode.AutoAssignTeams);
        PerRoundSelector.SetActive(gameMode.RoundFormat == GameModeSettings.RoundFormatOptions.AllowBoth);
    }

    public void OnChangeGameMode(int id)
    {
        GameGoalSelector.currentOption = 0;
        MaxPlayersSelector.currentOption = 0;
        TimeLimitSelector.currentOption = 0;
        SetupGameModeDependentsSelectors();
    }

    public void OnChangeMap(int id)
    {
        roomPreview?.Show(LobbyRoomCreator.Instance.BuildRoomInfo());
        var allModes = GetModesForCurrentMap();
        GameModeSelector.SetOptions(allModes.Select(x => x.ModeName).ToArray());
        SetupGameModeDependentsSelectors();
    }

    private GameModeSettings[] GetModesForCurrentMap()
    {
        var currentMap = LobbyRoomCreator.Instance.MapList[MapSettingsSelector.currentOption];
        return currentMap.GetAllowedGameModes(MatchLobby.Instance.GameModes);
    }

    public void OnChangeRoomPrivate(bool toPrivate) { if (!toPrivate) roomPasswordInputField.text = ""; }
    #region Getters
    public int Map => MapSettingsSelector.currentOption;
    public int GameMode => GameModeSelector.currentOption;
    public GameModeSettings CurrentGameMode
    {
        get
        {
            if(allModes == null)
            {
                allModes = GetModesForCurrentMap();
            }
            return allModes[GameModeSelector.currentOption];
        }
    }
    public int MaxPlayer => MaxPlayersSelector.currentOption;
    public int Goal => GameGoalSelector.currentOption;
    public int TimeLimit => TimeLimitSelector.currentOption;
    public int MaxPing => MaxPingSelector.currentOption;
    public string RoomName => roomInputField.text;
    public string RoomPassword => roomPasswordInputField.text;
    public bool IsPrivate => !string.IsNullOrEmpty(roomPasswordInputField.text);
    public bool FriendlyFire => FriendlyFireSelector.CurrentSelectionAsBool();
    public bool BotsActive => BotsActiveSelector.CurrentSelectionAsBool();
    public bool AutoAssignTeams => TeamSelectionSelector.CurrentSelectionAsBool();
    public RoundStyle GamePerRound => PerRoundSelector.CurrentSelectionAsBool() == true ? RoundStyle.Rounds : RoundStyle.OneMacht;
    #endregion

    private static LobbyRoomCreatorUI _instance;
    public static LobbyRoomCreatorUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LobbyRoomCreatorUI>();
            }
            if (_instance == null && LobbyUI.Instance != null)
            {
                _instance = LobbyUI.Instance.GetComponentInChildren<LobbyRoomCreatorUI>(true);
            }
            return _instance;
        }set => _instance = value;
    }
}