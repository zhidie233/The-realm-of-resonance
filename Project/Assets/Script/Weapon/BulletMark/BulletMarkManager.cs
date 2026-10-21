using UnityEngine;

   public class BulletMarkManager : BulletMarkManagerBase
   {
       public int maxDecalInstances = 100;
       public BulletMarkBase decalPrefab;
       [ScriptableDrawer] public BulletMarkList decalList;

       private BulletMarkBase[] decalPool;
       private int currentPool = -1;
       private Vector3 decalBaseScale = Vector3.one;
       private bool showDecals = true;

       private void Start()
       {
           Init();
       }

       /// <summary>
       /// 在射线检测命中点生成弹痕
       /// </summary>
       public override void InstanceDecal(RaycastHit raycastHit)
       {
           if (raycastHit.transform == null || !showDecals) return;

           Init();

           if (!showDecals) return;

           var decalInstance = GetPool();

           // 弹痕挂在碰撞体上，碰撞体销毁时弹痕也会被销毁
           // 所以必须确认它不为空，为空就替换掉这个弹痕。
           if(decalInstance == null)
           {
               decalPool[currentPool] = Instantiate(decalPrefab.gameObject).GetComponent<BulletMarkBase>();
               decalPool[currentPool].Init(transform);
               decalInstance = decalPool[currentPool];
           }

           var decalData = decalList.GetDecalForTag(raycastHit.transform);

           decalInstance
               .SetDecalMaterial(decalData.GetMaterial())
               .SetToHit(raycastHit, true)
               .SetScaleVariation(decalBaseScale, decalData.SizeRange);
       }

       public BulletMarkBase GetPool()
       {
           currentPool = (currentPool + 1) % maxDecalInstances;
           return decalPool[currentPool];
       }

       /// <summary>
       /// 初始化弹痕池列表
       /// </summary>
       private void Init()
       {
           if (decalPool != null) return;

           showDecals = GameConfig.Instance.impactDecals;

           if (!showDecals) return;

           decalBaseScale = decalPrefab.transform.localScale;

           decalPool = new BulletMarkBase[maxDecalInstances];
           for (int i = 0; i < maxDecalInstances; i++)
           {
               decalPool[i] = Instantiate(decalPrefab.gameObject).GetComponent<BulletMarkBase>();
               decalPool[i].Init(transform);
           }
       }
   }