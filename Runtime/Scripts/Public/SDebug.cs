// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Diagnostics;
using SPlugin.RemoteConsole.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SPlugin
{
    public static class SDebug
    {
#if !DISABLE_SREMOTE_CONSOLE
        internal static SConsoleMain ConsoleMain { get; } = new SConsoleMain();
#endif

        public static void Log(string logMessage_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLog(LogType.Log, logMessage_, unityObject_: null);
#endif
        }

        public static void Log(string logMessage_, Object context_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLog(LogType.Log, logMessage_, context_);
#endif
        }

        public static void LogWarning(string logMessage_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLog(LogType.Warning, logMessage_, unityObject_: null);
#endif
        }

        public static void LogWarning(string logMessage_, Object object_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLog(LogType.Warning, logMessage_, object_);
#endif
        }

        public static void Assert(bool condition_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            if (condition_ == false)
                ConsoleMain.SendLog(LogType.Assert, logMessage_: string.Empty, unityObject_: null);
#endif
        }

        public static void Assert(bool condition_, string logMessage_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            if (condition_ == false)
                ConsoleMain.SendLog(LogType.Assert, logMessage_, unityObject_: null);
#endif
        }

        public static void Assert(bool condition_, string logMessage_, Object object_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            if (condition_ == false)
                ConsoleMain.SendLog(LogType.Assert, logMessage_, object_);
#endif
        }

        public static void LogError(string logMessage_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLog(LogType.Error, logMessage_, unityObject_: null);
#endif
        }

        public static void LogError(string logMessage_, Object object_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLog(LogType.Error, logMessage_, object_);
#endif
        }

        public static void LogException(Exception exception_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLogException(exception_, unityObject_: null);
#endif
        }

        public static void LogException(Exception exception_, Object object_)
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.SendLogException(exception_, object_);
#endif
        }
        
        public static void OpenConsole(int sortingOrder_ = Int32.MaxValue)
        {
#if !DISABLE_SREMOTE_CONSOLE
            SConsoleRuntimeMain.Instance.OpenConsole(sortingOrder_);
#endif
        }
        
        public static void CloseConsole()
        {
#if !DISABLE_SREMOTE_CONSOLE
            SConsoleRuntimeMain.Instance.CloseConsole();
#endif
        }

        [RuntimeInitializeOnLoadMethod]
        internal static void RuntimeInitializeOnLoadMethod()
        {
#if !DISABLE_SREMOTE_CONSOLE
            ConsoleMain.StartIfNotStarted();
#endif
        }
    }
}