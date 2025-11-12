using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUtils
{
    [InitializeOnLoad]
    public static class WindowControlsOverlay
    {
        private const string ContainerName = "WindowControlsContainer";
        private static bool _attempted;
        private static bool _isVisible = false;
        private static IntPtr _unityWindowHandle = IntPtr.Zero;
        
        static WindowControlsOverlay()
        {
            EditorApplication.delayCall += Initialize;
        }
        
        private static void Initialize()
        {
            _unityWindowHandle = GetUnityMainWindow();
            var settings = EditorUISettings.Instance;
            settings.LoadSettings();
            
            if (settings.showWindowControls)
            {
                ShowControls();
            }
        }
        
        public static void ShowControls()
        {
            _isVisible = true;
            EditorApplication.update += TryInstall;
        }
        
        public static void HideControls()
        {
            _isVisible = false;
            _attempted = false;
            RemoveFromToolbar();
            EditorApplication.update -= TryInstall;
        }
        
        private static void TryInstall()
        {
            if (_attempted || !_isVisible) return;

            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return;

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars == null || toolbars.Length == 0) return;

            var toolbar = toolbars[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var root = rootField?.GetValue(toolbar) as VisualElement;
            if (root == null) return;

            // Ищем toolbar и его элементы
            var rightZone = root.Q("ToolbarZoneRightAlign") ?? root.Q("ToolbarZoneRightAlign", "ToolbarZone");
            if (rightZone == null)
            {
                // Если правой зоны нет, ищем конкретные элементы для позиционирования
                var layoutButton = root.Q("Layout");
                var cloudButton = root.Q("CloudBuild");
                var accountButton = root.Q("Account");
                
                if (layoutButton != null)
                {
                    rightZone = layoutButton.parent;
                }
                else if (cloudButton != null)
                {
                    rightZone = cloudButton.parent;
                }
                else if (accountButton != null)
                {
                    rightZone = accountButton.parent;
                }
                else
                {
                    rightZone = root.Q("ToolbarZoneLeftAlign") ?? root.Q("ToolbarZoneLeftAlign", "ToolbarZone");
                }
            }
            
            if (rightZone == null) return;

            // Избегаем дубликатов
            if (rightZone.Q(ContainerName) != null)
            {
                _attempted = true;
                EditorApplication.update -= TryInstall;
                return;
            }

            // Создаем контейнер для наших кнопок
            var container = new VisualElement()
            {
                name = ContainerName,
                style = {
                    flexDirection = FlexDirection.Row,
                    marginLeft = 3,
                    marginRight = 2
                }
            };

            // Кнопка минимизации
            var minimizeButton = CreateWindowButton("—", "Minimize window", MinimizeWindow, new Color(0.3f, 0.5f, 0.7f));
            container.Add(minimizeButton);

            // Кнопка максимизации/восстановления  
            var maximizeButton = CreateWindowButton(IsWindowMaximized() ? "R" : "M", "Maximize/Restore window", ToggleMaximizeWindow, new Color(0.3f, 0.7f, 0.3f));
            container.Add(maximizeButton);

            // Кнопка закрытия
            var closeButton = CreateWindowButton("X", "Close window", CloseWindow, new Color(0.7f, 0.3f, 0.3f));
            container.Add(closeButton);

            // Добавляем в самый конец rightZone (после всех существующих элементов)
            rightZone.Add(container);

            _attempted = true;
            EditorApplication.update -= TryInstall;
        }
        
        private static Button CreateWindowButton(string text, string tooltip, System.Action action, Color? accentColor = null)
        {
            var button = new Button(action)
            {
                text = text,
                tooltip = tooltip,
                style = {
                    minWidth = 24,
                    minHeight = 18,
                    maxWidth = 24,
                    maxHeight = 18,
                    marginLeft = 1,
                    marginRight = 1,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    fontSize = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    // Используем стиль похожий на toolbar кнопки
                    backgroundColor = Color.clear,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    // Цвет текста зависит от темы
                    color = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };
            
            // Добавляем класс toolbar кнопки для нативного стиля
            button.AddToClassList("unity-toolbar-button");
            
            // Эффекты при наведении с акцентным цветом
            var hoverColor = accentColor ?? (EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.8f, 0.8f, 0.8f));
            
            button.RegisterCallback<MouseEnterEvent>((evt) => {
                button.style.backgroundColor = hoverColor;
            });
            
            button.RegisterCallback<MouseLeaveEvent>((evt) => {
                button.style.backgroundColor = Color.clear;
            });
            
            button.RegisterCallback<MouseDownEvent>((evt) => {
                button.style.backgroundColor = new Color(hoverColor.r * 0.8f, hoverColor.g * 0.8f, hoverColor.b * 0.8f);
            });
            
            button.RegisterCallback<MouseUpEvent>((evt) => {
                button.style.backgroundColor = hoverColor;
            });
            
            return button;
        }
        
        private static void RemoveFromToolbar()
        {
            try
            {
                var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
                if (toolbarType == null) return;

                var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                if (toolbars == null || toolbars.Length == 0) return;

                var toolbar = toolbars[0];
                var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                var root = rootField?.GetValue(toolbar) as VisualElement;
                if (root == null) return;

                var container = root.Q(ContainerName);
                container?.RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to remove window controls from toolbar: {e.Message}");
            }
        }
        
        private static void MinimizeWindow()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();
                    
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(_unityWindowHandle, SW_MINIMIZE);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to minimize window: {e.Message}");
            }
        }
        
        private static void ToggleMaximizeWindow()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();
                    
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    if (IsWindowMaximized())
                    {
                        ShowWindow(_unityWindowHandle, SW_RESTORE);
                    }
                    else
                    {
                        ShowWindow(_unityWindowHandle, SW_MAXIMIZE);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to toggle maximize window: {e.Message}");
            }
        }
        
        private static void CloseWindow()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();
                    
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    SendMessage(_unityWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to close window: {e.Message}");
            }
        }
        
        private static bool IsWindowMaximized()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();
                    
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    var placement = new WINDOWPLACEMENT();
                    placement.length = Marshal.SizeOf(placement);
                    
                    if (GetWindowPlacement(_unityWindowHandle, ref placement))
                    {
                        return placement.showCmd == SW_MAXIMIZE;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to check window state: {e.Message}");
            }
            
            return false;
        }
        
        private static IntPtr GetUnityMainWindow()
        {
            try
            {
                IntPtr activeWindow = GetActiveWindow();
                if (activeWindow != IntPtr.Zero)
                {
                    return activeWindow;
                }
                
                return GetForegroundWindow();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get Unity window handle: {e.Message}");
                return IntPtr.Zero;
            }
        }
        
        // Windows API константы
        private const int SW_MINIMIZE = 6;
        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;
        private const int WM_CLOSE = 0x0010;
        
        // Структура для GetWindowPlacement
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        
        // Windows API imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
    }
}