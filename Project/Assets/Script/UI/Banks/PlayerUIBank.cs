using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIBank : MonoBehaviour
{
    [Header("REFERENCES")]
    public Canvas PlayerUICanvas;
    public GameObject KillZoneUI;
    public GameObject WeaponStatsUI;
    public GameObject playerStatsUI;
    public GameObject MaxKillsUI;
    public GameObject TimeUIRoot;
    public Image PlayerStateIcon;
    public Image HealthBar;
    public CanvasGroup DamageAlpha;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI HealthText;

    private void Awake()
    {
        UpdateUIDisplay();
    }

    public void UpdateUIDisplay()
    {
        TimeUIRoot.SetActive(UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Time));
        WeaponStatsUI.SetActive(UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.WeaponData));
        playerStatsUI.SetActive(UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.PlayerStats));
        if (EquipmentScreenBase.Instance != null) EquipmentScreenBase.Instance.SetActive(UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Equipment));
        GameEvents.DispatchUIMaskChange(UIReferences.Instance.UIMask);
    }
}