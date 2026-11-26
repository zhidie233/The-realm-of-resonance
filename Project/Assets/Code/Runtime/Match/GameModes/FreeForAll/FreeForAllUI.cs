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

    public class FreeForAllUI : MonoBehaviour
    {
        public GameObject Content;
        public TextMeshProUGUI ScoreText;

        public void SetScores(GamePlayer bestPlayer)
        {
            string scoreText = string.Format(GameTexts.PlayerStart, bestPlayer.Name);
            ScoreText.text = scoreText;
        }

        public void ShowUp()
        {
            if (!UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard)) return;

            Content.SetActive(true);
        }

        public void Hide()
        {
            Content.SetActive(false);
        }

        private static FreeForAllUI _instance;
        public static FreeForAllUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FreeForAllUI>();
                }
                return _instance;
            }
        }
    }
}
