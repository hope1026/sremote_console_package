// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace SPlugin
{
    internal static class ConsoleEditorPrefs
    {
        private static readonly List<ConsoleEditorPrefsSearchContext> _quickSearchContextList = new List<ConsoleEditorPrefsSearchContext>();
        private static ConsoleEditorPrefsFlags _consoleEditorPrefsFlags = ConsoleEditorPrefsFlags.NONE;
        public static bool CanClearLog { get; set; }
        public static string SearchString { get; set; }
        public static string ExcludeFilterString { get; set; }
        public static float ProfileRefreshIntervalTimeS { get; private set; }
        public static UInt32 SkipStackFrameCount { get; private set; }
        public static string LogDirectoryAbsolutePath { get; set; }
        public static LogFileManager.FileType LogFileType { get; set; }
        public static Color32 BackgroundColor { get; set; }
        public static Color32 TextColor { get; set; }
        public static Color32 LogViewBackground1Color { get; set; }
        public static Color32 LogViewBackground2Color { get; set; }
        public static Color32 LogViewSelectedBackgroundColor { get; set; }
        public static Color32 LogTextColor { get; set; }
        public static Color32 WarningTextColor { get; set; }
        public static Color32 ErrorTextColor { get; set; }

        public static void ReadPrefs()
        {
            ConsoleEditorPrefs._quickSearchContextList.Clear();

            const ConsoleEditorPrefsFlags DEFAULT_FLAGS = ConsoleEditorPrefsFlags.SHOW_LOG | ConsoleEditorPrefsFlags.SHOW_LOG | ConsoleEditorPrefsFlags.SHOW_WARNING |
                                                          ConsoleEditorPrefsFlags.SHOW_ERROR | ConsoleEditorPrefsFlags.SHOW_TIME | ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT |
                                                          ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME | ConsoleEditorPrefsFlags.SHOW_FONT_EFFECT | ConsoleEditorPrefsFlags.SHOW_SYSTEM_MESSAGE;
            ConsoleEditorPrefs._consoleEditorPrefsFlags = (ConsoleEditorPrefsFlags)EditorPrefs.GetInt(ConsoleEditorPrefsIds.OPTION_FLAG, (int)DEFAULT_FLAGS);
            ConsoleEditorPrefs._consoleEditorPrefsFlags |= ConsoleEditorPrefsFlags.SHOW_FONT_EFFECT;
            ConsoleEditorPrefs._consoleEditorPrefsFlags |= ConsoleEditorPrefsFlags.SHOW_SYSTEM_MESSAGE;

            int searchCount = EditorPrefs.GetInt(ConsoleEditorPrefsIds.SEARCH_LIST_COUNT, 0);

            for (int searchListIndex = 0; searchListIndex < searchCount; searchListIndex++)
            {
                string keyString = $"{ConsoleEditorPrefsIds.SEARCH_LIST_STRING}[{searchListIndex}]";
                string keyDoSearching = $"{ConsoleEditorPrefsIds.SEARCH_LIST_DO_SEARCHING}[{searchListIndex}]";

                ConsoleEditorPrefsSearchContext tempConsoleEditorPrefsSearchContext = new ConsoleEditorPrefsSearchContext();
                tempConsoleEditorPrefsSearchContext.SearchString = EditorPrefs.GetString(keyString, string.Empty);
                if (true == bool.TryParse(EditorPrefs.GetString(keyDoSearching, "false"), out bool outFilter))
                {
                    tempConsoleEditorPrefsSearchContext.DoSearching = outFilter;
                }

                ConsoleEditorPrefs._quickSearchContextList.Add(tempConsoleEditorPrefsSearchContext);
            }

            ConsoleEditorPrefs.ProfileRefreshIntervalTimeS = EditorPrefs.GetFloat(ConsoleEditorPrefsIds.PROFILE_REFRESH_INTERVAL_TIME_S, 10f);
            ConsoleEditorPrefs.SkipStackFrameCount = (UInt32)Mathf.Max(0, EditorPrefs.GetInt(ConsoleEditorPrefsIds.SKIP_STACK_FRAME_COUNT, 0));
            ConsoleEditorPrefs.LogDirectoryAbsolutePath = EditorPrefs.GetString(ConsoleEditorPrefsIds.LOG_DIRECTORY_PATH_A, LogFileManager.DEFAULT_DIRECTORY_ABSOLUTE_PATH);
            if (false == Directory.Exists(ConsoleEditorPrefs.LogDirectoryAbsolutePath) && false == string.IsNullOrEmpty(ConsoleEditorPrefs.LogDirectoryAbsolutePath))
            {
                Directory.CreateDirectory(ConsoleEditorPrefs.LogDirectoryAbsolutePath);
            }

            ConsoleEditorPrefs.LogFileType = (LogFileManager.FileType)EditorPrefs.GetInt(ConsoleEditorPrefsIds.LOG_FILE_TYPE, (int)LogFileManager.FileType.NONE);

            ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth = EditorPrefs.GetFloat(ConsoleEditorPrefsIds.LOG_VIEW_TIME_WIDTH, ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth);
            ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth = EditorPrefs.GetFloat(ConsoleEditorPrefsIds.LOG_VIEW_FRAME_COUNT_WIDTH, ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth);
            ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth = EditorPrefs.GetFloat(ConsoleEditorPrefsIds.LOG_VIEW_OBJECT_NAME_WIDTH, ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth);

            ConsoleEditorPrefs.ResetDefaultColors();
            ReadColorPrefs();
        }

        public static void WritePrefs()
        {
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.OPTION_FLAG, (int)ConsoleEditorPrefs._consoleEditorPrefsFlags);
            EditorPrefs.SetFloat(ConsoleEditorPrefsIds.PROFILE_REFRESH_INTERVAL_TIME_S, ConsoleEditorPrefs.ProfileRefreshIntervalTimeS);
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.SKIP_STACK_FRAME_COUNT, (int)ConsoleEditorPrefs.SkipStackFrameCount);

            EditorPrefs.SetString(ConsoleEditorPrefsIds.LOG_DIRECTORY_PATH_A, ConsoleEditorPrefs.LogDirectoryAbsolutePath);
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.LOG_FILE_TYPE, (int)ConsoleEditorPrefs.LogFileType);

            EditorPrefs.SetFloat(ConsoleEditorPrefsIds.LOG_VIEW_TIME_WIDTH, ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth);
            EditorPrefs.SetFloat(ConsoleEditorPrefsIds.LOG_VIEW_FRAME_COUNT_WIDTH, ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth);
            EditorPrefs.SetFloat(ConsoleEditorPrefsIds.LOG_VIEW_OBJECT_NAME_WIDTH, ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth);

            WriteSearchStringListPrefs();
            WriteColorPrefs();
        }

        public static void WriteSearchStringListPrefs()
        {
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.SEARCH_LIST_COUNT, ConsoleEditorPrefs._quickSearchContextList.Count);
            for (int searchListIndex = 0; searchListIndex < ConsoleEditorPrefs._quickSearchContextList.Count; searchListIndex++)
            {
                string keyString = $"{ConsoleEditorPrefsIds.SEARCH_LIST_STRING}[{searchListIndex}]";
                string keyIsFiltering = $"{ConsoleEditorPrefsIds.SEARCH_LIST_DO_SEARCHING}[{searchListIndex}]";

                EditorPrefs.SetString(keyString, ConsoleEditorPrefs._quickSearchContextList[searchListIndex].SearchString);
                EditorPrefs.SetString(keyIsFiltering, ConsoleEditorPrefs._quickSearchContextList[searchListIndex].DoSearching.ToString());
            }
        }

        public static void WriteColorPrefs()
        {
            string colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.BackgroundColor);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.BACKGROUND_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.TextColor);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.TEXT_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogViewBackground1Color);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.LOG_VIEW_BACKGROUND1_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogViewBackground2Color);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.LOG_VIEW_BACKGROUND2_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogViewSelectedBackgroundColor);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.LOG_VIEW_SELECTED_BACKGROUND_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogTextColor);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.LOG_TEXT_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.WarningTextColor);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.WARNING_TEXT_COLOR, colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.ErrorTextColor);
            EditorPrefs.SetString(ConsoleEditorPrefsIds.ERROR_TEXT_COLOR, colorString);
        }

        private static void ReadColorPrefs()
        {
            string colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.BackgroundColor);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.BACKGROUND_COLOR, colorString);
            ConsoleEditorPrefs.BackgroundColor = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.TextColor);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.TEXT_COLOR, colorString);
            ConsoleEditorPrefs.TextColor = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogViewBackground1Color);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.LOG_VIEW_BACKGROUND1_COLOR, colorString);
            ConsoleEditorPrefs.LogViewBackground1Color = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogViewBackground2Color);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.LOG_VIEW_BACKGROUND2_COLOR, colorString);
            ConsoleEditorPrefs.LogViewBackground2Color = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogViewSelectedBackgroundColor);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.LOG_VIEW_SELECTED_BACKGROUND_COLOR, colorString);
            ConsoleEditorPrefs.LogViewSelectedBackgroundColor = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.LogTextColor);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.LOG_TEXT_COLOR, colorString);
            ConsoleEditorPrefs.LogTextColor = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.WarningTextColor);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.WARNING_TEXT_COLOR, colorString);
            ConsoleEditorPrefs.WarningTextColor = ConsoleEditorPrefs._StringToColor32(colorString);

            colorString = ConsoleEditorPrefs._Color32ToString(ConsoleEditorPrefs.ErrorTextColor);
            colorString = EditorPrefs.GetString(ConsoleEditorPrefsIds.ERROR_TEXT_COLOR, colorString);
            ConsoleEditorPrefs.ErrorTextColor = ConsoleEditorPrefs._StringToColor32(colorString);

            SGuiStyle.RequestUpdateColors = true;
        }

        public static void SetFlags(ConsoleEditorPrefsFlags consoleEditorPrefsFlags_, ConsoleEditorPrefsFlags masks_)
        {
            ConsoleEditorPrefsFlags onFlags = consoleEditorPrefsFlags_ & masks_;
            ConsoleEditorPrefsFlags offFlags = (~consoleEditorPrefsFlags_ & masks_);

            ConsoleEditorPrefs._consoleEditorPrefsFlags |= onFlags;
            ConsoleEditorPrefs._consoleEditorPrefsFlags &= ~offFlags;
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.OPTION_FLAG, (int)ConsoleEditorPrefs._consoleEditorPrefsFlags);
        }

        public static bool GetFlagState(ConsoleEditorPrefsFlags mask_)
        {
            if (mask_ == (ConsoleEditorPrefs._consoleEditorPrefsFlags & mask_))
                return true;

            return false;
        }

        public static void SetProfileRefreshTimeS(float timeSeconds_)
        {
            const float MIN_INTERVAL_TIME_SECONDS = 1f;
            ConsoleEditorPrefs.ProfileRefreshIntervalTimeS = Mathf.Max(timeSeconds_, MIN_INTERVAL_TIME_SECONDS);
            EditorPrefs.SetFloat(ConsoleEditorPrefsIds.PROFILE_REFRESH_INTERVAL_TIME_S, ConsoleEditorPrefs.ProfileRefreshIntervalTimeS);
        }

        public static void SetSkipStackFrameCount(UInt32 count_)
        {
            ConsoleEditorPrefs.SkipStackFrameCount = count_;
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.SKIP_STACK_FRAME_COUNT, (int)ConsoleEditorPrefs.SkipStackFrameCount);
        }

        public static void SetLogDirectoryPathA(string directoryPathA_)
        {
            ConsoleEditorPrefs.LogDirectoryAbsolutePath = directoryPathA_;
            EditorPrefs.SetString(ConsoleEditorPrefsIds.LOG_DIRECTORY_PATH_A, ConsoleEditorPrefs.LogDirectoryAbsolutePath);
        }

        public static void SetLogFileType(LogFileManager.FileType logFileType_)
        {
            ConsoleEditorPrefs.LogFileType = logFileType_;
            EditorPrefs.SetInt(ConsoleEditorPrefsIds.LOG_FILE_TYPE, (int)ConsoleEditorPrefs.LogFileType);
        }

        public static void AddQuickSearchString(string searchString_, bool writePrefs_ = true)
        {
            if (true == string.IsNullOrEmpty(searchString_))
                return;

            foreach (ConsoleEditorPrefsSearchContext context in ConsoleEditorPrefs._quickSearchContextList)
            {
                if (true == context.SearchString.Equals(searchString_))
                    return;
            }

            ConsoleEditorPrefsSearchContext tempConsoleEditorPrefsSearchContext = new ConsoleEditorPrefsSearchContext();
            tempConsoleEditorPrefsSearchContext.SearchString = searchString_;
            tempConsoleEditorPrefsSearchContext.DoSearching = false;
            ConsoleEditorPrefs._quickSearchContextList.Add(tempConsoleEditorPrefsSearchContext);
            ConsoleEditorPrefs._quickSearchContextList.Sort();

            if (writePrefs_)
                WriteSearchStringListPrefs();
        }

        public static void RemoveQuickSearchString(string searchString_)
        {
            foreach (ConsoleEditorPrefsSearchContext context in ConsoleEditorPrefs._quickSearchContextList)
            {
                if (true == context.SearchString.Equals(searchString_))
                {
                    ConsoleEditorPrefs._quickSearchContextList.Remove(context);
                    WriteSearchStringListPrefs();
                    return;
                }
            }
        }

        public static ConsoleEditorPrefsSearchContext GetQuickSearchContext(int filterIndex_)
        {
            if (ConsoleEditorPrefs._quickSearchContextList.Count > filterIndex_)
                return ConsoleEditorPrefs._quickSearchContextList[filterIndex_];

            return null;
        }

        public static int GetQuickSearchListCount()
        {
            return ConsoleEditorPrefs._quickSearchContextList.Count;
        }

        public static void ClearQuickSearchDataCount()
        {
            foreach (ConsoleEditorPrefsSearchContext data in ConsoleEditorPrefs._quickSearchContextList)
            {
                data.SearchCount = 0;
            }
        }

        public static bool IsEnableCheckQuickSearchList()
        {
            foreach (ConsoleEditorPrefsSearchContext data in ConsoleEditorPrefs._quickSearchContextList)
            {
                if (null != data && data.DoSearching)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ResetDefaultColors()
        {
            if (true == UnityEditor.EditorGUIUtility.isProSkin)
            {
                ConsoleEditorPrefs.BackgroundColor = Color.black;
                ConsoleEditorPrefs.TextColor = new Color32(255, 255, 255, 128);
                ConsoleEditorPrefs.LogViewBackground1Color = Color.black;
                ConsoleEditorPrefs.LogViewBackground2Color = Color.grey;
                ConsoleEditorPrefs.LogViewSelectedBackgroundColor = new Color32(62, 91, 144, 255);
                ConsoleEditorPrefs.LogTextColor = Color.white;
                ConsoleEditorPrefs.WarningTextColor = Color.yellow;
                ConsoleEditorPrefs.ErrorTextColor = Color.red;
            }
            else
            {
                ConsoleEditorPrefs.BackgroundColor = new Color32(194, 194, 194, 255);
                ConsoleEditorPrefs.TextColor = Color.black;
                ConsoleEditorPrefs.LogViewBackground1Color = new Color32(230, 230, 230, 230);
                ConsoleEditorPrefs.LogViewBackground2Color = new Color32(216, 216, 216, 255);
                ConsoleEditorPrefs.LogViewSelectedBackgroundColor = new Color32(62, 91, 144, 255);
                ConsoleEditorPrefs.LogTextColor = Color.black;
                ConsoleEditorPrefs.WarningTextColor = Color.yellow;
                ConsoleEditorPrefs.ErrorTextColor = Color.red;
            }

            SGuiStyle.RequestUpdateColors = true;
        }

        private static Color32 _StringToColor32(string colorString_)
        {
            byte[] byteArray = { 0, 0, 0, 0 };
            string[] byteStringArray = colorString_.Split(',');
            for (int index = 0; index < byteStringArray.Length; index++)
            {
                byte.TryParse(byteStringArray[index], out byteArray[index]);
            }

            return new Color32(byteArray[0], byteArray[1], byteArray[2], byteArray[3]);
        }

        private static string _Color32ToString(Color32 color_)
        {
            return $"{color_.r},{color_.g},{color_.b},{color_.a}";
        }
    }
}