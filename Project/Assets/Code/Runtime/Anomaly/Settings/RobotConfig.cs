using UnityEngine;
namespace Resonance.Anomaly
{

    [CreateAssetMenu(fileName = "RobotConfig", menuName = "项目/共振/RobotConfig")]
    public class RobotConfig : ScriptableObject
    {
        public int robotSpawnInterval=1000;
    }
}
