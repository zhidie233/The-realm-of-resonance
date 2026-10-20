using System;

   [Serializable]
   public enum BotMatchState : byte
   {
       Playing = 0,
       Death,
       Replaced,
       WaitingNextRound,
   }

   [Serializable]
   public class GameBotProperties
   {
       public string Name;
       public BotMatchState GameState;
       public int Kills;
       public int Deaths;
       public int Score;
       public Team Team;
       public int ViewID;
   }