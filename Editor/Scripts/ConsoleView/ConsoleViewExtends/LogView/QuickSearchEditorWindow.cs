// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using SPlugin;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class QuickSearchEditorWindow : EditorWindow
{
    // Static event for notifying LogView when quick search list changes
    public static System.Action OnQuickSearchChanged;
    
    private VisualElement _rootElement;
    private TextField _searchInput;
    private Button _addButton;
    private VisualElement _searchList;
    private List<ConsoleEditorPrefsSearchContext> _searchItems = new List<ConsoleEditorPrefsSearchContext>();

    private void CreateGUI()
    {
        titleContent = new GUIContent("Quick Search");
        CreateUIElements();
        BindEvents();
        RefreshSearchList();
    }

    private void CreateUIElements()
    {
        // Load UXML template
        var visualTree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/QuickSearchEditorWindow");
        if (visualTree == null)
        {
            UnityEngine.Debug.LogError("QuickSearchEditorWindow.uxml not found in Resources/UI/");
            return;
        }

        _rootElement = visualTree.Instantiate();
        rootVisualElement.Add(_rootElement);

        // Load USS styles
        var baseStyles = UnityEngine.Resources.Load<StyleSheet>("UI/BaseStyles");
        var quickSearchStyles = UnityEngine.Resources.Load<StyleSheet>("UI/QuickSearchEditorWindowStyles");
        
        if (baseStyles != null)
        {
            rootVisualElement.styleSheets.Add(baseStyles);
        }
        if (quickSearchStyles != null)
        {
            rootVisualElement.styleSheets.Add(quickSearchStyles);
        }

        // Get references to UI elements
        _searchInput = _rootElement.Q<TextField>("search-input");
        _addButton = _rootElement.Q<Button>("add-button");
        _searchList = _rootElement.Q<VisualElement>("search-list");
    }

    private void BindEvents()
    {
        // Add button click
        _addButton?.RegisterCallback<ClickEvent>(evt =>
        {
            AddSearchString();
        });

        // Enter key in text field
        _searchInput?.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                AddSearchString();
                evt.StopPropagation();
            }
        });
    }

    private void AddSearchString()
    {
        string searchText = _searchInput?.value?.Trim();
        if (!string.IsNullOrEmpty(searchText))
        {
            ConsoleEditorPrefs.AddQuickSearchString(searchText);
            _searchInput.value = string.Empty;
            RefreshSearchList();
            
            // Notify LogView that quick search list has changed
            OnQuickSearchChanged?.Invoke();
        }
    }

    private void RefreshSearchList()
    {
        if (_searchList == null) return;

        // Clear existing items
        _searchList.Clear();
        _searchItems.Clear();

        // Add current search items
        int filterCount = ConsoleEditorPrefs.GetQuickSearchListCount();
        for (int filterIndex = 0; filterIndex < filterCount; filterIndex++)
        {
            ConsoleEditorPrefsSearchContext filterContext = ConsoleEditorPrefs.GetQuickSearchContext(filterIndex);
            if (filterContext != null)
            {
                _searchItems.Add(filterContext);
                CreateSearchItem(filterContext);
            }
        }
    }

    private void CreateSearchItem(ConsoleEditorPrefsSearchContext searchContext)
    {
        var itemContainer = new VisualElement();
        itemContainer.AddToClassList("quick-search-item");

        var label = new Label(searchContext.SearchString);
        label.AddToClassList("quick-search-item-label");

        var deleteButton = new Button(() =>
        {
            ConsoleEditorPrefs.RemoveQuickSearchString(searchContext.SearchString);
            RefreshSearchList();
            
            // Notify LogView that quick search list has changed
            OnQuickSearchChanged?.Invoke();
        });
        deleteButton.text = "Delete";
        deleteButton.AddToClassList("quick-search-delete-btn");

        itemContainer.Add(label);
        itemContainer.Add(deleteButton);
        _searchList.Add(itemContainer);
    }
}