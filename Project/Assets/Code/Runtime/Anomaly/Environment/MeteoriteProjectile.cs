using UnityEngine;
using Resonance.Combat;

namespace Resonance.Anomaly
{
    public class MeteoriteProjectile : LauncherProjectile
    {
        private Vector3 targetPosition;
        private bool hasTarget = false;

        public void SetTarget(Vector3 target)
        {
            targetPosition = target;
            hasTarget = true;
        }

        private void FixedUpdate()
        {
            if (!hasTarget) return;

            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * Time.fixedDeltaTime * bulletData.Speed;

            if (Vector3.Distance(transform.position, targetPosition) < 1f)
            {
                Explode(transform.position);
            }
        }
    }
}
