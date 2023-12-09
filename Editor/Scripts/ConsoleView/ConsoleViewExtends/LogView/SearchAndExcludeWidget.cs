// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class SearchAndExcludeWidget
    {
        private bool _isFocusedSearchTextField = false;
        private bool _isFocusedExcludeTextField = false;
        private LogView _ownerLogView = null;

        public void Initialize(LogView ownerLogView_)
        {
            _ownerLogView = ownerLogView_;
            ConsoleEditorPrefs.SearchString = string.Empty;
        }

        public void OnGuiCustom()
        {
            if (_ownerLogView == null || _ownerLogView.CurrentAppRef == null)
                return;

            OnGuiBackgroundBox();

            GUILayout.BeginArea(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.areaRect);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            OnGuiSearchBar();
            OnGuiExcludeBar();
            EditorGUILayout.EndHorizontal();

            OnGuiQuickSearchList();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void OnGuiBackgroundBox()
        {
            SGuiUtility.BeginBackgroundColor(ConsoleEditorPrefs.BackgroundColor);

            SGuiStyle.BoxStyle.normal.background = EditorGUIUtility.whiteTexture;
            GUI.Box(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.areaRect, "", SGuiStyle.BoxStyle);
            SGuiStyle.BoxStyle.normal.background = GUI.skin.box.normal.background;

            SGuiUtility.EndBackgroundColor();
        }

        private void OnGuiSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            GUIContent content = new GUIContent();
            content.text = "Search";
            content.text = SGuiUtility.ReplaceBoldString(content.text);
            content.tooltip = "You can search for a specific string.";
            GUILayout.Label(content, SGuiStyle.BoxStyle, GUILayout.Width(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.SEARCH_LABEL_WIDTH));

            if (Event.current.type == EventType.Repaint)
            {
                _isFocusedSearchTextField = ConsoleViewNameDefines.GuiControllerUniqueName.SearchAndExcludeMenu.TEXT_FIELD_SEARCH.Equals(GUI.GetNameOfFocusedControl());
            }

            string searchString = ConsoleEditorPrefs.SearchString;
            bool isEmptyString = (true == string.IsNullOrEmpty(ConsoleEditorPrefs.SearchString) ? true : false);
            if (true == isEmptyString)
            {
                searchString = "You can search for a specific string.";
                searchString = SGuiUtility.ReplaceColorString(searchString, "808080ff");
            }

            if (true == isEmptyString && true == _isFocusedSearchTextField)
            {
                searchString = string.Empty;
            }

            if (true == _isFocusedSearchTextField)
            {
                SGuiUtility.OnGuiCheckTextFieldCopyAndPaste(ref searchString);
            }

            GUILayoutOption layoutOptionWidth = GUILayout.ExpandWidth(true);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.LINE_HEIGHT);

            GUI.SetNextControlName(ConsoleViewNameDefines.GuiControllerUniqueName.SearchAndExcludeMenu.TEXT_FIELD_SEARCH);
            string oldSearchString = searchString;
            string newSearchString = GUILayout.TextField(oldSearchString, SGuiStyle.TextAreaStyle, layoutOptionWidth, layoutOptionHeight);

            if (false == oldSearchString.Equals(newSearchString))
            {
                ConsoleEditorPrefs.SearchString = newSearchString;
                _ownerLogView.CurrentAppRef.logCollection.FilteringBuffer();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnGuiExcludeBar()
        {
            EditorGUILayout.BeginHorizontal();

            GUIContent content = new GUIContent();
            content.text = "Exclude";
            content.text = SGuiUtility.ReplaceBoldString(content.text);
            content.tooltip = "You can exclude for a specific string.";
            GUILayout.Label(content, SGuiStyle.BoxStyle, GUILayout.Width(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.EXCLUDE_FILTER_LABEL_WIDTH));

            if (Event.current.type == EventType.Repaint)
            {
                _isFocusedExcludeTextField = ConsoleViewNameDefines.GuiControllerUniqueName.SearchAndExcludeMenu.TEXT_FIELD_EXCLUDE.Equals(GUI.GetNameOfFocusedControl());
            }

            bool isEmptyString = (true == string.IsNullOrEmpty(ConsoleEditorPrefs.ExcludeFilterString) ? true : false);
            string excludeFilterString = ConsoleEditorPrefs.ExcludeFilterString;
            if (true == isEmptyString)
            {
                excludeFilterString = "You can exclude for a specific string.";
                excludeFilterString = SGuiUtility.ReplaceColorString(excludeFilterString, "808080ff");
            }

            if (true == isEmptyString && true == _isFocusedExcludeTextField)
            {
                excludeFilterString = string.Empty;
            }

            if (true == _isFocusedExcludeTextField)
            {
                SGuiUtility.OnGuiCheckTextFieldCopyAndPaste(ref excludeFilterString);
            }

            GUILayoutOption layoutOptionWidth = GUILayout.ExpandWidth(true);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.LINE_HEIGHT);

            GUI.SetNextControlName(ConsoleViewNameDefines.GuiControllerUniqueName.SearchAndExcludeMenu.TEXT_FIELD_EXCLUDE);
            string oldExcludeFilterStringString = excludeFilterString;
            string newExcludeFilterString = GUILayout.TextField(excludeFilterString, SGuiStyle.TextAreaStyle, layoutOptionWidth, layoutOptionHeight);

            if (false == oldExcludeFilterStringString.Equals(newExcludeFilterString))
            {
                ConsoleEditorPrefs.ExcludeFilterString = newExcludeFilterString;
                _ownerLogView.CurrentAppRef.logCollection.FilteringBuffer();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnGuiQuickSearchList()
        {
            if (Event.current.type == EventType.KeyUp)
                return;

            float lineWidth = 0f;
            float lineWidthMax = ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.areaRect.width;
            lineWidthMax -= (ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.SEARCH_LABEL_WIDTH + ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.SEARCH_TEXT_FIELD_WIDTH);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            string quickSearchTitleString = "QuickSearch +";
            string quickSearchButtonString = "+";
            quickSearchTitleString = SGuiUtility.ReplaceBoldString(quickSearchTitleString);
            quickSearchButtonString = SGuiUtility.ReplaceBoldString(quickSearchButtonString);

            if (true == GUILayout.Button(quickSearchTitleString, SGuiStyle.ButtonStyle, GUILayout.Width(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.QUICK_SEARCH_LABEL_WIDTH)))
            {
                Rect windowRect = new(Event.current.mousePosition.x, ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.yMax, ConsoleViewLayoutDefines.windowSize.x * 0.6f, ConsoleViewLayoutDefines.windowSize.y * 0.6f);
                QuickSearchEditorWindow window = EditorWindow.GetWindowWithRect<QuickSearchEditorWindow>(windowRect);
                window.ShowTab();
            }

            int lineCount = 1;
            int quickSearchCount = ConsoleEditorPrefs.GetQuickSearchListCount();
            if (quickSearchCount <= 0)
            {
                string helpString = SGuiUtility.ReplaceColorString("<--- You can add a quick search string by clicking the [QuickSearch+] button.", Color.grey);
                GUILayout.Box(helpString, SGuiStyle.BoxStyle);
            }
            else
            {
                GUILayout.Box("");
                lineWidth += ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.QUICK_SEARCH_LABEL_WIDTH;
                const int WIDTH_PER_CHARACTER = 12;

                SGuiStyle.ButtonStyle.margin.left = 0;
                SGuiStyle.ButtonStyle.margin.right = 0;

                for (int quickSearchListIndex = 0; quickSearchListIndex < quickSearchCount; quickSearchListIndex++)
                {
                    ConsoleEditorPrefsSearchContext consoleEditorPrefsSearchContext = ConsoleEditorPrefs.GetQuickSearchContext(quickSearchListIndex);
                    if (null != consoleEditorPrefsSearchContext)
                    {
                        lineWidth += consoleEditorPrefsSearchContext.SearchString.Length * WIDTH_PER_CHARACTER + 20f;
                        if (lineWidth >= lineWidthMax)
                        {
                            lineCount++;
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            lineWidth = 0;
                        }

                        GUILayoutOption widthOption = GUILayout.ExpandWidth(true);
                        GUILayoutOption heightOption = GUILayout.Height(ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.LINE_HEIGHT);
                        string filterName = consoleEditorPrefsSearchContext.SearchString;
                        if (consoleEditorPrefsSearchContext.SearchCount > 0)
                        {
                            filterName = $"{consoleEditorPrefsSearchContext.SearchString}({consoleEditorPrefsSearchContext.SearchCount})";
                        }

                        consoleEditorPrefsSearchContext.DoSearching = GUILayout.Toggle(consoleEditorPrefsSearchContext.DoSearching, filterName, SGuiStyle.ButtonStyle, widthOption, heightOption);
                        if (true == GUI.changed)
                        {
                            ConsoleEditorPrefs.WriteSearchStringListPrefs();
                            _ownerLogView.CurrentAppRef.logCollection.FilteringBuffer();
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.lineCount != lineCount + 1)
            {
                ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.lineCount = lineCount + 1;
                ConsoleViewLayoutDefines.LogViewExcludeFilterAndSearchMenuWidget.OnChangeWindowSize();
                ConsoleViewLayoutDefines.LogListWidget.OnChangeWindowSize();
            }
        }
    }
}