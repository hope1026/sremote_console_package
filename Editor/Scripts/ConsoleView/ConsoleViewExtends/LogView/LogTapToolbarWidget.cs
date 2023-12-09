// 
// Copyright 2015 https://github.com/hope1026

using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class LogTapToolbarWidget
    {
        public bool IsPause { get; set; }
        private bool _requestStep = false;
        private LogView _logViewRef = null;

        public void Initialize(LogView consoleView_)
        {
            _logViewRef = consoleView_;
            IsPause = false;
        }

        public void OnGuiCustom()
        {
            if (_logViewRef == null || _logViewRef.CurrentAppRef == null)
                return;

            GUILayout.BeginArea(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect);

            GUILayout.BeginHorizontal();

            OnGuiClearButton();
            OnGuiCollapseButton();
            OnGuiClearOnPlayButton();
            OnGuiDefaultFilter();
            OnGuiPause();

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        public void UpdateCustom()
        {
            if (true == _requestStep)
            {
                EditorApplication.Step();
                _requestStep = false;
            }
        }

        private void OnGuiClearButton()
        {
            GUIContent content = new GUIContent();
            content.text = SGuiUtility.ReplaceBoldString("Clear");
            content.tooltip = "Removes any messages generated from your code.";

            GUILayoutOption layoutOptionWidth = GUILayout.Width(60f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.height);

            if (true == GUILayout.Button(content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight))
            {
                ConsoleEditorPrefs.CanClearLog = true;
            }
        }

        private void OnGuiCollapseButton()
        {
            GUIContent content = new GUIContent();
            content.text = SGuiUtility.ReplaceBoldString("Collapse");
            content.tooltip = "Shows only the first instance of recurring error messages.";

            GUILayoutOption layoutOptionWidth = GUILayout.Width(80f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.height);

            bool tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.IS_COLLAPSE);
            tempToggle = GUILayout.Toggle(tempToggle, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);
            if (true == GUI.changed)
            {
                ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.IS_COLLAPSE : ConsoleEditorPrefsFlags.NONE);
                ConsoleEditorPrefsFlags masks = ConsoleEditorPrefsFlags.IS_COLLAPSE;
                ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks);
                _logViewRef.CurrentAppRef.logCollection.FilteringBuffer();
                _logViewRef.ForceLogViewScrollBarBottomFixed();
            }
        }

        private void OnGuiClearOnPlayButton()
        {
            GUIContent content = new GUIContent();
            content.text = SGuiUtility.ReplaceBoldString("Clear On Play");
            content.tooltip = "Clears the Console automatically whenever you enter Play mode.";

            GUILayoutOption layoutOptionWidth = GUILayout.Width(100.0f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.height);

            bool tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.IS_CLEAR_ON_PLAY);
            tempToggle = GUILayout.Toggle(tempToggle, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);
            if (true == GUI.changed)
            {
                ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.IS_CLEAR_ON_PLAY : ConsoleEditorPrefsFlags.NONE);
                ConsoleEditorPrefsFlags masks = ConsoleEditorPrefsFlags.IS_CLEAR_ON_PLAY;
                ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks);
            }
        }

        private void OnGuiDefaultFilter()
        {
            GUIContent content = new GUIContent();

            const float DEFAULT_WIDTH = 30f;

            content.text = _logViewRef.CurrentAppRef.logCollection.LogCount.ToString();
            content.text = SGuiUtility.ReplaceColorString(content.text, ConsoleEditorPrefs.TextColor);
            GUILayoutOption layoutOptionWidth = GUILayout.Width(DEFAULT_WIDTH + _logViewRef.CurrentAppRef.logCollection.LogCount.ToString().Length * ConsoleViewLayoutDefines.WIDTH_PER_CHAR);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.height);

            bool tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_LOG);
            content.image = SGuiResources.LogIconTexture;
            content.tooltip = "Displays the number of messages in the Console. Click to show/hide messages.";
            tempToggle = GUILayout.Toggle(tempToggle, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);
            if (true == GUI.changed)
            {
                ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_LOG : ConsoleEditorPrefsFlags.NONE);
                ConsoleEditorPrefsFlags masks = ConsoleEditorPrefsFlags.SHOW_LOG;
                ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks);
                _logViewRef.CurrentAppRef.logCollection.FilteringBuffer();
                _logViewRef.ForceLogViewScrollBarBottomFixed();
            }

            layoutOptionWidth = GUILayout.Width(DEFAULT_WIDTH + _logViewRef.CurrentAppRef.logCollection.WarningCount.ToString().Length * ConsoleViewLayoutDefines.WIDTH_PER_CHAR);
            content.text = _logViewRef.CurrentAppRef.logCollection.WarningCount.ToString();
            content.text = SGuiUtility.ReplaceColorString(content.text, ConsoleEditorPrefs.WarningTextColor);
            tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_WARNING);
            content.image = SGuiResources.WarningIconTexture;
            content.tooltip = "Displays the number of warnings in the Console. Click to show/hide warnings.";
            tempToggle = GUILayout.Toggle(tempToggle, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);
            if (true == GUI.changed)
            {
                ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_WARNING : ConsoleEditorPrefsFlags.NONE);
                ConsoleEditorPrefsFlags masks = ConsoleEditorPrefsFlags.SHOW_WARNING;
                ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks);
                _logViewRef.CurrentAppRef.logCollection.FilteringBuffer();
                _logViewRef.ForceLogViewScrollBarBottomFixed();
            }

            layoutOptionWidth = GUILayout.Width(DEFAULT_WIDTH + _logViewRef.CurrentAppRef.logCollection.ErrorCount.ToString().Length * ConsoleViewLayoutDefines.WIDTH_PER_CHAR);
            content.text = _logViewRef.CurrentAppRef.logCollection.ErrorCount.ToString();
            content.text = SGuiUtility.ReplaceColorString(content.text, ConsoleEditorPrefs.ErrorTextColor);
            tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_ERROR);
            content.image = SGuiResources.ErrorIconTexture;
            content.tooltip = "Displays the number of errors in the Console. Click to show/hide errors.";
            tempToggle = GUILayout.Toggle(tempToggle, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);
            if (true == GUI.changed)
            {
                ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_ERROR : ConsoleEditorPrefsFlags.NONE);
                ConsoleEditorPrefsFlags masks = ConsoleEditorPrefsFlags.SHOW_ERROR;
                ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks);
                _logViewRef.CurrentAppRef.logCollection.FilteringBuffer();
                _logViewRef.ForceLogViewScrollBarBottomFixed();
            }
        }

        private void OnGuiPause()
        {
            GUIContent content = new GUIContent();
            content.text = SGuiUtility.ReplaceBoldString("Error Pause");

            GUILayoutOption layoutOptionWidth = GUILayout.Width(100f);
            GUILayoutOption layoutOptionHeight = GUILayout.Height(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.height);

            bool tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.ERROR_PAUSE);
            content.tooltip = "Pauses playback whenever Error is called from a script.";
            tempToggle = GUILayout.Toggle(tempToggle, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);
            if (true == GUI.changed)
            {
                ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.ERROR_PAUSE : ConsoleEditorPrefsFlags.NONE);
                ConsoleEditorPrefsFlags masks = ConsoleEditorPrefsFlags.ERROR_PAUSE;
                ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks);
            }

            content.text = string.Empty;
            content.tooltip = string.Empty;
            if (_logViewRef.CurrentAppRef.IsPlaying())
            {
                content.image = SGuiResources.PausePlayTexture;
            }
            else
            {
                content.image = SGuiResources.PauseTexture;
            }

            layoutOptionWidth = GUILayout.Width(30f);

            bool oldPauseState = IsPause;
            IsPause = GUILayout.Toggle(IsPause, content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight);

            if (oldPauseState != IsPause)
            {
                ForcePause(IsPause, false);
            }

            if (_logViewRef.CurrentAppRef.IsPlaying())
            {
                content.image = SGuiResources.StepPlayTexture;
            }
            else
            {
                content.image = SGuiResources.StepTexture;
            }

            if (true == GUILayout.Button(content, SGuiStyle.ButtonStyle, layoutOptionWidth, layoutOptionHeight))
            {
                IsPause = true;
                ForcePause(IsPause, true);
            }
        }

        public void ForcePause(bool pause_, bool cnaStep_)
        {
            IsPause = pause_;
            _logViewRef.CurrentAppRef?.SendPause(pause_, cnaStep_, ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.ERROR_PAUSE));
        }
    }
}