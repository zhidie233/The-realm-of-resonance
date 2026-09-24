using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotManager : EnvironmentEffectManager
{
    public RobotConfig config;
    private int lastTriggeredMultiple = -1;

    protected override void Start()
    {
        base.Start();
        lastTriggeredMultiple = Mathf.FloorToInt(currentResonance / config.robotSpawnInterval);
    }

    public override void UpdateEffect()
    {
        // 检查共鸣值是否为100的倍数且大于0
        int currentMultiple = Mathf.FloorToInt(currentResonance / config.robotSpawnInterval);
        
        if (currentMultiple > 0 && currentMultiple > lastTriggeredMultiple)
        {
            lastTriggeredMultiple = currentMultiple;
            bl_AIMananger.Instance.SpawnBot(null, Team.Team3);
        }
    }
}
