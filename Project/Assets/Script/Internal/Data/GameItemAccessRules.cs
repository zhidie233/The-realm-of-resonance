using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

   [Serializable]
   public class GameItemAccessRules
   {
       public AccessRulesMethod UnlockMethod = AccessRulesMethod.UnlockedByDefault;
       public ItemTypeEnum ItemType = ItemTypeEnum.Weapon;
       public int Price = 0;
       public int UnlockAtLevel = 0;
       [Tooltip("禁止用于购买该物品的货币；留空表示所有货币均可购买")]
       [ItemCoinID] public int[] BlockedCoins;
       /// 此物品对本地玩家是否已解锁？
       public bool IsUnlocked(int itemID)
       {
           if (UnlockMethod == AccessRulesMethod.UnlockedByDefault) return true;
           return GetLockReason(itemID) == LockReason.None;
       }
       /// 锁定时返回原因，否则返回 LockReason.None
       public LockReason GetLockReason(int itemID)
       {
           var reason = LockReason.None;
           if (UnlockMethod == AccessRulesMethod.UnlockedByDefault) return reason;
           if (UnlockMethod == AccessRulesMethod.Hidden) return LockReason.Hidden;

           bool isPurchased = false;
           if (isPurchased) { }
           return reason;
       }

       public bool IsFree() => Price <= 0;

       public bool CanBePurchased()
       {
           if (UnlockMethod == AccessRulesMethod.UnlockedByDefault || UnlockMethod == AccessRulesMethod.LevelUpOnly) return false;
           return true;
       }

       public bool CanBeUnlockedByLevel()
       {
           if (UnlockMethod == AccessRulesMethod.UnlockedByDefault || UnlockMethod == AccessRulesMethod.PurchasedOnly) return false;
           return true;
       }

       public bool AllowUnlockByLevel()
       {
           if (!CanBeUnlockedByLevel() || UnlockAtLevel <= 0) return false;
           return true;
       }
       /// 获取可用于此物品的硬币
       /// 与游戏内所有可用硬币比较（在 GameData -> Game Coins 中指定）
       public List<GameCoin> GetAllowedCoins()
       {
           var all = GameCore.Coins.GetAllCoins();
           var copy = new List<GameCoin>();
           foreach (var c in all)
           {
               copy.Add(c);
           }

           if (BlockedCoins == null || BlockedCoins.Length <= 0) return copy;

           return copy.Except(GetBlockedCoins()).ToList();
       }

       public List<GameCoin> GetBlockedCoins()
       {
           var copy = new List<GameCoin>();
           for (int i = 0; i < BlockedCoins.Length; i++)
           {
               copy.Add((GameCoin)BlockedCoins[i]);
           }
           return copy;
       }

       [Serializable]
       public enum AccessRulesMethod
       {
           UnlockedByDefault = 0,
           PurchasedOnly = 1,
           LevelUpOnly,
           PurchasedOrLevelUp,
           Hidden,
       }
       [System.Serializable]
       public enum ItemTypeEnum
       {
           Weapon = 0,
           WeaponCamo = 1,
           PlayerSkin = 2,
           PlayerAccesory = 3,
           Emblem = 4,
           CallingCard = 5,
           Emote = 6,
           SeasonPass = 7,
           LootBox = 8,
           Bundle = 9,
           CoinPack = 10,
           None = 99,
       }
       public enum LockReason
       {
           None,
           NoPurchased,
           Level,
           NoPurchasedAndLevel,
           Hidden,
       }
   }
#if UNITY_EDITOR
   [CustomPropertyDrawer(typeof(GameItemAccessRules))]
   public class GameItemAccessRulesDrawer : PropertyDrawer
   {
       public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
       {
           // 在父属性上使用 BeginProperty / EndProperty，可以让
           // 预制体覆盖逻辑作用于整个属性。
           EditorGUI.BeginProperty(position, label, property);
           EditorGUI.DrawRect(position, new Color(0, 0, 0, 0.1f));
           var r = position;
           r.height = EditorGUIUtility.singleLineHeight;
           property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label);
           // 不让子字段缩进
           var indent = EditorGUI.indentLevel;
           EditorGUI.indentLevel++;
           if (property.isExpanded)
           {
               r.y += EditorGUIUtility.singleLineHeight;
               var unlockMethod = property.FindPropertyRelative("UnlockMethod");
               EditorGUI.PropertyField(r, unlockMethod);
               r.y += EditorGUIUtility.singleLineHeight;
               var itemType = property.FindPropertyRelative("ItemType");
               EditorGUI.PropertyField(r, itemType);
               var um = (GameItemAccessRules.AccessRulesMethod)unlockMethod.enumValueIndex;
               GUI.enabled = um != GameItemAccessRules.AccessRulesMethod.UnlockedByDefault;
               if (um != GameItemAccessRules.AccessRulesMethod.LevelUpOnly)
               {
                   r.y += EditorGUIUtility.singleLineHeight;
                   EditorGUI.PropertyField(r, property.FindPropertyRelative("Price"));
               }
               if (um != GameItemAccessRules.AccessRulesMethod.PurchasedOnly)
               {
                   r.y += EditorGUIUtility.singleLineHeight;
                   EditorGUI.PropertyField(r, property.FindPropertyRelative("UnlockAtLevel"));
               }
               if (um != GameItemAccessRules.AccessRulesMethod.LevelUpOnly)
               {
                   r.y += EditorGUIUtility.singleLineHeight;
                   EditorGUI.PropertyField(r, property.FindPropertyRelative("BlockedCoins"), true);
               }
               GUI.enabled = true;
           }
           // 恢复原有缩进
           EditorGUI.indentLevel = indent;
           EditorGUI.EndProperty();
       }
       public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
       {
           if (!property.isExpanded)
               return base.GetPropertyHeight(property, label);
           else
           {
               float less = 0;
               var unlockMethod = property.FindPropertyRelative("UnlockMethod").enumValueIndex;
               if (unlockMethod == 2) less = EditorGUIUtility.singleLineHeight * 2;
               else if (unlockMethod == 1) less = EditorGUIUtility.singleLineHeight;
               return EditorGUI.GetPropertyHeight(property, label, true) - less;
           }
       }
   }
#endif
