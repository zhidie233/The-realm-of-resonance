using UnityEngine;

   public class KillReport
   {
       public string Killer;
       public string Killed;
       public string Message;
       public bool HeadShot;
       public int GunID;
       public Team KillerTeam;
       public Color KillerColor;
       public Color KilledColor;
       public float TimeToShow;
       public KillReportMessageType messageType;
   }

   public enum KillReportMessageType
   {
       WeaponKillEvent = 0,
       TeamHighlightMessage = 1,
       Message = 2,
   }

   public enum KillReportWeaponShowMode
   {
       WeaponName = 0,
       WeaponIcon = 1
   }

   public struct GameLocalNotification
   {
       public string Message { get; private set; }

       public GameLocalNotification(string message)
       {
           Message = message;
           GameEvents.onLocalNotification?.Invoke(this);
       }
   }