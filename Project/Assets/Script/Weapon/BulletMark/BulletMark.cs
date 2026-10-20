using UnityEngine;

   public class BulletMark : BulletMarkBase
   {
       public Renderer meshRender;

       private Transform defaultParent;
       private Transform pendingParent;

       private Transform _transform;
       public Transform Transform
       {
           get
           {
               if (_transform == null) _transform = transform;
               return _transform;
           }
       }

       private GameObject _gameObject;
       public GameObject ThisGameObject
       {
           get
           {
               if (_gameObject == null) _gameObject = gameObject;
               return _gameObject;
           }
       }

       public override BulletMarkBase Init(Transform parent)
       {
           defaultParent = parent;
           Transform.parent = defaultParent;
           ThisGameObject.SetActive(false);
           return this;
       }

       public override BulletMarkBase SetDecalMaterial(Material mat)
       {
           if (ThisGameObject != null && !ThisGameObject.activeSelf) ThisGameObject.SetActive(true);

           // 用 .sharedMaterial，因为 .material 会为每个弹痕创建新的材质实例。
           meshRender.sharedMaterial = mat;
           return this;
       }

       /// <summary>
       /// 平移到射线检测命中点并旋转
       /// </summary>
       /// <returns></returns>
       public override BulletMarkBase SetToHit(RaycastHit hit, bool asPendingParent = false)
       {
           Transform.position = hit.point;
           Transform.rotation = Quaternion.LookRotation(-hit.normal);

           if (asPendingParent) pendingParent = hit.transform;
           else Transform.parent = hit.transform;

           return this;
       }

       /// <summary>
       /// 随机化弹痕大小
       /// </summary>
       /// <returns></returns>
       public override BulletMarkBase SetScaleVariation(Vector3 scaleBase, Vector2 Range)
       {
           if(pendingParent != null) Transform.parent = null;
           Transform.localScale = scaleBase * Random.Range(Range.x, Range.y);
           if (pendingParent != null)
           {
               Transform.SetParent(pendingParent);
               pendingParent = null;
           }

           return this;
       }

       /// <summary>
       /// 禁用并设回默认父节点
       /// </summary>
       public override void BackToOrigin()
       {
           Transform.parent = defaultParent;
           ThisGameObject.SetActive(false);
       }
   }