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
            bool prevShowWindowControls = settings.showWindowControls;
            bool prevHideStatusBar = settings.hideStatusBar;

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
            
            if (prevShowWindowControls != settings.showWindowControls)
            {
                settings.SaveSettings();
                if (settings.showWindowControls)
                {
                    Debug.Log("Window Controls checkbox checked - showing controls");
                    WindowControlsOverlay.ShowControls();
                }
                else
                {
                    Debug.Log("Window Controls checkbox unchecked - hiding controls");
                    WindowControlsOverlay.HideControls();
                }
            }
            
            if (prevHideStatusBar != settings.hideStatusBar)
            {
                settings.SaveSettings();
                if (settings.hideStatusBar)
                {
                    Debug.Log("Status Bar checkbox checked - hiding status bar");
                    StatusBarHider.HideStatusBar();
                }
                else
                {
                    Debug.Log("Status Bar checkbox unchecked - showing status bar");
                    StatusBarHider.ShowStatusBar();
                }
                if (settings.hideTitleBar)
                {
                    MenuBarHider.ShowTitleBar();
                    MenuBarHider.HideTitleBar();
                }
                else
                {
                    MenuBarHider.HideTitleBar();
                    MenuBarHider.ShowTitleBar();
                }   
            }
        }
    }
}