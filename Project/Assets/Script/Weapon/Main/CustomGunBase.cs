using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomGunBase : MonoBehaviour
{
    /// <summary>
    /// 启动时该武器在玩家配装中，由 FPWeapon 调用
    /// </summary>
    /// <param name="gun"></param>
    public abstract void Initialitate(GunController gun);

    /// <summary>
    /// 点击一次开火按钮时调用
    /// </summary>
    public abstract void OnFireDown();

    /// <summary>
    /// 按住开火按钮时调用
    /// </summary>
    public abstract void OnFire();

    /// <summary>
    /// 需要生成投掷物时由 TPWeapon（网络枪）调用
    /// 在这里实现自定义开火逻辑
    /// </summary>
    public abstract void TPFire(RemoteGun tpWeapon, ExitGames.Client.Photon.Hashtable data);
}