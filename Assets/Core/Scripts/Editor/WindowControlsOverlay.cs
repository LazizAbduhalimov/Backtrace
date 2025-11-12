using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string DragAreaName = "DragArea";
        private static bool _attempted;
        private static bool _isVisible = false;
        private static IntPtr _unityWindowHandle = IntPtr.Zero;
        
        // Windows API для перетаскивания окна
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessageForDrag(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        private const uint WM_NCLBUTTONDOWN = 0xA1;
        private const uint HTCAPTION = 0x2;
        
        // Кеш для меню элементов
        private static List<string> _cachedMenuItems = null;
        private static Dictionary<string, List<string>> _cachedGroupedMenus = null;
        private static bool _menuCacheInitialized = false;
        private static int _lastAssemblyCount = 0;
        
        static WindowControlsOverlay()
        {
            EditorApplication.delayCall += Initialize;
            // Инициализируем кеш меню в фоне
            EditorApplication.delayCall += InitializeMenuCache;
        }
        
        private static void Initialize()
        {
            _unityWindowHandle = GetUnityMainWindow();
            var settings = EditorUISettings.Instance;
            settings.LoadSettings();
            
            // Показываем элементы toolbar если включен хотя бы один из компонентов
            if (settings.showWindowControls || settings.showMenuBar)
            {
                ShowControls();
            }
        }
        
        private static void InitializeMenuCache()
        {
            if (!_menuCacheInitialized)
            {
                try
                {
                    Debug.Log("Initializing menu cache...");
                    var startTime = System.DateTime.Now;
                    
                    _cachedMenuItems = GetAllMenuItems();
                    _cachedGroupedMenus = GroupMenuItems(_cachedMenuItems);
                    _menuCacheInitialized = true;
                    _lastAssemblyCount = System.AppDomain.CurrentDomain.GetAssemblies().Length;
                    
                    var elapsed = System.DateTime.Now - startTime;
                    Debug.Log($"Menu cache initialized in {elapsed.TotalMilliseconds:F0}ms. Found {_cachedMenuItems.Count} menu items in {_cachedGroupedMenus.Count} categories.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to initialize menu cache: {e.Message}");
                }
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
            
            var settings = EditorUISettings.Instance;
            
            if (!settings.showWindowControls)
            {
                RemoveWindowControlsFromToolbar();
            }
            
            if (!settings.showMenuBar)
            {
                RemoveMenuBarButtonFromToolbar();
            }
            
            if (!settings.enableWindowDrag)
            {
                RemoveDragAreaFromToolbar();
            }
            
            // Отключаем обновление только если все компоненты отключены
            if (!settings.showWindowControls && !settings.showMenuBar && !settings.enableWindowDrag)
            {
                EditorApplication.update -= TryInstall;
            }
            else
            {
                // Если хотя бы один компонент должен быть виден, перезапускаем установку
                _attempted = false;
                EditorApplication.update -= TryInstall;
                EditorApplication.update += TryInstall;
            }
        }
        
        public static void ShowMenuBarButton()
        {
            var settings = EditorUISettings.Instance;
            if (!settings.showMenuBar) return;
            
            // Перезапускаем установку элементов toolbar
            _attempted = false;
            EditorApplication.update += TryInstall;
        }
        
        public static void HideMenuBarButton()
        {
            RemoveMenuBarButtonFromToolbar();
        }
        
        public static void ShowWindowControls()
        {
            var settings = EditorUISettings.Instance;
            if (!settings.showWindowControls) return;
            
            // Перезапускаем установку элементов toolbar
            _attempted = false;
            EditorApplication.update += TryInstall;
        }
        
        public static void HideWindowControls()
        {
            RemoveWindowControlsFromToolbar();
        }
        
        public static void ShowDragArea()
        {
            // Перезапускаем установку элементов toolbar
            _attempted = false;
            EditorApplication.update += TryInstall;
        }
        
        public static void HideDragArea()
        {
            RemoveDragAreaFromToolbar();
        }
        
        private static void TryInstall()
        {
            if (_attempted) return;

            try
            {
                var settings = EditorUISettings.Instance;
                if (settings == null || (!settings.showWindowControls && !settings.showMenuBar && !settings.enableWindowDrag)) return;

                var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
                if (toolbarType == null) return;

                var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                if (toolbars == null || toolbars.Length == 0) return;

                var toolbar = toolbars[0];
                var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                var root = rootField?.GetValue(toolbar) as VisualElement;
                if (root == null) return;

            // Ищем левую зону для кнопки MenuBar
            var leftZone = root.Q("ToolbarZoneLeftAlign") ?? root.Q("ToolbarZoneLeftAlign", "ToolbarZone");
            
            // Ищем правую зону для кнопок управления окном
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
            
            // Создаем кнопку MenuBar в левой зоне (если включена в настройках)
            if (leftZone != null && leftZone.Q("MenuBarButton") == null && settings.showMenuBar)
            {
                var menuBarButton = CreateMenuBarButton();
                leftZone.Add(menuBarButton);
            }

            // Создаем область для перетаскивания в центральной части toolbar
            CreateDragArea(root);

            // Создаем контейнер для кнопок управления окном только если включен showWindowControls
            if (settings.showWindowControls && rightZone != null)
            {
                // Избегаем дубликатов для window controls
                if (rightZone.Q(ContainerName) != null)
                {
                    _attempted = true;
                    EditorApplication.update -= TryInstall;
                    return;
                }

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
            }

            _attempted = true;
            EditorApplication.update -= TryInstall;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"TryInstall error: {e.Message}");
                _attempted = true;
                EditorApplication.update -= TryInstall;
            }
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
        
        private static Button CreateMenuBarButton()
        {
            var menuBarButton = new Button()
            {
                text = "MenuBar ▼",
                name = "MenuBarButton",
                tooltip = "Show main menu bar",
                style = {
                    minWidth = 75,
                    minHeight = 20,
                    maxHeight = 20,
                    marginLeft = 5,
                    marginRight = 3,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 2,
                    paddingBottom = 2,
                    fontSize = 11,
                    backgroundColor = Color.clear,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    color = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };
            
            // Добавляем класс toolbar кнопки для нативного стиля
            menuBarButton.AddToClassList("unity-toolbar-button");
            
            // Эффекты при наведении
            var hoverColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.8f, 0.8f, 0.8f);
            
            menuBarButton.RegisterCallback<MouseEnterEvent>((evt) => {
                menuBarButton.style.backgroundColor = hoverColor;
            });
            
            menuBarButton.RegisterCallback<MouseLeaveEvent>((evt) => {
                menuBarButton.style.backgroundColor = Color.clear;
            });
            
            // Используем клик для более точного позиционирования
            menuBarButton.RegisterCallback<ClickEvent>((evt) => {
                ShowMenuBarDropdown(menuBarButton);
            });
            
            return menuBarButton;
        }
        
        private static void CreateDragArea(VisualElement root)
        {
            try
            {
                var settings = EditorUISettings.Instance;
                if (settings == null || !settings.enableWindowDrag) return;
                
                // Проверяем что обработчик еще не добавлен
                if (root == null || root.Q(DragAreaName) != null) return;
                
                // Создаем невидимый маркер что обработчик добавлен
                var marker = new VisualElement()
                {
                    name = DragAreaName,
                    style = {
                        position = Position.Absolute,
                        width = 0,
                        height = 0,
                        opacity = 0
                    }
                };
                root.Add(marker);
                
                // Добавляем обработчик к root toolbar
                root.RegisterCallback<MouseDownEvent>((evt) => {
                    try
                    {
                        // Проверяем что клик не по кнопке или другому интерактивному элементу
                        var target = evt.target as VisualElement;
                        if (target != null && IsEmptyToolbarArea(target, root))
                        {
                            if (evt.button == 0) // Левая кнопка мыши
                            {
                                StartWindowDrag();
                                evt.StopPropagation();
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Drag handler error: {e.Message}");
                    }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"CreateDragArea error: {e.Message}");
            }
        }
        
        private static bool IsEmptyToolbarArea(VisualElement target, VisualElement root)
        {
            if (target == null || root == null) return false;
            
            // Проверяем что это не кнопка, не поле ввода и не другой интерактивный элемент
            if (target is Button || target is TextField || target is Toggle || 
                target.ClassListContains("unity-button") ||
                target.ClassListContains("unity-toolbar-button") ||
                (!string.IsNullOrEmpty(target.name) && target.name.Contains("Button")) ||
                (!string.IsNullOrEmpty(target.name) && target.name.Contains("Field")))
            {
                return false;
            }
            
            // Проверяем что это root или пустая зона
            return target == root || 
                   (!string.IsNullOrEmpty(target.name) && target.name.Contains("Zone")) || 
                   (!string.IsNullOrEmpty(target.name) && target.name.Contains("Toolbar")) ||
                   string.IsNullOrEmpty(target.name);
        }
        
        private static void ShowMenuBarDropdown(VisualElement button = null)
        {
            var menu = new GenericMenu();
            
            try
            {
                // Используем кешированные данные или создаем их, если кеш не готов
                List<string> menuItems;
                Dictionary<string, List<string>> groupedMenus;
                
                // Проверяем, не изменилось ли количество сборок (новые плагины/скрипты)
                var currentAssemblyCount = System.AppDomain.CurrentDomain.GetAssemblies().Length;
                bool assembliesChanged = _lastAssemblyCount != 0 && _lastAssemblyCount != currentAssemblyCount;
                
                if (_menuCacheInitialized && _cachedMenuItems != null && _cachedGroupedMenus != null && !assembliesChanged)
                {
                    Debug.Log("Using cached menu items");
                    menuItems = _cachedMenuItems;
                    groupedMenus = _cachedGroupedMenus;
                }
                else
                {
                    if (assembliesChanged)
                    {
                        Debug.Log($"Assembly count changed from {_lastAssemblyCount} to {currentAssemblyCount}, refreshing cache...");
                    }
                    else
                    {
                        Debug.Log("Cache not ready, generating menu items on demand...");
                    }
                    
                    var startTime = System.DateTime.Now;
                    menuItems = GetAllMenuItems();
                    groupedMenus = GroupMenuItems(menuItems);
                    
                    // Обновляем кеш
                    _cachedMenuItems = menuItems;
                    _cachedGroupedMenus = groupedMenus;
                    _menuCacheInitialized = true;
                    _lastAssemblyCount = currentAssemblyCount;
                    
                    var elapsed = System.DateTime.Now - startTime;
                    Debug.Log($"Menu cache updated in {elapsed.TotalMilliseconds:F0}ms");
                }
                
                // Добавляем элементы в меню
                Debug.Log($"Found {menuItems.Count} total menu items");
                
                var sortedCategories = groupedMenus.OrderBy(x => GetMenuOrder(x.Key)).ToList();
                
                for (int categoryIndex = 0; categoryIndex < sortedCategories.Count; categoryIndex++)
                {
                    var category = sortedCategories[categoryIndex];
                    Debug.Log($"Category '{category.Key}' has {category.Value.Count} items");
                    
                    foreach (var menuItem in category.Value.OrderBy(x => x))
                    {
                        var displayName = menuItem;
                        var isValidMenuItem = IsValidMenuItem(menuItem);
                        
                        if (isValidMenuItem)
                        {
                            menu.AddItem(new GUIContent(displayName), false, () => {
                                try
                                {
                                    EditorApplication.ExecuteMenuItem(menuItem);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogWarning($"Failed to execute menu item '{menuItem}': {e.Message}");
                                }
                            });
                        }
                        else
                        {
                            // Добавляем как отключенный элемент только если это не пустая строка
                            if (!string.IsNullOrEmpty(displayName))
                            {
                                menu.AddDisabledItem(new GUIContent(displayName));
                            }
                        }
                    }
                    
                    // Добавляем разделитель после каждой категории (кроме последней)
                    if (categoryIndex < sortedCategories.Count - 1)
                    {
                        menu.AddSeparator("");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to build dynamic menu: {e.Message}");
                
                // Fallback к статичному меню при ошибке
                AddFallbackMenuItems(menu);
            }
            
            // Показываем меню
            if (button != null)
            {
                var screenPos = button.worldBound;
                menu.DropDown(new Rect(screenPos.x, screenPos.y + screenPos.height, 0, 0));
            }
            else
            {
                menu.ShowAsContext();
            }
        }
        
        private static Dictionary<string, List<string>> GroupMenuItems(List<string> menuItems)
        {
            var groupedMenus = new Dictionary<string, List<string>>();
            
            foreach (var item in menuItems)
            {
                var parts = item.Split('/');
                if (parts.Length > 0)
                {
                    var mainCategory = parts[0];
                    
                    // Исключаем ненужные категории
                    if (mainCategory.Equals("CONTEXT", System.StringComparison.OrdinalIgnoreCase) ||
                        mainCategory.Equals("Internal", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    
                    if (!groupedMenus.ContainsKey(mainCategory))
                        groupedMenus[mainCategory] = new List<string>();
                        
                    groupedMenus[mainCategory].Add(item);
                }
            }
            
            return groupedMenus;
        }
        
        /// <summary>
        /// Принудительно обновляет кеш меню (полезно при добавлении новых MenuItem в рантайме)
        /// </summary>
        public static void RefreshMenuCache()
        {
            Debug.Log("Refreshing menu cache...");
            var startTime = System.DateTime.Now;
            
            try
            {
                _cachedMenuItems = GetAllMenuItems();
                _cachedGroupedMenus = GroupMenuItems(_cachedMenuItems);
                _menuCacheInitialized = true;
                _lastAssemblyCount = System.AppDomain.CurrentDomain.GetAssemblies().Length;
                
                var elapsed = System.DateTime.Now - startTime;
                Debug.Log($"Menu cache refreshed in {elapsed.TotalMilliseconds:F0}ms. Found {_cachedMenuItems.Count} menu items in {_cachedGroupedMenus.Count} categories.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to refresh menu cache: {e.Message}");
                _menuCacheInitialized = false;
            }
        }
        
        /// <summary>
        /// Очищает кеш меню
        /// </summary>
        public static void ClearMenuCache()
        {
            Debug.Log("Clearing menu cache...");
            _cachedMenuItems = null;
            _cachedGroupedMenus = null;
            _menuCacheInitialized = false;
        }
        
        private static List<string> GetAllMenuItems()
        {
            var menuItems = new List<string>();
            
            try
            {
                // Получаем все типы с MenuItem атрибутами
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        
                        foreach (var type in types)
                        {
                            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            
                            foreach (var method in methods)
                            {
                                var menuItemAttrs = method.GetCustomAttributes(typeof(MenuItem), false);
                                
                                foreach (MenuItem attr in menuItemAttrs)
                                {
                                    if (!string.IsNullOrEmpty(attr.menuItem))
                                    {
                                        menuItems.Add(attr.menuItem);
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        // Пропускаем сборки, которые не удается загрузить
                        continue;
                    }
                }
                
                // Также добавляем стандартные Unity меню, которые могут не иметь MenuItem атрибутов
                AddStandardUnityMenus(menuItems);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get menu items via reflection: {e.Message}");
            }
            
            return menuItems.Distinct().Where(item => 
                !string.IsNullOrEmpty(item) && 
                !item.Contains("%") && // Исключаем элементы с горячими клавишами
                !item.StartsWith("internal:", System.StringComparison.OrdinalIgnoreCase) && // Исключаем внутренние
                !item.StartsWith("CONTEXT/", System.StringComparison.OrdinalIgnoreCase) && // Исключаем контекстные
                !item.Contains("---") && // Исключаем разделители
                !item.Contains("_MenuItem") // Исключаем внутренние Unity элементы
            ).ToList();
        }
        
        private static void AddStandardUnityMenus(List<string> menuItems)
        {
            // Добавляем стандартные Unity меню, которые могут не обнаруживаться через рефлексию
            var standardMenus = new string[]
            {
                // File
                "File/New Scene", "File/Open Scene", "File/Save", "File/Save As...", "File/Save Project",
                "File/Build Settings...", "File/Build And Run", "File/Exit",
                
                // Edit
                "Edit/Undo", "Edit/Redo", "Edit/Cut", "Edit/Copy", "Edit/Paste", "Edit/Duplicate", "Edit/Delete",
                "Edit/Frame Selected", "Edit/Lock View to Selected", "Edit/Find", "Edit/Select All",
                "Edit/Play", "Edit/Pause", "Edit/Step",
                "Edit/Project Settings...", "Edit/Preferences...",
                
                // Assets
                "Assets/Create/Folder", "Assets/Create/C# Script", "Assets/Create/Material", "Assets/Create/Scene",
                "Assets/Show in Explorer", "Assets/Open", "Assets/Delete", "Assets/Refresh",
                "Assets/Import New Asset...", "Assets/Export Package...",
                
                // GameObject
                "GameObject/Create Empty", "GameObject/Create Empty Child",
                "GameObject/3D Object/Cube", "GameObject/3D Object/Sphere", "GameObject/3D Object/Capsule",
                "GameObject/Camera", "GameObject/Light/Directional Light",
                
                // Component - основные
                "Component/Physics/Rigidbody", "Component/Physics/Box Collider",
                "Component/Mesh/Mesh Renderer", "Component/Audio/Audio Source",
                
                // Window
                "Window/General/Project", "Window/General/Console", "Window/General/Hierarchy",
                "Window/General/Inspector", "Window/General/Scene", "Window/General/Game",
                "Window/Package Manager",
                
                // Help
                "Help/About Unity", "Help/Unity Manual", "Help/Scripting Reference", "Help/Report a Bug..."
            };
            
            foreach (var menu in standardMenus)
            {
                if (!menuItems.Contains(menu))
                {
                    menuItems.Add(menu);
                }
            }
        }
        
        private static int GetMenuOrder(string category)
        {
            // Определяем порядок основных категорий
            switch (category)
            {
                case "File": return 0;
                case "Edit": return 1;
                case "Assets": return 2;
                case "GameObject": return 3;
                case "Component": return 4;
                case "Services": return 5;
                case "Jobs": return 6;
                case "Tools": return 7;
                case "Window": return 8;
                case "Help": return 9;
                default: return 999; // Кастомные меню в конце
            }
        }
        
        private static bool IsValidMenuItem(string menuItem)
        {
            try
            {
                // Проверяем, существует ли команда меню
                // Некоторые меню могут быть недоступны в зависимости от контекста
                return !string.IsNullOrEmpty(menuItem) && !menuItem.Contains("---");
            }
            catch
            {
                return false;
            }
        }
        
        private static void AddFallbackMenuItems(GenericMenu menu)
        {
            // Основные меню как fallback
            menu.AddItem(new GUIContent("File/New Scene"), false, () => EditorApplication.ExecuteMenuItem("File/New Scene"));
            menu.AddItem(new GUIContent("File/Save"), false, () => EditorApplication.ExecuteMenuItem("File/Save"));
            menu.AddSeparator("File/");
            menu.AddItem(new GUIContent("Edit/Preferences..."), false, () => EditorApplication.ExecuteMenuItem("Edit/Preferences..."));
            menu.AddItem(new GUIContent("Edit/Project Settings..."), false, () => EditorApplication.ExecuteMenuItem("Edit/Project Settings..."));
            menu.AddSeparator("Edit/");
            menu.AddItem(new GUIContent("Assets/Create/C# Script"), false, () => EditorApplication.ExecuteMenuItem("Assets/Create/C# Script"));
            menu.AddItem(new GUIContent("Assets/Create/Folder"), false, () => EditorApplication.ExecuteMenuItem("Assets/Create/Folder"));
            menu.AddSeparator("Assets/");
            menu.AddItem(new GUIContent("GameObject/Create Empty"), false, () => EditorApplication.ExecuteMenuItem("GameObject/Create Empty"));
            menu.AddItem(new GUIContent("GameObject/3D Object/Cube"), false, () => EditorApplication.ExecuteMenuItem("GameObject/3D Object/Cube"));
            menu.AddSeparator("GameObject/");
            menu.AddItem(new GUIContent("Window/General/Console"), false, () => EditorApplication.ExecuteMenuItem("Window/General/Console"));
            menu.AddItem(new GUIContent("Window/General/Project"), false, () => EditorApplication.ExecuteMenuItem("Window/General/Project"));
            menu.AddSeparator("Window/");
            menu.AddItem(new GUIContent("Help/About Unity"), false, () => EditorApplication.ExecuteMenuItem("Help/About Unity"));
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
                
                var menuBarButton = root.Q("MenuBarButton");
                menuBarButton?.RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to remove window controls from toolbar: {e.Message}");
            }
        }
        
        private static void RemoveMenuBarButtonFromToolbar()
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

                var menuBarButton = root.Q("MenuBarButton");
                menuBarButton?.RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to remove MenuBar button from toolbar: {e.Message}");
            }
        }
        
        private static void RemoveWindowControlsFromToolbar()
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

                // Удаляем только контейнер с кнопками управления окном
                var container = root.Q(ContainerName);
                container?.RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to remove window controls from toolbar: {e.Message}");
            }
        }
        
        private static void RemoveDragAreaFromToolbar()
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

                // Убираем обработчик событий с root элемента
                if (root.userData != null && root.userData.Equals("drag_handler_added"))
                {
                    // Создаем новый root без обработчиков (Unity не дает прямого способа удалить обработчики)
                    root.userData = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to remove drag area from toolbar: {e.Message}");
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
        
        private static void StartWindowDrag()
        {
            try
            {
                if (_unityWindowHandle == IntPtr.Zero)
                    _unityWindowHandle = GetUnityMainWindow();
                    
                if (_unityWindowHandle != IntPtr.Zero)
                {
                    // Освобождаем захват мыши и отправляем сообщение о перетаскивании
                    ReleaseCapture();
                    SendMessageForDrag(_unityWindowHandle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to start window drag: {e.Message}");
            }
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