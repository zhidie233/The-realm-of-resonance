using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemDropManager : EnvironmentEffectManager
{
    public ItemDropConfig config;
    private GameObject[] itemDropPrefab;
    
    [Header("掉落区域设置")]
    private BoxCollider dropArea;
    
    private int lastTriggeredMultiple = -1;
    private int lastIndex=0;
    
    protected override void Start()
    {
        base.Start();
        dropArea=GameObject.FindGameObjectWithTag("DropArea").GetComponent<BoxCollider>();
        itemDropPrefab=Resources.LoadAll<GameObject>("ItemDropPrefab");
        // 初始化上次触发倍数
        lastTriggeredMultiple = Mathf.FloorToInt(currentResonance / config.itemDropInterval);
    }
    
    public override void UpdateEffect()
    {
        // 检查共鸣值是否为100的倍数且大于0
        int currentMultiple = Mathf.FloorToInt(currentResonance / config.itemDropInterval);
        
        if (currentMultiple > 0 && currentMultiple > lastTriggeredMultiple)
        {
            lastTriggeredMultiple = currentMultiple;
            DropItem();
        }
    }
    
    private void DropItem()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (itemDropPrefab == null || itemDropPrefab.Length == 0) return;
        if (dropArea == null) return;
        
        // 在掉落区域内随机生成位置
        Vector3 randomPosition = GetRandomPositionInDropArea();
        
        // 随机选择道具
        int index=lastIndex++;
        GameObject selectedPrefab = itemDropPrefab[index];
        if (index >= itemDropPrefab.Length-1)
            lastIndex = 0;
        
        // 网络同步生成道具
        PhotonNetwork.Instantiate("ItemDropPrefab/"+selectedPrefab.name, randomPosition, Quaternion.identity);
        
    }
    
    private Vector3 GetRandomPositionInDropArea()
    {
        if (dropArea == null) return Vector3.zero;
        
        // 计算掉落区域内的随机位置
        Vector3 localRandomPos = new Vector3(
            Random.Range(-dropArea.size.x * 0.5f, dropArea.size.x * 0.5f),
            Random.Range(-dropArea.size.y * 0.5f, dropArea.size.y * 0.5f),
            Random.Range(-dropArea.size.z * 0.5f, dropArea.size.z * 0.5f)
        );
        
        // 转换为世界坐标
        Vector3 worldPos = dropArea.transform.TransformPoint(localRandomPos);
        Vector3 offset=new Vector3(0, 1f, 0);
        return worldPos-offset;
    }
}