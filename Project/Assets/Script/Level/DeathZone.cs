using UnityEngine;
using TMPro;

namespace Game.Runtime.Level
{
    public class DeathZone : NetworkHelper
    {
        [GameToggle] public bool instaKill = false;
        public int countDown = 5;
        [TextArea(2, 4)]
        public string CustomMessage = "you're in a zone prohibited \n returns to the playing area or die at \n";

        private bool mOn = false;
        private GameObject m_killZoneUI = null;
        private int CountDown;
        private Collider m_Collider = null;

        void Awake()
        {
            CountDown = countDown;
            m_Collider = transform.GetComponent<Collider>();

            if (m_Collider == null)
            {
                Debug.LogWarning($"Kill Zone object doesn't have a collider attached!", gameObject);
            }

            m_killZoneUI = UIReferences.Instance.PlayerUI.KillZoneUI;
        }

        void OnTriggerEnter(Collider mCol)
        {
            if (mCol.isLocalPlayerCollider())//when is player local enter
            {
                DoPlayerDamage(mCol.transform.root);
            }
            else if (mCol.CompareTag("Metal"))
            {
            }
        }

        private void DoPlayerDamage(Transform root)
        {
            var pdm = root.GetComponentInChildren<PlayerHealthManagerBase>(true);// get the component damage
            if (pdm != null && pdm.isMine && pdm.GetHealth() > 0 && !mOn)
            {
                if (instaKill)
                {
                    GameCore.LocalPlayer.Suicide();
                    return;
                }
                InvokeRepeating(nameof(DoCountDown), 1, 1);
                UpdateUI();
                mOn = true;
            }
        }

        void OnTriggerExit(Collider mCol)
        {
            if (mCol.isLocalPlayerCollider())// if player exit of zone then cancel countdown
            {
                CancelInvoke(nameof(DoCountDown));
                CountDown = countDown; // restart time
                if (m_killZoneUI != null)
                {
                    m_killZoneUI.SetActive(false);
                }
                mOn = false;
            }
        }

        /// <summary>
        /// 玩家进入触发器时开始倒计时
        /// </summary>
        void DoCountDown()
        {
            CountDown--;
            UpdateUI();
            if (CountDown <= 0)
            {
                GameObject player = FindPlayerRoot(GameController.LocalPlayerViewID);
                if (player != null)
                {
                    player.GetComponent<PlayerHealthManagerBase>().Suicide();
                }
                CancelInvoke(nameof(DoCountDown));
                CountDown = countDown;
                if (m_killZoneUI != null) m_killZoneUI.SetActive(false);
                mOn = false;
            }
        }

        private void UpdateUI()
        {
            if (m_killZoneUI == null) return;

            m_killZoneUI.SetActive(true);
            var text = m_killZoneUI.GetComponentInChildren<TextMeshProUGUI>();
            var message = string.IsNullOrEmpty(CustomMessage) ? string.Format(GameTexts.KillZoneMessage, CountDown) : string.Format(CustomMessage, CountDown);
            text.text = message;

        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var c = GetComponent<BoxCollider>();
            if (c == null) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            if (gameObject == UnityEditor.Selection.activeGameObject)
            {
                // 若直接选中了本对象（而非仅选中父对象）
                // 用负尺寸绘制，得到可从内部看到的“外翻”立方体
                Gizmos.color = new Color(1.0f, 1.0f, 0.5f, 0.8f);
                Gizmos.DrawCube(c.center, -c.size);
            }
            Gizmos.color = new Color(1.0f, 0.5f, 0.5f, 0.3f);
            Gizmos.DrawCube(c.center, c.size);

            UnityEditor.Handles.Label(transform.position, "KILL ZONE");
        }
#endif
    }
}