using System;
using System.Text;
using UnityEngine;
   [CreateAssetMenu(menuName = "项目/Coin", fileName = "Coin")]
   public class GameCoin : ScriptableObject
   {
       public string CoinName;
       public string Acronym;
       [Tooltip("该货币相对 1 的币值，例如价值 0.1 时，标价 100 的商品需要 1000 枚该货币")]
       public float CoinValue = 1;
       public int InitialCoins = 0;
       [TextArea(2, 3)] public string Description;
       [SpritePreview] public Sprite CoinIcon;
       /// 增加并保存硬币
       /// <param name="coins">要增加的硬币数</param>
       public GameCoin Add(int coins, string forUser = "")
       {
           int savedCoins = GetCoins(forUser);
           savedCoins += coins;

           PlayerPrefs.SetInt(Key(forUser), savedCoins);
           return this;
       }
       /// 增加并保存硬币
       /// <param name="coins">要增加的硬币数</param>
       public GameCoin Deduct(int coins, string forUser = "")
       {
           int savedCoins = GetCoins(forUser);
           savedCoins -= coins;

           if(savedCoins < 0)
           {
               Debug.LogWarning("交易执行前资金校验未通过。");
               savedCoins = 0;
           }

           PlayerPrefs.SetInt(Key(forUser), savedCoins);
           return this;
       }
       /// 获取本地保存的硬币
       public int GetCoins(string endPoint = "")
       {
           int savedCoins = PlayerPrefs.GetInt(Key(endPoint), InitialCoins);
           return savedCoins;
       }
       /// 返回此硬币相对基准价格（值为 1）的换算比
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
