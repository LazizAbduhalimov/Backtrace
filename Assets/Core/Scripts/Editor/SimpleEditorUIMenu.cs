using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    public static class EditorUIMenu
    {
        [MenuItem("Tools/Editor UI/Settings")]
        public static void OpenSettings()
        {
            var settings = EditorUISettings.Instance;
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem("Tools/Editor UI/Test Menu Bar Hide")]
        public static void TestMenuBarHide()
        {
            Debug.Log("=== Testing Menu Bar Hide ===");
            MenuBarHider.HideMenuBar();
        }

        [MenuItem("Tools/Editor UI/Test Menu Bar Show")]
        public static void TestMenuBarShow()
        {
            Debug.Log("=== Testing Menu Bar Show ===");
            MenuBarHider.ShowMenuBar();
        }

        [MenuItem("Tools/Editor UI/Toggle Title Bar")]
        public static void ToggleTitleBar()
        {
            MenuBarHider.ToggleTitleBar();
        }

        [MenuItem("Tools/Editor UI/Reset to Default")]
        public static void ResetToDefault()
        {
            MenuBarHider.ShowMenuBar();
            MenuBarHider.ShowTitleBar();
            var settings = EditorUISettings.Instance;
            settings.hideTitleBar = false;
            settings.hideMenuBar = false;
            settings.autoHideOnStart = false;
            EditorUtility.SetDirty(settings);
            Debug.Log("Editor UI reset to default");
        }
    }
}