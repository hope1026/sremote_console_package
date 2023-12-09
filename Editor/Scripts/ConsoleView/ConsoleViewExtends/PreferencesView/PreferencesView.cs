// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.IO;
using SPlugin;
using UnityEditor;
using UnityEngine;

internal class PreferencesView : ConsoleViewAbstract
{
    public override ConsoleViewType ConsoleViewType => ConsoleViewType.PREFERENCES;

    private bool _toggleFile = false;
    private bool _toggleColor = false;
    private bool _requestedSendingPreferencesPacket = false;

    protected override void OnShow()
    {
        _requestedSendingPreferencesPacket = false;
    }

    protected override void OnHide()
    {
        if (_requestedSendingPreferencesPacket == true)
        {
            AppManager.Instance.GetActivatedApp().SendPreferences();
            _requestedSendingPreferencesPacket = false;
        }
    }

    public override void OnGuiCustom()
    {
        GUILayout.BeginVertical();

        GUILayout.Space(5f);

        GUILayoutOption height = GUILayout.Height(2f);
        GUILayoutOption width = GUILayout.ExpandWidth(true);

        OnGuiShowOption();
        SGuiUtility.OnGuiLine(width, height);

        OnGuiPreferences();
        SGuiUtility.OnGuiLine(width, height);

        // OnGuiLogFile();
        // SGuiUtility.OnGuiLine();

        OnGuiColors();

        GUILayout.EndVertical();
    }

    private void OnGuiShowOption()
    {
        string tempContent = "Show Time";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        bool tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME);
        tempToggle = GUILayout.Toggle(tempToggle, tempContent, SGuiStyle.ToggleStyle);
        if (true == GUI.changed)
        {
            ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_TIME : ConsoleEditorPrefsFlags.NONE);
            ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks_: ConsoleEditorPrefsFlags.SHOW_TIME);
        }

        tempContent = "Show FrameCount";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT);
        tempToggle = GUILayout.Toggle(tempToggle, tempContent, SGuiStyle.ToggleStyle);
        if (true == GUI.changed)
        {
            ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT : ConsoleEditorPrefsFlags.NONE);
            ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks_: ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT);
        }

        tempContent = "Show ObjectName";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME);
        tempToggle = GUILayout.Toggle(tempToggle, tempContent, SGuiStyle.ToggleStyle);
        if (true == GUI.changed)
        {
            ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME : ConsoleEditorPrefsFlags.NONE);
            ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks_: ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME);
        }

        tempContent = "Show UnityDebugLog";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        tempToggle = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG);
        tempToggle = GUILayout.Toggle(tempToggle, tempContent, SGuiStyle.ToggleStyle);
        if (true == GUI.changed)
        {
            ConsoleEditorPrefsFlags consoleEditorPrefsFlags = (true == tempToggle ? ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG : ConsoleEditorPrefsFlags.NONE);
            ConsoleEditorPrefs.SetFlags(consoleEditorPrefsFlags, masks_: ConsoleEditorPrefsFlags.SHOW_UNITY_DEBUG_LOG);
            _requestedSendingPreferencesPacket = true;
        }
    }

    private void OnGuiPreferences()
    {
        GUILayoutOption layoutWidth = GUILayout.Width(300f);

        float refreshIntervalTimeS = ConsoleEditorPrefs.ProfileRefreshIntervalTimeS;
        string tempContent = "ProfilerRefreshTime(Seconds)";
        EditorGUILayout.BeginHorizontal();
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle, layoutWidth);
        refreshIntervalTimeS = EditorGUILayout.FloatField(refreshIntervalTimeS, SGuiStyle.NumberFieldStyle);
        EditorGUILayout.EndHorizontal();
        if (Math.Abs(refreshIntervalTimeS - ConsoleEditorPrefs.ProfileRefreshIntervalTimeS) > float.Epsilon)
        {
            ConsoleEditorPrefs.SetProfileRefreshTimeS(refreshIntervalTimeS);
            _requestedSendingPreferencesPacket = true;
        }

        int skipStackFrameCount = (int)ConsoleEditorPrefs.SkipStackFrameCount;

        EditorGUILayout.BeginHorizontal();
        tempContent = "SkipStackFrameCount";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle, layoutWidth);
        skipStackFrameCount = EditorGUILayout.IntField(skipStackFrameCount, SGuiStyle.NumberFieldStyle);
        EditorGUILayout.EndHorizontal();
        skipStackFrameCount = Mathf.Max(skipStackFrameCount, 0);
        if (skipStackFrameCount != ConsoleEditorPrefs.SkipStackFrameCount)
        {
            ConsoleEditorPrefs.SetSkipStackFrameCount((UInt32)skipStackFrameCount);
            _requestedSendingPreferencesPacket = true;
        }
    }

    private void OnGuiLogFile()
    {
        string tempContent = "File";
        tempContent = SGuiUtility.ReplaceBoldString(tempContent);
        _toggleFile = EditorGUILayout.Foldout(_toggleFile, tempContent, SGuiStyle.FoldOutStyle);
        if (true == _toggleFile)
        {
            //파일 타입
            GUILayoutOption layoutWidth = GUILayout.Width(ConsoleViewLayoutDefines.windowSize.x * 0.5f);
            tempContent = "LogFileType";
            tempContent = SGuiUtility.ReplaceBoldString(tempContent);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle, layoutWidth);
            LogFileManager.FileType oldFileType = ConsoleEditorPrefs.LogFileType;
            ConsoleEditorPrefs.LogFileType = (LogFileManager.FileType)EditorGUILayout.EnumPopup(ConsoleEditorPrefs.LogFileType);
            EditorGUILayout.EndHorizontal();
            
            if (oldFileType != ConsoleEditorPrefs.LogFileType)
            {
                ConsoleEditorPrefs.SetLogFileType(ConsoleEditorPrefs.LogFileType);
                LogFileManager.CloseFile();
                LogFileManager.GenerateFilePath();
            }

            //디렉토리 관련
            EditorGUILayout.BeginHorizontal();
            tempContent = "SaveDirectory";
            tempContent = SGuiUtility.ReplaceBoldString(tempContent);
            EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle, layoutWidth);
            tempContent = ConsoleEditorPrefs.LogDirectoryAbsolutePath;
            EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (true == GUILayout.Button("OpenDirectory", layoutWidth))
            {
                EditorUtility.OpenWithDefaultApp(ConsoleEditorPrefs.LogDirectoryAbsolutePath);
            }

            if (true == GUILayout.Button("ChangeDirectory", layoutWidth))
            {
                string directory = EditorUtility.OpenFolderPanel("SelectLogDirectory", ConsoleEditorPrefs.LogDirectoryAbsolutePath, string.Empty);
                if (false == string.IsNullOrEmpty(directory))
                {
                    ConsoleEditorPrefs.LogDirectoryAbsolutePath = directory;
                    ConsoleEditorPrefs.SetLogDirectoryPathA(ConsoleEditorPrefs.LogDirectoryAbsolutePath);
                }
            }

            EditorGUILayout.EndHorizontal();

            //파일 관련
            EditorGUILayout.BeginHorizontal();
            tempContent = "SaveFile";
            tempContent = SGuiUtility.ReplaceBoldString(tempContent);
            EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle, layoutWidth);
            tempContent = LogFileManager.FilePathAbsolute;
            EditorGUILayout.LabelField(tempContent, SGuiStyle.BoxStyle);
            EditorGUILayout.EndHorizontal();

            if (true == File.Exists(LogFileManager.FilePathAbsolute))
            {
                EditorGUILayout.BeginHorizontal();
                if (true == GUILayout.Button("OpenFile", layoutWidth))
                {
                    EditorUtility.OpenWithDefaultApp(LogFileManager.FilePathAbsolute);
                }

                if (true == GUILayout.Button("Save as...", layoutWidth))
                {
                    string saveAsFilePathAbsolute = string.Empty;
                    string fileName = LogFileManager.FileName + "_Copy";
                    saveAsFilePathAbsolute = EditorUtility.SaveFilePanel("Save SendLog File as..", ConsoleEditorPrefs.LogDirectoryAbsolutePath, fileName,
                                                                         LogFileManager.FileTypeToExtension(ConsoleEditorPrefs.LogFileType));

                    LogFileManager.SaveAs(saveAsFilePathAbsolute);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void OnGuiColors()
    {
        string tempContent = SGuiUtility.ReplaceBoldString("Color");
        _toggleColor = EditorGUILayout.Foldout(_toggleColor, tempContent, SGuiStyle.FoldOutStyle);
        if (false == _toggleColor)
            return;

        ConsoleEditorPrefs.BackgroundColor = EditorGUILayout.ColorField("Background", ConsoleEditorPrefs.BackgroundColor);
        ConsoleEditorPrefs.TextColor = EditorGUILayout.ColorField("TextColor", ConsoleEditorPrefs.TextColor);
        if (true == GUI.changed)
        {
            SGuiStyle.RequestUpdateColors = true;
        }

        ConsoleEditorPrefs.LogViewBackground1Color = EditorGUILayout.ColorField("LogListViewBackground1", ConsoleEditorPrefs.LogViewBackground1Color);
        ConsoleEditorPrefs.LogViewBackground2Color = EditorGUILayout.ColorField("LogListViewBackground2", ConsoleEditorPrefs.LogViewBackground2Color);
        ConsoleEditorPrefs.LogViewSelectedBackgroundColor = EditorGUILayout.ColorField("LogListViewSelectedBackground", ConsoleEditorPrefs.LogViewSelectedBackgroundColor);

        if (true == GUILayout.Button("ResetDefaultColors"))
        {
            ConsoleEditorPrefs.ResetDefaultColors();
            SGuiStyle.UpdateColor();
            ConsoleEditorPrefs.WriteColorPrefs();
        }
    }
}