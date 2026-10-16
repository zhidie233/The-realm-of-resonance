using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ResonanceManager : MonoBehaviourPunCallbacks
{
    public static ResonanceManager Instance { get; private set; }
    
    [Header("配置")]
    public ResonanceConfig resonanceConfig;
    
    [Header("当前共鸣值")]
    [SerializeField] private float currentResonance = 0f;
    
    // 事件系统
    public event System.Action<float> OnResonanceChanged;
    
    // Room Property Key
    private const string RESONANCE_KEY = "ResonanceValue";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            if (resonanceConfig != null)
                resonanceConfig.Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RESONANCE_KEY))
        {
            currentResonance = (float)PhotonNetwork.CurrentRoom.CustomProperties[RESONANCE_KEY];
        }
    }

    // 修改共鸣值（网络同步）
    public void ChangeResonance(string changeKey, float multiplier = 1f)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        float changeValue = resonanceConfig.GetResonanceChangeValue(changeKey) * multiplier;
        currentResonance += changeValue;
        
        var properties = new ExitGames.Client.Photon.Hashtable();
        properties[RESONANCE_KEY] = currentResonance;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        
        OnResonanceChanged?.Invoke(currentResonance);
    }
    
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RESONANCE_KEY))
        {
            currentResonance = (float)propertiesThatChanged[RESONANCE_KEY];
            OnResonanceChanged?.Invoke(currentResonance);
        }
    }
    
    public float GetCurrentResonance()
    {
        return currentResonance;
    }
}
