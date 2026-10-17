using UnityEngine;
using Game.Internal.Structures;
using Game.Runtime.UI.Bindings;

namespace Game.Runtime.UI
{
    public class KillFeedUI : KillFeedUIBase
    {
        public int numberOfPooledPrefabs = 6;
        public Transform KillfeedPanel;
        public GameObject KillfeedPrefab;

        private KillFeedUIBindingBase[] pool;
        private int currentPooled = 0;

        private void Start()
        {
            PreparePool();
        }

        void PreparePool()
        {
            if (pool != null) return;

            pool = new KillFeedUIBindingBase[numberOfPooledPrefabs];
            for (int i = 0; i < numberOfPooledPrefabs; i++)
            {
                var obj = Instantiate(KillfeedPrefab) as GameObject;
                obj.transform.SetParent(KillfeedPanel, false);
                pool[i] = obj.GetComponent<KillFeedUIBindingBase>();
                obj.SetActive(false);
            }
            KillfeedPrefab.SetActive(false);
        }

        /// <summary>
        /// Global notification (on right corner) when kill someone
        /// </summary>
        public override void SetKillFeed(KillFeed feed)
        {
            if (!UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.KillFeed)) return;

            if (pool == null) PreparePool();

            var newkillfeed = pool[currentPooled];
            newkillfeed.Init(feed);
            newkillfeed.transform.SetParent(KillfeedPanel, false);
            newkillfeed.transform.SetAsFirstSibling();

            currentPooled = (currentPooled + 1) % numberOfPooledPrefabs;
        }
    }
}