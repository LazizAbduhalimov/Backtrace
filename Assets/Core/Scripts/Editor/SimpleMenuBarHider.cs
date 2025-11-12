using System;
using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    [InitializeOnLoad]
    public static class MenuBarHider
    {
        private static IntPtr _unityWindowHandle = IntPtr.Zero;
        private static bool _isMenuBarHidden = false;

        static MenuBarHider()
        {
            EditorApplication.delayCall += InitializeMenuBarHiding;
        }

        private static void InitializeMenuBarHiding()
        {
            _unityWindowHandle = GetUnityMainWindow();
            var settings = EditorUISettings.Instance;
            
            if (settings.autoHideOnStart && settings.hideMenuBar)
            {
                HideMenuBar();
            }
            
            if (settings.autoHideOnStart && settings.hideTitleBar)
            {
                HideTitleBar();
            }
        }

        private static IntPtr _originalMenu = IntPtr.Zero;
        
        public static void HideMenuBar()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();

                Debug.Log($"Unity window handle: {_unityWindowHandle}");

                if (_unityWindowHandle != IntPtr.Zero)
                {
                    // Сохраняем оригинальное меню перед удалением
                    _originalMenu = GetMenu(_unityWindowHandle);
                    Debug.Log($"Original menu handle: {_originalMenu}");
                    
                    if (_originalMenu != IntPtr.Zero)
                    {
                        // Удаляем меню полностью
                        bool setResult = SetMenu(_unityWindowHandle, IntPtr.Zero);
                        Debug.Log($"SetMenu result: {setResult}");
                        
                        // Перерисовываем окно без меню
                        bool drawResult = DrawMenuBar(_unityWindowHandle);
                        Debug.Log($"DrawMenuBar result: {drawResult}");
                        
                        _isMenuBarHidden = true;
                        Debug.Log("Menu bar hidden successfully");
                    }
                    else
                    {
                        Debug.LogWarning("No menu found to hide");
                    }
                }
                else
                {
                    Debug.LogError("Could not get Unity window handle");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to hide menu bar: {e.Message}");
            }
        }

        public static void ShowMenuBar()
        {
            try
            {
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    if (_originalMenu != IntPtr.Zero)
                    {
                        // Восстанавливаем сохраненное меню
                        SetMenu(_unityWindowHandle, _originalMenu);
                    }
                    else
                    {
                        // Если меню было потеряно, создаем пустое меню и перезапускаем Unity
                        Debug.LogWarning("Original menu lost. Unity restart may be required.");
                        
                        // Попытка принудительно пересоздать меню через обновление окна
                        SetWindowPos(_unityWindowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                    }
                    
                    // Перерисовываем окно с меню
                    DrawMenuBar(_unityWindowHandle);
                    InvalidateRect(_unityWindowHandle, IntPtr.Zero, true);
                    UpdateWindow(_unityWindowHandle);
                    
                    _isMenuBarHidden = false;
                    Debug.Log("Menu bar restore attempted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to show menu bar: {e.Message}");
            }
        }

        public static void HideTitleBar()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();

                if (_unityWindowHandle != IntPtr.Zero)
                {
                    // Убираем заголовок окна через Windows API
                    int style = GetWindowLong(_unityWindowHandle, GWL_STYLE);
                    SetWindowLong(_unityWindowHandle, GWL_STYLE, style & ~WS_CAPTION);
                    
                    // Обновляем окно
                    SetWindowPos(_unityWindowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                    
                    Debug.Log("Title bar hidden");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to hide title bar: {e.Message}");
            }
        }

        public static void ShowTitleBar()
        {
            try
            {
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    // Восстанавливаем заголовок окна
                    int style = GetWindowLong(_unityWindowHandle, GWL_STYLE);
                    SetWindowLong(_unityWindowHandle, GWL_STYLE, style | WS_CAPTION);
                    
                    SetWindowPos(_unityWindowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                    
                    Debug.Log("Title bar restored");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to show title bar: {e.Message}");
            }
        }

        public static void ToggleMenuBar()
        {
            if (_isMenuBarHidden)
            {
                ShowMenuBar();
            }
            else
            {
                HideMenuBar();
            }
        }

        public static void ToggleTitleBar()
        {
            // Простой способ определить состояние title bar
            if (_unityWindowHandle == IntPtr.Zero)
                _unityWindowHandle = GetUnityMainWindow();

            if (_unityWindowHandle != IntPtr.Zero)
            {
                int style = GetWindowLong(_unityWindowHandle, GWL_STYLE);
                bool hasTitleBar = (style & WS_CAPTION) != 0;
                
                if (hasTitleBar)
                {
                    HideTitleBar();
                }
                else
                {
                    ShowTitleBar();
                }
            }
        }

        public static void ToggleBoth()
        {
            ToggleMenuBar();
            ToggleTitleBar();
        }

        public static bool IsMenuBarHidden => _isMenuBarHidden;

        private static IntPtr GetUnityMainWindow()
        {
            try
            {
                // Метод 1: Через GetActiveWindow
                IntPtr activeWindow = GetActiveWindow();
                if (activeWindow != IntPtr.Zero)
                {
                    return activeWindow;
                }
                
                // Метод 2: Поиск окна Unity по заголовку
                IntPtr unityWindow = FindWindowByTitle();
                if (unityWindow != IntPtr.Zero)
                {
                    return unityWindow;
                }
                
                return GetForegroundWindow();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get Unity window handle: {e.Message}");
                return IntPtr.Zero;
            }
        }

        private static IntPtr FindWindowByTitle()
        {
            try
            {
                // Пытаемся найти окно Unity по части заголовка
                string[] possibleTitles = {
                    Application.productName,
                    PlayerSettings.productName,
                    "Unity",
                    "Backtrace"
                };

                foreach (string title in possibleTitles)
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        IntPtr hwnd = FindWindow(null, title);
                        if (hwnd != IntPtr.Zero)
                        {
                            return hwnd;
                        }
                    }
                }

                return IntPtr.Zero;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to find Unity window by title: {e.Message}");
                return IntPtr.Zero;
            }
        }

        // Windows API константы
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_SYSMENU = 0x00080000;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const int SM_CYMENU = 15;

        // Windows API структуры
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Windows API imports
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UpdateWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetMenu(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetMenu(IntPtr hWnd, IntPtr hMenu);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool DrawMenuBar(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}