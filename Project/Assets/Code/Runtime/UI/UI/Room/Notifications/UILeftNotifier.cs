using UnityEngine;
using System.Collections;
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

    public class UILeftNotifier : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI m_Text = null;


        public void SetInfo(string t, float time)
        {
            m_Text.text = t;
            StartCoroutine(Hide(time));
        }

        IEnumerator Hide(float t)
        {
            yield return new WaitForSeconds(t);
            gameObject.SetActive(false);
        }
    }
}
