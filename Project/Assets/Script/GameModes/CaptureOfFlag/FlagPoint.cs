using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Game.GameModes.CaptureOfFlag
{
    public class FlagPoint : NetworkHelper
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

            var data = CommonUtil.CreatePhotonHashTable();
            data.Add("cmd", 0);
            data.Add("state", newState);
            data.Add("team", flagTeam);
            data.Add("player", NetworkWrapper.LocalPlayer);
            data.Add("viewID", GameController.LocalPlayerViewID);
            NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.CaptureOfFlagMode, data);
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
                // 掉落旗帜
                var data = CommonUtil.CreatePhotonHashTable();
                data.Add("cmd", 1);
                data.Add("team", flagTeam);
                data.Add("pos", transform.position);
                NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.CaptureOfFlagMode, data);
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

            if (oppositeFlag.IsHome() == true && CommonUtil.Distance(transform.position, oppositeFlag.transform.position) <= CaptureOfFlagController.Instance.captureAreaRange)
            {
                ChangeFlagState(FlagState.Captured);
                // 立即在本地切换状态，避免多次检测到夺旗。
                State = FlagState.Captured;
            }
        }

        /// <summary>
        /// 判断此实例是否在己方基地
        /// </summary>
        /// <returns></returns>
        public bool IsHome()
        {
            return transform.position == originalPos;
        }

        /// <summary>
        /// 玩家掉落旗帜时在所有客户端调用。
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

            if (NetworkWrapper.IsMasterClient)
            {
                Invoke(nameof(ReturnInvoke), ReturnTime);
            }
        }

        /// <summary>
        /// 玩家成功夺旗时在所有客户端调用。
        /// </summary>
        public void OnCapture(Player actor)
        {
            carriyingPlayer = null;
            SetFlagToOrigin();

            //只有夺旗的玩家更新这些属性
            if (NetworkWrapper.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                KillReportEntry.Instance.SendTeamHighlightMessage(NetworkWrapper.LocalPlayer.NickName, GameTexts.CaptureTheFlag, actor.GetPlayerTeam());
                GameController.Instance.SetPointFromLocalPlayer(1, GameMode.CTF);
                //增加个人得分
                NetworkWrapper.LocalPlayer.PostScore(CaptureOfFlagController.Instance.scorePerCapture);
                CaptureOfFlagController.Instance.onCapture?.Invoke();
            }
        }

        /// <summary>
        /// 玩家归还旗帜时在所有客户端调用。
        /// </summary>
        public void Recover(Player actor)
        {
            SetFlagToOrigin();

            if (actor.ActorNumber == NetworkWrapper.LocalPlayer.ActorNumber)
            {
                NetworkWrapper.LocalPlayer.PostScore(CaptureOfFlagController.Instance.scorePerRecover);
                CaptureOfFlagController.Instance.onRecover?.Invoke();
            }
        }

        /// <summary>
        /// 玩家拾取这面旗帜时在所有客户端调用。
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
                    //旗帜被掉落又被队友捡起，即为归还旗帜。
                    if (NetworkWrapper.IsMasterClient) ChangeFlagState(FlagState.InHome);
                }
            }
            else
            {
                SetFlagToCarrier(logic);
            }

            //显示夺旗提示
            if (NetworkWrapper.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                Team enemyTeam = CaptureOfFlagController.GetOppositeTeam(NetworkWrapper.LocalPlayer.GetPlayerTeam());
                string obtainedText = string.Format(GameTexts.ObtainedFlag, enemyTeam.GetTeamName());
                KillReportEntry.Instance.SendTeamHighlightMessage(NetworkWrapper.LocalPlayer.NickName, obtainedText, NetworkWrapper.LocalPlayer.GetPlayerTeam());
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
            var data = CommonUtil.CreatePhotonHashTable();
            data.Add("cmd", 2);
            data.Add("team", flagTeam);
            NetworkWrapper.Instance.SendDataOverNetwork(NetworkKeys.CaptureOfFlagMode, data);
        }

        public bool CanBePickedUpBy(PlayerConfig logic)
        {
            //旗帜在己方基地位置时，只有敌方队伍能拿
            if (IsHome() == true)
            {
                return logic.PlayerTeam != flagTeam;
            }

            //如果已有其他玩家持旗，其他人不能拿
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
                CommonUtil.DrawWireArc(v, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
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