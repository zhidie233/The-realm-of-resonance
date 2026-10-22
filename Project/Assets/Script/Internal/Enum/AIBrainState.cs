   /// 机器人默认状态
   public enum AIBrainState
   {
       Idle = 0,
       Patroling = 1,
       Following = 2,
       Covering = 3,
       Looking = 4,
       Searching = 5,
       HoldingPosition = 6,
   }
   /// 机器人行为模式
   public enum AIBrainAction
   {
       Agressive = 0,
       Pasive = 1,
       Protective = 2,
   }
   /// 仅供内部使用
   public enum AICoverZone
   {
       ToPoint = 0,
       ToTarget = 1,
       ToRandomPoint = 2,
   }
   /// 仅供内部使用
   public enum AIOutOfRangeAction
   {
       SearchNewNearestTarget,
       KeepFollowingBasedOnState,
   }

   public enum AIBotWeaponSpread
   {
       Pro,
       Casual,
       Novice,
   }

   public enum AIGaze
   {
       Target,
       Path,
       HitDirection,
       PathToTarget,
   }

   public enum AIRemoteCallKind : byte
   {
       DestroyBot = 0,
       SyncTarget = 1,
       CrouchState = 2,
       Respawn = 3,
   }

   public enum BotKillPolicy
   {
       /// 机器人击杀计入对局记分板，击杀/得分归于击杀者。
       SameAsRealPlayers,
       /// 机器人击杀计入该玩家的对局记分板，但不增加账号统计（得分与击杀数）
       OnlyAffectMatchScoreboard,
       /// 机器人击杀连对局记分板都不计入
       DoNotCountAtAll,
   }
