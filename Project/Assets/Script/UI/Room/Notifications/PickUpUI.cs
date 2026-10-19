using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 默认游戏拾取 UI 处理器
/// 此脚本负责接收武器拾取事件的信息并显示给玩家
/// 要改行为请不要修改此脚本，新建自己的脚本并继承 PickUpUIBase
/// 以此作为参考。
/// </summary>
public class PickUpUI : PickUpUIBase
{
    public GameObject content;
    public TextMeshProUGUI mainText;
    public Image iconImg;
    public TextMeshProUGUI keyText;

    private FirearmPickUpBase CacheGunPickUp = null;


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

    public override void OnOverWeapon(FirearmPickUpBase gunPickUp)
    {
        if (gunPickUp == null) return;

        var info = gunPickUp.GunInfo;
        var inputName = InputHandler.GetButtonName("Interact");
        string t =  string.Format(GameTexts.PickUpWeapon, info.Name, inputName);
        Show(new DisplayInformation()
        {
            Text = t,
            Icon = info.GunIcon,
            InputName = inputName
        });
        CacheGunPickUp = gunPickUp;
    }

    public override void Show(DisplayInformation information)
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

    public override void Hide()
    {
        content.SetActive(false);
    }

    /// <summary>
    /// 由拾取 UI 按钮调用
    /// </summary>
    public void OnPickUpClicked()
    {
        if (CacheGunPickUp != null)
        {
            CacheGunPickUp.PickUp();
        }
    }
}