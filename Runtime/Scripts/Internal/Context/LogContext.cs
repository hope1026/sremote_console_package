// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEngine;

namespace SPlugin
{
    internal class LogContext
    {
        public LogType LogType { get; set; }
        public string LogString { get; set; }
        public string LogStackTrace { get; set; }
        public float TimeSeconds { get; set; }
        public Int32 FrameCount { get; set; }
        public Int32 ObjectInstanceID { get; set; }
        public string ObjectName { get; set; }
    }
}