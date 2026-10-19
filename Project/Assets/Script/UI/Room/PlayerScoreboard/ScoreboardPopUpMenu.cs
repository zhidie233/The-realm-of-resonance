using System.Collections.Generic;
using Game.Internal;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardPopUpMenu : ScoreboardPopUpMenuBase
{
    public List<MenuOptions> options;
    public ListLayoutHelper listHandler;
    [SerializeField] private GameObject content = null;

    private void OnEnable()
    {
        PrepareMenu();
        ((RectTransform)transform).position = Input.mousePosition;
    }

    public override ScoreboardPopUpMenuBase SetActive(bool active)
    {
        PrepareMenu();
        content.SetActive(active);
        return this;
    }

    public override ScoreboardPopUpMenuBase SetTargetPlayer(Player player)
    {
        TargetPlayer = player;
        return this;
    }

    public override ScoreboardPopUpMenuBase FilterMenuOptions(MenuFilter filter)
    {
        foreach (var item in options)
        {
            if (filter.IsBot)
            {
                if (!item.AllowedForBots)
                {
                    item.OptionButton.interactable = false;
                    continue;
                }
            }

            if (filter.IsLocalPlayer)
            {
                if (!item.AllowedForLocal)
                {
                    item.OptionButton.interactable = false;
                    continue;
                }
            }

            item.OptionButton.interactable = true;
        }
        return this;
    }

    public void PrepareMenu()
    {
        if (listHandler.IsInitialize) return;

        listHandler.Prefab.SetActive(false);
        for (int i = 0; i < options.Count; i++)
        {
            var btn = listHandler.InstatiateAndGet<Button>();
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null)
            {
                text.text = options[i].Title;
            }
            options[i].OptionButton = btn;
            int id = i;
            btn.onClick.AddListener(() => { OnOptionClicked(id); });
        }
    }

    public override int AddButton(string buttonName, MenuFilter filter)
    {
        if (!listHandler.IsInitialize)
        {
            PrepareMenu();
        }

        var btn = listHandler.InstatiateAndGet<Button>();
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = buttonName;
        }
        int id = listHandler.Count - 1;
        btn.onClick.AddListener(() => { OnOptionClicked(id); });

        options.Add(new MenuOptions()
        {
            Title = buttonName,
            OptionButton = btn,
            AllowedForBots = filter.IsBot,
            AllowedForLocal = filter.IsLocalPlayer
        });

        return id;
    }

    /// <summary>
    /// 玩家点击弹窗按钮之一时调用
    /// </summary>
    /// <param name="option"></param>
    public void OnOptionClicked(int option)
    {
        // 踢人事件在 KickPollUI.cs 中处理
        onScoreboardMenuAction?.Invoke(option);
        SetActive(false);
    }
}