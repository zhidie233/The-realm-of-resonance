using UnityEngine;

public class LauncherProjectile : ProjectileBase
{
    public GameObject explosionPrefab;
    public bool Pooled = false;
    public TrailRenderer trailRenderer;

    public float ExplosionRadius { get; set; } = 10;

    private bool detecting = true;
    ShotData bulletData;
    private Rigidbody _rigidbody;
    private float instanceTime = 0;

    public override void InitProjectile(ShotData data)
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        bulletData = data;
        ExplosionRadius = data.ImpactForce;
        _rigidbody.AddForce(transform.forward * bulletData.Speed, ForceMode.Impulse);
        instanceTime = Time.time;

        if (trailRenderer != null && !GameConfig.Instance.projectileTrailsEnabled)
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

    private void OnCollisionEnter(Collision collision)
    {
        OnHit(collision);
    }

    protected virtual void OnHit(Collision collision)
    {
        if (!detecting) return;
        if(bulletData != null)
        {
            if (!bulletData.isNetwork)
            {
                PlayerNetworkHandler pn = collision.gameObject.GetComponent<PlayerNetworkHandler>();
                if (pn != null && pn.isMine) { return; }//if the player hit itself
            }
            else
            {
                if (Time.time - instanceTime <= 0.005f) return;
            }
        }

        GameObject e = Instantiate(explosionPrefab, collision.contacts[0].point, Quaternion.identity) as GameObject;
        var blast = e.GetComponent<ExplosionBase>();
        if (blast != null)
        {
            bulletData.Position = transform.position;
            blast.SetRadius(ExplosionRadius);
            blast.InitExplosion(bulletData, GameCore.LocalPlayer.GameActor);
        }
        detecting = false;
        if (Pooled) { gameObject.SetActive(false); }
        else { Destroy(gameObject); }
    }
}