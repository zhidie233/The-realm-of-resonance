using UnityEngine;

// Contains all the information about an instanced bullet
public class BulletData 
{

    // The name of the weapon from which this bullet was fired.
    public string WeaponName;

    // The base damage that this bullet will cause
    public float Damage;

    // The Position from where this bullet was fired.
    public Vector3 Position;

    // The amount of force to applied to the hit object in case this have a RigidBody
    public float ImpactForce;

    // The bullet inaccuracy to apply to the projected direction
    public Vector3 Inaccuracity = Vector3.zero;

    // Bullet Speed
    public float Speed;

    // The max distance that this bullet can travel without hit anything.
    public float Range;

    // The amount of bullet drop
    public float DropFactor;

    // GunID of the weapon from which this bullet was fire
    public int WeaponID;

    // Was this bullet created by a remote player?
    public bool isNetwork;

    // Was this bullet created by the actual local player The difference between this and <see cref="isNetwork"/> Is that IsNetwork can be False (meaning is not a remote bullet) when a Bot created the bullet But this ensure that the bullet was created by the real local player.
    public bool IsLocalPlayer;

    // The Cached Network View
    public int ActorViewID { get; set; }

    // The MFPS Actor who create this bullet
    public MFPSPlayer MFPSActor { get; set; }

    // Create the bullet data and fetch info from the <see cref="DamageData"/>
    // <param name="data"></param>
    public BulletData(DamageData data)
    {
        MFPSActor = data.MFPSActor;
        Damage = data.Damage;
        ActorViewID = data.ActorViewID;
        WeaponID = data.GunID;
        Position = data.Direction;
    }

    // Calculate and assign the projectile inaccuracy vector
    // <param name="spreadBase"></param>
    // <param name="maxSpread"></param>
    // <returns></returns>
    public BulletData SetInaccuracity(float spreadBase, float maxSpread)
    {
        Inaccuracity = new Vector3(Random.Range(-maxSpread, maxSpread) * spreadBase, Random.Range(-maxSpread, maxSpread) * spreadBase, 1);
        return this;
    }

    public BulletData()
    {
    }
}