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

    public class TimedDestroyer : MonoBehaviour
    {

        public float destroyAfter = 15.0f;

        void Start()
        {
            if(destroyAfter > 0)
            Destroy(gameObject, destroyAfter);
        }

        public void Desactive()
        {
            gameObject.SetActive(false);
        }
    }
}
