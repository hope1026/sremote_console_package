// 
// Copyright 2015 https://github.com/hope1026

using SPlugin;
using UnityEditor;
using UnityEngine;

internal class ExtendLogEditorWindow : EditorWindow
{
    private LogItem _selectedLogItemData = null;
    private Vector2 _scrollPos;
    private string _searchString = string.Empty;
    private bool _isFocusedSearchTextField = false;

    void OnGUI()
    {
        OnGuiSearchBar();
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        
        if (null != _selectedLogItemData)
        {
            string tempString = _selectedLogItemData.LogData;

            if (false == string.IsNullOrEmpty(_searchString))
            {
                tempString = tempString.Replace(_searchString, $"<size=15><b>{_searchString}</b></size>");
            }

            switch (_selectedLogItemData.LogType)
            {
                case LogType.Log:
                {
                    tempString = SGuiUtility.ReplaceColorString(tempString, ConsoleEditorPrefs.LogTextColor);
                    break;
                }
                case LogType.Warning:
                {
                    tempString = SGuiUtility.ReplaceColorString(tempString, ConsoleEditorPrefs.WarningTextColor);
                    break;
                }

                case LogType.Error:
                {
                    tempString = SGuiUtility.ReplaceColorString(tempString, ConsoleEditorPrefs.ErrorTextColor);
                    break;
                }
            }

            SGuiStyle.StackTextStyle.alignment = TextAnchor.UpperLeft;
            SGuiStyle.StackTextStyle.wordWrap = true;

            EditorGUILayout.SelectableLabel($"{tempString}\n{_selectedLogItemData.StackString}", 
                                            SGuiStyle.StackTextStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        GUILayout.EndScrollView();
    }

    private void OnGuiSearchBar()
    {
        EditorGUILayout.BeginHorizontal();
        string context = "Search";
        context = SGuiUtility.ReplaceBoldString(context);
        GUILayout.Label(context, SGuiStyle.BoxStyle, GUILayout.Width(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.SEARCH_LABEL_WIDTH));

        bool isEmptyString = (true == string.IsNullOrEmpty(_searchString) ? true : false);
        if (Event.current.type == EventType.Repaint)
        {
            _isFocusedSearchTextField = ConsoleViewNameDefines.GuiControllerUniqueName.ExtendLogEditor.TEXT_FIELD_SEARCH.Equals(GUI.GetNameOfFocusedControl());
        }

        context = _searchString;
        if (true == isEmptyString)
        {
            context = "All";
            context = SGuiUtility.ReplaceColorString(context, "808080ff");
        }

        if (true == isEmptyString && true == _isFocusedSearchTextField)
        {
            context = string.Empty;
        }

        if (true == _isFocusedSearchTextField)
        {
            SGuiUtility.OnGuiCheckTextFieldCopyAndPaste(ref context);
        }

        GUI.SetNextControlName(ConsoleViewNameDefines.GuiControllerUniqueName.ExtendLogEditor.TEXT_FIELD_SEARCH);
        string oldString = context;
        context = GUILayout.TextField(context, SGuiStyle.TextAreaStyle, GUILayout.ExpandWidth(true));

        if (false == oldString.Equals(context))
        {
            _searchString = context;
        }

        EditorGUILayout.EndHorizontal();
    }

    public void SetLogItem(LogItem logLogItem_)
    {
        _selectedLogItemData = logLogItem_;
    }
}