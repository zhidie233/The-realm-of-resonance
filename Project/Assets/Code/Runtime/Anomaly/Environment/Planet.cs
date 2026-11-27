using UnityEngine;

namespace Resonance.Anomaly
{
    public class Planet : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f; // 旋转速度（度/秒）

        private void Update()
        {
            // 绕局部 X 轴自转
            transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
        }
    }
}
