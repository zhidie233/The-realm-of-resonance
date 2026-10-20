using System;
using UnityEngine;

   [CreateAssetMenu(fileName = "Bullet Decal List", menuName = "Game/Weapons/Decal/List")]
   public class BulletMarkList : ScriptableObject
   {
       public int genericSurfaceId = 0;
       public SurfaceMark[] surfaceDecals;

       /// <summary>
       /// 从给定表面随机取一个材质
       /// </summary>
       /// <param name="surfaceTag"></param>
       /// <returns></returns>
       public SurfaceMark GetDecalForSurface(string surfaceTag)
       {
           for (int i = 0; i < surfaceDecals.Length; i++)
           {
               if (surfaceDecals[i].SurfaceTag == surfaceTag)
               {
                   return surfaceDecals[i];
               }
           }
           return surfaceDecals[genericSurfaceId];
       }

       /// <summary>
       /// 从给定表面随机取一个材质
       /// </summary>
       /// <param name="surfaceTag"></param>
       /// <returns></returns>
       public SurfaceMark GetDecalForTag(Transform trans)
       {
           for (int i = 0; i < surfaceDecals.Length; i++)
           {
               if (trans.CompareTag(surfaceDecals[i].SurfaceTag))
               {
                   return surfaceDecals[i];
               }
           }
           return surfaceDecals[genericSurfaceId];
       }

       [Serializable]
       public class SurfaceMark
       {
           public string SurfaceTag;
           public Material[] DecalMaterials;
           public Vector2 SizeRange = new Vector2(0.9f, 1.2f);

           public Material GetMaterial()
           {
               return DecalMaterials[UnityEngine.Random.Range(0, DecalMaterials.Length)];
           }
       }
   }