using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class DeathCamUI :  DeathCamUIBase
{
    [SerializeField] private GameObject content = null;
    [SerializeField] private TextMeshProUGUI KillerNameText = null;
    [SerializeField] private TextMeshProUGUI KillerHealthText = null;
    [SerializeField] private TextMeshProUGUI GunNameText = null;
    [SerializeField] private TextMeshProUGUI respawnCountdown = null;
    [SerializeField] private Image GunImage = null;
    public Image levelIcon;
    public TextMeshProUGUI KillCamSpectatingText;

    public override void Show(DeathCamBase.DeathCamInfo kcinfo)
{
        var killer = kcinfo.TargetName;
        if (string.IsNullOrEmpty(killer) && kcinfo.Target != null)
        {
            killer = kcinfo.Target.name;
        }
        
        content.SetActive(true);
        FirearmData info = GameConfig.Instance.GetWeapon(kcinfo.GunID);
        GunImage.sprite = info.WeaponIcon;
        GunNameText.text = info.Name.ToUpper();
        killer = killer.Replace("(die)", "");
        KillerNameText.text = killer;
        KillCamSpectatingText.text = string.Format("<size=8>{0}:</size>\n{1}", GameTexts.Spectating.ToUpper(), killer);

        levelIcon.gameObject.SetActive(false);
        StartCoroutine(RespawnCountDown());
        GamePlayer actor = GameController.Instance.FindActor(killer);
        if(actor != null)
        {

            var pdm = actor.Actor.GetComponent<PlayerHealthManagerBase>();
            int health = Mathf.FloorToInt(pdm.GetHealth());
            if (pdm != null) { KillerHealthText.text = string.Format("HEALTH: {0}", health); }

            if (actor.isRealPlayer)
            {
            }
            else
            {
            }
        }
        else
        {
            KillerHealthText.text = string.Empty;
        }
    }

    IEnumerator RespawnCountDown()
    {
        float d = 0;
        float rt = GameConfig.Instance.respawnDelay;
        while (d < 1)
        {
            d += Time.deltaTime / rt;
            int remaing = Mathf.FloorToInt(rt * (1 - d));
            remaing = Mathf.Max(0, remaing);
            respawnCountdown.text = string.Format(GameTexts.RespawnIn, remaing);
            yield return null;
        }
    }

    public override void Hide()
    {
        content.SetActive(false);
    }
}