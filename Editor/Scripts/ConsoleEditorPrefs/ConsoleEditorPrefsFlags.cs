// 
// Copyright 2015 https://github.com/hope1026

using System;

namespace SPlugin.RemoteConsole.Editor
{
    [Flags]
    internal enum ConsoleEditorPrefsFlags
    {
        NONE /*------------------*/ = 0,
        SHOW_LOG /*--------------*/ = 1 << 0,
        SHOW_WARNING /*----------*/ = 1 << 1,
        SHOW_ERROR /*------------*/ = 1 << 2,
        SHOW_TIME /*-------------*/ = 1 << 3,
        SHOW_FRAME_COUNT /*------*/ = 1 << 4,
        SHOW_OBJECT_NAME /*------*/ = 1 << 5,
        SHOW_UNITY_DEBUG_LOG /*--*/ = 1 << 6,
        IS_CLEAR_ON_PLAY /*------*/ = 1 << 10,
        IS_COLLAPSE /*-----------*/ = 1 << 11,
        ERROR_PAUSE /*-----------*/ = 1 << 12,
        SHOW_FONT_EFFECT /*------*/ = 1 << 24,
        SHOW_SYSTEM_MESSAGE /*---*/ = 1 << 25,
    }
}