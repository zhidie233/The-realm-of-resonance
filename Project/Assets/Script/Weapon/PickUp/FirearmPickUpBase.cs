using System;
using UnityEngine.Serialization;

public abstract class FirearmPickUpBase : MonoBehaviourBase
{
    /// 弹药信息
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
    /// 武器标识
    [FirearmID] public int GunID = 0;
    /// 这把掉落武器的弹药数据
    [FormerlySerializedAs("Info")] 
    public AmmoInformation Ammunition;
    /// 这把武器将被自动销毁
    /// 是否在特定时间后？
    public bool AutoDestroy = true;

    public FirearmData GunInfo
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
    private FirearmData _gunInfo;
    /// 拾取武器
    public abstract void PickUp();
    /// 延迟后销毁该武器实例
    public abstract void DestroyAfter(float delay);
}
