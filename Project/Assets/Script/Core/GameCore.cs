using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using static ViewRayBase;

public static class GameCore
{
    public static float MusicVolume = 1;

    public static RuntimeSettingsProfile Settings => GameConfig.Instance.RuntimeSettings;
    public static PlayerRig LocalPlayerReferences => GameController.Instance.LocalPlayerReferences;
    public static List<FirearmData> WeaponTable => GameConfig.Instance.WeaponTable;

    public const string LOCAL_PLAYER_TAG = "Player";
    public const string REMOTE_PLAYER_TAG = "Remote";
    public const string AI_TAG = "AI";
    public const string HITBOX_TAG = "BodyPart";
    /// 只与本地玩家相关的辅助工具类。
    public static class LocalPlayer
    {
        public static int ViewID => GameController.LocalPlayerViewID;
        public static GamePlayer GameActor => GameController.Instance.LocalActor;
        /// 本地玩家所在的队伍
        public static Team Team
        {
            get;
            set;
        }
        /// 本地玩家的战绩
        public static class Stats
        {
            /// 获取本地玩家的总击杀数
            public static int GetAllTimeKills()
            {
                return PlayerPrefs.GetInt(NetworkKeys.GetUniqueKeyForPlayer("kills", NetworkWrapper.NickName));
            }
            /// 获取本地玩家的总死亡数
            public static int GetAllTimeDeaths()
            {
                return PlayerPrefs.GetInt(NetworkKeys.GetUniqueKeyForPlayer("deaths", NetworkWrapper.NickName));
            }
            /// 获取本地玩家的总得分
            public static int GetAllTimeScore()
            {
                return PlayerPrefs.GetInt(NetworkKeys.GetUniqueKeyForPlayer("score", NetworkWrapper.NickName));
            }
        }
        /// 让本地玩家死亡
        public static bool Suicide(bool increaseWarnings = true)
        {
            if (!PhotonNetwork.InRoom || GameController.Instance.LocalPlayerReferences == null) return false;

            var pdm = GameController.Instance.LocalPlayerReferences.playerHealthManager;
            if (!pdm.Suicide()) return false;

            CommonUtil.LockCursor(true);
            if(increaseWarnings)
            GameController.SuicideCount++;
            //如果玩家消极送死或滥杀自己，就踢出房间
            if (GameController.SuicideCount > GameConfig.Instance.selfKillLimit)
            {              
                IsAlive = false;
                CommonUtil.LockCursor(false);
                LobbyMenu.Instance.LeaveRoom();
            }
            return true;
        }
        /// 本地玩家是否存活？
        public static bool IsAlive
        {
            get
            {
                return GameController.Instance.LocalActor.isAlive;
            }
            set
            {
                GameController.Instance.LocalActor.isAlive = value;
            }
        }
        /// 返回带队伍/战队标签的本地玩家昵称（如果有）
        public static string FullNickName()
        {
            string nick = $"{GameConfig.Instance.RolePrefix} {NetworkWrapper.NickName}";
            return nick;
        }
        /// 让本地玩家忽略碰撞体
        public static void IgnoreColliders(Collider[] colliders, bool ignore)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.playerRagdoll.IgnoreColliders(colliders, ignore);
        }
        /// 让玩家在相机看向物体时检测到它
        public static byte AddCameraRayDetection(DetecableInfo detecableInfo)
        {
            if (LocalPlayerReferences == null) return 0;

            return LocalPlayerReferences.cameraRay.AddTrigger(detecableInfo);
        }

        public static void RemoveCameraDetection(DetecableInfo detecableInfo)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.cameraRay.RemoveTrigger(detecableInfo);
        }
    }

    public static class Network
    {
        /// 向同一房间内的所有其他客户端发送类 RPC 调用（无需 PhotonView）。
        public static void SendNetworkCall(byte code, Hashtable data) => NetworkWrapper.Instance.SendDataOverNetwork(code, data);
    }
    /// 游戏金币的静态函数
    public static class Coins
    {
        /// 按 ID 获取 GameData 中列出的金币。
        public static GameCoin GetCoinData(int coinID)
        {
            if (coinID >= GameConfig.Instance.gameCoins.Count) return null;

            return GameConfig.Instance.gameCoins[coinID];
        }
        /// 按金币数据获取它的索引/ID
        public static int GetIndexOfCoin(GameCoin coin)
        {
            return GetAllCoins().FindIndex(x => x.CoinName == coin.CoinName);
        }

        public static List<GameCoin> GetAllCoins() => GameConfig.Instance.gameCoins;
    }

    public static class Utility
    {
        /// 检查给定的 Photon 玩家是否是本地玩家
        /// <param name="player">要检查是否为本地玩家的 Player</param>
        public static bool IsLocalPlayer(Player player)
        {
            return player.ActorNumber == NetworkWrapper.LocalPlayer.ActorNumber;
        }
        /// 检查给定的 viewID 是否是本地玩家
        /// <param name="player">要检查是否为本地玩家的 ViewID</param>
        public static bool IsLocalPlayer(int viewID)
        {
            return viewID == LocalPlayer.ViewID;
        }
    }

    public static class GameData
    {
        public static bool UsingReadyRoom()
        {
            return GameConfig.Instance.lobbyAccess == LobbyAccess.ReadyRoom;
        }
    }
}
