using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// 自动为 UI 元素应用队伍颜色
/// 或为 UI 文本设置队伍名称
public class GraphicTeamColor : MonoBehaviour
{
    public Team team = Team.All;
    public bool applyColor = true;
    public bool applyTeamName = false;
    public bool toUpper = true;

    private Graphic graphic;

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        if (graphic == null) { graphic = GetComponent<Graphic>(); }
        if (applyColor)
        {
            graphic.color = team.GetTeamColor();
        }

        if (applyTeamName)
        {
            var t = GetComponent<Text>();
            if (t != null) t.text = toUpper ? team.GetTeamName().ToUpper() : team.GetTeamName();
            else
            {
                var tmp = GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = toUpper ? team.GetTeamName().ToUpper() : team.GetTeamName();
            }
        }
    }
}
