using System;
using UnityEngine.Serialization;

public abstract class GunPickUpBase : MonoBehaviourBase
{
    /// <summary>
    /// Ammunition information
    /// </summary>
    [Serializable]
    public class AmmoInformation
    {
        public int Bullets = 0;
        public int Clips = 0;

        public int GetBullets
        {
            get
            {
                int b = Bullets;
                if (GameConfig.Instance.AmmoType == AmmunitionType.Bullets)
                {
                    b = Bullets * Clips;
                }
                return b;
            }
        }
    }

    /// <summary>
    /// Identifier of the weapon
    /// </summary>
    [GunID] public int GunID = 0;

    /// <summary>
    /// Ammunition data of this dropped weapon
    /// </summary>
    [FormerlySerializedAs("Info")] 
    public AmmoInformation Ammunition;

    /// <summary>
    /// This weapon will be automatically be destroyed
    /// after certain time?
    /// </summary>
    public bool AutoDestroy = true;

    public GunData GunInfo
    {
        get
        {
            if (_gunInfo == null)
            {
                _gunInfo = GameConfig.Instance.GetWeapon(GunID);
            }
            return _gunInfo;
        }
        private set => _gunInfo = value;
    }
    private GunData _gunInfo;

    /// <summary>
    /// Pick up the weapon
    /// </summary>
    public abstract void PickUp();

    /// <summary>
    /// Destroy this weapon instance after a delay
    /// </summary>
    /// <param name="delay"></param>
    public abstract void DestroyAfter(float delay);
}