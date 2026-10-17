using UnityEngine;

/// <summary>
/// 派生类负责向玩家显示武器信息
/// 即玩家即将拾取的武器。
/// </summary>
public abstract class PickUpUIBase : MonoBehaviour
{
    /// <summary>
    /// 玩家进入触发器或准星指向地图中的枪械拾取物时调用
    /// </summary>
    public abstract void OnOverWeapon(GunPickUpBase toPickUpWeapon);

    public abstract void Show(DisplayInformation information);

    public abstract void Hide();

    public struct DisplayInformation
    {
        public string Text;
        public Sprite Icon;
        public string InputName;
    }

    private static PickUpUIBase _instance;
    public static PickUpUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<PickUpUIBase>(); }
            return _instance;
        }
    }
}