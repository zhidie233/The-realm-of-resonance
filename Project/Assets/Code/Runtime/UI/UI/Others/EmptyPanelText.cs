using UnityEngine;
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

    public class EmptyPanelText : MonoBehaviour
    {
        private void Start()
        {
            Check();
        }

        private void OnTransformParentChanged()
        {
        }

        public void Check()
        {
            var children = transform.parent.childCount;
            if(children <= 1)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
