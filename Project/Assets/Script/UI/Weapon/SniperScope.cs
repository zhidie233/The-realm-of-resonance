﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Runtime.UI;

public class SniperScope : SniperScopeBase
{
    #region Public members
    [Range(0.02f, 0.5f)] public float transitionDuration = 0.2f;
    [Range(0, 0.5f)] public float fadeInDelay = 0.2f;
    public float breathingAmplitude = 0.14f;
    [Space]
    [GameToggle] public bool ShowDistance = true;
    [Space]
    [Tooltip("Objects to disable when the scope shown, usually the weapon and arms meshes.")]
    public List<GameObject> OnScopeDisable = new List<GameObject>();
    #endregion

    #region Private members
    private GunController m_gun;
    private Vector3 m_point = Vector3.zero;
    private float m_dist = 0.0f;
    private Text DistanceText;
    private bool returnedAim = true;
    private bool aiming = false;
    RaycastHit m_ray;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_gun = GetComponent<GunController>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        ScopeUIBase.Instance.SetupWeapon(this);
        aiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (ScopeUIBase.Instance != null) ScopeUIBase.Instance.SetActive(false);
        aiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (m_gun == null || ScopeTexture == null)
            return;

        if (m_gun.isAiming && !m_gun.isReloading)
        {
            GetDistance();

            if (!aiming)
            {
                returnedAim = false;
                ScopeUIBase.Instance?.Crossfade(true, transitionDuration, fadeInDelay, null, () =>
                  {
                      foreach (GameObject go in OnScopeDisable)
                      {
                          if (go == null) continue;
                          go.SetActive(false);
                      }
                  });

                // Active breathing effect
                if (breathingAmplitude > 0)
                {
                    m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(true, breathingAmplitude);
                }

                aiming = true;
            }
        }
        else
        {
            if (aiming)
            {
                float inverse = MathUtility.InverseLerp(m_gun.GetDefaultPosition(), m_gun.AimPosition, m_gun.transform.localPosition);
                bool wasFullAiming = inverse >= 0.78f;
                ScopeUIBase.Instance?.Crossfade(false, transitionDuration, 0, () =>
                   {
                       if (!returnedAim && m_gun.Info != null)
                       {
                           if(wasFullAiming) m_gun.SetToAim();
                           returnedAim = true;
                       }
                      
                       foreach (GameObject go in OnScopeDisable)
                       {
                           if (go == null) continue;
#if GameTPV
                           if (CameraViewSettings.IsThirdPerson()) continue;
#endif
                           go.SetActive(true);
                       }

                   });
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(false);
            }
            aiming = false;
        }
        if (ShowDistance && DistanceText)
        {
            DistanceText.text = m_dist.ToString("00") + "<size=10>m</size>";
        }
    }

    /// <summary>
    /// calculate the distance to the first object that raycast hits
    /// </summary>  
    void GetDistance()
    {
        if (!ShowDistance) return;

        Vector3 fwd = GameController.Instance.CameraRendered.transform.forward;
        if (Physics.Raycast(GameController.Instance.CameraRendered.transform.position, fwd, out m_ray, 1000))
        {
            m_point = m_ray.point;
            m_dist = UtilityHelper.Distance(m_point, GameController.Instance.CameraRendered.transform.position);
        }
        else
        {
            m_dist = 0.0f;
        }
    }
}