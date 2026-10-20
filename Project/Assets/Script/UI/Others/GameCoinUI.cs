using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

   public class GameCoinUI : MonoBehaviour
   {
       [ItemCoinID] public int coin;
       [SerializeField] private TextMeshProUGUI coinText = null;
       [SerializeField] private Text coinTextUGUI = null;
       [SerializeField] private Image coinIconImg = null;

       private void OnEnable()
       {
           if(GameConfig.isDataCached) OnCoinUpdate(null);
           GameEvents.onCoinUpdate += OnCoinUpdate;
       }

       private void OnDisable()
       {
           GameEvents.onCoinUpdate -= OnCoinUpdate;
       }

       void OnCoinUpdate(GameCoin updatedCoin)
       {
           var coinData = GameCore.Coins.GetCoinData(coin);
           if (coinData == null) return;

           if (coinText != null) coinText.text = coinData.GetCoins(NetworkWrapper.NickName).ToString();
           if (coinTextUGUI != null) coinTextUGUI.text = coinData.GetCoins(NetworkWrapper.NickName).ToString();
           if (coinIconImg != null) coinIconImg.sprite = coinData.CoinIcon;
       }
   }