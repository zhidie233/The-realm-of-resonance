
   /// <summary>
   /// 处理对局结束后出现的 UI 窗口
   /// 默认收集本地玩家数据并显示到屏幕上
   /// </summary>
   public interface IGameResumeScreen
   {
       void CollectData();
       void Show();
   }