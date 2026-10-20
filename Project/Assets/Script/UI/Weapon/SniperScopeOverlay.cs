using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SniperScopeOverlay : SniperScopeOverlayBase
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
    private FirearmController m_gun;
    private Vector3 m_point = Vector3.zero;
    private float m_dist = 0.0f;
    private Text DistanceText;
    private bool returnedAim = true;
    private bool aiming = false;
    RaycastHit m_ray;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        m_gun = GetComponent<FirearmController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ScopeOverlayBase.Instance.SetupWeapon(this);
        aiming = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (ScopeOverlayBase.Instance != null) ScopeOverlayBase.Instance.SetActive(false);
        aiming = false;
    }

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
                ScopeOverlayBase.Instance?.Crossfade(true, transitionDuration, fadeInDelay, null, () =>
                  {
                      foreach (GameObject go in OnScopeDisable)
                      {
                          if (go == null) continue;
                          go.SetActive(false);
                      }
                  });

                // 启用呼吸晃动效果
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
                float inverse = MathHelpers.InverseLerp(m_gun.GetDefaultPosition(), m_gun.AimPosition, m_gun.transform.localPosition);
                bool wasFullAiming = inverse >= 0.78f;
                ScopeOverlayBase.Instance?.Crossfade(false, transitionDuration, 0, () =>
                   {
                       if (!returnedAim && m_gun.Info != null)
                       {
                           if(wasFullAiming) m_gun.SetToAim();
                           returnedAim = true;
                       }
                      
                       foreach (GameObject go in OnScopeDisable)
                       {
                           if (go == null) continue;
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
    /// 计算射线首次命中物体的距离
    /// </summary>  
    void GetDistance()
    {
        if (!ShowDistance) return;

        Vector3 fwd = GameController.Instance.CameraRendered.transform.forward;
        if (Physics.Raycast(GameController.Instance.CameraRendered.transform.position, fwd, out m_ray, 1000))
        {
            m_point = m_ray.point;
            m_dist = CommonUtil.Distance(m_point, GameController.Instance.CameraRendered.transform.position);
        }
        else
        {
            m_dist = 0.0f;
        }
    }
}