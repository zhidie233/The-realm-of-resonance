#if UNITY_EDITOR
using UnityEngine;
   /// 绘制 SpawnLocation 的圆顶形出生区域。
   public class DomeGizmo
   {
       const int ArcCount = 8;
       const int ArcSteps = 24;

       public float horizon;

       public DomeGizmo(float horizon)
       {
           this.horizon = horizon;
       }

       public void Draw(Transform target, Color color, float space)
       {
           if (target == null) { return; }

           var matrix = Gizmos.matrix;
           var previous = Gizmos.color;

           Gizmos.color = color;
           Gizmos.matrix = Matrix4x4.TRS(target.position, target.rotation, Vector3.one);

           float radius = Mathf.Max(0.01f, space);
           float limit = Mathf.Clamp(horizon, 1f, 180f) * Mathf.Deg2Rad;

           for (int arc = 0; arc < ArcCount; arc++)
           {
               float yaw = arc * (360f / ArcCount) * Mathf.Deg2Rad;
               Vector3 last = Vector3.zero;
               for (int step = 0; step <= ArcSteps; step++)
               {
                   float pitch = (step / (float)ArcSteps) * limit;
                   Vector3 point = new Vector3(Mathf.Sin(pitch) * Mathf.Cos(yaw), Mathf.Cos(pitch), Mathf.Sin(pitch) * Mathf.Sin(yaw)) * radius;
                   if (step > 0) { Gizmos.DrawLine(last, point); }
                   last = point;
               }
           }

           Gizmos.matrix = matrix;
           Gizmos.color = previous;
       }
   }
#endif
