// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;

namespace SPlugin
{
    internal static class RemoteConsoleInternalLog
    {
        private static readonly Action<LogType, string> _delegateOnLog = null;

        internal static void Log(LogType logType_, string log_)
        {
            switch (logType_)
            {
                case LogType.Log:
                {
                    Log(log_);
                    break;
                }
                case LogType.Warning:
                {
                    Warning(log_);
                    break;
                }
                case LogType.Error:
                {
                    Error(log_);
                    break;
                }
                default:
                {
                    Error(log_);
                    break;
                }
            }
        }

        internal static void Log(string log_)
        {
            if (null != _delegateOnLog)
            {
                _delegateOnLog(LogType.Log, log_);
            }
        }

        internal static void Warning(string log_)
        {
            if (null != _delegateOnLog)
            {
                _delegateOnLog(LogType.Warning, log_);
            }
        }

        internal static void Error(string log_)
        {
            if (null != _delegateOnLog)
            {
                _delegateOnLog(LogType.Error, log_);
            }
        }

        internal static void Exception(Exception exception_)
        {
            if (null != _delegateOnLog)
            {
                _delegateOnLog(LogType.Exception, exception_.ToString());
            }
        }
    }
}