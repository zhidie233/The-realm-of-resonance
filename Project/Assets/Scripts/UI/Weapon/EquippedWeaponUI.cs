using TMPro;
using UnityEngine;

public class EquippedWeaponUI : EquippedWeaponUIBase
{
    [SerializeField] private TextMeshProUGUI AmmoText;
    [SerializeField] private TextMeshProUGUI ClipText;
    [SerializeField] private TextMeshProUGUI FireTypeText;
    public Gradient AmmoTextColorGradient;

    /// <summary>
    /// 
    /// </summary>
    public override void SetAmmoOf(GunController gun)
    {
        int bullets = gun.bulletsLeft;
        int clips = gun.RemainingClips;
        float per = (float)bullets / (float)gun.bulletsPerClip;
        Color c = AmmoTextColorGradient.Evaluate(per);

        if (gun.Info.Type != GunType.Knife)
        {
            AmmoText.text = bullets.ToString();
            if (gun.HaveInfinityAmmo)
                ClipText.text = "∞";
            else
                ClipText.text = ClipText.text = clips.ToString("F0");
            AmmoText.color = c;
            ClipText.color = c;
        }
        else
        {
            AmmoText.text = "--";
            ClipText.text = ClipText.text = "--";
            AmmoText.color = Color.white;
            ClipText.color = Color.white;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    public override void SetFireType(GunBase.FireType fireType)
    {
        if (FireTypeText == null) return;

        string fireName = "--";
        switch (fireType)
        {
            case GunBase.FireType.Auto:   fireName = GameTexts.FireTypeAuto.Localized(45);   break;
            case GunBase.FireType.Semi:   fireName = GameTexts.FireTypeSemi.Localized(47);   break;
            case GunBase.FireType.Single: fireName = GameTexts.FireTypeSingle.Localized(46); break;
        }
        FireTypeText.text = fireName;
    }
}