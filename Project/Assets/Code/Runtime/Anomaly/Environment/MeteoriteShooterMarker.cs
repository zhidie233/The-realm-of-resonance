using UnityEngine;
using Resonance.Core;

namespace Resonance.Anomaly
{
    public class MeteoriteShooterMarker : MonoBehaviour
    {
        private GamePlayer targetPlayer;
        private float displayTime;
        private float timer;
        private Camera _cachedCamera;

        public void Initialize(GamePlayer player, float time)
        {
            targetPlayer = player;
            displayTime = time;
            timer = 0f;

            if (player.Actor != null)
            {
                transform.SetParent(player.Actor);
                transform.localPosition = Vector3.up * 3f;
            }
        }

        private void Update()
        {
            if (targetPlayer == null || targetPlayer.Actor == null)
            {
                Destroy(gameObject);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= displayTime)
            {
                Destroy(gameObject);
                return;
            }

            Camera cam = ResolveActiveCamera();
            if (cam != null)
            {
                transform.LookAt(cam.transform);
                transform.Rotate(0f, 180f, 0f);
            }
        }

        private Camera ResolveActiveCamera()
        {
            if (RuntimeServices.LocalPlayerReferences != null && RuntimeServices.LocalPlayerReferences.playerCamera != null)
                return RuntimeServices.LocalPlayerReferences.playerCamera;

            if (_cachedCamera == null)
                _cachedCamera = Camera.main;

            return _cachedCamera;
        }
    }
}
