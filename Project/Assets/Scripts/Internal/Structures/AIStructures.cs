using System;

namespace GFWK.Runtime.AI
{
    [Serializable]
    public enum BotGameState : byte
    {
        Playing = 0,
        Death,
        Replaced,
        WaitingNextRound,
    }

    [Serializable]
    public class GFWKBotProperties
    {
        public string Name;
        public BotGameState GameState;
        public int Kills;
        public int Deaths;
        public int Score;
        public Team Team;
        public int ViewID;
    }
}