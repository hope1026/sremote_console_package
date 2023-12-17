// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SPlugin
{
    internal class LogStackWidget
    {
        private LogItem _selectedLogItem = null;
        private LogItem.StackContext _selectedStackContext = null;
        private bool _drawStackItemMenuPopup = false;
        private Vector2 _scrollPos;

        public void ChangeSelectedLogItem(LogItem logItem_)
        {
            _selectedLogItem = logItem_;
        }

        public void OnGuiCustom()
        {
            if (_selectedLogItem == null)
                return;

            UnityEngine.Event currentEvent = Event.current;
            GUILayout.BeginArea(ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            OnGuiStackTrace(_selectedLogItem);

            EditorGUILayout.EndScrollView();

            OnGuiContextMenuIfAble();

            GUILayout.EndArea();
        }

        private void OnGuiStackTrace(LogItem logItem_)
        {
            if (null == logItem_ || logItem_.StackList.Count <= 0)
                return;

            SGuiStyle.StackTextStyle.richText = true;
            foreach (LogItem.StackContext stackContext in logItem_.StackList)
            {
                EditorGUILayout.LabelField(stackContext.DisplayStackString, SGuiStyle.StackTextStyle);
                Rect labelRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0 && 2 == Event.current.clickCount &&
                    labelRect.Contains(Event.current.mousePosition))
                {
                    LogItem.OpenStackTraceFile(stackContext.FilePath, stackContext.LineNumber);
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 1 &&
                         labelRect.Contains(Event.current.mousePosition))
                {
                    _selectedStackContext = stackContext;
                    _drawStackItemMenuPopup = true;
                }
            }
        }

        private void OnGuiContextMenuIfAble()
        {
            if (true == _drawStackItemMenuPopup && null != _selectedStackContext)
            {
                GenericMenu stackItemMenu = new GenericMenu();
                if (true == File.Exists(_selectedStackContext.FilePath))
                {
                    stackItemMenu.AddItem(new GUIContent("Open Source File"), false, OnStackItemMenuOpenSourceFileHandler);
                }
                else
                {
                    stackItemMenu.AddDisabledItem(new GUIContent("Open Source File"));
                }

                stackItemMenu.AddItem(new GUIContent("Copy Selected"), false, OnStackItemMenuCopySelectedHandler);
                stackItemMenu.AddItem(new GUIContent("Copy All"), false, OnStackItemMenuCopyAllHandler);

                stackItemMenu.ShowAsContext();
                _drawStackItemMenuPopup = false;
            }
        }

        private void OnStackItemMenuCopyAllHandler()
        {
            if (null != _selectedLogItem)
            {
                EditorGUIUtility.systemCopyBuffer = _selectedLogItem.StackString;
            }
        }

        private void OnStackItemMenuCopySelectedHandler()
        {
            if (null != _selectedStackContext)
            {
                EditorGUIUtility.systemCopyBuffer = _selectedStackContext.originalStackString;
            }
        }

        private void OnStackItemMenuOpenSourceFileHandler()
        {
            if (null != _selectedStackContext)
            {
                LogItem.OpenStackTraceFile(_selectedStackContext.FilePath, _selectedStackContext.LineNumber);
            }
        }
    }
}