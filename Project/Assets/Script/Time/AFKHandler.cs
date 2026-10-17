using UnityEngine;
using TMPro;

namespace Game.Runtime.Misc
{
    public class AFKHandler : MonoBehaviourBase
    {
        public TextMeshProUGUI afkText;

        private float lastInput;
        private Vector3 oldMousePosition = Vector3.zero;
        private bool Leaving = false;
        private bool Watching = false;
        private float AFKTimeLimit = 60;

        protected override void Awake()
        {
            base.Awake();
            AFKTimeLimit = GameConfig.Instance.AFKTimeLimit;
            if (!GameConfig.Instance.DetectAFK)
            {
                this.enabled = false;
                gameObject.SetActive(false);
            }
        }

        public override void OnUpdate()
        {
            float time = Time.time;
            //如果检测不到玩家的移动或操作，就重新开始计时
            if ((PhotonNetworkWrapper.LocalPlayer == null || Input.anyKey) || ((oldMousePosition != Input.mousePosition)))
            {
                lastInput = time;
                if (Watching)
                {
                    afkText.gameObject.SetActive(false);
                    Watching = false;
                }
            }
            else if ((time - lastInput) > AFKTimeLimit * 0.5f)
            {
                Watching = true;
            }
            oldMousePosition = Input.mousePosition;
            if (((lastInput + AFKTimeLimit) - 10f) < time)
            {
                float t = AFKTimeLimit - (time - lastInput);
                if (t >= 0)
                {
                    SetAFKCount(t);
                }
            }
            //如果挂机时间达到上限，就返回大厅。
            if ((lastInput + AFKTimeLimit) < time && !Leaving)
            {
                UtilityHelper.LockCursor(false);
                PhotonNetworkWrapper.Instance.hasAFKKick = true;
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
            if (PhotonNetworkWrapper.IsConnected)
            {
                PhotonNetworkWrapper.LeaveRoom();
            }
            else
            {
                UtilityHelper.LoadLevel(GameConfig.Instance.MainMenuScene);
            }
        }
    }
}