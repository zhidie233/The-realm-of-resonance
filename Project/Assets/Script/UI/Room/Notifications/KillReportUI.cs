using UnityEngine;
using Game.Internal.Structures;
using Game.Runtime.UI.Bindings;

namespace Game.Runtime.UI
{
    public class KillReportUI : KillReportUIBase
    {
        public int numberOfPooledPrefabs = 6;
        public Transform KillfeedPanel;
        public GameObject KillfeedPrefab;

        private KillReportBindingBase[] pool;
        private int currentPooled = 0;

        private void Start()
        {
            PreparePool();
        }

        void PreparePool()
        {
            if (pool != null) return;

            pool = new KillReportBindingBase[numberOfPooledPrefabs];
            for (int i = 0; i < numberOfPooledPrefabs; i++)
            {
                var obj = Instantiate(KillfeedPrefab) as GameObject;
                obj.transform.SetParent(KillfeedPanel, false);
                pool[i] = obj.GetComponent<KillReportBindingBase>();
                obj.SetActive(false);
            }
            KillfeedPrefab.SetActive(false);
        }

        /// <summary>
        /// 击杀他人时在右上角弹出全局提示
        /// </summary>
        public override void SetKillFeed(KillReport feed)
        {
            if (!UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.KillReport)) return;

            if (pool == null) PreparePool();

            var newkillfeed = pool[currentPooled];
            newkillfeed.Init(feed);
            newkillfeed.transform.SetParent(KillfeedPanel, false);
            newkillfeed.transform.SetAsFirstSibling();

            currentPooled = (currentPooled + 1) % numberOfPooledPrefabs;
        }
    }
}