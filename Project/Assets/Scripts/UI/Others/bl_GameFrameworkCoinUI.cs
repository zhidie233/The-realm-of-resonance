using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GFWK.Internal.Scriptables;
using GFWKEditor;

namespace GFWK.Runtime.UI
{
    public class bl_GFWKCoinUI : MonoBehaviour
    {
        [GFWKCoinID] public int coin;
        [SerializeField] private TextMeshProUGUI coinText = null;
        [SerializeField] private Text coinTextUGUI = null;
        [SerializeField] private Image coinIconImg = null;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if(bl_GameData.isDataCached) OnCoinUpdate(null);
            bl_EventHandler.onCoinUpdate += OnCoinUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onCoinUpdate -= OnCoinUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedCoin"></param>
        void OnCoinUpdate(GFWKCoin updatedCoin)
        {
            var coinData = bl_GFWK.Coins.GetCoinData(coin);
            if (coinData == null) return;

            if (coinText != null) coinText.text = coinData.GetCoins(bl_PhotonNetwork.NickName).ToString();
            if (coinTextUGUI != null) coinTextUGUI.text = coinData.GetCoins(bl_PhotonNetwork.NickName).ToString();
            if (coinIconImg != null) coinIconImg.sprite = coinData.CoinIcon;
        }
    }
}