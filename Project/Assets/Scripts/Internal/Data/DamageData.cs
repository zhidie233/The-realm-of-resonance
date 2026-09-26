using UnityEngine;
using Photon.Realtime;

// GFWK class with all the Damage information
public class DamageData
{
    // The amount of damage to apply to the IDamageable object
    public int Damage = 10;

    // The cached name of the actor of this damage Since the damage can't always comes from a <see cref="GFWKActor"/> (player or bot) This the alternative way to get the name of the actor.
    public string From;

    // Cause of the damage
    public DamageCause Cause = DamageCause.Player;

    // The position from where this damage origins (bullet origin, explosion origin, etc...)
    public Vector3 Direction { get; set; } = Vector3.zero;

    // Is this damage comes from a head shot?
    public bool isHeadShot { get; set; } = false;

    // The GunID of the weapon that cause this damage in case it was origin from a weapon if the damage was not from a weapon you can skip it.
    public int GunID { get; set; } = 0;

    // The network player from which this damage
    public Player Actor { get; set; }

    // The GFWK Actor of this damage Can be a real player or bot
    public GFWKPlayer GFWKActor { get; set; }

    // The cached network view id of the actor of this damage
    public int ActorViewID { get; set; }

    // Create a hashtable with the this damage data
    // <returns></returns>
    public ExitGames.Client.Photon.Hashtable GetAsHashtable()
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("d", Damage);
        data.Add("gi", GunID);
        data.Add("vi", ActorViewID);
        data.Add("c", Cause);
        data.Add("f", From);
        if (Direction != Vector3.zero)
            data.Add("dr", Direction);

        return data;
    }

    public DamageData(ExitGames.Client.Photon.Hashtable data)
    {
        Damage = (int)data["d"];
        GunID = (int)data["gi"];
        ActorViewID = (int)data["vi"];
        Cause = (DamageCause)data["c"];
        From = (string)data["f"];
        if (data.ContainsKey("dr")) Direction = (Vector3)data["dr"];

        GFWKActor = bl_GameManager.Instance.GetGFWKActor(ActorViewID);
    }

    public DamageData() { }
}

public struct GFWKHitData
{
    // The name of the object who was hit
    public string HitName;

    // The amount of damage applied to the hit object
    public int Damage;

    // The point of the hit collision
    public Vector3 HitPosition;

    // The transform reference of the hit object A null check is required before access this since is can be destroyed
    public Transform HitTransform;
}