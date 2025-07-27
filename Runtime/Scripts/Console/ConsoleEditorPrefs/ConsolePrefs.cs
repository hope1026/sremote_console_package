// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace SPlugin.RemoteConsole.Runtime
{
    internal static class ConsolePrefs
    {
        private static ConsolePrefsFlags _consolePrefsFlags = ConsolePrefsFlags.NONE;
        public static bool CanClearLog { get; set; }
        public static string SearchString { get; set; }
        public static float ProfileRefreshIntervalTimeS { get; private set; }
        public static UInt32 SkipStackFrameCount { get; private set; }
        public static string LogDirectoryAbsolutePath { get; set; }
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
            const ConsolePrefsFlags DEFAULT_FLAGS = ConsolePrefsFlags.SHOW_LOG | ConsolePrefsFlags.SHOW_LOG | ConsolePrefsFlags.SHOW_WARNING |
                                                    ConsolePrefsFlags.SHOW_ERROR | ConsolePrefsFlags.SHOW_TIME | ConsolePrefsFlags.SHOW_FRAME_COUNT |
                                                    ConsolePrefsFlags.SHOW_OBJECT_NAME | ConsolePrefsFlags.SHOW_FONT_EFFECT | ConsolePrefsFlags.SHOW_UNITY_DEBUG_LOG;
            ConsolePrefs._consolePrefsFlags = (ConsolePrefsFlags)EditorPrefs.GetInt(ConsolePrefsIds.OPTION_FLAG, (int)DEFAULT_FLAGS);
            ConsolePrefs._consolePrefsFlags |= ConsolePrefsFlags.SHOW_FONT_EFFECT;
            ConsolePrefs._consolePrefsFlags |= ConsolePrefsFlags.SHOW_SYSTEM_MESSAGE;

            ConsolePrefs.ProfileRefreshIntervalTimeS = EditorPrefs.GetFloat(ConsolePrefsIds.PROFILE_REFRESH_INTERVAL_TIME_S, 10f);
            ConsolePrefs.SkipStackFrameCount = (UInt32)Mathf.Max(0, EditorPrefs.GetInt(ConsolePrefsIds.SKIP_STACK_FRAME_COUNT, 0));
            if (false == Directory.Exists(ConsolePrefs.LogDirectoryAbsolutePath) && false == string.IsNullOrEmpty(ConsolePrefs.LogDirectoryAbsolutePath))
            {
                Directory.CreateDirectory(ConsolePrefs.LogDirectoryAbsolutePath);
            }

            ConsolePrefs.ResetDefaultColors();
        }

        public static void WritePrefs()
        {
            EditorPrefs.SetInt(ConsolePrefsIds.OPTION_FLAG, (int)ConsolePrefs._consolePrefsFlags);
            EditorPrefs.SetFloat(ConsolePrefsIds.PROFILE_REFRESH_INTERVAL_TIME_S, ConsolePrefs.ProfileRefreshIntervalTimeS);
            EditorPrefs.SetInt(ConsolePrefsIds.SKIP_STACK_FRAME_COUNT, (int)ConsolePrefs.SkipStackFrameCount);
        }

        public static void SetFlags(ConsolePrefsFlags consolePrefsFlags_, ConsolePrefsFlags masks_)
        {
            ConsolePrefsFlags onFlags = consolePrefsFlags_ & masks_;
            ConsolePrefsFlags offFlags = (~consolePrefsFlags_ & masks_);

            ConsolePrefs._consolePrefsFlags |= onFlags;
            ConsolePrefs._consolePrefsFlags &= ~offFlags;
            EditorPrefs.SetInt(ConsolePrefsIds.OPTION_FLAG, (int)ConsolePrefs._consolePrefsFlags);
        }

        public static bool GetFlagState(ConsolePrefsFlags mask_)
        {
            if (mask_ == (ConsolePrefs._consolePrefsFlags & mask_))
                return true;

            return false;
        }

        public static void SetProfileRefreshTimeS(float timeSeconds_)
        {
            const float MIN_INTERVAL_TIME_SECONDS = 1f;
            ConsolePrefs.ProfileRefreshIntervalTimeS = Mathf.Max(timeSeconds_, MIN_INTERVAL_TIME_SECONDS);
            EditorPrefs.SetFloat(ConsolePrefsIds.PROFILE_REFRESH_INTERVAL_TIME_S, ConsolePrefs.ProfileRefreshIntervalTimeS);
        }

        public static void SetSkipStackFrameCount(UInt32 count_)
        {
            ConsolePrefs.SkipStackFrameCount = count_;
            EditorPrefs.SetInt(ConsolePrefsIds.SKIP_STACK_FRAME_COUNT, (int)ConsolePrefs.SkipStackFrameCount);
        }

        public static void ResetDefaultColors()
        {
            if (true == UnityEditor.EditorGUIUtility.isProSkin)
            {
                Color32 backgroundColor = new Color32(56, 56, 56, 255); // Dark theme background color

                ConsolePrefs.BackgroundColor = backgroundColor;
                ConsolePrefs.TextColor = new Color32(255, 255, 255, 128);
                // One row same as background, other row slightly darker
                ConsolePrefs.LogViewBackground1Color = backgroundColor;              // Same as background
                ConsolePrefs.LogViewBackground2Color = new Color32(48, 48, 48, 255); // Slightly darker
                ConsolePrefs.LogViewSelectedBackgroundColor = new Color32(62, 91, 144, 255);
                ConsolePrefs.LogTextColor = Color.white;
                ConsolePrefs.WarningTextColor = Color.yellow;
                ConsolePrefs.ErrorTextColor = Color.red;
            }
            else
            {
                Color32 backgroundColor = new Color32(194, 194, 194, 255); // Light theme background color

                ConsolePrefs.BackgroundColor = backgroundColor;
                ConsolePrefs.TextColor = Color.black;
                // One row same as background, other row slightly darker
                ConsolePrefs.LogViewBackground1Color = backgroundColor;                 // Same as background
                ConsolePrefs.LogViewBackground2Color = new Color32(180, 180, 180, 255); // Slightly darker
                ConsolePrefs.LogViewSelectedBackgroundColor = new Color32(62, 91, 144, 255);
                ConsolePrefs.LogTextColor = Color.black;
                ConsolePrefs.WarningTextColor = Color.yellow;
                ConsolePrefs.ErrorTextColor = Color.red;
            }
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