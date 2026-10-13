using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomGunBase : MonoBehaviour
{
    /// <summary>
    /// Called from FPWeapon at start when this weapon is in the player load out
    /// </summary>
    /// <param name="gun"></param>
    public abstract void Initialitate(GunController gun);

    /// <summary>
    /// Called when Fire button is clicked (one time)
    /// </summary>
    public abstract void OnFireDown();

    /// <summary>
    /// Called when Fire button is pressed (keep)
    /// </summary>
    public abstract void OnFire();

    /// <summary>
    /// Called from TPWeapon (Network Gun) when a projectile has to be instance
    /// use this to implement your custom fire logic
    /// </summary>
    public abstract void TPFire(RemoteGun tpWeapon, ExitGames.Client.Photon.Hashtable data);
}