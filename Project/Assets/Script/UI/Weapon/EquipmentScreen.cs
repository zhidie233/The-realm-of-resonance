using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EquipmentScreen : MonoBehaviour
{
    [Serializable]
    public enum VisibilityMode
    {
        AutoHide,
        AlwaysShow,
    }

    [Flags]
    public enum EquipmentFlags : byte
    {
        None = 0,
        Animated = 1
    }

    private static EquipmentScreen _instance;
    public static EquipmentScreen Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<EquipmentScreen>(); }
            if(_instance == null) { _instance = UIReferences.Instance.GetComponentInChildren<EquipmentScreen>(true); }
            return _instance;
        }
    }

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

    public void SetVisibility(VisibilityMode visibility)
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

    /// 在槽位 UI 中显示给定的武器
    public void SetEquipment(List<FirearmController> guns)
    {
        for (int i = 0; i < SlotsGroups.Length; i++)
        {
            IconsImg[i].canvasRenderer.SetColor(Color.white);
            if (guns[i] == null || guns[i].Info == null) { SlotsGroups[i].gameObject.SetActive(false); continue; }

            Image img = SlotsGroups[i].GetComponentInChildren<Image>(false);
            img.sprite = guns[i].Info.WeaponIcon;
        }
        BackRect.position = SlotsGroups[0].position;
        current = 0;
        IconsImg[0].canvasRenderer.SetColor(Color.black);
    }

    /// 用给定武器信息替换指定槽位的 UI。
    /// <param name="slot">要替换的槽位。</param>
    /// <param name="newGun">读取信息的武器。</param>
    public void ReplaceSlot(int slot, FirearmController newGun)
    {
        if (IconsImg[slot].sprite == null)
        {
            Color c = IconsImg[slot].color;
            c.a = 1;
            IconsImg[slot].color = c;
        }
        IconsImg[slot].sprite = newGun.Info.WeaponIcon;
        SlotsGroups[slot].gameObject.SetActive(true);
    }

    /// 把给定槽位标记为当前装备武器
    /// <param name="slotID">要标记为已装备的槽位。</param>
    public void SetEquippedSlot(int nextSlot)
    {
        if (!GameConfig.Instance.ShowWeaponEquipment) return;

        StopAllCoroutines();
        StartCoroutine(ChangeSlot(nextSlot));
    }

    /// 清空武器槽位 UI
    /// 卸下武器时槽位不会自动刷新，需手动调用。
    public void ClearSlots(EquipmentFlags flags = EquipmentFlags.None)
    {
        for (int i = 0; i < IconsImg.Length; i++)
        {
            IconsImg[i].sprite = null;
            Color c = IconsImg[i].color;
            c.a = 0;
            IconsImg[i].color = c;
        }
        if (flags.IsEnumFlagPresent(EquipmentFlags.Animated))
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

    public void SetActive(bool active)
    {
        Alpha.gameObject.SetActive(active);
    }

    public VisibilityMode GetVisibilityMode()
    {
        return showMode;
    }
}