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

    public class CameraLookup : MonoBehaviour
    {
        // Start 在第一帧更新前调用
        void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
        }
    }
}
