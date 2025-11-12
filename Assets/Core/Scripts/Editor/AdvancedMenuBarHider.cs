using System;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace EditorUtils
{
    [InitializeOnLoad]
    public static class AdvancedMenuBarHider
    {
        private static IntPtr _unityWindowHandle = IntPtr.Zero;
        private static bool _isMenuBarHidden = false;

        static AdvancedMenuBarHider()
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
        }

        public static void HideMenuBar()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();

                Debug.Log($"Attempting to hide menu bar for window: {_unityWindowHandle}");

                if (_unityWindowHandle != IntPtr.Zero)
                {
                    // Получаем информацию об окне
                    StringBuilder className = new StringBuilder(256);
                    GetClassName(_unityWindowHandle, className, className.Capacity);
                    Debug.Log($"Window class: {className}");

                    StringBuilder windowTitle = new StringBuilder(256);
                    GetWindowText(_unityWindowHandle, windowTitle, windowTitle.Capacity);
                    Debug.Log($"Window title: {windowTitle}");

                    // Метод 1: Через SetWindowLong изменяем стили
                    int currentStyle = GetWindowLong(_unityWindowHandle, GWL_STYLE);
                    Debug.Log($"Current window style: 0x{currentStyle:X}");

                    // Убираем WS_SYSMENU и пробуем другие флаги
                    int newStyle = currentStyle & ~(WS_SYSMENU | WS_MAXIMIZEBOX | WS_MINIMIZEBOX);
                    SetWindowLong(_unityWindowHandle, GWL_STYLE, newStyle);

                    // Обновляем окно
                    SetWindowPos(_unityWindowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

                    // Метод 2: Пытаемся найти и скрыть меню через дочерние окна
                    HideMenuThroughChildWindows(_unityWindowHandle);

                    _isMenuBarHidden = true;
                    Debug.Log("Menu bar hide attempt completed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to hide menu bar: {e.Message}");
            }
        }

        private static void HideMenuThroughChildWindows(IntPtr parentHandle)
        {
            try
            {
                // Перечисляем дочерние окна
                EnumChildWindows(parentHandle, (hwnd, lParam) =>
                {
                    StringBuilder className = new StringBuilder(256);
                    GetClassName(hwnd, className, className.Capacity);
                    
                    Debug.Log($"Found child window: {hwnd}, class: {className}");
                    
                    // Ищем окна похожие на меню
                    string classStr = className.ToString().ToLower();
                    if (classStr.Contains("menu") || classStr.Contains("toolbar"))
                    {
                        Debug.Log($"Attempting to hide menu window: {hwnd}");
                        ShowWindow(hwnd, SW_HIDE);
                    }
                    
                    return true; // Продолжаем перечисление
                }, IntPtr.Zero);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to enumerate child windows: {e.Message}");
            }
        }

        public static void ShowMenuBar()
        {
            try
            {
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    // Восстанавливаем стили окна
                    int currentStyle = GetWindowLong(_unityWindowHandle, GWL_STYLE);
                    int newStyle = currentStyle | WS_SYSMENU | WS_MAXIMIZEBOX | WS_MINIMIZEBOX;
                    SetWindowLong(_unityWindowHandle, GWL_STYLE, newStyle);

                    SetWindowPos(_unityWindowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

                    // Показываем скрытые дочерние окна
                    ShowMenuThroughChildWindows(_unityWindowHandle);

                    _isMenuBarHidden = false;
                    Debug.Log("Menu bar show attempt completed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to show menu bar: {e.Message}");
            }
        }

        private static void ShowMenuThroughChildWindows(IntPtr parentHandle)
        {
            try
            {
                EnumChildWindows(parentHandle, (hwnd, lParam) =>
                {
                    StringBuilder className = new StringBuilder(256);
                    GetClassName(hwnd, className, className.Capacity);
                    
                    string classStr = className.ToString().ToLower();
                    if (classStr.Contains("menu") || classStr.Contains("toolbar"))
                    {
                        Debug.Log($"Attempting to show menu window: {hwnd}");
                        ShowWindow(hwnd, SW_SHOW);
                    }
                    
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to show child windows: {e.Message}");
            }
        }

        public static void ToggleMenuBar()
        {
            if (_isMenuBarHidden)
                ShowMenuBar();
            else
                HideMenuBar();
        }

        private static IntPtr GetUnityMainWindow()
        {
            try
            {
                IntPtr activeWindow = GetActiveWindow();
                Debug.Log($"Active window: {activeWindow}");
                return activeWindow;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get Unity window handle: {e.Message}");
                return IntPtr.Zero;
            }
        }

        public static bool IsMenuBarHidden => _isMenuBarHidden;

        // Windows API constants
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        // Delegate for EnumChildWindows
        private delegate bool EnumChildProc(IntPtr hwnd, IntPtr lParam);

        // Windows API imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }
}