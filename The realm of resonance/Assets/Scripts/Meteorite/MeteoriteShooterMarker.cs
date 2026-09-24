using UnityEngine;
using System.Collections;

public class MeteoriteShooterMarker : MonoBehaviour
{
    private MFPSPlayer targetPlayer;
    private float displayTime;
    private float timer;
    
    public void Initialize(MFPSPlayer player, float time)
    {
        targetPlayer = player;
        displayTime = time;
        timer = 0f;
        
        // 将标记设置为玩家的子对象，跟随移动
        if (player.Actor != null)
        {
            transform.SetParent(player.Actor);
            transform.localPosition = Vector3.up * 3f; // 头顶位置
        }
    }
    
    private void Update()
    {
        if (targetPlayer == null || targetPlayer.Actor == null)
        {
            Destroy(gameObject);
            return;
        }
        
        timer += Time.deltaTime;
        if (timer >= displayTime)
        {
            Destroy(gameObject);
        }
        
        // 始终面向摄像机
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // 让箭头正面朝向摄像机
        }
    }
}