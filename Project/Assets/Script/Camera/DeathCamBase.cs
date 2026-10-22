using Photon.Realtime;
using UnityEngine;

public abstract class DeathCamBase : MonoBehaviourBase
{
    public abstract void SetActive(bool active);

    public abstract DeathCamBase SetTarget(DeathCamInfo info);

    private static DeathCamBase _killcam;
    public static DeathCamBase Instance
    {
        get
        {
            if (_killcam == null) _killcam = SpawnPointManager.Instance.killCameraInstance;
            return _killcam;
        }
    }

    public struct DeathCamInfo
    {
        public Transform Target;
        public string TargetName;
        public int WeaponID;
        public Transform FallbackTarget;
        public Player RealPlayer;
    }
}
