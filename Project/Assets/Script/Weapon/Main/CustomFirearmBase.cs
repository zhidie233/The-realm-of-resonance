using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomFirearmBase : MonoBehaviour
{
    /// 启动时该武器在玩家配装中，由 FPWeapon 调用
    public abstract void Initialitate(FirearmController gun);
    /// 点击一次开火按钮时调用
    public abstract void OnFireDown();
    /// 按住开火按钮时调用
    public abstract void OnFire();
    /// 需要生成投掷物时由 TPWeapon（网络枪）调用
    /// 在这里实现自定义开火逻辑
    public abstract void TPFire(NetworkFirearm tpWeapon, ExitGames.Client.Photon.Hashtable data);
}
