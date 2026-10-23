using UnityEngine;

public class RobotManager : EnvironmentEffectManager
{
    private int lastTriggeredMultiple = -1;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;
        lastTriggeredMultiple = Mathf.FloorToInt(currentResonance / GameConfig.Instance.robotConfig.robotSpawnInterval);
    }

    public override void UpdateEffect()
    {
        if (!NetworkWrapper.IsMasterClient) return;

        int currentMultiple = Mathf.FloorToInt(currentResonance / GameConfig.Instance.robotConfig.robotSpawnInterval);

        if (currentMultiple <= 0 || currentMultiple <= lastTriggeredMultiple) return;

        lastTriggeredMultiple = currentMultiple;
        AIManager.Instance.SpawnBot(null, Team.Team3);
    }
}
