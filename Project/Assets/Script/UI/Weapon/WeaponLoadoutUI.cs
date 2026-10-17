using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponLoadoutUI : WeaponLoadoutUIBase
{
    [SerializeField] private VisibilityMode showMode = VisibilityMode.AutoHide;
    [SerializeField] private RectTransform BackRect = null;
    [SerializeField] private RectTransform[] SlotsGroups = null;
    [SerializeField] private CanvasGroup Alpha = null;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private int current = 0;
    private Image[] IconsImg = null;

    private void Awake()
    {
        Alpha.alpha = 0;
        Alpha.gameObject.SetActive(false);
        IconsImg = new Image[SlotsGroups.Length];
        for (int i = 0; i < SlotsGroups.Length; i++)
        {
            IconsImg[i] = SlotsGroups[i].GetComponentInChildren<Image>();
        }
        IconsImg[0].CrossFadeColor(Color.black, 0.1f, true, true);
    }

    public override void SetVisibility(VisibilityMode visibility)
    {
        showMode = visibility;
        if (showMode == VisibilityMode.AlwaysShow)
        {
            StopAllCoroutines();
            if (Alpha.alpha <= 0)
            {
                StartCoroutine(ChangeSlot(current));
            }
        }
    }

    public override void SetLoadout(List<GunController> guns)
    {
        for (int i = 0; i < SlotsGroups.Length; i++)
        {
            IconsImg[i].canvasRenderer.SetColor(Color.white);
            if (guns[i] == null || guns[i].Info == null) { SlotsGroups[i].gameObject.SetActive(false); continue; }

            Image img = SlotsGroups[i].GetComponentInChildren<Image>(false);
            img.sprite = guns[i].Info.GunIcon;
        }
        BackRect.position = SlotsGroups[0].position;
        current = 0;
        IconsImg[0].canvasRenderer.SetColor(Color.black);
    }

    public override void ReplaceSlot(int slot, GunController newGun)
    {
        if (IconsImg[slot].sprite == null)
        {
            Color c = IconsImg[slot].color;
            c.a = 1;
            IconsImg[slot].color = c;
        }
        IconsImg[slot].sprite = newGun.Info.GunIcon;
        SlotsGroups[slot].gameObject.SetActive(true);
    }

    public override void SetEquippedSlot(int nextSlot)
    {
        if (!GameConfig.Instance.ShowWeaponLoadout) return;

        StopAllCoroutines();
        StartCoroutine(ChangeSlot(nextSlot));
    }

    public override void ClearSlots(LoadoutFlags flags)
    {
        for (int i = 0; i < IconsImg.Length; i++)
        {
            IconsImg[i].sprite = null;
            Color c = IconsImg[i].color;
            c.a = 0;
            IconsImg[i].color = c;
        }
        if (flags.IsEnumFlagPresent(LoadoutFlags.Animated))
        {
            StopAllCoroutines();
            StartCoroutine(ChangeSlot(current));
        }
    }

    IEnumerator ChangeSlot(int nextSlot)
    {
        int cacheActual = current;
        current = nextSlot;
        Alpha.gameObject.SetActive(true);
        while (Alpha.alpha < 1)
        {
            Alpha.alpha += Time.deltaTime * 4;
            yield return null;
        }
        float d = 0;
        float t;
        IconsImg[cacheActual].CrossFadeColor(Color.white, 0.3f, true, true);
        IconsImg[nextSlot].CrossFadeColor(Color.black, 0.3f, true, true);
        while (d < 1)
        {
            d += Time.deltaTime * 5.5f;
            t = transitionCurve.Evaluate(d);
            BackRect.position = Vector3.Lerp(SlotsGroups[cacheActual].position, SlotsGroups[nextSlot].position, t);
            yield return null;
        }
        if (showMode == VisibilityMode.AutoHide)
        {
            yield return new WaitForSeconds(2.5f);
            while (Alpha.alpha > 0)
            {
                Alpha.alpha -= Time.deltaTime * 4;
                yield return null;
            }
            Alpha.gameObject.SetActive(false);
        }
    }

    public override void SetActive(bool active)
    {
        Alpha.gameObject.SetActive(active);
    }

    public override VisibilityMode GetVisibilityMode()
    {
        return showMode;
    }
}