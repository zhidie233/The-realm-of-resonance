using UnityEngine;

public class MeteoriteProjectile : LauncherProjectile
{
    private Vector3 targetPosition;
    private bool hasTarget = false;
    
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;
    }
    
    public override void OnFixedUpdate()
    {
        if (!hasTarget)
        {
            base.OnFixedUpdate();
            return;
        }

        // 让陨石朝向目标移动
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * Time.fixedDeltaTime * bulletData.Speed; // 下落速度取自 MeteoriteConfig

        // 检查是否接近目标
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            // 触发爆炸
            Explode(transform.position);
        }
    }
}