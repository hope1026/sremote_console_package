// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SPlugin
{
    public static class SDebug
    {
        internal static SConsoleMain ConsoleMain { get; } = new SConsoleMain();

        public static void Log(string logMessage_)
        {
            ConsoleMain.SendLog(LogType.Log, logMessage_, unityObject_: null);
            UnityEngine.Debug.Log("log");
        }

        public static void Log(string logMessage_, Object context_)
        {
            ConsoleMain.SendLog(LogType.Log, logMessage_, context_);
        }

        public static void LogWarning(string logMessage_)
        {
            ConsoleMain.SendLog(LogType.Warning, logMessage_, unityObject_: null);
        }

        public static void LogWarning(string logMessage_, Object object_)
        {
            ConsoleMain.SendLog(LogType.Warning, logMessage_, object_);
        }

        public static void Assert(bool condition_)
        {
            if (condition_ == false)
                ConsoleMain.SendLog(LogType.Assert, logMessage_: string.Empty, unityObject_: null);
        }

        public static void Assert(bool condition_, string logMessage_)
        {
            if (condition_ == false)
                ConsoleMain.SendLog(LogType.Assert, logMessage_, unityObject_: null);
        }

        public static void Assert(bool condition_, string logMessage_, Object object_)
        {
            if (condition_ == false)
                ConsoleMain.SendLog(LogType.Assert, logMessage_, object_);
        }

        public static void LogError(string logMessage_)
        {
            ConsoleMain.SendLog(LogType.Error, logMessage_, unityObject_: null);
        }

        public static void LogError(string logMessage_, Object object_)
        {
            ConsoleMain.SendLog(LogType.Error, logMessage_, object_);
        }

        public static void LogException(Exception exception_)
        {
            ConsoleMain.SendLogException(exception_, unityObject_: null);
        }

        public static void LogException(Exception exception_, Object object_)
        {
            ConsoleMain.SendLogException(exception_, object_);
        }

        [RuntimeInitializeOnLoadMethod]
        internal static void RuntimeInitializeOnLoadMethod()
        {
            ConsoleMain.StartIfNotStarted();
        }
    }
}