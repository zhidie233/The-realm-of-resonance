using UnityEngine;
using UnityEngine.UI;
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

    public class GameRoomPreview : MonoBehaviour
    {

        [Header("References")]
        public Image MapPreview;
        public TextMeshProUGUI MapNameText;

        public void Show(GameRoomInfo info)
        {
            var map = info.GetMapInfo();
            MapPreview.sprite = map.Preview;
            MapNameText.text = map.ShowName.ToUpper();
        }
    }
}
