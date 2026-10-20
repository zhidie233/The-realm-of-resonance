using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

   public class GameCoinPriceUI : MonoBehaviour
   {
       public CoinUI[] coins;
       public LayoutGroup layoutGroup;

       public GameCoinPriceUI SetPrice(int realPrice)
       {
           foreach (var item in coins)
           {
               item.ParsePrice(realPrice);
           }
           return this;
       }

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

       public void SetActive(bool active)
       {
           gameObject.SetActive(active);
       }

       [Serializable]
       public class CoinUI
       {
           [ItemCoinID] public int CoinID;
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