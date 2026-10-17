using UnityEngine;
using UnityEngine.Animations;

public static class MathUtility
{
    public static Vector3 Gravity => Physics.gravity;
    
    /// <summary>
    /// 返回两个向量之间的归一化交点值
    /// </summary>
    /// <returns></returns>
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public static Quaternion ClampRotationAroundAxis(Quaternion q, float minimun, float maximun, Axis axis)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angle = 0;
        if (axis == Axis.X)
        {
            angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angle = Mathf.Clamp(angle, minimun, maximun);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);
        }
        else
        {
            angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angle = Mathf.Clamp(angle, minimun, maximun);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);
        }

        return q;
    }

    public static float Distance(Vector3 from, Vector3 to)
    {
        Vector3 v = new Vector3(from.x - to.x, from.y - to.y, from.z - to.z);
        return Mathf.Sqrt((v.x * v.x + v.y * v.y + v.z * v.z));
    }

    /// <summary>
    /// 把角度限制在 0-360，最小值可以大于最大值
    /// </summary>
    /// <returns></returns>
    public static float Clamp360Angle(float angle, float min, float max)
    {
        if (angle <= min)
        {
            if (min > max)
            {
                if (angle > max)
                {
                    angle = GetNearestBetweenTwo(angle, min, max);
                }
            }
            else
            {
                angle = min;
            }
        }
        if (angle >= max)
        {
            if (min > max)
            {
                if (angle < min)
                {
                    angle = GetNearestBetweenTwo(angle, min, max);
                }
            }
            else
            {
                angle = max;
            }
        }
        return angle;
    }

    /// <summary>
    /// 返回 min 与 max 中距 val 较近的那个值
    /// </summary>
    /// <returns></returns>
    public static float GetNearestBetweenTwo(float val, float min, float max)
    {
        float v1 = min - val;
        float v2 = max - val;
        if (Mathf.Abs(v1) < Mathf.Abs(v2)) return min;
        return max;
    }

    /// <summary>
    /// 获取 ClampAngle
    /// </summary>
    /// <returns></returns>
    public static float ClampAngle(float ang, float min, float max)
    {
        if (ang < -360f)
        {
            ang += 360f;
        }
        if (ang > 360f)
        {
            ang -= 360f;
        }
        return Mathf.Clamp(ang, min, max);
    }

    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }


    /// <summary>
    /// dirA 与 dirB 绕 axis 的角度
    /// </summary>
    /// <returns></returns>
    public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // 把 A 和 B 投影到与目标轴正交的平面
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // 求 A 与 B 之间的（正）夹角
        float angle = Vector3.Angle(dirA, dirB);

        // 返回乘以 1 或 -1 后的角度
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }

    public static float AngleSigned(Vector3 vector1, Vector3 vector2, Vector3 normal)
    {
        return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(vector1, vector2)), Vector3.Dot(vector1, vector2)) * Mathf.Rad2Deg;
    }

    public static bool Approximately(float a, float b, float tolerance = 1E-06f)
    {
        return Mathf.Abs(a - b) < tolerance;
    }

    /// <summary>
	/// 返回朝向向量与目标位置之间的角度。
	/// 可用于各种瞄准逻辑
	/// </summary>
	public static float LookAtAngle(Vector3 fromPosition, Vector3 fromForward, Vector3 toPosition)
    {
        return (Vector3.Cross(fromForward, (toPosition - fromPosition).normalized).y < 0.0f) ?
                -Vector3.Angle(fromForward, (toPosition - fromPosition).normalized) :
                Vector3.Angle(fromForward, (toPosition - fromPosition).normalized);
    }


    /// <summary>
    /// 返回俯视视角下朝向向量与目标位置之间的角度，
    /// 按四正方向计算。适用于 GUI 指示箭头
    /// </summary>
    public static float LookAtAngleHorizontal(Vector3 fromPosition, Vector3 fromForward, Vector3 toPosition)
    {
        return LookAtAngle(
            HorizontalVector(fromPosition),
            HorizontalVector(fromForward),
            HorizontalVector(toPosition));
    }

    /// <summary>
	/// 把 Vector3 的 y 分量置零，适用于需要
	/// 做 2D 物理计算的场合。
	/// </summary>
	public static Vector3 HorizontalVector(Vector3 value)
    {
        value.y = 0.0f;
        return value;
    }

    /// <summary>
    /// 计算子弹的阻力加速度
    /// </summary>
    /// <returns></returns>
    public static Vector3 CalculateDrag(Vector3 velocityVec)
    {

        float m = 0.2f; // kg
        float C_d = 0.5f;
        float A = Mathf.PI * 0.05f * 0.05f; // m^2
        float rho = 1.225f; // kg/m3

        float k = 0.5f * C_d * rho * A;

        float vSqr = velocityVec.sqrMagnitude;

        float aDrag = (k * vSqr) / m;

        //方向必须与子弹速度向量相反
        Vector3 dragVec = aDrag * velocityVec.normalized * -1f;

        return dragVec;
    }

    /// <summary>
    /// 使用弹簧/抖动效果插值两个数值
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static float Spring(float from, float to, float time)
    {
        time = Mathf.Clamp01(time);
        time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
        return from + (to - from) * time;
    }

    /// <summary>
    /// 使用弹簧/抖动效果插值两个向量
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector3 Spring(Vector3 from, Vector3 to, float time)
    {
        return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
    }
}