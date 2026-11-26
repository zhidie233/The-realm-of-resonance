using System.Collections.Generic;
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

    [CreateAssetMenu(fileName = "ResonanceConfig", menuName = "项目/共振/ResonanceConfig")]
    public class ResonanceConfig : ScriptableObject
    {
        [System.Serializable]
        public class ResonanceChangeData
        {
            public string key;
            public float magnification;
        }

        [Header("共鸣值变化配置")]
        public List<ResonanceChangeData> resonanceChanges = new List<ResonanceChangeData>();

        // 运行时使用的Dictionary
        public Dictionary<string, float> resonanceChangeDict;

        public void Initialize()
        {
            resonanceChangeDict = new Dictionary<string, float>();
            foreach (var data in resonanceChanges)
            {
                if (!resonanceChangeDict.ContainsKey(data.key))
                {
                    resonanceChangeDict.Add(data.key, data.magnification);
                }
            }
        }

        public float GetResonanceChangeValue(string key)
        {
            if (resonanceChangeDict == null)
                Initialize();

            return resonanceChangeDict.ContainsKey(key) ? resonanceChangeDict[key] : 0f;
        }

        // 在Inspector中更新后重新初始化
        private void OnValidate()
        {
            if (Application.isPlaying)
                Initialize();
        }
    }
}
