using UnityEngine;
using TMPro;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
namespace Resonance.UI
{

    public class FrameRate : MonoBehaviour
    {
        [Range(0.1f, 1)] public float updateInterval = 0.4f;
        public string textFormat = "<b>FPS:</b> {0}";
        public TextMeshProUGUI TextUI = null;

        private string framerate;
        private float timeleft;
        private int rate = 0;
        private float accum;
        private int frames;
        private bool countFPS = true;

        void Start()
        {
            timeleft = updateInterval;
            OnSettingsChanged();
        }

        protected virtual void OnEnable()
        {
            EventBus.Subscribe<GameSettingsChangedEvent>(OnEvt_onGameSettingsChange_OnSettingsChanged);
        }

        protected virtual void OnDisable()
        {
            EventBus.Unsubscribe<GameSettingsChangedEvent>(OnEvt_onGameSettingsChange_OnSettingsChanged);
        }

        void OnSettingsChanged()
        {
            countFPS = (bool)RuntimeServices.Settings.GetSettingOf("显示帧率");
            if (TextUI != null) TextUI.gameObject.SetActive(countFPS);
        }

        protected virtual void Update()
        {
            if (TextUI == null || !countFPS) return;

            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;
            if (timeleft <= 0)
            {
                rate = Mathf.FloorToInt(accum / frames);
                framerate = rate.ToString();
                timeleft = updateInterval;
                accum = 0;
                frames = 0;
            }
            TextUI.text = string.Format(textFormat, framerate);
        }

        private void OnEvt_onGameSettingsChange_OnSettingsChanged(GameSettingsChangedEvent e) => OnSettingsChanged();
    }
}
