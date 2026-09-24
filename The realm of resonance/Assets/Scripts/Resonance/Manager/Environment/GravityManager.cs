using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GravityManager : EnvironmentEffectManager
{
    [Header("配置")]
    public GravityConfig config;
    private AnimationCurve positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    // 受响应的物体
    private bl_FirstPersonController _blFirstPersonControllers;
    private List<GravityFacility> _gravityFacilities = new List<GravityFacility>();
    
    protected override void Start()
    {
        base.Start();
        
        // 查找所有GravityFacility物体
        FindAllGravityFacilities();
        
        // 初始更新
        UpdateEffect();
    }

    private void FindAllGravityFacilities()
    {
        _gravityFacilities.Clear();
        
        GameObject[] facilityObjects = GameObject.FindGameObjectsWithTag("GravityFacility");
        foreach (GameObject obj in facilityObjects)
        {
            GravityFacility facility = obj.GetComponent<GravityFacility>();
            if (facility != null)
            {
                _gravityFacilities.Add(facility);
                
                // 确保物体有PhotonView组件
                PhotonView photonView = obj.GetComponent<PhotonView>();
                if (photonView == null)
                {
                    photonView = obj.AddComponent<PhotonView>();
                    photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
                    photonView.ObservedComponents = new List<Component> { facility };
                }
            }
        }
    }

    public void SetBlFirstPersonController(bl_FirstPersonController controller)
    {
        this._blFirstPersonControllers = controller;
    }

    public override void UpdateEffect()
    {
        // 更新弹跳力（本地玩家）
        if (_blFirstPersonControllers != null)
        {
            float gravityMultiplier = config.jumpForce.Evaluate(currentResonance);
            _blFirstPersonControllers.jumpSpeed = gravityMultiplier;
        }
        
        // 更新所有GravityFacility物体的位置
        UpdateFacilityTransform();
    }

    private void UpdateFacilityTransform()
    {
        float maxResonance = config.GetMaxResonanceValue();
        
        foreach (GravityFacility facility in _gravityFacilities)
        {
            if (facility != null)
            {
                facility.UpdateTransformBasedOnResonance(currentResonance, maxResonance, positionCurve);
            }
        }
    }
    
    // 动态添加GravityFacility（如果需要）
    public void RegisterGravityFacility(GravityFacility facility)
    {
        if (!_gravityFacilities.Contains(facility))
        {
            _gravityFacilities.Add(facility);
        }
    }
    
    // 动态移除GravityFacility
    public void UnregisterGravityFacility(GravityFacility facility)
    {
        _gravityFacilities.Remove(facility);
    }
}