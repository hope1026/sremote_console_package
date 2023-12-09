// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using SPlugin;
using UnityEditor;
using UnityEngine;

public class QuickSearchEditorWindow : EditorWindow
{
    private string _searchString = string.Empty;
    private bool _requestSearchString = false;

    void OnGUI()
    {
        GUILayout.BeginVertical();

        OnGuiSearchList();

        GUILayout.EndVertical();
    }

    private void OnGuiSearchList()
    {
        GUILayout.BeginVertical();
        const int SPACE_PIXELS = 20;

        string tempContent = "Quick Search String";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        GUILayout.Box(tempContent, SGuiStyle.BoxStyle, GUILayout.ExpandWidth(true));

        GUILayout.BeginHorizontal();
        GUILayout.Space(SPACE_PIXELS);
        GUI.SetNextControlName(ConsoleViewNameDefines.GuiControllerUniqueName.PreferenceName.TEXT_FIELD_ADD_SEARCH);
        _searchString = GUILayout.TextField(_searchString, GUILayout.MinWidth(200f));

        if (Event.current.type == EventType.Repaint)
        {
            if (true == ConsoleViewNameDefines.GuiControllerUniqueName.PreferenceName.TEXT_FIELD_ADD_SEARCH.Equals(GUI.GetNameOfFocusedControl()))
            {
                SGuiUtility.OnGuiCheckTextFieldCopyAndPaste(ref _searchString);
            }
        }

        if (true == ConsoleViewNameDefines.GuiControllerUniqueName.PreferenceName.TEXT_FIELD_ADD_SEARCH.Equals(GUI.GetNameOfFocusedControl()) &&
            EventType.KeyUp == Event.current.type && KeyCode.Return == Event.current.keyCode)
        {
            _requestSearchString = true;
        }

        if (true == GUILayout.Button("Add String", GUILayout.Width(80f)) ||
            (true == _requestSearchString && EventType.KeyUp != Event.current.type))
        {
            ConsoleEditorPrefs.AddQuickSearchString(_searchString);
            _searchString = string.Empty;
            _requestSearchString = false;
        }

        GUILayout.EndHorizontal();

        int filterCount = ConsoleEditorPrefs.GetQuickSearchListCount();
        List<ConsoleEditorPrefsSearchContext> removeList = new List<ConsoleEditorPrefsSearchContext>();
        for (int filterIndex = 0; filterIndex < filterCount; filterIndex++)
        {
            ConsoleEditorPrefsSearchContext filterContext = ConsoleEditorPrefs.GetQuickSearchContext(filterIndex);
            if (null != filterContext)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(SPACE_PIXELS);
                GUILayout.Label(filterContext.SearchString);
                if (true == GUILayout.Button("Delete", GUILayout.Width(80f)))
                {
                    removeList.Add(filterContext);
                }

                GUILayout.EndHorizontal();
            }
        }

        foreach (ConsoleEditorPrefsSearchContext searchData in removeList)
        {
            ConsoleEditorPrefs.RemoveQuickSearchString(searchData.SearchString);
        }

        GUILayout.EndVertical();
    }
}