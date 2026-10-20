using UnityEngine;

   public class AIBotWeaponHandler : MonoBehaviour
   {
       [Header("Info")]
       [FirearmID] public int GunID;
       [Range(1, 60)] public int Bullets = 30;
       [Range(1, 6)] public int bulletsPerShot = 1;
       public int maxFollowingShots = 5;
       public string BulletName = "bullet";
       [Header("References")]
       public Transform FirePoint;
       public ParticleSystem MuzzleFlash;
       public AudioClip fireSound;
       public AudioClip[] reloadSounds;

       public void Initialize(AIBotAttackBase shooterWeapon)
       {

       }

       private FirearmData m_info;
       public FirearmData Info
       {
           get
           {
               if (m_info == null)
               {
                   m_info = GameConfig.Instance.GetWeapon(GunID); ;
               }
               return m_info;
           }
       }

       private void OnDrawGizmosSelected()
       {
           Vector3 origin = FirePoint.position;
           Vector3 dir = transform.root.position + (transform.root.forward * 25);
#if UNITY_EDITOR
           UnityEditor.Handles.color = Color.yellow;
           UnityEditor.Handles.DrawDottedLine(origin, dir, 3);
           UnityEditor.Handles.color = Color.white;
#endif
       }
   }