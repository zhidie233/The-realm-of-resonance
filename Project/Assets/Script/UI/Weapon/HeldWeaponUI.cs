using TMPro;
using UnityEngine;

public class HeldWeaponUI : HeldWeaponUIBase
{
    [SerializeField] private TextMeshProUGUI AmmoText;
    [SerializeField] private TextMeshProUGUI ClipText;
    [SerializeField] private TextMeshProUGUI FireTypeText;
    public Gradient AmmoTextColorGradient;

    public override void SetAmmoOf(FirearmController gun)
    {
        int bullets = gun.bulletsLeft;
        int clips = gun.RemainingClips;
        float per = (float)bullets / (float)gun.bulletsPerClip;
        Color c = AmmoTextColorGradient.Evaluate(per);

        if (gun.Info.Type != FirearmType.Knife)
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

    public override void SetFireType(FirearmBase.FireType fireType)
    {
        if (FireTypeText == null) return;

        string fireName = "--";
        switch (fireType)
        {
            case FirearmBase.FireType.Auto:   fireName = GameTexts.FireTypeAuto;   break;
            case FirearmBase.FireType.Semi:   fireName = GameTexts.FireTypeSemi;   break;
            case FirearmBase.FireType.Single: fireName = GameTexts.FireTypeSingle; break;
        }
        FireTypeText.text = fireName;
    }
}