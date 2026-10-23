using UnityEngine;
/// 默认游戏名称牌（头顶名称）脚本
/// 若想改用其他方案，例如用 UGUI 代替 OnGUI
/// 新建自定义脚本并继承 PlayerTagBase.cs
public class PlayerTagDrawer : PlayerTagBase
{
    #region Public members
    [Header("Settings")]
    public float distanceModifier = 1;
    public float hideDistance = 25;

    [Header("References")]
    public NameplateStyle StylePreset;
    public Transform positionReference;
    #endregion

    #region Private members
    private float distance;
    private Transform myTransform;
    private PlayerHealthManagerBase healthManager;
    private bool ShowHealthBar = false;
    private bool isFinish = false;
    float screenHeight = 0;
    Vector3 screenPoint;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        healthManager = GetComponent<PlayerHealthManagerBase>();
        ShowHealthBar = GameConfig.Instance.allyHealthBars;
        isFinish = MatchTimeManagerBase.Instance.IsTimeUp();
        this.myTransform = this.transform;

        if(GameController.Instance.CameraRendered != null)
        distance = CommonUtil.Distance(myTransform.position, GameController.Instance.CameraRendered.transform.position);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onRoundEnd += OnGameFinish;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onRoundEnd -= OnGameFinish;
    }

    public override void SetName(string DrawName)
    {
        PlayerName = DrawName;
    }

    public override void SetActive(bool active)
    {
        this.enabled = active;
    }

    void OnGameFinish()
    {
        isFinish = true;
    }

    void OnGUI()
    {
        if (BlockDraw || GameController.Instance.CameraRendered == null || isFinish || PauseMenuBase.IsMenuOpen)
            return;
        if (StylePreset == null) return;

        screenPoint = GameController.Instance.CameraRendered.WorldToScreenPoint(positionReference.position);
        if (screenPoint.z > 0)
        {
            int vertical = ShowHealthBar ? 15 : 10;
            screenHeight = Screen.height;
            if (this.distance < hideDistance)
            {
                float distanceDifference = Mathf.Clamp(distance - 0.1f, 1, 12);
                screenPoint.y += distanceDifference * distanceModifier;              

                GUI.Label(new Rect(screenPoint.x - 5, (screenHeight - screenPoint.y) - vertical, 10, 11), PlayerName, StylePreset.style);
                if (ShowHealthBar)
                {
                    GUI.color = StylePreset.HealthBackColor;
                    GUI.DrawTexture(new Rect(screenPoint.x - (50), (screenHeight - screenPoint.y), 100, StylePreset.HealthBarThickness), StylePreset.HealthBarTexture);
                    GUI.color = StylePreset.HealthBarColor;
                    GUI.DrawTexture(new Rect(screenPoint.x - (50), (screenHeight - screenPoint.y), healthManager.GetHealth(), StylePreset.HealthBarThickness), StylePreset.HealthBarTexture);
                    GUI.color = Color.white;
                }
            }
            else
            {
                float iconSize = StylePreset.IndicatorIconSize;
                GUI.DrawTexture(new Rect(screenPoint.x - (iconSize * 0.5f), (screenHeight - screenPoint.y) - (iconSize * 0.5f), iconSize, iconSize), StylePreset.IndicatorIcon);
            }
        }
    }

    public override void OnSlowUpdate()
    {
        if (GameController.Instance.CameraRendered == null)
            return;

        distance = CommonUtil.Distance(myTransform.position, GameController.Instance.CameraRendered.transform.position);
    }
}
