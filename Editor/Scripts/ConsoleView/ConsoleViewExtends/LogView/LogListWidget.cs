// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SPlugin
{
    internal class LogListWidget
    {
        private readonly SGuiDragLine _stackTraceDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _iconDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _timeDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _frameCountDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _objectNameDragLine = new SGuiDragLine();

        private LogView _ownerLogView = null;
        private float _scrollValue = 0f;
        private bool _isBottomFixedScrollBar = true;

        private LogItem _selectedLogItemData = null;
        private bool _drawLogItemMenuPopup = false;
        private int _itemCountByDrawableArea = 0;

        private int _viewStartIndex = 0;
        private int _viewLastIndex = 0;
        private float _logItemDrawStartPosY = 0.0f;

        private LogItem.Stack _selectedStackData = null;
        private bool _drawStackItemMenuPopup = false;
        private float _stackItemDrawStartOffsetPosY = 0f;
        private bool _pressedMouseLeftButtonOnStackTraceRect = false;

        public void Initialize(LogView ownerLogView_)
        {
            _ownerLogView = ownerLogView_;
        }

        public void OnGuiCustom()
        {
            if (_ownerLogView == null || _ownerLogView.CurrentAppRef == null)
                return;

            GUILayout.BeginArea(ConsoleViewLayoutDefines.LogListWidget.areaRect);
            OnGuiLogList();
            OnGuiTitle();
            OnGuiVerticalDragLine();
            OnGuiStack();
            GUILayout.EndArea();
        }

        public void UpdateCustom()
        {
            if (_ownerLogView == null || _ownerLogView.CurrentAppRef == null)
                return;

            _itemCountByDrawableArea = Mathf.FloorToInt(ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.height / ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT);
            CheckScrollValueMax();
        }

        private void CheckScrollValueMax()
        {
            if (true == _isBottomFixedScrollBar && true == CanShowVerticalScrollBar())
            {
                _scrollValue = _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _itemCountByDrawableArea;
            }
        }

        private void OnGuiTitle()
        {
            GUI.Box(ConsoleViewLayoutDefines.LogListWidget.Area.areaTitleRect, "", SGuiStyle.BoxStyle);

            Rect titleRect = new Rect(ConsoleViewLayoutDefines.LogListWidget.Area.areaTitleRect);
            float drewWidth = titleRect.xMin;
            string context = "FrameCount";
            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME))
            {
                titleRect.xMin = drewWidth;
                titleRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth;

                context = "Time(S)";
                context = SGuiUtility.ReplaceBoldString(context);
                GUI.Label(titleRect, context, SGuiStyle.BoxStyle);
                drewWidth += titleRect.width;
            }

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT))
            {
                titleRect.xMin = drewWidth;
                titleRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth;

                context = "FrameCount";
                context = SGuiUtility.ReplaceBoldString(context);
                GUI.Label(titleRect, context, SGuiStyle.BoxStyle);
                drewWidth += titleRect.width;
            }

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME))
            {
                titleRect.xMin = drewWidth;
                titleRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth;

                context = "ObjectName";
                context = SGuiUtility.ReplaceBoldString(context);
                GUI.Label(titleRect, context, SGuiStyle.BoxStyle);
                drewWidth += titleRect.width;
            }

            titleRect.xMin = drewWidth;
            titleRect.width = ConsoleViewLayoutDefines.LogListWidget.areaRect.width - drewWidth;

            context = "SendLog";
            context = SGuiUtility.ReplaceBoldString(context);
            GUI.Label(titleRect, context, SGuiStyle.BoxStyle);
        }

        private void OnGuiVerticalDragLine()
        {
            SGuiStyle.BoxStyle.normal.background = EditorGUIUtility.whiteTexture;
            Color lineColor = new Color(0.15f, 0.15f, 0.15f);

            Rect titleRect = new Rect(ConsoleViewLayoutDefines.LogListWidget.Area.areaTitleRect);
            Rect dragLineRect = new Rect(ConsoleViewLayoutDefines.LogListWidget.Area.areaTitleRect);
            dragLineRect.width = 1f;
            dragLineRect.height = ConsoleViewLayoutDefines.LogListWidget.areaRect.height - ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect.height;

            float drewWidth = titleRect.xMin;
            Vector2 mousePosition = Vector2.zero;
            Rect collisionOffset = new Rect(-5f, 0f, 10f, 0f);
            _iconDragLine.OnGuiCustom(ref mousePosition, dragLineRect, collisionOffset, lineColor, SGuiStyle.BoxStyle, true);

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME))
            {
                titleRect.x = drewWidth;
                titleRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth;
                dragLineRect.x = titleRect.xMax;

                if (true == _timeDragLine.OnGuiCustom(ref mousePosition, dragLineRect, collisionOffset, lineColor, SGuiStyle.BoxStyle, true))
                {
                    ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth = Mathf.Max(40f, mousePosition.x - drewWidth);
                }

                drewWidth += titleRect.width;
            }

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT))
            {
                titleRect.x = drewWidth;
                titleRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth;
                dragLineRect.x = titleRect.xMax;

                if (true == _frameCountDragLine.OnGuiCustom(ref mousePosition, dragLineRect, collisionOffset, lineColor, SGuiStyle.BoxStyle, true))
                {
                    ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth = Mathf.Max(40f, mousePosition.x - drewWidth);
                }

                drewWidth += titleRect.width;
            }

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME))
            {
                titleRect.x = drewWidth;
                titleRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth;
                dragLineRect.x = titleRect.xMax;

                if (true == _objectNameDragLine.OnGuiCustom(ref mousePosition, dragLineRect, collisionOffset, lineColor, SGuiStyle.BoxStyle, true))
                {
                    ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth = Mathf.Max(40f, mousePosition.x - drewWidth);
                }

                drewWidth += titleRect.width;
            }

            SGuiStyle.BoxStyle.normal.background = GUI.skin.box.normal.background;
        }

        private void OnGuiLogList()
        {
            GUILayout.BeginArea(ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect);

            CheckScrollMouseWheel();
            OnGuiLogListItems();
            OnGuiLogListVerticalScroll();

            GUILayout.EndArea();
        }

        private void OnGuiLogListItems()
        {
            int showAbleItemTotalCount = _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount();
            if (true == CanShowVerticalScrollBar())
            {
                if (true == _isBottomFixedScrollBar)
                {
                    _viewLastIndex = showAbleItemTotalCount - 1;
                    _viewStartIndex = Mathf.Max(0, _viewLastIndex - _itemCountByDrawableArea);
                    _logItemDrawStartPosY = -(_scrollValue - (float)_viewStartIndex) * ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
                }
            }
            else
            {
                _viewStartIndex = 0;
                _viewLastIndex = showAbleItemTotalCount - 1;
                _logItemDrawStartPosY = 0f;
            }

            float drawStartPosY = _logItemDrawStartPosY;

            Rect drawRect = new Rect();
            for (int index = _viewStartIndex; index <= _viewLastIndex; index++)
            {
                if (true == CanShowVerticalScrollBar())
                {
                    drawRect.Set(0.0f, drawStartPosY, ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.width - ConsoleViewLayoutDefines.VERTICAL_SCROLL_WIDTH,
                                 ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT);
                }
                else
                {
                    drawRect.Set(0.0f, drawStartPosY, ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.width,
                                 ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT);
                }

                if (true == drawRect.Contains(Event.current.mousePosition))
                {
                    if (EventType.MouseDown == Event.current.type)
                    {
                        _selectedLogItemData = _ownerLogView.CurrentAppRef.logCollection.GetFilteredItem(index);
                        ClickItem(_selectedLogItemData);
                        if (null != _selectedLogItemData && 2 == Event.current.clickCount && Event.current.button == 0)
                        {
                            OpenStackTraceFile(_selectedLogItemData.FilePath, _selectedLogItemData.LineNumber);
                        }
                    }

                    if (EventType.MouseUp == Event.current.type && Event.current.button == 1)
                    {
                        _drawLogItemMenuPopup = true;
                    }
                }

                OnGuiLogItem(index, drawRect);
                drawStartPosY += ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
            }

            if (true == _drawLogItemMenuPopup)
            {
                if (null != _selectedLogItemData)
                {
                    GenericMenu logItemMenu = new GenericMenu();
                    logItemMenu.AddItem(new GUIContent("Show Log"), false, OnLogItemMenuShowLogHandler);
                    logItemMenu.AddItem(new GUIContent("Copy dLog"), false, OnLogItemMenuCopyHandler);
                    if (true == File.Exists(_selectedLogItemData.FilePath))
                    {
                        logItemMenu.AddItem(new GUIContent("Open Source File"), false, OnLogItemMenuOpenSourceFileHandler);
                    }

                    logItemMenu.ShowAsContext();
                }

                _drawLogItemMenuPopup = false;
            }
        }

        private void OnGuiLogItem(int itemIndex_, Rect drawRect_)
        {
            LogItem tempLogItem = _ownerLogView.CurrentAppRef.logCollection.GetFilteredItem(itemIndex_);
            if (null == tempLogItem)
                return;

            if (tempLogItem != _selectedLogItemData)
            {
                SGuiUtility.BeginBackgroundColor((itemIndex_ % 2 == 0 ? ConsoleEditorPrefs.LogViewBackground1Color : ConsoleEditorPrefs.LogViewBackground2Color));
            }
            else
            {
                SGuiUtility.BeginBackgroundColor(ConsoleEditorPrefs.LogViewSelectedBackgroundColor);
                SGuiStyle.BoxStyle.normal.background = EditorGUIUtility.whiteTexture;
            }

            GUI.Box(drawRect_, "", SGuiStyle.BoxStyle);
            SGuiStyle.BoxStyle.normal.background = GUI.skin.box.normal.background;
            SGuiUtility.EndBackgroundColor();

            string tempString = tempLogItem.LogData;
            if (1000 < tempString.Length)
            {
                tempString = tempString.Remove(1000);
            }

            foreach (string searchString in tempLogItem.ContainSearchStringList)
            {
                tempString = tempString.Replace(searchString, $"<size=15><b>{searchString}</b></size>");
            }

            switch (tempLogItem.LogType)
            {
                case LogType.Log:
                {
                    tempString = SGuiUtility.ReplaceColorString(tempString, ConsoleEditorPrefs.LogTextColor);
                    break;
                }
                case LogType.Warning:
                {
                    tempString = SGuiUtility.ReplaceColorString(tempString, ConsoleEditorPrefs.WarningTextColor);
                    break;
                }
                case LogType.Error:
                {
                    tempString = SGuiUtility.ReplaceColorString(tempString, ConsoleEditorPrefs.ErrorTextColor);
                    break;
                }
            }

            Rect tempRect = new Rect(drawRect_);
            float drewWidth = tempRect.xMin;

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME))
            {
                SGuiStyle.LabelStyle.alignment = TextAnchor.MiddleCenter;
                tempRect.x = drewWidth;
                tempRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.timeWidth;
                EditorGUI.LabelField(tempRect, tempLogItem.TimeSeconds.ToString(CultureInfo.InvariantCulture), SGuiStyle.LabelStyle);
                drewWidth += tempRect.width;
            }

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT))
            {
                SGuiStyle.LabelStyle.alignment = TextAnchor.MiddleCenter;
                tempRect.x = drewWidth;
                tempRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.frameCountWidth;
                EditorGUI.LabelField(tempRect, tempLogItem.FrameCount.ToString(), SGuiStyle.LabelStyle);
                drewWidth += tempRect.width;
            }

            if (true == ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME))
            {
                SGuiStyle.LabelStyle.alignment = TextAnchor.MiddleCenter;
                tempRect.x = drewWidth;
                tempRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.objectNameWidth;
                EditorGUI.LabelField(tempRect, tempLogItem.ObjectName, SGuiStyle.LabelStyle);
                drewWidth += tempRect.width;
            }

            SGuiStyle.LabelStyle.alignment = TextAnchor.UpperLeft;
            SGuiStyle.LabelStyle.wordWrap = true;
            tempRect.xMin = drewWidth;
            tempRect.width = drawRect_.width - drewWidth;

            EditorGUI.LabelField(tempRect, tempString, SGuiStyle.LabelStyle);
            drewWidth += tempRect.width;

            if (tempLogItem.CollapseCount > 0)
            {
                SGuiStyle.HorizontalScrollbarThumbStyle.alignment = TextAnchor.MiddleCenter;
                tempRect.x = drewWidth - ConsoleViewLayoutDefines.LogListWidget.Area.ICON_WIDTH - 10f;
                tempRect.width = ConsoleViewLayoutDefines.LogListWidget.Area.ICON_WIDTH + 10f;
                string context = tempLogItem.CollapseCount.ToString();
                EditorGUI.LabelField(tempRect, context, SGuiStyle.HorizontalScrollbarThumbStyle);
            }
        }

        private void OnLogItemMenuShowLogHandler()
        {
            ExtendLogEditorWindow window = EditorWindow.GetWindow<ExtendLogEditorWindow>();
            window.ShowUtility();
            window.position.Set(ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.center.x, ConsoleViewLayoutDefines.LogViewToolbarWidget.areaRect.yMax, window.position.width, window.position.height);
            window.SetLogItem(_selectedLogItemData);
        }

        private void OnLogItemMenuCopyHandler()
        {
            if (null != _selectedLogItemData)
            {
                EditorGUIUtility.systemCopyBuffer = _selectedLogItemData.LogData;
            }
        }

        private void OnLogItemMenuOpenSourceFileHandler()
        {
            if (null != _selectedLogItemData)
            {
                OpenStackTraceFile(_selectedLogItemData.FilePath, _selectedLogItemData.LineNumber);
            }
        }

        private void OpenStackTraceFile(string filePath_, int lineNumber_)
        {
            if (true == Application.isEditor)
            {
                const string OPENER_KEY = "kScriptsDefaultApp";
                const string OPENER_EXE = "UnityVS.OpenFile.exe";

                string openerName = EditorPrefs.GetString(OPENER_KEY);
                if (false == openerName.EndsWith(OPENER_EXE))
                {
                    InternalEditorUtility.OpenFileAtLineExternal(filePath_, lineNumber_, 0);
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.Arguments = (filePath_ + " " + (lineNumber_ - 1)).ToString(CultureInfo.InvariantCulture);
                    startInfo.FileName = openerName;
                    Process.Start(startInfo);
                }
            }
        }

        private void ClickItem(LogItem logItem_)
        {
            if (null == logItem_)
                return;

            EditorGUIUtility.PingObject(logItem_.ObjectInstanceID);
            _stackItemDrawStartOffsetPosY = 0f;
        }

        private void OnGuiLogListVerticalScroll()
        {
            if (true == CanShowVerticalScrollBar())
            {
                float oldValue = _scrollValue;
                _scrollValue = GUI.VerticalScrollbar(ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.scrollVerticalRect,
                                                     _scrollValue, _itemCountByDrawableArea, 0, _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount());

                if (float.Epsilon < Math.Abs(oldValue - _scrollValue))
                {
                    if (_scrollValue >= _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _itemCountByDrawableArea)
                    {
                        SetBottomFixedScrollBar(true);
                    }
                    else
                    {
                        SetBottomFixedScrollBar(false);
                    }
                }
            }
            else
            {
                SetBottomFixedScrollBar(true);
            }
        }

        private void CheckScrollMouseWheel()
        {
            if (true == CanShowVerticalScrollBar() &&
                EventType.ScrollWheel == Event.current.type &&
                true == ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.Contains(Event.current.mousePosition))
            {
                _scrollValue += Event.current.delta.y;
                _scrollValue = Mathf.Max(_scrollValue, 0f);
                _scrollValue = Mathf.Min(_scrollValue, _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _itemCountByDrawableArea);
                if (_scrollValue >= _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _itemCountByDrawableArea)
                {
                    SetBottomFixedScrollBar(true);
                }
                else
                {
                    SetBottomFixedScrollBar(false);
                }
            }
        }

        public void SetBottomFixedScrollBar(bool fix_)
        {
            _isBottomFixedScrollBar = fix_;
            if (false == _isBottomFixedScrollBar)
            {
                _viewStartIndex = (int)_scrollValue;
                _viewLastIndex = _viewStartIndex + _itemCountByDrawableArea;
                _logItemDrawStartPosY = -(_scrollValue - (float)_viewStartIndex) * ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
            }
            else
            {
                _scrollValue = (int)_ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _itemCountByDrawableArea;
                _scrollValue = Mathf.Max(_scrollValue, 0f);
                _viewLastIndex = _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - 1;
                _viewStartIndex = Mathf.Max(0, _viewLastIndex - _itemCountByDrawableArea);
                _logItemDrawStartPosY = -(_scrollValue - (float)_viewStartIndex) * ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
            }
        }

        private void OnGuiStack()
        {
            OnGuiStackTraceLineBox();
            EditorGUIUtility.AddCursorRect(ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect, MouseCursor.Text);
            OnGuiStackTrace(_selectedLogItemData, ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect);
        }

        private void OnGuiStackTrace(LogItem logItem_, Rect stackTraceDrawRect_)
        {
            if (null == logItem_)
                return;

            if (logItem_.StackList.Count == 0)
                return;

            List<Rect> stackDrawRectList = new List<Rect>();
            float offsetStackItemY = _stackItemDrawStartOffsetPosY;
            float needTotalHeight = 0f;
            int startFrameIndex = -1;
            for (int stackFrameIndex = 0; stackFrameIndex < logItem_.StackList.Count; stackFrameIndex++)
            {
                GUIContent stackItemContent = new GUIContent(logItem_.StackList[stackFrameIndex].StackString);
                float minX = stackTraceDrawRect_.x;
                float minY = stackTraceDrawRect_.y + offsetStackItemY;
                float width = stackTraceDrawRect_.width;
                float height = SGuiStyle.LabelStyle.CalcHeight(stackItemContent, width);
                Rect stackItemDrawRect = new Rect(minX, minY, width, height);
                if (offsetStackItemY >= 0f && -1 == startFrameIndex)
                {
                    startFrameIndex = stackFrameIndex;
                }

                stackDrawRectList.Add(stackItemDrawRect);
                offsetStackItemY += height;
                needTotalHeight += height;
            }

            startFrameIndex = Mathf.Clamp(startFrameIndex, 0, logItem_.StackList.Count);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && true == stackTraceDrawRect_.Contains(Event.current.mousePosition))
            {
                _pressedMouseLeftButtonOnStackTraceRect = true;
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                _pressedMouseLeftButtonOnStackTraceRect = false;
            }

            if (Event.current.type == EventType.MouseDrag && true == _pressedMouseLeftButtonOnStackTraceRect)
            {
                if (stackTraceDrawRect_.height >= needTotalHeight)
                {
                    _stackItemDrawStartOffsetPosY = 0f;
                }
                else
                {
                    _stackItemDrawStartOffsetPosY -= Event.current.delta.y;
                    float posMin = stackTraceDrawRect_.height - needTotalHeight;
                    _stackItemDrawStartOffsetPosY = Mathf.Clamp(_stackItemDrawStartOffsetPosY, posMin, 0f);
                }
            }

            for (int stackFrameIndex = startFrameIndex; stackFrameIndex < logItem_.StackList.Count; stackFrameIndex++)
            {
                if (true == stackDrawRectList[stackFrameIndex].Contains(Event.current.mousePosition))
                {
                    SGuiUtility.BeginBackgroundColor(new Color32(62, 91, 144, 255));
                    SGuiStyle.BoxStyle.normal.background = EditorGUIUtility.whiteTexture;
                    GUI.Box(stackDrawRectList[stackFrameIndex], "", SGuiStyle.BoxStyle);
                    SGuiStyle.BoxStyle.normal.background = GUI.skin.box.normal.background;
                    SGuiUtility.EndBackgroundColor();

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && 2 == Event.current.clickCount)
                    {
                        OpenStackTraceFile(logItem_.StackList[stackFrameIndex].FilePath, logItem_.StackList[stackFrameIndex].LineNumber);
                    }
                    else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                    {
                        _selectedStackData = logItem_.StackList[stackFrameIndex];
                        _drawStackItemMenuPopup = true;
                    }
                }

                GUIContent stackItemContent = new GUIContent(logItem_.StackList[stackFrameIndex].StackString);
                SGuiStyle.LabelStyle.wordWrap = true;
                GUI.Label(stackDrawRectList[stackFrameIndex], stackItemContent, SGuiStyle.LabelStyle);
            }

            if (true == _drawStackItemMenuPopup && null != _selectedStackData)
            {
                GenericMenu stackItemMenu = new GenericMenu();
                if (true == File.Exists(_selectedStackData.FilePath))
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
            if (null != _selectedLogItemData)
            {
                EditorGUIUtility.systemCopyBuffer = _selectedLogItemData.StackString;
            }
        }

        private void OnStackItemMenuCopySelectedHandler()
        {
            if (null != _selectedStackData)
            {
                EditorGUIUtility.systemCopyBuffer = _selectedStackData.StackString;
            }
        }

        private void OnStackItemMenuOpenSourceFileHandler()
        {
            if (null != _selectedStackData)
            {
                OpenStackTraceFile(_selectedStackData.FilePath, _selectedStackData.LineNumber);
            }
        }

        private void OnGuiStackTraceLineBox()
        {
            Rect lineRect = new Rect(ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect);
            lineRect.height = 1.0f;

            if (null != _stackTraceDragLine)
            {
                Vector2 mousePosition = Vector2.zero;
                Rect collisionOffset = new Rect(0f, -5f, -ConsoleViewLayoutDefines.VERTICAL_SCROLL_WIDTH, 10f);
                if (true == _stackTraceDragLine.OnGuiCustom(ref mousePosition, lineRect, collisionOffset, Color.black, SGuiStyle.BoxStyle, false))
                {
                    ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRatio = mousePosition.y / (ConsoleViewLayoutDefines.LogListWidget.areaRect.height);
                    float oldStackTraceHeight = ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect.height;
                    ConsoleViewLayoutDefines.LogListWidget.OnChangeWindowSize();

                    float diffHeight = ConsoleViewLayoutDefines.LogListWidget.Area.areaStackTraceRect.height - oldStackTraceHeight;
                    if (_stackItemDrawStartOffsetPosY < 0f)
                    {
                        _stackItemDrawStartOffsetPosY += diffHeight;
                        _stackItemDrawStartOffsetPosY = Mathf.Min(_stackItemDrawStartOffsetPosY, 0f);
                    }
                }
            }
        }

        private bool CanShowVerticalScrollBar()
        {
            if (_ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() > _itemCountByDrawableArea)
                return true;

            return false;
        }
    }
}