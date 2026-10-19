using System;
using System.Text;
using UnityEngine;
using GameEditor;

namespace Game.Internal.Scriptables
{
    [CreateAssetMenu(menuName = "Game/Coin", fileName = "Coin")]
    public class GameCoin : ScriptableObject
    {
        public string CoinName;
        public string Acronym;
        [Tooltip("The value of this coin with respect to 1, e.g items priced 100 can be purchase with 1000 coins with value of 0.1")]
        public float CoinValue = 1;
        public int InitialCoins = 0;
        [TextArea(2, 3)] public string Description;
        [SpritePreview] public Sprite CoinIcon;

        /// <summary>
        /// 增加并保存硬币
        /// </summary>
        /// <param name="coins">要增加的硬币数</param>
        /// <returns></returns>
        public GameCoin Add(int coins, string forUser = "")
        {
            int savedCoins = GetCoins(forUser);
            savedCoins += coins;

            PlayerPrefs.SetInt(Key(forUser), savedCoins);
            return this;
        }

        /// <summary>
        /// 增加并保存硬币
        /// </summary>
        /// <param name="coins">要增加的硬币数</param>
        /// <returns></returns>
        public GameCoin Deduct(int coins, string forUser = "")
        {
            int savedCoins = GetCoins(forUser);
            savedCoins -= coins;

            if(savedCoins < 0)
            {
                Debug.LogWarning("Something weird happen, funds where not verified before execute a transaction");
                savedCoins = 0;
            }

            PlayerPrefs.SetInt(Key(forUser), savedCoins);
            return this;
        }

        /// <summary>
        /// 获取本地保存的硬币
        /// </summary>
        /// <returns></returns>
        public int GetCoins(string endPoint = "")
        {
            int savedCoins = PlayerPrefs.GetInt(Key(endPoint), InitialCoins);
            return savedCoins;
        }

        /// <summary>
        /// 返回此硬币相对基准价格（值为 1）的换算比
        /// </summary>
        /// <param name="realPrice"></param>
        /// <returns></returns>
        public int DoConversion(int realPrice)
        {
            return Mathf.FloorToInt(realPrice / CoinValue);
        }

        private string Key(string endPoint = "")
        {
            var k = $"{Application.productName}.coin.{CoinName}.{endPoint}";
            //最基础的混淆方式；不建议将硬币存在本地。
            //正式项目请将硬币保存在外部服务器。
            k = Convert.ToBase64String(Encoding.UTF8.GetBytes(k));
            return k;
        }

        public static implicit operator int(GameCoin coin) => GameCore.Coins.GetIndexOfCoin(coin);
        public static explicit operator GameCoin(int coinID) => GameCore.Coins.GetAllCoins()[coinID];
    }
}