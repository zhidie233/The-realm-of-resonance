using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using static Resonance.Actors.ViewRay;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    public static class RuntimeServices
    {
        public static float MusicVolume = 1;

        public static RuntimeSettingsProfile Settings => ProjectConfigRoot.Instance.RuntimeSettings;
        public static PlayerRig LocalPlayerReferences => GameController.Instance.LocalPlayerReferences;
        public static List<FirearmData> WeaponTable => ProjectConfigRoot.Instance.WeaponTable;

        public const string LOCAL_PLAYER_TAG = "Player";
        public const string REMOTE_PLAYER_TAG = "RemoteActor";
        public const string AI_TAG = "AI";
        public const string HITBOX_TAG = "HitZone";

        public static class LocalPlayer
        {
            public static int ViewID => GameController.LocalPlayerViewID;
            public static GamePlayer GameActor => GameController.Instance.LocalActor;
            public static Team Team { get; set; }

            public static class Stats
            {
                public static int GetAllTimeKills() =>
                    PlayerPrefs.GetInt(NetProtocol.GetUniqueKeyForPlayer("kills", NetworkWrapper.NickName));

                public static int GetAllTimeDeaths() =>
                    PlayerPrefs.GetInt(NetProtocol.GetUniqueKeyForPlayer("deaths", NetworkWrapper.NickName));

                public static int GetAllTimeScore() =>
                    PlayerPrefs.GetInt(NetProtocol.GetUniqueKeyForPlayer("score", NetworkWrapper.NickName));
            }

            public static bool Suicide(bool increaseWarnings = true)
            {
                if (!PhotonNetwork.InRoom || GameController.Instance.LocalPlayerReferences == null) return false;

                var pdm = GameController.Instance.LocalPlayerReferences.playerHealthManager;
                if (!pdm.Suicide()) return false;

                CommonUtil.LockCursor(true);
                if (increaseWarnings) GameController.SuicideCount++;
                if (GameController.SuicideCount > ProjectConfigRoot.Instance.suicideLimit)
                {
                    IsAlive = false;
                    CommonUtil.LockCursor(false);
                    LobbyMenu.Instance.LeaveRoom();
                }
                return true;
            }

            public static bool IsAlive
            {
                get => GameController.Instance.LocalActor.isAlive;
                set => GameController.Instance.LocalActor.isAlive = value;
            }

            public static string FullNickName() => $"{ProjectConfigRoot.Instance.RolePrefix} {NetworkWrapper.NickName}";

            public static void IgnoreColliders(Collider[] colliders, bool ignore)
            {
                if (LocalPlayerReferences == null) return;
                LocalPlayerReferences.playerRagdoll.IgnoreColliders(colliders, ignore);
            }

            public static byte AddCameraRayDetection(DetectableInfo detectableInfo)
            {
                if (LocalPlayerReferences == null) return 0;
                return LocalPlayerReferences.cameraRay.AddTrigger(detectableInfo);
            }

            public static void RemoveCameraDetection(DetectableInfo detectableInfo)
            {
                if (LocalPlayerReferences == null) return;
                LocalPlayerReferences.cameraRay.RemoveTrigger(detectableInfo);
            }
        }

        public static class Network
        {
            public static void SendNetworkCall(byte code, Hashtable data) => NetworkWrapper.Instance.SendDataOverNetwork(code, data);
        }

        public static class Utility
        {
            public static bool IsLocalPlayer(Player player) => player.ActorNumber == NetworkWrapper.LocalPlayer.ActorNumber;
            public static bool IsLocalPlayer(int viewID) => viewID == LocalPlayer.ViewID;
        }

        public static class GameData
        {
            public static bool UsingReadyRoom() => ProjectConfigRoot.Instance.lobbyAccess == LobbyAccess.ReadyRoom;
        }
    }
}
