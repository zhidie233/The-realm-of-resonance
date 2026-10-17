using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Game.Runtime.Settings;
using Game.Internal.Scriptables;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using static CameraRayBase;

public static class GameCore
{
    public static float MusicVolume = 1;

    public static RuntimeSettingsProfile Settings => GameConfig.Instance.RuntimeSettings;
    public static PlayerReferencesHandler LocalPlayerReferences => GameController.Instance.LocalPlayerReferences;
    public static List<GunData> AllWeapons => GameConfig.Instance.AllWeapons;

    public const string LOCAL_PLAYER_TAG = "Player";
    public const string REMOTE_PLAYER_TAG = "Remote";
    public const string AI_TAG = "AI";
    public const string HITBOX_TAG = "BodyPart";
    
    /// <summary>
    /// Class helper with some useful function referenced to the local player only.
    /// </summary>
    public static class LocalPlayer
    {
        public static int ViewID => GameController.LocalPlayerViewID;
        public static GamePlayer GameActor => GameController.Instance.LocalActor;

        /// <summary>
        /// The team of the local player
        /// </summary>
        public static Team Team
        {
            get;
            set;
        }

        /// <summary>
        /// Stats of the local player
        /// </summary>
        public static class Stats
        {
            /// <summary>
            /// Get the all time kill count of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeKills()
            {
                return PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("kills", PhotonNetworkWrapper.NickName));
            }

            /// <summary>
            /// Get the all time death count of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeDeaths()
            {
                return PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("deaths", PhotonNetworkWrapper.NickName));
            }

            /// <summary>
            /// Get the all time score of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeScore()
            {
                return PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", PhotonNetworkWrapper.NickName));
            }
        }

        /// <summary>
        /// Make the local player die
        /// </summary>
        public static bool Suicide(bool increaseWarnings = true)
        {
            if (!PhotonNetwork.InRoom || GameController.Instance.LocalPlayerReferences == null) return false;

            var pdm = GameController.Instance.LocalPlayerReferences.playerHealthManager;
            if (!pdm.Suicide()) return false;

            UtilityHelper.LockCursor(true);
            if(increaseWarnings)
            GameController.SuicideCount++;
            //if player is a joker o abuse of suicide, them kick of room
            if (GameController.SuicideCount > GameConfig.Instance.maxSuicideAttempts)
            {              
                IsAlive = false;
                UtilityHelper.LockCursor(false);
                RoomMenu.Instance.LeaveRoom();
            }
            return true;
        }

        /// <summary>
        /// Is the Local player alive?
        /// </summary>
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

        /// <summary>
        /// Will Return the local player nick name with the clan/crew tag (if any)
        /// </summary>
        /// <returns></returns>
        public static string FullNickName()
        {
            string nick = $"{GameConfig.Instance.RolePrefix} {PhotonNetworkWrapper.NickName}";
            return nick;
        }

        /// <summary>
        /// Make the local player ignore colliders
        /// </summary>
        public static void IgnoreColliders(Collider[] colliders, bool ignore)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.playerRagdoll.IgnoreColliders(colliders, ignore);
        }

        /// <summary>
        /// Make the player detect an object when the camera look at it
        /// </summary>
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
        /// <summary>
        /// Send a RPC-like call (without a Photon view required) to all other clients in the same room.
        /// </summary>
        public static void SendNetworkCall(byte code, Hashtable data) => PhotonNetworkWrapper.Instance.SendDataOverNetwork(code, data);
    }

    /// <summary>
    /// Static functions for the Game coins
    /// </summary>
    public static class Coins
    {
        /// <summary>
        /// Get one of the listed coins in GameData by their ID.
        /// </summary>
        /// <param name="coinID"></param>
        /// <returns></returns>
        public static GameCoin GetCoinData(int coinID)
        {
            if (coinID >= GameConfig.Instance.gameCoins.Count) return null;

            return GameConfig.Instance.gameCoins[coinID];
        }

        /// <summary>
        /// Get the coin index/id by the coin data
        /// </summary>
        /// <param name="coin"></param>
        /// <returns></returns>
        public static int GetIndexOfCoin(GameCoin coin)
        {
            return GetAllCoins().FindIndex(x => x.CoinName == coin.CoinName);
        }

        public static List<GameCoin> GetAllCoins() => GameConfig.Instance.gameCoins;
    }

    public static class Utility
    {

        /// <summary>
        /// Check if the given photon player is the local player
        /// </summary>
        /// <param name="player">Player to check if is local</param>
        /// <returns></returns>
        public static bool IsLocalPlayer(Player player)
        {
            return player.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber;
        }

        /// <summary>
        /// Check if the given viewID is the local player
        /// </summary>
        /// <param name="player">ViewID to check if is local</param>
        /// <returns></returns>
        public static bool IsLocalPlayer(int viewID)
        {
            return viewID == LocalPlayer.ViewID;
        }
    }

    public static class GameData
    {
        public static bool UsingWaitingRoom()
        {
            return GameConfig.Instance.lobbyJoinMethod == Game.Internal.Structures.LobbyJoinMethod.WaitingRoom;
        }
    }
}