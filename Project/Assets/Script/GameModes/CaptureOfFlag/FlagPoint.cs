using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Game.GameModes.CaptureOfFlag
{
    public class FlagPoint : PhotonHelper
    {
        public Team flagTeam;
        public FlagState State = FlagState.InHome;
        public Texture2D FlagIcon;
        public Transform IconTarget;
        [SerializeField] private Transform homeMark = null;
        public Vector2 IconSize = new Vector2(7, 7);
        public float ReturnTime;

        Vector3 originalPos, originalRot, originalScale;
        FlagPoint oppositeFlag;
        public PlayerConfig carriyingPlayer/* { get; set; } = null*/;
        private Color IconColor;

        void Awake()
        {
            originalPos = transform.position;
            originalRot = transform.eulerAngles;
            originalScale = transform.localScale;
            oppositeFlag = CaptureOfFlagController.Instance.GetFlag(CaptureOfFlagController.GetOppositeTeam(flagTeam));
            IconColor = flagTeam.GetTeamColor();
            if (homeMark != null) homeMark.parent = transform.parent;
            GameEvents.onLocalPlayerDeath += this.OnLocalPlayerDeath;
        }

        private void OnDestroy()
        {
            GameEvents.onLocalPlayerDeath -= this.OnLocalPlayerDeath;
        }

        public void ChangeFlagState(FlagState newState)
        {
            if (State == newState) return;

            var data = UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 0);
            data.Add("state", newState);
            data.Add("team", flagTeam);
            data.Add("player", PhotonNetworkWrapper.LocalPlayer);
            data.Add("viewID", GameController.LocalPlayerViewID);
            PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.isLocalPlayerCollider())
            {
                var player = collider.gameObject.GetComponent<PlayerConfig>();
                if (CanBePickedUpBy(player) == true)
                {
                    ChangeFlagState(FlagState.PickUp);
                }
            }
        }

        void OnLocalPlayerDeath()
        {
            if (carriyingPlayer == null)
            {
                return;
            }

            var local = GameCore.LocalPlayerReferences;
            if (local == null) return;

            if (carriyingPlayer.View.ViewID == local.ViewID)
            {
                // Drop the flag
                var data = UtilityHelper.CreatePhotonHashTable();
                data.Add("cmd", 1);
                data.Add("team", flagTeam);
                data.Add("pos", transform.position);
                PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
            }
        }

        public void HandleFlagCapture()
        {
            if (carriyingPlayer == null)
                return;

            if (carriyingPlayer.View.ViewID != GameCore.LocalPlayer.ViewID)
            {
                return;
            }

            if (oppositeFlag.IsHome() == true && UtilityHelper.Distance(transform.position, oppositeFlag.transform.position) <= CaptureOfFlagController.Instance.captureAreaRange)
            {
                ChangeFlagState(FlagState.Captured);
                // change the state locally instantly to prevent detect the capture multiple times.
                State = FlagState.Captured;
            }
        }

        /// <summary>
        /// Determines whether this instance is at the home base
        /// </summary>
        /// <returns></returns>
        public bool IsHome()
        {
            return transform.position == originalPos;
        }

        /// <summary>
        /// Called on all clients when a player drop the flag.
        /// </summary>
        /// <param name="data"></param>
        public void DropFlag(Hashtable data)
        {
            var position = (Vector3)data["pos"];

            State = FlagState.Dropped;
            carriyingPlayer = null;
            transform.parent = null;
            transform.position = position;
            transform.eulerAngles = originalRot;
            transform.localScale = originalScale;

            if (PhotonNetworkWrapper.IsMasterClient)
            {
                Invoke(nameof(ReturnInvoke), ReturnTime);
            }
        }

        /// <summary>
        /// Called on all clients when a player successfully capture the flag.
        /// </summary>
        public void OnCapture(Player actor)
        {
            carriyingPlayer = null;
            SetFlagToOrigin();

            //Only the player who captures the flag, updates the properties
            if (PhotonNetworkWrapper.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                KillFeedBase.Instance.SendTeamHighlightMessage(PhotonNetworkWrapper.LocalPlayer.NickName, GameTexts.CaptureTheFlag, actor.GetPlayerTeam());
                GameController.Instance.SetPointFromLocalPlayer(1, GameMode.CTF);
                //Add Point for personal score
                PhotonNetworkWrapper.LocalPlayer.PostScore(CaptureOfFlagController.Instance.scorePerCapture);
                CaptureOfFlagController.Instance.onCapture?.Invoke();
            }
        }

        /// <summary>
        /// Called on all clients when a player recover the flag.
        /// </summary>
        public void Recover(Player actor)
        {
            SetFlagToOrigin();

            if (actor.ActorNumber == PhotonNetworkWrapper.LocalPlayer.ActorNumber)
            {
                PhotonNetworkWrapper.LocalPlayer.PostScore(CaptureOfFlagController.Instance.scorePerRecover);
                CaptureOfFlagController.Instance.onRecover?.Invoke();
            }
        }

        /// <summary>
        /// Called on all clients when a player pick up this flag.
        /// </summary>
        /// <param name="actor"></param>
        public void OnPickup(Player actor, int viewID)
        {
            Transform actorTransform = GameController.Instance.FindActor(viewID);
            if (actorTransform == null)
            {
                return;
            }

            var logic = actorTransform.GetComponent<PlayerConfig>();
            if (!CanBePickedUpBy(logic))
            {
                return;
            }

            if (logic.PlayerTeam == flagTeam)
            {
                if (IsHome() == false)
                {
                    // the flag is dropped and taken by a team member = recovering the flag.
                    if (PhotonNetworkWrapper.IsMasterClient) ChangeFlagState(FlagState.InHome);
                }
            }
            else
            {
                SetFlagToCarrier(logic);
            }

            //show capture notification
            if (PhotonNetworkWrapper.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                Team enemyTeam = CaptureOfFlagController.GetOppositeTeam(PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam());
                string obtainedText = string.Format(GameTexts.ObtainedFlag, enemyTeam.GetTeamName());
                KillFeedBase.Instance.SendTeamHighlightMessage(PhotonNetworkWrapper.LocalPlayer.NickName, obtainedText, PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam());
                CaptureOfFlagController.Instance.onPickUp?.Invoke();
            }
        }

        public void SetFlagToCarrier(PlayerConfig carrier)
        {
            carriyingPlayer = carrier;
            transform.parent = carrier.FlagPosition;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            if (CaptureOfFlagController.Instance.moveFlagWithCarrierMotion && carrier.carrierPoint != null)
                transform.parent = carrier.carrierPoint;
        }

        void ReturnInvoke()
        {
            var data = UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 2);
            data.Add("team", flagTeam);
            PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
        }

        public bool CanBePickedUpBy(PlayerConfig logic)
        {
            //If the flag is at its home position, only the enemy team can grab it
            if (IsHome() == true)
            {
                return logic.PlayerTeam != flagTeam;
            }

            //If another player is already carrying the flag, no one else can grab it
            if (carriyingPlayer != null)
            {
                return false;
            }

            return true;
        }

        public void SetFlagToOrigin()
        {
            transform.parent = null;
            transform.position = originalPos;
            transform.eulerAngles = originalRot;
            transform.localScale = originalScale;
            State = FlagState.InHome;
        }

        #region GUI
        void OnGUI()
        {
            if (carriyingPlayer != null && carriyingPlayer.View.ViewID == GameCore.LocalPlayer.ViewID) return;

            GUI.color = IconColor;
            if (GameController.Instance.CameraRendered)
            {
                Vector3 vector = GameController.Instance.CameraRendered.WorldToScreenPoint(this.IconTarget.position);
                if (vector.z > 0)
                {
                    GUI.DrawTexture(new Rect(vector.x - 5, (Screen.height - vector.y) - 7, 13 + IconSize.x, 13 + IconSize.y), this.FlagIcon);
                }
            }
            GUI.color = Color.white;
        }

        private SphereCollider SpheCollider;
        private void OnDrawGizmos()
        {
            if (SpheCollider != null)
            {
                Vector3 v = SpheCollider.bounds.center;
                v.y = transform.position.y;
                UtilityHelper.DrawWireArc(v, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
            }
            else
            {
                SpheCollider = GetComponent<SphereCollider>();
            }
        }
        #endregion

        [System.Serializable]
        public enum FlagState
        {
            InHome = 0,
            PickUp = 1,
            Captured = 2,
            Dropped = 3,
        }
    }
}