// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SPlugin.RemoteConsole.Editor;

internal class ExtendLogEditorWindow : EditorWindow
{
    private LogItem _selectedLogItemData = null;
    private string _searchString = string.Empty;
    
    // UI Elements
    private VisualElement _rootElement;
    private TextField _searchField;
    private TextField _logContent;

    private void CreateGUI()
    {
        titleContent = new GUIContent("Extended Log");
        CreateUIElements();
        BindEvents();
        RefreshLogContent();
    }

    private void CreateUIElements()
    {
        // Load UXML template
        var visualTree = Resources.Load<VisualTreeAsset>("UI/ExtendLogEditorWindow");
        if (visualTree == null)
        {
            Debug.LogError("ExtendLogEditorWindow.uxml not found in Resources/UI/");
            return;
        }

        _rootElement = visualTree.Instantiate();
        rootVisualElement.Add(_rootElement);

        // Load USS styles
        var baseStyles = Resources.Load<StyleSheet>("UI/BaseStyles");
        var windowStyles = Resources.Load<StyleSheet>("UI/ExtendLogEditorWindowStyles");
        
        if (baseStyles != null)
        {
            _rootElement.styleSheets.Add(baseStyles);
        }
        if (windowStyles != null)
        {
            _rootElement.styleSheets.Add(windowStyles);
        }

        // Get UI element references
        _searchField = _rootElement.Q<TextField>("search-field");
        _logContent = _rootElement.Q<TextField>("log-content");
        
        // Setup initial state
        if (_searchField != null)
        {
            _searchField.value = _searchString;
        }
    }

    private void BindEvents()
    {
        _searchField?.RegisterValueChangedCallback(OnSearchChanged);
    }

    private void OnSearchChanged(ChangeEvent<string> evt_)
    {
        _searchString = evt_.newValue ?? "";
        RefreshLogContent();
    }

    private void RefreshLogContent()
    {
        if (_logContent == null || _selectedLogItemData == null) return;

        // Combine log data and stack trace
        string fullLogText = $"{_selectedLogItemData.LogData}\n{_selectedLogItemData.StackString}";
        
        // Apply search highlighting if search string is provided
        if (!string.IsNullOrEmpty(_searchString))
        {
            fullLogText = fullLogText.Replace(_searchString, $"<b><size=15>{_searchString}</size></b>");
        }

        _logContent.value = fullLogText;

        // Apply color based on log type
        _logContent.RemoveFromClassList("log-type-log");
        _logContent.RemoveFromClassList("log-type-warning");
        _logContent.RemoveFromClassList("log-type-error");

        switch (_selectedLogItemData.LogType)
        {
            case LogType.Log:
                _logContent.AddToClassList("log-type-log");
                _logContent.style.color = new StyleColor(ConsoleEditorPrefs.LogTextColor);
                break;
            case LogType.Warning:
                _logContent.AddToClassList("log-type-warning");
                _logContent.style.color = new StyleColor(ConsoleEditorPrefs.WarningTextColor);
                break;
            case LogType.Error:
            case LogType.Exception:
                _logContent.AddToClassList("log-type-error");
                _logContent.style.color = new StyleColor(ConsoleEditorPrefs.ErrorTextColor);
                break;
        }
    }

    public void SetLogItem(LogItem logItem_)
    {
        _selectedLogItemData = logItem_;
        RefreshLogContent();
    }

    private void OnDestroy()
    {
        // Cleanup event handlers
        _searchField?.UnregisterValueChangedCallback(OnSearchChanged);
    }
}