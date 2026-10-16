#if UNITY_EDITOR
using System.Collections;
using UnityEditor;

namespace GameEditor
{
    /// <summary>
    /// Runs an enumerator on the editor update loop.
    /// </summary>
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
}
#endif
