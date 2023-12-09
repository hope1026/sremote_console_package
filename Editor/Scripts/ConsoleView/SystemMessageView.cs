// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using UnityEngine;

namespace SPlugin
{
    internal class SystemMessageView
    {
        private readonly List<LogItem> _systemLogList = new List<LogItem>();
        private readonly List<LogItem> _drawLogList = new List<LogItem>();
        private bool _changedLogList = false;
        private Rect _drawRect = new Rect();
        private bool _showAble = false;

        public void OnGuiCustom()
        {
            if (true == _showAble)
            {
                GUI.FocusWindow(ConsoleViewLayoutDefines.WindowID.SYSTEM_MESSAGE_VIEW);
                _drawRect.Set(ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.xMin + (ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.width * 0.1f),
                              ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.yMin + (ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.height * 0.2f),
                              ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.xMax - (ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.width * 0.2f),
                              ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.yMax - (ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.height * 0.4f));
                GUILayout.Window(ConsoleViewLayoutDefines.WindowID.SYSTEM_MESSAGE_VIEW, _drawRect,
                                 _HandleOnGuiLogListWindow, ConsoleViewNameDefines.Window.SYSTEM_MESSAGE);
            }
        }

        public void UpdateCustom()
        {
            if (true == _changedLogList)
            {
                lock (_systemLogList)
                {
                    _drawLogList.AddRange(_systemLogList);
                    _systemLogList.Clear();
                    _showAble = true;
                    _changedLogList = false;
                }
            }
        }

        private void _HandleOnGuiLogListWindow(int windowID_)
        {
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
            {
                int count = _drawLogList.Count;
                for (int index = 0; index < count; index++)
                {
                    GUILayout.Label(new GUIContent(_drawLogList[index].LogData), SGuiStyle.LabelStyle);
                }
            }

            if (Event.current.type == EventType.MouseUp && _drawRect.Contains(Event.current.mousePosition))
            {
                _showAble = false;
                _drawLogList.Clear();
            }
        }

        public void AddSystemLogData(LogItem logData_)
        {
            if (false == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_SYSTEM_MESSAGE))
                return;

            switch (logData_.LogType)
            {
                case LogType.Log:
                {
                    logData_.LogData = SGuiUtility.ReplaceColorString(logData_.LogData, ConsoleEditorPrefs.LogTextColor);
                }
                    break;
                case LogType.Warning:
                {
                    logData_.LogData = SGuiUtility.ReplaceColorString(logData_.LogData, ConsoleEditorPrefs.WarningTextColor);
                }
                    break;
                case LogType.Error:
                {
                    logData_.LogData = SGuiUtility.ReplaceColorString(logData_.LogData, ConsoleEditorPrefs.ErrorTextColor);
                }
                    break;
            }

            lock (_systemLogList)
            {
                _systemLogList.Add(logData_);
                _changedLogList = true;
            }
        }
    }
}