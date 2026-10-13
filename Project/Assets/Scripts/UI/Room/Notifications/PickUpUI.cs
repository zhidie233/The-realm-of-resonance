using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Default GFWK PickUp UI handler
/// This script take care of receive information about pick up weapon events and display to the player
/// To modify the behave DO NOT modify this script, instead create your own and inherited from PickUpUIBase
/// Use this as reference.
/// </summary>
public class PickUpUI : PickUpUIBase
{
    public GameObject content;
    public TextMeshProUGUI mainText;
    public Image iconImg;
    public TextMeshProUGUI keyText;

    private GunPickUpBase CacheGunPickUp = null;

#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 13, 12 };
    private string[] LocaleStrings;
#endif

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        GameEvents.onLocalPlayerDeath += OnLocalDeath;
#if LOCALIZATION
        LocaleStrings = LocalizationTable.Instance.GetTextArray(LocaleTextIDs);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        GameEvents.onLocalPlayerDeath -= OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnOverWeapon(GunPickUpBase gunPickUp)
    {
        if (gunPickUp == null) return;

        var info = gunPickUp.GunInfo;
        var inputName = InputHandler.GetButtonName("Interact");
#if LOCALIZATION
        string t = string.Format(LocaleStrings[1], info.Name, inputName);
#else
        string t =  string.Format(GameTexts.PickUpWeapon, info.Name, inputName);
#endif
        Show(new DisplayInformation()
        {
            Text = t,
            Icon = info.GunIcon,
            InputName = inputName
        });
        CacheGunPickUp = gunPickUp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="information"></param>
    public override void Show(DisplayInformation information)
    {
        mainText.text = information.Text;
        iconImg.sprite = information.Icon;
        iconImg.gameObject.SetActive(information.Icon != null);
        if (!UtilityHelper.isMobile)
            keyText.text = information.InputName.ToUpper();
        else
            keyText.text = GameTexts.Touch.ToUpper();

        content.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Hide()
    {
        content.SetActive(false);
    }

    /// <summary>
    /// Called from the pick up UI button
    /// </summary>
    public void OnPickUpClicked()
    {
        if (CacheGunPickUp != null)
        {
            CacheGunPickUp.PickUp();
        }
    }
}