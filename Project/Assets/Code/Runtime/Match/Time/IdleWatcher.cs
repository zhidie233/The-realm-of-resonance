using UnityEngine;
using TMPro;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Match
{

    public class IdleWatcher : MonoBehaviour
    {
        public TextMeshProUGUI afkText;

        private float lastInput;
        private Vector3 oldMousePosition = Vector3.zero;
        private bool Leaving = false;
        private bool Watching = false;
        private float idleKickTime = 60;

        protected virtual void Awake()
        {
            idleKickTime = ProjectConfigRoot.Instance.idleKickTime;
            if (!ProjectConfigRoot.Instance.watchIdlePlayers)
            {
                this.enabled = false;
                gameObject.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            float time = Time.time;
            //如果检测不到玩家的移动或操作，就重新开始计时
            if ((NetworkWrapper.LocalPlayer == null || Input.anyKey) || ((oldMousePosition != Input.mousePosition)))
            {
                lastInput = time;
                if (Watching)
                {
                    afkText.gameObject.SetActive(false);
                    Watching = false;
                }
            }
            else if ((time - lastInput) > idleKickTime * 0.5f)
            {
                Watching = true;
            }
            oldMousePosition = Input.mousePosition;
            if (((lastInput + idleKickTime) - 10f) < time)
            {
                float t = idleKickTime - (time - lastInput);
                if (t >= 0)
                {
                    SetAFKCount(t);
                }
            }
            //如果挂机时间达到上限，就返回大厅。
            if ((lastInput + idleKickTime) < time && !Leaving)
            {
                CommonUtil.LockCursor(false);
                NetworkWrapper.Instance.hasAFKKick = true;
                LeaveMatch();
                Leaving = true;
            }
        }

        public void SetAFKCount(float seconds)
        {
            afkText.gameObject.SetActive(true);
            afkText.text = string.Format(GameTexts.AFKWarning, seconds.ToString("F2"));
        }

        public void LeaveMatch()
        {
            if (NetworkWrapper.IsConnected)
            {
                NetworkWrapper.LeaveRoom();
            }
            else
            {
                CommonUtil.LoadLevel(ProjectConfigRoot.Instance.MainMenuScene);
            }
        }
    }
}
