using UnityEngine;

   public enum LobbyState
   {
       PlayerName,
       MainMenu,
       Host,
       Join,
       Settings,
       ChangeServer,
       Quit
   }

   public enum LobbyAccess
   {
       DirectToMap = 1,
       ReadyRoom = 2,
   }

   public enum LobbyConnectionState
   {
       Disconnected,
       Connected,
       Connecting,
       Authenticating,
       WaitingForUserName,
       ChangingRegion,
   }