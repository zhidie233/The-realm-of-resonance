using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f; // 旋转速度（度/秒）
    
    void Update()
    {
        // 绕Y轴旋转（本地坐标系）
        transform.Rotate(rotationSpeed * Time.deltaTime,0 , 0);
    }
}