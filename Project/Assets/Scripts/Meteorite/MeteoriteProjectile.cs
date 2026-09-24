using UnityEngine;

public class MeteoriteProjectile : bl_GrenadeLauncherProjectile
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
        if (hasTarget)
        {
            // 让陨石朝向目标移动
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * Time.fixedDeltaTime * 40f; // 陨石下落速度
            
            // 检查是否接近目标
            if (Vector3.Distance(transform.position, targetPosition) < 1f)
            {
                // 触发爆炸
                OnHit(new Collision());
            }
        }
        else
        {
            base.OnFixedUpdate();
        }
    }
}