using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
/// 默认游戏对象池系统
/// 想用自己的对象池系统，继承 ObjectRecyclerBase.cs 即可
/// 并确保默认池系统中的预制体使用相同的键/标识。
public class ObjectRecycler : ObjectRecyclerBase
{
    [SerializeField, FormerlySerializedAs("RegistreOnStart")]
    private PreRegister[] pooledPrefabs;

    private Dictionary<string, PooledInstance> pools = new Dictionary<string, PooledInstance>();

    private void Awake()
    {
        for (int i = 0; i < pooledPrefabs.Length; i++)
        {
            RegisterObject(pooledPrefabs[i].Name, pooledPrefabs[i].Prefab, pooledPrefabs[i].Lenght);
        }
    }
    /// 添加一个新的池化预制体
    /// <param name="poolName">此对象池的标识</param>
    public void RegisterObject(string poolName, GameObject prefab, int count)
    {
        if (prefab == null)
        {
            Debug.LogWarning("无法回收 " + poolName + "，尚未指定预制体。");
            return;
        }

        var p = new PooledInstance();
        p.Name = poolName;
        p.Prefab = prefab;
        GameObject g;
        p.PoolList = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            g = Instantiate(prefab, transform) as GameObject;
            p.PoolList[i] = g;
            g.SetActive(false);
        }
        pools.Add(poolName, p);
    }
    /// 实例化池化预制体
    /// 用它代替 GameObject.Instantiate(...)
    public override GameObject Instantiate(string objectName, Vector3 position, Quaternion rotation)
    {
        PooledInstance pool = pools[objectName];
        if(pool != null)
        {
            GameObject g = pool.GetCurrent();
            if(g == null)
            {
                g = Instantiate(pool.Prefab, transform);
                pool.ReplaceCurrent(g);
            }
            g.transform.SetPositionAndRotation(position, rotation);
            g.SetActive(true);
            pool.SetNext();
            return g;
        }
        else
        {
            Debug.LogError(string.Format("对象 {0} 未登记到对象池。", objectName));
            return null;
        }
    }

    public class PooledInstance
    {
        public string Name;
        public GameObject Prefab;
        public GameObject[] PoolList;
        public int CurrentPool;

        public GameObject GetCurrent() => PoolList[CurrentPool];
        public void SetNext() { CurrentPool = (CurrentPool + 1) % PoolList.Length; }
        public void ReplaceCurrent(GameObject g) => PoolList[CurrentPool] = g;
    } 

    [Serializable]
    public class PreRegister
    {
        public string Name;
        public GameObject Prefab;
        public int Lenght;
    }
}
