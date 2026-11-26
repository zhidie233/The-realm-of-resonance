using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.UI;
namespace Resonance.Anomaly
{

    public class RobotManager : EnvironmentEffectManager
    {
        private int lastTriggeredMultiple = -1;

        protected override void Start()
        {
            base.Start();
            if (!enabled) return;
            lastTriggeredMultiple = Mathf.FloorToInt(currentResonance / ProjectConfigRoot.Instance.robotConfig.robotSpawnInterval);
        }

        public override void UpdateEffect()
        {
            if (!NetworkWrapper.IsMasterClient) return;

            int currentMultiple = Mathf.FloorToInt(currentResonance / ProjectConfigRoot.Instance.robotConfig.robotSpawnInterval);

            if (currentMultiple <= 0 || currentMultiple <= lastTriggeredMultiple) return;

            lastTriggeredMultiple = currentMultiple;
            AIManager.Instance.SpawnBot(null, Team.Team3);
        }
    }
}
