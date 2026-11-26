using System;
using UnityEngine;
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

    public class AnimatorReloadEvent : StateMachineBehaviour
    {
        public static Action<bool, Animator, AnimatorStateInfo> OnTPReload;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != 1) return;
            OnTPReload?.Invoke(true, animator, stateInfo);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != 1) return;
            OnTPReload?.Invoke(false, animator, stateInfo);
        }
    }
}
