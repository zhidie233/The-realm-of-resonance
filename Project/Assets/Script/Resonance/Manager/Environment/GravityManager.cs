using System.Collections.Generic;
using UnityEngine;

public class GravityManager : EnvironmentEffectManager
{
    private AnimationCurve positionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private FirstPersonController _localPlayerController;
    private List<GravityFacility> _gravityFacilities = new List<GravityFacility>();

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;

        FindAllGravityFacilities();
        BindLocalPlayer();
        UpdateEffect();

        GameEvents.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEvents.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    void OnLocalPlayerSpawn()
    {
        BindLocalPlayer();
        UpdateEffect();
    }

    void BindLocalPlayer()
    {
        if (GameController.Instance == null || GameController.Instance.LocalPlayer == null) return;

        _localPlayerController = GameController.Instance.LocalPlayer.GetComponent<FirstPersonController>();
    }

    private void FindAllGravityFacilities()
    {
        _gravityFacilities.Clear();

        GameObject[] facilityObjects = GameObject.FindGameObjectsWithTag("GravityFacility");
        foreach (GameObject obj in facilityObjects)
        {
            GravityFacility facility = obj.GetComponent<GravityFacility>();
            if (facility != null)
                _gravityFacilities.Add(facility);
        }
    }

    public override void UpdateEffect()
    {
        if (_localPlayerController != null)
        {
            float gravityMultiplier = GameConfig.Instance.gravityConfig.jumpForce.Evaluate(currentResonance);
            _localPlayerController.jumpSpeed = gravityMultiplier;
        }

        UpdateFacilityTransform();
    }

    private void UpdateFacilityTransform()
    {
        float maxResonance = GameConfig.Instance.gravityConfig.GetMaxResonanceValue();

        foreach (GravityFacility facility in _gravityFacilities)
        {
            if (facility != null)
                facility.UpdateTransformBasedOnResonance(currentResonance, maxResonance, positionCurve);
        }
    }
}
