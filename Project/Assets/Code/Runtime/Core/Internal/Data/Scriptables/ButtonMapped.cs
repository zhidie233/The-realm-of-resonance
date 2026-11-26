using System;
using System.Collections.Generic;
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

    [Serializable, CreateAssetMenu(fileName = "Button Mapped", menuName = "项目/Input/Input Mapped")]
    public class ButtonMapped : ScriptableObject
    {
        public InputType inputType = InputType.Keyboard;
        public Mapped mapped = new Mapped();
        public List<ButtonData> ButtonMap { get { return mapped.ButtonMap; } }
    }
}
