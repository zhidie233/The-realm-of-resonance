#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
   /// 在编辑器的更新循环里跑一个协程迭代器。
   public static class EditorCoroutines
   {
       public static void StartBackgroundTask(IEnumerator task)
       {
           if (task == null) { return; }

           void Step()
           {
               if (!task.MoveNext()) { EditorApplication.update -= Step; }
           }

           EditorApplication.update += Step;
       }
   }
#endif
