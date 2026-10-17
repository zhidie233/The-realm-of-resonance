using System;
using UnityEngine.Serialization;

public abstract class GunPickUpBase : MonoBehaviourBase
{
    /// <summary>
    /// 弹药信息
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
    /// 武器标识
    /// </summary>
    [GunID] public int GunID = 0;

    /// <summary>
    /// 这把掉落武器的弹药数据
    /// </summary>
    [FormerlySerializedAs("Info")] 
    public AmmoInformation Ammunition;

    /// <summary>
    /// 这把武器将被自动销毁
    /// 是否在特定时间后？
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
    /// 拾取武器
    /// </summary>
    public abstract void PickUp();

    /// <summary>
    /// 延迟后销毁该武器实例
    /// </summary>
    /// <param name="delay"></param>
    public abstract void DestroyAfter(float delay);
}