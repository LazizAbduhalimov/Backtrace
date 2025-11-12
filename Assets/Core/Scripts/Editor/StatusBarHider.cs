using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    public static class StatusBarHider
    {
        private static bool _isStatusBarHidden = false;
        private static object _appStatusBarInstance;
        private static Type _appStatusBarType;
        private static Rect _originalPosition;
        private static bool _initialized = false;
        
        private static void EnsureInitialized()
        {
            if (!_initialized)
            {
                FindAppStatusBar();
                _initialized = true;
            }
        }
        
        private static void FindAppStatusBar()
        {
            try
            {
                _appStatusBarType = typeof(Editor).Assembly.GetType("UnityEditor.AppStatusBar");
                if (_appStatusBarType != null)
                {
                    var instances = Resources.FindObjectsOfTypeAll(_appStatusBarType);
                    if (instances != null && instances.Length > 0)
                    {
                        _appStatusBarInstance = instances[0];
                        
                        // Сохраняем оригинальную позицию
                        var positionProperty = _appStatusBarType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
                        if (positionProperty != null)
                        {
                            _originalPosition = (Rect)positionProperty.GetValue(_appStatusBarInstance);
                            Debug.Log($"AppStatusBar found and saved. Original position: {_originalPosition}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to find AppStatusBar: {e.Message}");
            }
        }
        
        public static void HideStatusBar()
        {
            try
            {
                Debug.Log("=== Hiding Status Bar ===");
                
                EnsureInitialized();
                
                if (_appStatusBarInstance == null || _appStatusBarType == null)
                {
                    FindAppStatusBar();
                }
                
                if (_appStatusBarInstance != null && _appStatusBarType != null)
                {
                    var positionProperty = _appStatusBarType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
                    if (positionProperty != null)
                    {
                        var currentPosition = (Rect)positionProperty.GetValue(_appStatusBarInstance);
                        
                        // Сохраняем оригинальную позицию если еще не сохранена
                        if (_originalPosition.height == 0)
                        {
                            _originalPosition = currentPosition;
                        }
                        
                        // Устанавливаем высоту в 0
                        var newPosition = new Rect(currentPosition.x, currentPosition.y, currentPosition.width, 0);
                        positionProperty.SetValue(_appStatusBarInstance, newPosition);
                        
                        _isStatusBarHidden = true;
                        Debug.Log($"Status bar hidden. Position changed from {currentPosition} to {newPosition}");
                        
                        // Немедленная перерисовка
                        var repaintMethod = _appStatusBarType.GetMethod("Repaint", BindingFlags.Public | BindingFlags.Instance);
                        if (repaintMethod != null)
                        {
                            repaintMethod.Invoke(_appStatusBarInstance, null);
                        }
                        
                        return;
                    }
                }
                
                Debug.LogWarning("Failed to hide status bar - AppStatusBar not found");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to hide status bar: {e.Message}");
            }
        }
        
        public static void ShowStatusBar()
        {
            try
            {
                Debug.Log("=== Showing Status Bar ===");
                
                EnsureInitialized();
                
                if (_appStatusBarInstance == null || _appStatusBarType == null)
                {
                    FindAppStatusBar();
                }
                
                if (_appStatusBarInstance != null && _appStatusBarType != null)
                {
                    var positionProperty = _appStatusBarType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
                    if (positionProperty != null)
                    {
                        var currentPosition = (Rect)positionProperty.GetValue(_appStatusBarInstance);
                        
                        // Восстанавливаем оригинальную высоту
                        var targetHeight = _originalPosition.height > 0 ? _originalPosition.height : 20f;
                        var newPosition = new Rect(currentPosition.x, currentPosition.y, currentPosition.width, targetHeight);
                        positionProperty.SetValue(_appStatusBarInstance, newPosition);
                        
                        _isStatusBarHidden = false;
                        Debug.Log($"Status bar shown. Position changed from {currentPosition} to {newPosition}");
                        
                        // Принудительное включение через SetEnabled
                        var setEnabledMethod = _appStatusBarType.GetMethod("SetEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (setEnabledMethod != null)
                        {
                            setEnabledMethod.Invoke(_appStatusBarInstance, new object[] { true });
                            Debug.Log("SetEnabled(true) called");
                        }
                        
                        // Немедленная перерисовка
                        var repaintMethod = _appStatusBarType.GetMethod("Repaint", BindingFlags.Public | BindingFlags.Instance);
                        if (repaintMethod != null)
                        {
                            repaintMethod.Invoke(_appStatusBarInstance, null);
                            Debug.Log("Status bar repainted immediately");
                        }
                        
                        // Дополнительное обновление через delayCall
                        EditorApplication.delayCall += () =>
                        {
                            try
                            {
                                // Принудительное обновление MainView
                                var mainViewType = typeof(Editor).Assembly.GetType("UnityEditor.MainView");
                                if (mainViewType != null)
                                {
                                    var mainViews = Resources.FindObjectsOfTypeAll(mainViewType);
                                    foreach (var mainView in mainViews)
                                    {
                                        var useBottomViewProperty = mainViewType.GetProperty("useBottomView", BindingFlags.Public | BindingFlags.Instance);
                                        if (useBottomViewProperty != null)
                                        {
                                            useBottomViewProperty.SetValue(mainView, true);
                                            Debug.Log("MainView.useBottomView forced to true");
                                        }
                                        
                                        var repaintMainView = mainViewType.GetMethod("Repaint", BindingFlags.Public | BindingFlags.Instance);
                                        if (repaintMainView != null)
                                        {
                                            repaintMainView.Invoke(mainView, null);
                                            Debug.Log("MainView repainted");
                                        }
                                    }
                                }
                                
                                // Глобальная перерисовка
                                EditorApplication.RepaintHierarchyWindow();
                                EditorApplication.RepaintProjectWindow();
                                
                                Debug.Log("Delayed UI refresh completed");
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error during delayed refresh: {e.Message}");
                            }
                        };
                        
                        return;
                    }
                }
                
                Debug.LogWarning("Failed to show status bar - AppStatusBar not found");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to show status bar: {e.Message}");
            }
        }
    }
}