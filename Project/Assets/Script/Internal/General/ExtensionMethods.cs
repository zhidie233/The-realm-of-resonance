using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using Photon.Realtime;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public static class ExtensionMethods
{
    /// 为指定玩家记分并在网络上同步
    public static void PostScore(this Player player, int ScoreToAdd = 0)
    {
        int current = player.GetPlayerScore();
        current = current + ScoreToAdd;
        Hashtable score = new Hashtable();
        score[NetworkKeys.ScoreKey] = current;
        player.SetCustomProperties(score);
    }
    /// 获取指定玩家的得分
    public static int GetPlayerScore(this Player player)
    {
        int s = 0;

        if (player.CustomProperties.ContainsKey(NetworkKeys.ScoreKey))
        {
            s = (int)player.CustomProperties[NetworkKeys.ScoreKey];
            return s;
        }

        return s;
    }

    public static int GetKills(this Player p)
    {
        int k = 0;
        if (p.CustomProperties.ContainsKey(NetworkKeys.KillsKey))
        {
            k = (int)p.CustomProperties[NetworkKeys.KillsKey];
            return k;
        }
        return k;
    }

    public static int GetDeaths(this Player p)
    {
        int d = 0;
        if (p.CustomProperties.ContainsKey(NetworkKeys.DeathsKey))
        {
            d = (int)p.CustomProperties[NetworkKeys.DeathsKey];
            return d;
        }
        return d;
    }

    public static void PostKill(this Player p, int kills)
    {
        int current = p.GetKills();
        current = current + kills;
        Hashtable score = new Hashtable();
        score[NetworkKeys.KillsKey] = current;
        p.SetCustomProperties(score);
    }

    public static void PostDeaths(this Player p, int deaths)
    {
        int current = p.GetDeaths();
        current = current + deaths;
        Hashtable score = new Hashtable();
        score[NetworkKeys.DeathsKey] = current;
        p.SetCustomProperties(score);
    }
    /// 获取指定队伍的得分
    public static int GetRoomScore(this Room room, Team team)
    {
        object teamId;
        if (team == Team.Team1)
        {
            if (room.CustomProperties.TryGetValue(NetworkKeys.Team1Score, out teamId))
            {
                return (int)teamId;
            }
        } else if (team == Team.Team2)
        {
            if (room.CustomProperties.TryGetValue(NetworkKeys.Team2Score, out teamId))
            {
                return (int)teamId;
            }
        }

        return 0;
    }
    /// 为指定队伍增加得分并在网络上同步
    public static void SetTeamScore(this Room r, Team t, int scoreToAdd = 1)
    {
        if (t == Team.None) return;

        int score = 0;
        score = r.GetRoomScore(t);
        score += scoreToAdd;
        string key = (t == Team.Team1) ? NetworkKeys.Team1Score : NetworkKeys.Team2Score;
        Hashtable h = new Hashtable();
        h.Add(key, score);
        r.SetCustomProperties(h);
    }
    /// 获取指定玩家所属的队伍
    public static Team GetPlayerTeam(this Player p)
    {
        if (p == null) return Team.None;
        object teamId;
        string t = (string)p.CustomProperties[NetworkKeys.TeamKey];
        if (p.CustomProperties.TryGetValue(NetworkKeys.TeamKey, out teamId))
        {
            switch ((string)teamId)
            {
                case "Team2":
                    return Team.Team2;
                case "Team1":
                    return Team.Team1;
                case "All":
                    return Team.All;
                case "None":
                    return Team.None;

            }
        }
        return Team.None;
    }
    /// 同步指定玩家所属的队伍
    public static void SetPlayerTeam(this Player player, Team team)
    {
        Hashtable PlayerTeam = new Hashtable();
        PlayerTeam.Add(NetworkKeys.TeamKey, team.ToString());
        player.SetCustomProperties(PlayerTeam);
    }
    /// 按标识获取队伍名称
    public static string GetTeamName(this Team t)
    {
        switch (t)
        {
            case Team.Team1:
                return GameConfig.Instance.Team1Name;
            case Team.Team2:
                return GameConfig.Instance.Team2Name;
            default:
                return "Solo";
        }
    }
    /// 按标识获取队伍颜色
    public static Color GetTeamColor(this Team t, float alpha = 0)
    {
        Color c = Color.white;
        switch (t)
        {
            case Team.Team1:
               c = GameConfig.Instance.Team1Color;
                break;
            case Team.Team2:
               c = GameConfig.Instance.Team2Color;
                break;
            case Team.All:
                c = Color.white;
                break;
        }
        if(alpha > 0) { c.a = alpha; }

        return c;
    }

    public static GameModeSettings GetModeInfo(this GameMode mode)
    {
        for (int i = 0; i < GameConfig.Instance.gameModeTable.Count; i++)
        {
            if(GameConfig.Instance.gameModeTable[i].gameMode == mode) { return GameConfig.Instance.gameModeTable[i]; }
        }
        return null;
    }
    /// 将指定玩家职业（本地）保存为默认值
    private const string PLAYER_CLASS_KEY = "{0}.playerclass";
    public static void SavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        PlayerPrefs.SetInt(key, (int)pc);
    }
    /// 获取本地保存的玩家职业
    public static PlayerClass GetSavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        int id = PlayerPrefs.GetInt(key, 0);
        PlayerClass pclass = (PlayerClass)id;

        return pclass;
    }
    /// 获取玩家名称及其用户角色（若有）
    public static string NickNameAndRole(this Player p)
    {
        object role = "";
        if (p.CustomProperties.TryGetValue(NetworkKeys.UserRole, out role))
        {
            return string.Format("<b>{1}</b> {0}", p.NickName, (string)role);
        }
        return p.NickName;
    }
    /// 按标识获取完整的游戏模式名称
    public static string GetName(this GameMode mode)
    {
        GameModeSettings info = mode.GetModeInfo();
        if(info != null) { return info.ModeName; }
        else { return string.Format("Not define: " + mode.ToString()); }
    }
    /// 按标识获取游戏模式信息
    public static GameModeSettings GetGameModeInfo(this GameMode mode)
    {
        return GameConfig.Instance.gameModeTable.Find(x => x.gameMode == mode);
    }
    /// 此玩家是否与本地玩家同队？
    public static bool isTeamMate(this Player p)
    {
        bool b = false;
        if(p.GetPlayerTeam() == NetworkWrapper.LocalPlayer.GetPlayerTeam()) { b = true; }
        return b;
    }
    /// 获取指定队伍的玩家列表
    public static Player[] GetPlayersInTeam(this Player[] player, Team team)
    {
        List<Player> list = new List<Player>();
        for(int i = 0; i < NetworkWrapper.PlayerList.Length; i++)
        {
            if(NetworkWrapper.PlayerList[i].GetPlayerTeam() == team) { list.Add(NetworkWrapper.PlayerList[i]); }
        }
        return list.ToArray();
    }

    public static Player GetPlayerWithHighestScore(Player[] playerList)
    {
        if (playerList == null) return null;
        if (playerList.Length <= 1) return playerList[0];

        int high = 0, index = 0;
        for (int i = 0; i < playerList.Length; i++)
        {
            int h = playerList[i].GetPlayerScore();
            if(h > high)
            {
                high = h;
                index = i;
            }
        }

        return playerList[index];
    }
    /// 获取当前游戏模式
    public static GameMode GetGameMode(this RoomInfo room)
    {
        string gm = (string)room.CustomProperties[NetworkKeys.GameModeKey];
        GameMode mode = (GameMode)Enum.Parse(typeof(GameMode), gm);
        return mode;
    }
    /// 获取已解析到自定义 Game 类中的当前房间信息
    public static GameRoomInfo GetRoomInfo(this Room room)
    {
        return new GameRoomInfo(room);
    }
    /// 检查指定队伍是否为本地玩家的敌方队伍
    public static Team OppsositeTeam(this Team team)
    {
        if (team == Team.Team1) { return Team.Team2; }
        else if (team == Team.Team2) { return Team.Team1; }
        else
        {
            return Team.All;
        }
    }
    /// 延时调用方法的更好方案
    public static void InvokeAfter(this MonoBehaviour mono, float time, Action callback)
    {
        mono.StartCoroutine(WaitToExecute(time, callback));
    }

    static IEnumerator WaitToExecute(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        if (callback != null) callback.Invoke();
    }
    /// 调整 RawImage 尺寸以适配父 RectTransform 大小
    public static Vector2 SizeToParent(this RawImage image, float padding = 0)
    {
        float w = 0, h = 0;
        var parent = image.transform.parent.GetComponent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();
        // 检查是否有需要处理的内容
        if (image.texture != null)
        {
            if (!parent) { return imageTransform.sizeDelta; }
            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //图片旋转时反转边界
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //先按高度计算尺寸
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            {
                w = bounds.width * padding;
                h = w / ratio;
            }
        }
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }
    /// 随机化部分 AudioSource 属性以获得不同的音效结果
    public static void RandomizeAudioOutput(this AudioSource source)
    {
        source.pitch = UnityEngine.Random.Range(0.92f, 1.1f);
        source.spread = UnityEngine.Random.Range(0.98f, 1.25f);
    }
    /// 检查该碰撞体是否属于本地玩家
    public static bool isLocalPlayerCollider(this Collider collider)
    {
        return collider.CompareTag(GameCore.LOCAL_PLAYER_TAG);
    }
    /// 将 int 数组按指定字符拼接为字符串数组
    public static string[] AsStringArray(this int[] array, string endoint = "") => array.Select(x => (x.ToString() + endoint)).ToArray();
    /// 检查指定枚举标志属性中是否选中某个标志
    public static bool IsEnumFlagPresent<T>(this T value, T lookingForFlag) where T : Enum
    {
        int intValue = (int)(object)value;
        int intLookingForFlag = (int)(object)lookingForFlag;
        return ((intValue & intLookingForFlag) == intLookingForFlag);
    }
    public static GamePlayer[] ToGamePlayerList(this Player[] list)
    {
        var gameList = new GamePlayer[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            gameList[i] = GameController.Instance.GetGamePlayer(list[i].NickName);
        }
        return gameList;
    }
}
