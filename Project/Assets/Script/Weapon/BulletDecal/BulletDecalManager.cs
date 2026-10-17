using UnityEngine;
using Game.Internal.Scriptables;
using GameEditor;

namespace Game.Runtime.Level
{
    public class BulletDecalManager : BulletDecalManagerBase
    {
        public int maxDecalInstances = 100;
        public BulletDecalBase decalPrefab;
        [ScriptableDrawer] public BulletDecalList decalList;

        private BulletDecalBase[] decalPool;
        private int currentPool = -1;
        private Vector3 decalBaseScale = Vector3.one;
        private bool showDecals = true;

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Instantiate a decal in the raycastHit point
        /// </summary>
        public override void InstanceDecal(RaycastHit raycastHit)
        {
            if (raycastHit.transform == null || !showDecals) return;

            Init();

            if (!showDecals) return;

            var decalInstance = GetPool();

            // since the decals are attached to the colliders, if a collider is destroyed, the decal get destroyed as well
            // so we have to make sure it is not null, otherwise, replace the null decal.
            if(decalInstance == null)
            {
                decalPool[currentPool] = Instantiate(decalPrefab.gameObject).GetComponent<BulletDecalBase>();
                decalPool[currentPool].Init(transform);
                decalInstance = decalPool[currentPool];
            }

            var decalData = decalList.GetDecalForTag(raycastHit.transform);

            decalInstance
                .SetDecalMaterial(decalData.GetMaterial())
                .SetToHit(raycastHit, true)
                .SetScaleVariation(decalBaseScale, decalData.SizeRange);
        }

        public BulletDecalBase GetPool()
        {
            currentPool = (currentPool + 1) % maxDecalInstances;
            return decalPool[currentPool];
        }

        /// <summary>
        /// Initialize the decal pool list
        /// </summary>
        private void Init()
        {
            if (decalPool != null) return;

            showDecals = GameConfig.Instance.bulletDecals;

            if (!showDecals) return;

            decalBaseScale = decalPrefab.transform.localScale;

            decalPool = new BulletDecalBase[maxDecalInstances];
            for (int i = 0; i < maxDecalInstances; i++)
            {
                decalPool[i] = Instantiate(decalPrefab.gameObject).GetComponent<BulletDecalBase>();
                decalPool[i].Init(transform);
            }
        }
    }
}