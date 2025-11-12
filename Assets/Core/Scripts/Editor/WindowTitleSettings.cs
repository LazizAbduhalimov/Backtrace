using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    public class EditorUISettings : ScriptableObject
    {
        [Header("UI Hiding Configuration")]
        [Tooltip("Hide Unity window title bar")]
        public bool hideTitleBar = false;
        
        [Tooltip("Hide main menu bar (File, Edit, Assets, etc.)")]
        public bool hideMenuBar = false;
        
        [Header("Auto Settings")]
        [Tooltip("Automatically hide UI on Unity start")]
        public bool autoHideOnStart = false;
        
        [Header("Advanced Settings")]
        [Tooltip("Enable debug logging")]
        public bool enableDebugLogging = false;

        private static EditorUISettings _instance;
        public static EditorUISettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateOrLoadSettings();
                }
                return _instance;
            }
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

    [CustomEditor(typeof(EditorUISettings))]
    public class EditorUISettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(15);
            
            EditorGUILayout.LabelField("Quick Controls", EditorStyles.boldLabel);
            
            // Тест скрытия Menu Bar
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Menu Bar (File, Edit, Assets, Tools...)", EditorStyles.label);
            
            string menuStatus = MenuBarHider.IsMenuBarHidden ? "Hidden" : "Visible";
            EditorGUILayout.LabelField($"Status: {menuStatus}", EditorStyles.miniLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hide Menu Bar"))
            {
                Debug.Log("=== Hide Menu Bar Button Clicked ===");
                MenuBarHider.HideMenuBar();
            }
            
            if (GUILayout.Button("Show Menu Bar"))
            {
                Debug.Log("=== Show Menu Bar Button Clicked ===");
                MenuBarHider.ShowMenuBar();
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Один тоггл для Title Bar
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Title Bar (Window Header)", EditorStyles.label);
            
            if (GUILayout.Button("Toggle Title Bar", GUILayout.Height(30)))
            {
                MenuBarHider.ToggleTitleBar();
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Один тоггл для всего сразу
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Both Menu + Title Bar", EditorStyles.label);
            
            if (GUILayout.Button("Toggle Both", GUILayout.Height(30)))
            {
                MenuBarHider.ToggleBoth();
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(15);
            EditorGUILayout.HelpBox("Use menu: Tools → Editor UI → [Options]", MessageType.Info);
        }
    }
}