using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    public class EditorUISettings : ScriptableObject
    {
        public bool hideTitleBar = false;
        public bool hideMenuBar = false;
        public bool showWindowControls = false;
        public bool hideStatusBar = false;

        public static EditorUISettings Instance => _instance = _instance != null ? _instance : CreateOrLoadSettings();
        private static EditorUISettings _instance;

        private string HideTitleBarEditorPrefsKey => "EditorUISettings_HideTitleBar";
        private string HideMenuBarEditorPrefsKey => "EditorUISettings_HideMenuBar";
        private string ShowWindowControlsEditorPrefsKey => "EditorUISettings_ShowWindowControls";
        private string HideStatusBarEditorPrefsKey => "EditorUISettings_HideStatusBar";

        public void SaveSettings()
        {
            EditorPrefs.SetBool(HideTitleBarEditorPrefsKey, hideTitleBar);
            EditorPrefs.SetBool(HideMenuBarEditorPrefsKey, hideMenuBar);
            EditorPrefs.SetBool(ShowWindowControlsEditorPrefsKey, showWindowControls);
            EditorPrefs.SetBool(HideStatusBarEditorPrefsKey, hideStatusBar);
        }

        public void LoadSettings()
        {
            hideTitleBar = EditorPrefs.GetBool(HideTitleBarEditorPrefsKey, false);
            hideMenuBar = EditorPrefs.GetBool(HideMenuBarEditorPrefsKey, false);
            showWindowControls = EditorPrefs.GetBool(ShowWindowControlsEditorPrefsKey, false);
            hideStatusBar = EditorPrefs.GetBool(HideStatusBarEditorPrefsKey, false);
        }

        private static EditorUISettings CreateOrLoadSettings()
        {
            string[] assets = AssetDatabase.FindAssets("t:EditorUISettings");
            if (assets.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                return AssetDatabase.LoadAssetAtPath<EditorUISettings>(path);
            }
            else
            {
                var settings = CreateInstance<EditorUISettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Core/Scripts/Editor/EditorUISettings.asset");
                AssetDatabase.SaveAssets();
                return settings;
            }
        }
    }
}