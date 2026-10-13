using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GFWK.Runtime.AI;

public class PlayerScoreboardTable : PlayerScoreboardTableBase
{
    public Team team = Team.All;
    public RectTransform panel;
    public GameObject joinButton;
    public Graphic[] teamColorGraphics;
    public TextMeshProUGUI[] teamNameTexts;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Color c = team.GetTeamColor();
        foreach (var item in teamColorGraphics)
        {
            if (item != null)
                item.color = c;
        }
        string tn = team.GetTeamName().ToUpper();
        foreach (var item in teamNameTexts)
        {
            if (item != null)
                item.name = tn;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public override PlayerScoreboardUIBase Instance(Player player, GameObject uiPrefab)
    {
        GameObject instance = Instantiate(uiPrefab) as GameObject;
        instance.transform.SetParent(panel, false);
        var script = instance.GetComponent<PlayerScoreboardUIBase>();
        script.Init(player);
        instance.SetActive(true);
        return script;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public override PlayerScoreboardUIBase InstanceBot(GFWKBotProperties player, GameObject uiPrefab)
    {
        GameObject instance = Instantiate(uiPrefab) as GameObject;
        instance.transform.SetParent(panel, false);
        var script = instance.GetComponent<PlayerScoreboardUIBase>();
        script.Init(null, player);
        instance.SetActive(true);
        return script;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActiveJoinButton(bool active)
    {
        joinButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ResetJoinButton()
    {
        joinButton.GetComponent<Button>().interactable = true;
    }
}