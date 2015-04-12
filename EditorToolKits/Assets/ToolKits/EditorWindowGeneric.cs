using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ToolKits
{
    public class EditorWindowGeneric : EditorWindow
    {
        public static void ShowWindow<T>(string Title)
        {
            EditorWindow Window = EditorWindow.GetWindow(typeof(T));
            if (!string.IsNullOrEmpty(Title))
            {
                Window.title = Title;
            }
        }

        public static void ShowWindow<T>(string Title, Rect WindowRect)
        {
            EditorWindow Window = EditorWindow.GetWindowWithRect(typeof(T), WindowRect);
            if (!string.IsNullOrEmpty(Title))
            {
                Window.title = Title;
            }
        }

        protected void Log(string Value)
        {
#if DEBUG
            Debug.Log(Value);
#endif
        }

    }
}
