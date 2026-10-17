using Photon.Realtime;
using UnityEngine;

public abstract class KillCamBase : MonoBehaviourBase
{
    public abstract void SetActive(bool active);

    public abstract KillCamBase SetTarget(KillCamInfo info);

    private static KillCamBase _killcam;
    public static KillCamBase Instance
    {
        get
        {
            if (_killcam == null) _killcam = SpawnPointManager.Instance.killCameraInstance;
            return _killcam;
        }
    }

    public struct KillCamInfo
    {
        public Transform Target;
        public string TargetName;
        public int GunID;//with which the player was terminated
        public Transform FallbackTarget;
        public Player RealPlayer;
    }
}