using UnityEngine;

public class bl_GrenadeLauncherProjectile : bl_ProjectileBase
{
    public GameObject explosionPrefab;
    public bool Pooled = false;
    public TrailRenderer trailRenderer;

    public float ExplosionRadius { get; set; } = 10;

    private bool detecting = true;
    BulletData bulletData;
    private Rigidbody _rigidbody;
    private float instanceTime = 0;

    // <param name="data"></param>
    public override void InitProjectile(BulletData data)
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        bulletData = data;
        ExplosionRadius = data.ImpactForce;
        _rigidbody.AddForce(transform.forward * bulletData.Speed, ForceMode.Impulse);
        instanceTime = Time.time;

        if (trailRenderer != null && !bl_GameData.Instance.showProjectilesTrails)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        detecting = true;
    }

    // <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        OnHit(collision);
    }

    // <param name="collision"></param>
    protected virtual void OnHit(Collision collision)
    {
        if (!detecting) return;
        if(bulletData != null)
        {
            if (!bulletData.isNetwork)
            {
                bl_PlayerNetwork pn = collision.gameObject.GetComponent<bl_PlayerNetwork>();
                if (pn != null && pn.isMine) { return; }//if the player hit itself
            }
            else
            {
                if (Time.time - instanceTime <= 0.005f) return;
            }
        }

        GameObject e = Instantiate(explosionPrefab, collision.contacts[0].point, Quaternion.identity) as GameObject;
        var blast = e.GetComponent<bl_ExplosionBase>();
        if (blast != null)
        {
            bulletData.Position = transform.position;
            blast.SetRadius(ExplosionRadius);
            blast.InitExplosion(bulletData, bl_GFWK.LocalPlayer.GFWKActor);
        }
        detecting = false;
        if (Pooled) { gameObject.SetActive(false); }
        else { Destroy(gameObject); }
    }
}