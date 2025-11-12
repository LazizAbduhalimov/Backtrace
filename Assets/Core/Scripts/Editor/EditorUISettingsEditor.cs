using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    [CustomEditor(typeof(EditorUISettings))]
    public class EditorUISettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = target as EditorUISettings;
            
            bool prevHideTitleBar = settings.hideTitleBar;
            bool prevHideMenuBar = settings.hideMenuBar;
            
            DrawDefaultInspector();
            
            if (prevHideTitleBar != settings.hideTitleBar)
            {
                settings.SaveSettings();
                if (settings.hideTitleBar)
                {
                    Debug.Log("Title Bar checkbox checked - hiding title bar");
                    MenuBarHider.HideTitleBar();
                }
                else
                {
                    Debug.Log("Title Bar checkbox unchecked - showing title bar");
                    MenuBarHider.ShowTitleBar();
                }
            }
            
            if (prevHideMenuBar != settings.hideMenuBar)
            {
                settings.SaveSettings();
                if (settings.hideMenuBar)
                {
                    Debug.Log("Menu Bar checkbox checked - hiding menu bar");
                    MenuBarHider.HideMenuBar();
                }
                else
                {
                    Debug.Log("Menu Bar checkbox unchecked - showing menu bar");
                    MenuBarHider.ShowMenuBar();
                }
            }
        }
    }
}