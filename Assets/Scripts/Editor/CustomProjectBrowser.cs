using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
public class CustomProjectBrowser : EditorWindow
{
    private EditorWindow defaultProjectBrowser;
    private bool isInitialized = false;
    [MenuItem("Window/Custom Project Browser")]
    public static void ShowWindow()
    {
        GetWindow<CustomProjectBrowser>("Project").minSize = new Vector2(375, 300);
    }
    void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeProjectBrowser();
            isInitialized = true;
        }
    }
    private void InitializeProjectBrowser()
    {
        if (defaultProjectBrowser != null)
            return;
        var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        if (projectBrowserType == null)
        {
            Debug.LogError("ProjectBrowser type bulunamadı!");
            return;
        }
        try
        {
            // Önce ana pencereyi oluştur
            var mainWindow = GetWindow<CustomProjectBrowser>();
            defaultProjectBrowser = ScriptableObject.CreateInstance(projectBrowserType) as EditorWindow;
            if (defaultProjectBrowser == null)
            {
                Debug.LogError("ProjectBrowser instance oluşturulamadı!");
                return;
            }
            // Parent ilişkisini kur
            var parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var parentValue = parentField?.GetValue(mainWindow);
            defaultProjectBrowser.GetType().GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(defaultProjectBrowser, parentValue);
            // SearchFilter oluşturma ve ayarlama
            var searchFilterType = typeof(Editor).Assembly.GetType("UnityEditor.SearchFilter");
            object searchFilter = Activator.CreateInstance(searchFilterType);
            projectBrowserType.GetField("m_SearchFilter", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(defaultProjectBrowser, searchFilter);
            // Focus yönetimi için gerekli alanları ayarla
            var focusControllerType = typeof(Editor).Assembly.GetType("UnityEditor.EditorGUIUtility+FocusController");
            if (focusControllerType != null)
            {
                var focusController = Activator.CreateInstance(focusControllerType);
                typeof(EditorWindow).GetField("m_FocusController", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(defaultProjectBrowser, focusController);
            }
            // Başlatma metodlarını sırayla çağır
            defaultProjectBrowser.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(defaultProjectBrowser, null);
            defaultProjectBrowser.GetType().GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(defaultProjectBrowser, null);
            // ObjectListArea'yı başlat
            var initListAreaMethod = projectBrowserType.GetMethod("InitListArea",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(bool) },
                null);
            initListAreaMethod?.Invoke(defaultProjectBrowser, new object[] { false });
            // Pencere boyutlarını ayarla
            defaultProjectBrowser.minSize = mainWindow.minSize;
            defaultProjectBrowser.maxSize = mainWindow.maxSize;
        }
        catch (Exception e)
        {
            Debug.LogError($"Başlatma hatası: {e.Message}\n{e.StackTrace}");
            if (e.InnerException != null)
            {
                Debug.LogError($"Inner Exception: {e.InnerException.Message}\n{e.InnerException.StackTrace}");
            }
        }
    }
    void OnGUI()
    {
        if (defaultProjectBrowser == null)
            return;
        try
        {
            // Toolbar
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Yenile", EditorStyles.toolbarButton))
                {
                    AssetDatabase.Refresh();
                }
                // Search field
                var currentSearch = Kogane.ProjectBrowserInternal.SearchFieldText;
                var newSearch = EditorGUILayout.TextField(currentSearch, EditorStyles.toolbarSearchField);
                if (newSearch != currentSearch)
                {
                    Kogane.ProjectBrowserInternal.SetSearch(newSearch);
                }
            }
            // Calculate rect with clamped height
            var toolbarHeight = EditorStyles.toolbar.fixedHeight;
            float height = Mathf.Max(0, position.height - toolbarHeight);
            var rect = new Rect(0, toolbarHeight, position.width, height);
            if (rect.height <= 0)
                return;
            // Pencere pozisyonunu güncelle
            defaultProjectBrowser.position = new Rect(position.x, position.y, position.width, position.height);
            // Draw Project Browser
            var onGUIMethod = defaultProjectBrowser.GetType().GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.NonPublic);
            if (onGUIMethod != null)
            {
                try
                {
                    // Event handling
                    if (Event.current != null)
                    {
                        var currentEvent = Event.current;
                        var handleInputMethod = defaultProjectBrowser.GetType().GetMethod("HandleWindowInput",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        if (handleInputMethod != null && currentEvent.type != EventType.Repaint)
                        {
                            try
                            {
                                handleInputMethod.Invoke(defaultProjectBrowser, new object[] { currentEvent });
                            }
                            catch (Exception)
                            {
                                // ExitGUIException'ı göz ardı et
                            }
                        }
                    }
                    // GUI drawing
                    EditorGUILayout.BeginVertical();
                    try
                    {
                        onGUIMethod.Invoke(defaultProjectBrowser, null);
                    }
                    catch (TargetInvocationException e)
                    {
                        // ExitGUIException'ı göz ardı et
                        if (!(e.InnerException is ExitGUIException))
                        {
                            throw;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                catch (ExitGUIException)
                {
                    // ExitGUIException'ı göz ardı et
                }
            }
            // Pencere boyutlarını senkronize et
            if (defaultProjectBrowser != null)
            {
                defaultProjectBrowser.minSize = this.minSize;
                defaultProjectBrowser.maxSize = this.maxSize;
                // Pozisyonu güncelle
                if (defaultProjectBrowser.position != position)
                {
                    defaultProjectBrowser.position = position;
                }
            }
        }
        catch (Exception e)
        {
            if (!(e is ExitGUIException))
            {
                Debug.LogError($"GUI Error: {e.Message}\n{e.StackTrace}");
                if (e.InnerException != null && !(e.InnerException is ExitGUIException))
                {
                    Debug.LogError($"Inner Exception: {e.InnerException.Message}\n{e.InnerException.StackTrace}");
                }
            }
        }
    }
    void OnDestroy()
    {
        if (defaultProjectBrowser != null)
        {
            // Pencereyi doğru şekilde kapat
            defaultProjectBrowser.Close();
            DestroyImmediate(defaultProjectBrowser);
            defaultProjectBrowser = null;
        }
    }
}