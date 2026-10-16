﻿using System;
using System.Collections.Generic;
using UnityEngine;
using GameEditor;
using TMPro;
using UnityEngine.UI;
using Game.Internal.Structures;
using Game.Internal.Scriptables;
using System.Linq;

namespace Game.Runtime.UI
{
    public class GameCoinPriceUI : MonoBehaviour
    {
        public CoinUI[] coins;
        public LayoutGroup layoutGroup;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="realPrice"></param>
        public GameCoinPriceUI SetPrice(int realPrice)
        {
            foreach (var item in coins)
            {
                item.ParsePrice(realPrice);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="realPrice"></param>
        public GameCoinPriceUI SetPrice(GameItemUnlockability unlockability)
        {
            var noallowedCoins = unlockability.NoAllowedCoins.ToList();
            foreach (var item in coins)
            {
                item.ParsePrice(unlockability.Price);
                if(item.PriceText != null && noallowedCoins.Contains(GameCore.Coins.GetCoinData(item.CoinID)))
                {
                    item.PriceText.gameObject.SetActive(false);
                }
            }
            if(layoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                layoutGroup.enabled = false;
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        [Serializable]
        public class CoinUI
        {
            [GameCoinID] public int CoinID;
            public TextMeshProUGUI PriceText;
            public Image CoinIcon;

            public void ParsePrice(int realPrice)
            {
                var coin = GameCore.Coins.GetCoinData(CoinID);
                if (coin == null) return;

                if (CoinIcon != null) CoinIcon.sprite = coin.CoinIcon;
                if (PriceText != null) PriceText.text = coin.DoConversion(realPrice).ToString();
            }
        }
    }
}