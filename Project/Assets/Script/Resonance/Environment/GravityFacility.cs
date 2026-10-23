using UnityEngine;

public class GravityFacility : MonoBehaviour
{
    public Vector3 StartLimitPosition = new Vector3(0, 500f, 0); // 初始坐标
    public Vector3 FinalLimitPosition = new Vector3(0, 60f, 0);  // 最终坐标
    public Quaternion StartLimitRotation = Quaternion.identity;
    public Quaternion FinalLimitRotation = Quaternion.identity;

    public void UpdateTransformBasedOnResonance(float resonanceValue, float maxResonance)
    {
        // 将共振值映射到0-1的范围
        float t = Mathf.Clamp01(resonanceValue / maxResonance);
        // 计算新位置和旋转
        Vector3 newPosition = Vector3.Lerp(StartLimitPosition, FinalLimitPosition, t);
        Quaternion newRotation = Quaternion.Lerp(StartLimitRotation, FinalLimitRotation, t);
        // 只在位置或旋转有显著变化时更新
        bool positionChanged = Vector3.Distance(transform.position, newPosition) > 0.1f;
        bool rotationChanged = Quaternion.Angle(transform.rotation, newRotation) > 0.1f;
        if (positionChanged || rotationChanged)
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }
    }

    // 编辑器方法：移动到StartLimitPosition和StartLimitRotation
    public void MoveToStartTransform()
    {
        transform.position = StartLimitPosition;
        transform.rotation = StartLimitRotation;
    }

    // 编辑器方法：移动到FinalLimitPosition和FinalLimitRotation
    public void MoveToFinalTransform()
    {
        transform.position = FinalLimitPosition;
        transform.rotation = FinalLimitRotation;
    }
}
