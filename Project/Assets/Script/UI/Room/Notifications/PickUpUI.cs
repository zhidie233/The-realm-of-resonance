using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// 默认游戏拾取 UI 处理器
/// 此脚本负责接收武器拾取事件的信息并显示给玩家
public class PickUpUI : MonoBehaviour
{
    public struct DisplayInformation
    {
        public string Text;
        public Sprite Icon;
        public string InputName;
    }

    private static PickUpUI _instance;
    public static PickUpUI Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<PickUpUI>(); }
            return _instance;
        }
    }

    public GameObject content;
    public TextMeshProUGUI mainText;
    public Image iconImg;
    public TextMeshProUGUI keyText;

    private FirearmPickUp CacheGunPickUp = null;

    private void OnEnable()
    {
        GameEvents.onLocalPlayerDeath += OnLocalDeath;
    }

    private void OnDisable()
    {
        GameEvents.onLocalPlayerDeath -= OnLocalDeath;
    }

    void OnLocalDeath()
    {
        Hide();
    }

    /// 玩家进入触发器或准星指向地图中的枪械拾取物时调用
    public void OnOverWeapon(FirearmPickUp gunPickUp)
    {
        if (gunPickUp == null) return;

        var info = gunPickUp.GunInfo;
        var inputName = InputHandler.GetButtonName("Interact");
        string t =  string.Format(GameTexts.PickUpWeapon, info.Name, inputName);
        Show(new DisplayInformation()
        {
            Text = t,
            Icon = info.WeaponIcon,
            InputName = inputName
        });
        CacheGunPickUp = gunPickUp;
    }

    public void Show(DisplayInformation information)
    {
        mainText.text = information.Text;
        iconImg.sprite = information.Icon;
        iconImg.gameObject.SetActive(information.Icon != null);
        if (!CommonUtil.isMobile)
            keyText.text = information.InputName.ToUpper();
        else
            keyText.text = GameTexts.Touch.ToUpper();

        content.SetActive(true);
    }

    public void Hide()
    {
        content.SetActive(false);
    }
    /// 由拾取 UI 按钮调用
    public void OnPickUpClicked()
    {
        if (CacheGunPickUp != null)
        {
            CacheGunPickUp.PickUp();
        }
    }
}
