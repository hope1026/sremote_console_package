// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class LogListWidget
    {
        private readonly SGuiDragLine _iconDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _timeDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _frameCountDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _objectNameDragLine = new SGuiDragLine();
        private readonly SGuiDragLine _stackTraceDragLine = new SGuiDragLine();

        private LogView _ownerLogView = null;
        private float _scrollValue = 0f;
        private bool _isBottomFixedScrollBar = true;

        private LogItem _selectedLogItem = null;
        private bool _drawLogItemMenuPopup = false;
        private int _drawableLogItemCount = 0;

        private int _logItemStartIndex = 0;
        private int _logItemLastIndex = 0;
        private float _logItemDrawStartPosY = 0.0f;

        private readonly LogStackWidget _logStackWidget = new LogStackWidget();

        public void Initialize(LogView ownerLogView_)
        {
            _ownerLogView = ownerLogView_;
        }

        public void OnGuiCustom()
        {
            if (_ownerLogView == null || _ownerLogView.CurrentAppRef == null)
                return;

            GUILayout.BeginArea(ConsoleViewLayoutDefines.LogListWidget.areaRect);
            _logStackWidget.OnGuiCustom();
            OnGuiVerticalDragLine();
            OnGuiStackTraceLineBox();
            OnGuiLogList();
            OnGuiTitle();
            GUILayout.EndArea();
        }

        public void UpdateCustom()
        {
            if (_ownerLogView == null || _ownerLogView.CurrentAppRef == null)
                return;

            _drawableLogItemCount = Mathf.FloorToInt(ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.height / ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT);
            if (true == _isBottomFixedScrollBar && true == CanShowVerticalScrollBarOfLogList())
            {
                _scrollValue = _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _drawableLogItemCount;
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
                    ConsoleViewLayoutDefines.LogListWidget.OnChangeWindowSize();
                }
            }
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
            if (true == CanShowVerticalScrollBarOfLogList())
            {
                if (true == _isBottomFixedScrollBar)
                {
                    _logItemLastIndex = showAbleItemTotalCount - 1;
                    _logItemStartIndex = Mathf.Max(0, _logItemLastIndex - _drawableLogItemCount);
                    _logItemDrawStartPosY = -(_scrollValue - (float)_logItemStartIndex) * ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
                }
            }
            else
            {
                _logItemStartIndex = 0;
                _logItemLastIndex = showAbleItemTotalCount - 1;
                _logItemDrawStartPosY = 0f;
            }

            float drawStartPosY = _logItemDrawStartPosY;

            Rect drawRect = new Rect();
            for (int index = _logItemStartIndex; index <= _logItemLastIndex; index++)
            {
                if (true == CanShowVerticalScrollBarOfLogList())
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
                        _selectedLogItem = _ownerLogView.CurrentAppRef.logCollection.GetFilteredItem(index);
                        _logStackWidget.ChangeSelectedLogItem(_selectedLogItem);
                        ClickItem(_selectedLogItem);
                        if (null != _selectedLogItem && 2 == Event.current.clickCount && Event.current.button == 0)
                        {
                            LogItem.OpenStackTraceFile(_selectedLogItem.FilePath, _selectedLogItem.LineNumber);
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
                if (null != _selectedLogItem)
                {
                    GenericMenu logItemMenu = new GenericMenu();
                    logItemMenu.AddItem(new GUIContent("Show Log"), false, OnLogItemMenuShowLogHandler);
                    logItemMenu.AddItem(new GUIContent("Copy Log"), false, OnLogItemMenuCopyHandler);
                    if (true == File.Exists(_selectedLogItem.FilePath))
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

            if (tempLogItem != _selectedLogItem)
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
            window.SetLogItem(_selectedLogItem);
        }

        private void OnLogItemMenuCopyHandler()
        {
            if (null != _selectedLogItem)
            {
                EditorGUIUtility.systemCopyBuffer = _selectedLogItem.LogData;
            }
        }

        private void OnLogItemMenuOpenSourceFileHandler()
        {
            if (null != _selectedLogItem)
            {
                LogItem.OpenStackTraceFile(_selectedLogItem.FilePath, _selectedLogItem.LineNumber);
            }
        }

        private void ClickItem(LogItem logItem_)
        {
            if (null == logItem_)
                return;

            EditorGUIUtility.PingObject(logItem_.ObjectInstanceID);
        }

        private void OnGuiLogListVerticalScroll()
        {
            if (true == CanShowVerticalScrollBarOfLogList())
            {
                float oldValue = _scrollValue;
                _scrollValue = GUI.VerticalScrollbar(ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.scrollVerticalRect,
                                                     _scrollValue, _drawableLogItemCount, 0, _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount());

                if (float.Epsilon < Math.Abs(oldValue - _scrollValue))
                {
                    if (_scrollValue >= _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _drawableLogItemCount)
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
            if (true == CanShowVerticalScrollBarOfLogList() &&
                EventType.ScrollWheel == Event.current.type &&
                true == ConsoleViewLayoutDefines.LogListWidget.Area.logListAreaRect.Contains(Event.current.mousePosition))
            {
                _scrollValue += Event.current.delta.y;
                _scrollValue = Mathf.Max(_scrollValue, 0f);
                _scrollValue = Mathf.Min(_scrollValue, _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _drawableLogItemCount);
                if (_scrollValue >= _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _drawableLogItemCount)
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
                _logItemStartIndex = (int)_scrollValue;
                _logItemLastIndex = _logItemStartIndex + _drawableLogItemCount;
                _logItemDrawStartPosY = -(_scrollValue - (float)_logItemStartIndex) * ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
            }
            else
            {
                _scrollValue = (int)_ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - _drawableLogItemCount;
                _scrollValue = Mathf.Max(_scrollValue, 0f);
                _logItemLastIndex = _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() - 1;
                _logItemStartIndex = Mathf.Max(0, _logItemLastIndex - _drawableLogItemCount);
                _logItemDrawStartPosY = -(_scrollValue - (float)_logItemStartIndex) * ConsoleViewLayoutDefines.LogListWidget.Area.AreaItemList.ITEM_HEIGHT;
            }
        }

        private bool CanShowVerticalScrollBarOfLogList()
        {
            if (_ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount() > _drawableLogItemCount)
                return true;

            return false;
        }
    }
}