using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GFWK.Internal.Scriptables;
using GFWKEditor;

namespace GFWK.Runtime.UI
{
    public class GFWKCoinUI : MonoBehaviour
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
            if(GameConfig.isDataCached) OnCoinUpdate(null);
            GameEvents.onCoinUpdate += OnCoinUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            GameEvents.onCoinUpdate -= OnCoinUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedCoin"></param>
        void OnCoinUpdate(GFWKCoin updatedCoin)
        {
            var coinData = GFWKCore.Coins.GetCoinData(coin);
            if (coinData == null) return;

            if (coinText != null) coinText.text = coinData.GetCoins(PhotonNetworkWrapper.NickName).ToString();
            if (coinTextUGUI != null) coinTextUGUI.text = coinData.GetCoins(PhotonNetworkWrapper.NickName).ToString();
            if (coinIconImg != null) coinIconImg.sprite = coinData.CoinIcon;
        }
    }
}