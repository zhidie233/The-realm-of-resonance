using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFireManager : EnvironmentEffectManager
{
    public GroundFireConfig config;
    
    private Transform groundFiresTransform;
    private List<Transform> fireZones = new List<Transform>();
    
    private List<GameObject> activeFires = new List<GameObject>();
    private bool isActive = false;

    protected override void Start()
    {
        base.Start();
        groundFiresTransform=GameObject.FindGameObjectWithTag("GroundFire").transform;
        Transform[] tempTransform = groundFiresTransform.GetComponentsInChildren<Transform>();
        //剔除自身
        fireZones.AddRange(tempTransform);
        foreach (var firezone in fireZones)
        {
            if (firezone.gameObject.tag == "GroundFire")
            {
                fireZones.RemoveAt(fireZones.IndexOf(firezone));
                break;
            }
        }
    }

    public override void UpdateEffect()
    {
        if (currentResonance >= config.activationThreshold)
        {
            if (!isActive)
            {
                ActivateFires();
                isActive = true;
            }
            UpdateFireIntensity();
        }
        else
        {
            if (isActive)
            {
                DeactivateFires();
                isActive = false;
            }
        }
    }
    
    private void ActivateFires()
    {
        foreach (var zone in fireZones)
        {
            var fire = Instantiate(config.fireEffectPrefab, zone.position, Quaternion.identity, zone);
            activeFires.Add(fire);
        }
    }
    
    private void UpdateFireIntensity()
    {
        float damage = config.fireDamageCurve.Evaluate(currentResonance);
        foreach (var fire in activeFires)
        {
            var fireComponent = fire.GetComponent<bl_DamageArea>();
            if (fireComponent != null)
                fireComponent.Damage = (int)damage;

            fire.transform.localScale = new Vector3(1, (1f + currentResonance * 0.01f) * config.effectScaleMultiplier, 1);
        }
    }
    
    private void DeactivateFires()
    {
        foreach (var fire in activeFires)
        {
            Destroy(fire);
        }
        activeFires.Clear();
    }
}