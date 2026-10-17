namespace Game.Runtime.AI
{
    /// <summary>
    /// 机器人默认状态
    /// </summary>
    public enum AIAgentState
    {
        Idle = 0,
        Patroling = 1,
        Following = 2,
        Covering = 3,
        Looking = 4,
        Searching = 5,
        HoldingPosition = 6,
    }

    /// <summary>
    /// 机器人行为模式
    /// </summary>
    public enum AIAgentBehave
    {
        Agressive = 0,
        Pasive = 1,
        Protective = 2,
    }

    /// <summary>
    /// 仅供内部使用
    /// </summary>
    public enum AIAgentCoverArea
    {
        ToPoint = 0,
        ToTarget = 1,
        ToRandomPoint = 2,
    }

    /// <summary>
    /// 仅供内部使用
    /// </summary>
    public enum AITargetOutRangeBehave
    {
        SearchNewNearestTarget,
        KeepFollowingBasedOnState,
    }

    public enum AIWeaponAccuracy
    {
        Pro,
        Casual,
        Novice,
    }

    public enum AILookAt
    {
        Target,
        Path,
        HitDirection,
        PathToTarget,
    }

    public enum AIRemoteCallType : byte
    {
        DestroyBot = 0,
        SyncTarget = 1,
        CrouchState = 2,
        Respawn = 3,
    }

    public enum BotKillConsideration
    {
        /// <summary>
        /// 机器人击杀计入对局记分板，击杀/得分归于击杀者。
        /// </summary>
        SameAsRealPlayers,

        /// <summary>
        /// 机器人击杀计入该玩家的对局记分板，但不增加账号统计（得分与击杀数）
        /// </summary>
        OnlyAffectMatchScoreboard,

        /// <summary>
        /// 机器人击杀连对局记分板都不计入
        /// </summary>
        DoNotCountAtAll,
    }
}