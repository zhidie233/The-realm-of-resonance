using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Internal
{
    [Serializable]
    public class UIListHandler
    {
        [GameToggle] public bool PoolList = true;
        public int PoolCount = 10;
        public GameObject Prefab;
        public RectTransform Panel;

        private List<GameObject> poolList;
        public int Current { get; set; } = 0;
        public bool IsInitialize => poolList != null && poolList.Count > 0;

        /// <summary>
        /// 按需初始化处理器
        /// </summary>
        public void Initialize()
        {
            if (IsInitialize) return;

            if (PoolList)
            {
                poolList = new List<GameObject>();
                for (int i = 0; i < PoolCount; i++)
                {
                    var go = GameObject.Instantiate(Prefab) as GameObject;
                    go.transform.SetParent(Panel, false);
                    go.SetActive(false);
                    poolList.Add(go);
                }
            }

            Prefab.SetActive(false);
        }

        /// <summary>
        /// 从池中获取一个可用对象
        /// </summary>
        /// <returns></returns>
        public GameObject Instantiate()
        {
            GameObject go = null;
            if (PoolList)
            {
                go = poolList[Current];
            }
            else
            {
                go = GameObject.Instantiate(Prefab) as GameObject;
                go.transform.SetParent(Panel, false);

                if (poolList == null) poolList = new List<GameObject>();
                poolList.Add(go);
            }

            go.SetActive(true);
            Prefab.SetActive(false);
            Current = (Current + 1) % PoolCount;
            return go;
        }

        /// <summary>
        /// 获取池中下一个可用对象，并返回其上挂载的指定组件。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InstatiateAndGet<T>()
        {
            var go = Instantiate();
            return go.GetComponent<T>();
        }

        public void Clear()
        {
            if (poolList == null) return;

            foreach (var item in poolList)
            {
                if (item == null) continue;
                GameObject.Destroy(item.gameObject);
            }
            poolList.Clear();
        }

        public int Count
        {
            get
            {
                if (poolList == null) return 0;
                return poolList.Count;
            }
        }
    }
}