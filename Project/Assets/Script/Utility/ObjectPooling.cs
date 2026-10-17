using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 默认游戏对象池系统
/// 想用自己的对象池系统，继承 ObjectPoolingBase.cs 即可
/// 并确保默认池系统中的预制体使用相同的键/标识。
/// </summary>
public class ObjectPooling : ObjectPoolingBase
{
    [SerializeField, FormerlySerializedAs("RegistreOnStart")]
    private PreRegister[] pooledPrefabs;

    private Dictionary<string, PoolObject> pools = new Dictionary<string, PoolObject>();

    private void Awake()
    {
        for (int i = 0; i < pooledPrefabs.Length; i++)
        {
            RegisterObject(pooledPrefabs[i].Name, pooledPrefabs[i].Prefab, pooledPrefabs[i].Lenght);
        }
    }

    /// <summary>
    /// 添加一个新的池化预制体
    /// </summary>
    /// <param name="poolName">此对象池的标识</param>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    public void RegisterObject(string poolName, GameObject prefab, int count)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Can't pooled the prefab for: " + poolName + " because the prefab has not been assigned.");
            return;
        }

        var p = new PoolObject();
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

    /// <summary>
    /// 实例化池化预制体
    /// 用它代替 GameObject.Instantiate(...)
    /// </summary>
    /// <returns></returns>
    public override GameObject Instantiate(string objectName, Vector3 position, Quaternion rotation)
    {
        PoolObject pool = pools[objectName];
        if(pool != null)
        {
            GameObject g = pool.GetCurrent();
            if(g == null)//in case a pool object get destroyed, replace it 
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
            Debug.LogError(string.Format("Object {0} has not been register for pooling.", objectName));
            return null;
        }
    }

    public class PoolObject
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