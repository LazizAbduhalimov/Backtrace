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
            bool prevShowMenuBar = settings.showMenuBar;
            bool prevHideStatusBar = settings.hideStatusBar;
            bool prevEnableWindowDrag = settings.enableWindowDrag;

            DrawDefaultInspector();
            
            if (prevHideTitleBar != settings.hideTitleBar)
            {
                settings.SaveSettings();
                if (settings.hideTitleBar)
                {
                    MenuBarHider.HideTitleBar();
                }
                else
                {
                    MenuBarHider.ShowTitleBar();
                }
            }
            
            if (prevHideMenuBar != settings.hideMenuBar)
            {
                settings.SaveSettings();
                if (settings.hideMenuBar)
                {
                    MenuBarHider.HideMenuBar();
                }
                else
                {
                    MenuBarHider.ShowMenuBar();
                }
            }
            
            if (prevShowWindowControls != settings.showWindowControls)
            {
                settings.SaveSettings();
                if (settings.showWindowControls)
                {
                    EditorUtils.WindowControls.WindowControlsCoordinator.ShowWindowControls();
                }
                else
                {
                    EditorUtils.WindowControls.WindowControlsCoordinator.HideWindowControls();
                }
            }
            
            if (prevShowMenuBar != settings.showMenuBar)
            {
                settings.SaveSettings();
                if (settings.showMenuBar)
                {
                    EditorUtils.WindowControls.WindowControlsCoordinator.ShowMenuBarButton();
                }
                else
                {
                    EditorUtils.WindowControls.WindowControlsCoordinator.HideMenuBarButton();
                }
            }
            
            if (prevHideStatusBar != settings.hideStatusBar)
            {
                settings.SaveSettings();
                if (settings.hideStatusBar)
                {
                    StatusBarHider.HideStatusBar();
                }
                else
                {
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
            
            if (prevEnableWindowDrag != settings.enableWindowDrag)
            {
                settings.SaveSettings();
                if (settings.enableWindowDrag)
                {
                    EditorUtils.WindowControls.WindowControlsCoordinator.ShowDragArea();
                }
                else
                {
                    EditorUtils.WindowControls.WindowControlsCoordinator.HideDragArea();
                }
            }
        }
    }
}