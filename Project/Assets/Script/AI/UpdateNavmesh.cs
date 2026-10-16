using System.Collections;
using Unity.AI.Navigation; // 注意命名空间
using UnityEngine;

public class UpdateNavmesh : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;
    public float updateInterval = 1.0f; // 更新间隔，单位为秒

    private void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        StartCoroutine(PeriodicNavMeshUpdate());
    }

    IEnumerator PeriodicNavMeshUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            UpdateNavMeshData();
        }
    }

    private void UpdateNavMeshData()
    {
        // 方法1：完全重新烘焙（较耗时）
        navMeshSurface.BuildNavMesh();
        
        // 方法2：如果场景变化不大，可以尝试更新NavMeshData（更高效）
        // navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
    }
}