// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin
{
    internal class PreferencesContext
    {
        public float ProfileRefreshIntervalTimeSeconds { get; set; }
        public UInt32 SkipStackFrameCount { get; set; }
        public bool ShowUnityDebugLog { get; set; }
    }
}