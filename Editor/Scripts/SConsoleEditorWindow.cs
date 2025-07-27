// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Reflection;
using SPlugin.RemoteConsole.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SConsoleEditorWindow : EditorWindow
{
    private static SConsoleEditorWindow _consoleEditorWindow = null;
    private VisualElement _uiToolkitContainer;

    [MenuItem("Window/SPlugin/SConsole")]
    static void InstanceConsoleEditor()
    {
        _consoleEditorWindow = GetWindow<SConsoleEditorWindow>();
    }

    void CreateGUI()
    {
        try
        {
            // Create UI container
            _uiToolkitContainer = new VisualElement();
            _uiToolkitContainer.style.flexGrow = 1;
            rootVisualElement.Add(_uiToolkitContainer);
            
            // Initialize UI views in ConsoleViewMain
            ConsoleViewMain.Instance.InitializeUI(_uiToolkitContainer);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    void Update()
    {
        try
        {
            ConsoleViewMain.Instance.UpdateCustom();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    void OnEnable()
    {
        try
        {
            ConsoleViewMain.Instance.Terminate();
            ConsoleViewMain.Instance.Initialize(this);
            SPlugin.RemoteConsoleToLocalApplicationBridge.Instance.OnStartInEditorApplication();
            _consoleEditorWindow = this;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    void OnDisable()
    {
        try
        {
            ConsoleEditorPrefs.WritePrefs();
            ConsoleViewMain.Instance.Terminate();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    void OnDestroy()
    {
        try
        {
            ConsoleEditorPrefs.WritePrefs();
            ConsoleViewMain.Instance.Terminate();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void OnUpdateCheckConsoleOpenEventHandler()
    {
        if (_consoleEditorWindow != null)
            return;

        try
        {
            Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            if (null != unityEditorAssembly)
            {
                Type consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");
                if (null != consoleWindowType)
                {
                    FieldInfo staticFieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
                    if (null != staticFieldInfo)
                    {
                        EditorWindow consoleWindow = staticFieldInfo.GetValue(null) as EditorWindow;
                        if (consoleWindow != null && _consoleEditorWindow == null)
                        {
                            _consoleEditorWindow = GetWindow<SConsoleEditorWindow>(consoleWindowType);
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    [InitializeOnLoadMethod]
    public static void InitializeOnLoadEditor()
    {
        EditorApplication.update -= OnUpdateCheckConsoleOpenEventHandler;
        EditorApplication.update += OnUpdateCheckConsoleOpenEventHandler;
    }
}