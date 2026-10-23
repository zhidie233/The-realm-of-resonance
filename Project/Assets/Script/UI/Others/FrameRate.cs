using UnityEngine;
using TMPro;

public class FrameRate : MonoBehaviourBase
{
    [Range(0.1f, 1)] public float updateInterval = 0.4f;
    public string textFormat = "<b>FPS:</b> {0}";
    public TextMeshProUGUI TextUI = null;

    private string framerate;
    private float timeleft;
    private int rate = 0;
    private float accum;
    private int frames;
    private bool countFPS = true;

    void Start()
    {
        timeleft = updateInterval;
        OnSettingsChanged();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onGameSettingsChange += OnSettingsChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onGameSettingsChange -= OnSettingsChanged;
    }

    void OnSettingsChanged()
    {
        countFPS = (bool)GameCore.Settings.GetSettingOf("显示帧率");
        if (TextUI != null) TextUI.gameObject.SetActive(countFPS);
    }

    public override void OnUpdate()
    {
        if (TextUI == null || !countFPS) return;

        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;
        if (timeleft <= 0)
        {
            rate = Mathf.FloorToInt(accum / frames);
            framerate = rate.ToString();
            timeleft = updateInterval;
            accum = 0;
            frames = 0;
        }
        TextUI.text = string.Format(textFormat, framerate);
    }
}