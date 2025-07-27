// 
// Copyright 2015 https://github.com/hope1026

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Editor
{
    internal class LogListWidget
    {
        private ListView _logList;
        private readonly List<LogItem> _displayedLogs = new List<LogItem>();
        private LogItem _selectedLogItem;
        private ConsoleViewAbstract _ownerLogView;

        // Column resizing
        private Label _timeHeader;
        private Label _frameHeader;
        private Label _objectHeader;
        private VisualElement _timeHeaderSeparator;
        private VisualElement _frameHeaderSeparator;
        private VisualElement _objectHeaderSeparator;
        private ColumnResizeManipulator _timeHeaderResizer;
        private ColumnResizeManipulator _frameHeaderResizer;
        private ColumnResizeManipulator _objectHeaderResizer;
        private int _lastFilteredCount = -1;
        public event Action<LogItem> OnLogItemSelected;

        public void Initialize(VisualElement rootElement_, ConsoleViewAbstract ownerLogView_)
        {
            _ownerLogView = ownerLogView_;

            // Get UI elements
            _logList = rootElement_.Q<ListView>("log-list");
            _timeHeader = rootElement_.Q<Label>("time-header");
            _frameHeader = rootElement_.Q<Label>("frame-header");
            _objectHeader = rootElement_.Q<Label>("object-header");
            _timeHeaderSeparator = rootElement_.Q<VisualElement>("time-separator");
            _frameHeaderSeparator = rootElement_.Q<VisualElement>("frame-separator");
            _objectHeaderSeparator = rootElement_.Q<VisualElement>("object-separator");

            SetupLogList();
            SetupHeaderColumnResizing();
        }

        private void SetupLogList()
        {
            if (_logList == null)
            {
                Debug.LogError("LogList is null!");
                return;
            }

            // Configure ListView
            _logList.makeItem = MakeLogItem;
            _logList.bindItem = BindLogItem;
            _logList.unbindItem = UnbindLogItem;
            _logList.selectionChanged += OnLogSelectionChanged;
            _logList.selectionType = UnityEngine.UIElements.SelectionType.Single;
            _logList.showAlternatingRowBackgrounds = UnityEngine.UIElements.AlternatingRowBackground.ContentOnly;

            // Use DynamicHeight for text wrapping and variable height items
            _logList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            // Ensure list is visible and properly sized
            _logList.style.flexGrow = 1;
            _logList.style.flexShrink = 1;
            _logList.style.minHeight = 100;

            // Set initial data source
            _logList.itemsSource = _displayedLogs;
        }

        private void SetupHeaderColumnResizing()
        {
            // Setup resizing for Time column
            if (_timeHeaderSeparator != null && _timeHeader != null && _frameHeader != null)
            {
                _timeHeaderResizer = new ColumnResizeManipulator(_timeHeader, _frameHeader, "time", 40f);
                _timeHeaderResizer.OnColumnResized += OnColumnResized;
                _timeHeaderSeparator.AddManipulator(_timeHeaderResizer);
            }

            // Setup resizing for Frame column
            if (_frameHeaderSeparator != null && _frameHeader != null && _objectHeader != null)
            {
                _frameHeaderResizer = new ColumnResizeManipulator(_frameHeader, _objectHeader, "frame", 40f);
                _frameHeaderResizer.OnColumnResized += OnColumnResized;
                _frameHeaderSeparator.AddManipulator(_frameHeaderResizer);
            }

            // Setup resizing for Object column
            if (_objectHeaderSeparator != null && _objectHeader != null)
            {
                _objectHeaderResizer = new ColumnResizeManipulator(_objectHeader, null, "object", 40f);
                _objectHeaderResizer.OnColumnResized += OnColumnResized;
                _objectHeaderSeparator.AddManipulator(_objectHeaderResizer);
            }
        }

        private void OnColumnResized(string columnType_, float newWidth_)
        {
            // Update header column width to match
            switch (columnType_)
            {
                case "time":
                    if (_timeHeader != null)
                    {
                        _timeHeader.style.width = newWidth_;
                        _timeHeader.style.maxWidth = newWidth_;
                        _timeHeader.style.minWidth = newWidth_;
                    }
                    break;
                case "frame":
                    if (_frameHeader != null)
                    {
                        _frameHeader.style.width = newWidth_;
                        _frameHeader.style.maxWidth = newWidth_;
                        _frameHeader.style.minWidth = newWidth_;
                    }
                    break;
                case "object":
                    if (_objectHeader != null)
                    {
                        _objectHeader.style.width = newWidth_;
                        _objectHeader.style.maxWidth = newWidth_;
                        _objectHeader.style.minWidth = newWidth_;
                    }
                    break;
            }
        }

        public void RefreshLogList()
        {
            if (_logList == null) return;

            if (_ownerLogView.CurrentAppRef?.logCollection == null)
            {
                if (_displayedLogs.Count > 0)
                {
                    _displayedLogs.Clear();
                    _logList.RefreshItems();
                }
                return;
            }

            // Store the current selection
            LogItem previousSelection = _selectedLogItem;

            int currentFilteredCount = _ownerLogView.CurrentAppRef.logCollection.FilteredItemsCount();

            // Check if we need a full refresh (count changed significantly)
            bool needsFullRefresh = _lastFilteredCount != currentFilteredCount ||
                                    currentFilteredCount < _displayedLogs.Count;

            if (needsFullRefresh)
            {
                // Full refresh - clear and rebuild
                _displayedLogs.Clear();
                for (int i = 0; i < currentFilteredCount; i++)
                {
                    LogItem logItem = _ownerLogView.CurrentAppRef.logCollection.GetFilteredItem(i);
                    if (logItem != null)
                    {
                        _displayedLogs.Add(logItem);
                    }
                }

                _lastFilteredCount = currentFilteredCount;
                _logList.RefreshItems();
            }
            else if (currentFilteredCount > _displayedLogs.Count)
            {
                // Incremental update - only add new items
                for (int i = _displayedLogs.Count; i < currentFilteredCount; i++)
                {
                    LogItem logItem = _ownerLogView.CurrentAppRef.logCollection.GetFilteredItem(i);
                    if (logItem != null)
                    {
                        _displayedLogs.Add(logItem);
                    }
                }

                _lastFilteredCount = currentFilteredCount;
                _logList.RefreshItems();
            }

            // Restore selection if possible
            if (previousSelection != null && _displayedLogs.Contains(previousSelection))
            {
                int index = _displayedLogs.IndexOf(previousSelection);
                _logList.SetSelection(index);
            }
        }

        private VisualElement MakeLogItem()
        {
            VisualElement logItemElement = new VisualElement();
            logItemElement.AddToClassList("log-item");
            // Most styling moved to CSS - only essential structure remains

            // Time column - styling moved to CSS
            Label timeLabel = new Label();
            timeLabel.name = "time-label";
            logItemElement.Add(timeLabel);

            // Time separator - styling moved to CSS
            VisualElement timeSeparator = new VisualElement();
            timeSeparator.name = "time-separator";
            timeSeparator.AddToClassList("log-item-separator");
            logItemElement.Add(timeSeparator);

            // Frame column - styling moved to CSS
            Label frameLabel = new Label();
            frameLabel.name = "frame-label";

            // Add resize manipulator to time separator
            ColumnResizeManipulator timeItemResizer = new ColumnResizeManipulator(timeLabel, frameLabel, "time", 40f);
            timeItemResizer.OnColumnResized += OnColumnResized;
            timeSeparator.AddManipulator(timeItemResizer);
            logItemElement.Add(timeSeparator);

            logItemElement.Add(frameLabel);

            // Frame separator - styling moved to CSS
            VisualElement frameSeparator = new VisualElement();
            frameSeparator.name = "frame-separator";
            frameSeparator.AddToClassList("log-item-separator");
            logItemElement.Add(frameSeparator);

            // Object column - styling moved to CSS
            Label objectLabel = new Label();
            objectLabel.name = "object-label";

            // Add resize manipulator to frame separator (now we have objectLabel reference)
            ColumnResizeManipulator frameItemResizer = new ColumnResizeManipulator(frameLabel, objectLabel, "frame", 40f);
            frameItemResizer.OnColumnResized += OnColumnResized;
            frameSeparator.AddManipulator(frameItemResizer);

            logItemElement.Add(frameSeparator);
            logItemElement.Add(objectLabel);

            // Object separator - styling moved to CSS
            VisualElement objectSeparator = new VisualElement();
            objectSeparator.name = "object-separator";
            objectSeparator.AddToClassList("log-item-separator");

            // Add resize manipulator to object separator
            ColumnResizeManipulator objectItemResizer = new ColumnResizeManipulator(objectLabel, null, "object", 40f);
            objectItemResizer.OnColumnResized += OnColumnResized;
            objectSeparator.AddManipulator(objectItemResizer);

            logItemElement.Add(objectSeparator);

            // Message column - styling moved to CSS
            Label messageLabel = new Label();
            messageLabel.name = "message-label";
            logItemElement.Add(messageLabel);

            // Collapse count label - styling moved to CSS
            Label collapseLabel = new Label();
            collapseLabel.name = "collapse-label";
            logItemElement.Add(collapseLabel);

            return logItemElement;
        }

        private void UnbindLogItem(VisualElement element_, int index_)
        {
            // Clean up any event handlers to prevent memory leaks
            if (element_.userData != null)
            {
                element_.userData = null;
            }
        }

        private void BindLogItem(VisualElement element_, int index_)
        {
            if (index_ < 0 || index_ >= _displayedLogs.Count) return;

            LogItem logItem = _displayedLogs[index_];
            if (logItem == null) return;

            Label timeLabel = element_.Q<Label>("time-label");
            Label frameLabel = element_.Q<Label>("frame-label");
            Label objectLabel = element_.Q<Label>("object-label");
            Label messageLabel = element_.Q<Label>("message-label");
            Label collapseLabel = element_.Q<Label>("collapse-label");

            VisualElement timeSeparator = element_.Q<VisualElement>("time-separator");
            VisualElement frameSeparator = element_.Q<VisualElement>("frame-separator");
            VisualElement objectSeparator = element_.Q<VisualElement>("object-separator");

            // Update visibility based on preferences
            bool showTime = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME);
            bool showFrame = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT);
            bool showObject = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME);

            timeLabel.style.display = showTime ? DisplayStyle.Flex : DisplayStyle.None;
            frameLabel.style.display = showFrame ? DisplayStyle.Flex : DisplayStyle.None;
            objectLabel.style.display = showObject ? DisplayStyle.Flex : DisplayStyle.None;

            timeSeparator.style.display = showTime ? DisplayStyle.Flex : DisplayStyle.None;
            frameSeparator.style.display = showFrame ? DisplayStyle.Flex : DisplayStyle.None;
            objectSeparator.style.display = showObject ? DisplayStyle.Flex : DisplayStyle.None;

            // Sync column widths with headers
            SyncColumnWidths(timeLabel, frameLabel, objectLabel);

            // Set content
            if (showTime) timeLabel.text = logItem.TimeSeconds.ToString("F2");
            if (showFrame) frameLabel.text = logItem.FrameCount.ToString();
            if (showObject) objectLabel.text = logItem.ObjectName ?? "";

            // Process message
            string message = logItem.LogData ?? "";
            if (message.Length > 1000)
            {
                message = message.Substring(0, 1000);
            }

            // Highlight search strings
            if (logItem.ContainSearchStringList != null)
            {
                foreach (string searchString in logItem.ContainSearchStringList)
                {
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        message = message.Replace(searchString, $"<b><size=15>{searchString}</size></b>");
                    }
                }
            }

            messageLabel.text = message;

            // Apply color based on log type
            Color textColor = Color.white;
            switch (logItem.LogType)
            {
                case LogType.Log:
                    textColor = ConsoleEditorPrefs.TextColor;
                    break;
                case LogType.Warning:
                    textColor = ConsoleEditorPrefs.WarningTextColor;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    textColor = ConsoleEditorPrefs.ErrorTextColor;
                    break;
            }
            messageLabel.style.color = new StyleColor(textColor);

            // Handle collapse count
            if (logItem.CollapseCount > 0)
            {
                collapseLabel.text = logItem.CollapseCount.ToString();
                collapseLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                collapseLabel.style.display = DisplayStyle.None;
            }

            // Set background color based on selection and alternating rows
            if (logItem == _selectedLogItem)
            {
                element_.style.backgroundColor = new StyleColor(ConsoleEditorPrefs.LogViewSelectedBackgroundColor);
            }
            else
            {
                Color bgColor = (index_ % 2 == 0) ? ConsoleEditorPrefs.LogViewBackground1Color : ConsoleEditorPrefs.LogViewBackground2Color;
                element_.style.backgroundColor = new StyleColor(bgColor);
            }

            // Add context menu functionality
            SetupLogItemContextMenu(element_, logItem);
        }

        private void SyncColumnWidths(Label timeLabel_, Label frameLabel_, Label objectLabel_)
        {
            // Sync column widths with header widths
            if (_timeHeader != null && timeLabel_ != null)
            {
                float headerWidth = _timeHeader.resolvedStyle.width;
                if (headerWidth > 0)
                {
                    timeLabel_.style.width = headerWidth;
                    timeLabel_.style.maxWidth = headerWidth;
                    timeLabel_.style.minWidth = headerWidth;
                }
            }

            if (_frameHeader != null && frameLabel_ != null)
            {
                float headerWidth = _frameHeader.resolvedStyle.width;
                if (headerWidth > 0)
                {
                    frameLabel_.style.width = headerWidth;
                    frameLabel_.style.maxWidth = headerWidth;
                    frameLabel_.style.minWidth = headerWidth;
                }
            }

            if (_objectHeader != null && objectLabel_ != null)
            {
                float headerWidth = _objectHeader.resolvedStyle.width;
                if (headerWidth > 0)
                {
                    objectLabel_.style.width = headerWidth;
                    objectLabel_.style.maxWidth = headerWidth;
                    objectLabel_.style.minWidth = headerWidth;
                }
            }
        }

        private void OnLogSelectionChanged(IEnumerable<object> selectedItems_)
        {
            foreach (var item in selectedItems_)
            {
                // Handle both LogItem objects and indices
                LogItem logItem = null;
                if (item is LogItem directLogItem)
                {
                    logItem = directLogItem;
                }
                else if (item is int index && index >= 0 && index < _displayedLogs.Count)
                {
                    logItem = _displayedLogs[index];
                }

                if (logItem != null)
                {
                    _selectedLogItem = logItem;

                    // Notify parent about selection
                    OnLogItemSelected?.Invoke(logItem);

                    // Ping object in hierarchy
                    if (logItem.ObjectInstanceID != 0)
                    {
                        UnityEditor.EditorGUIUtility.PingObject(logItem.ObjectInstanceID);
                    }

                    // Refresh list to update selection highlighting
                    _logList?.RefreshItems();
                    break;
                }
            }
        }

        public void UpdateHeaderVisibility()
        {
            bool showTime = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_TIME);
            bool showFrame = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_FRAME_COUNT);
            bool showObject = ConsoleEditorPrefs.GetFlagState(ConsoleEditorPrefsFlags.SHOW_OBJECT_NAME);

            // Update header visibility using CSS classes
            if (_timeHeader != null)
            {
                if (showTime)
                    _timeHeader.RemoveFromClassList("hidden");
                else
                    _timeHeader.AddToClassList("hidden");
            }

            if (_frameHeader != null)
            {
                if (showFrame)
                    _frameHeader.RemoveFromClassList("hidden");
                else
                    _frameHeader.AddToClassList("hidden");
            }

            if (_objectHeader != null)
            {
                if (showObject)
                    _objectHeader.RemoveFromClassList("hidden");
                else
                    _objectHeader.AddToClassList("hidden");
            }

            // Update separator visibility using CSS classes
            if (_timeHeaderSeparator != null)
            {
                if (showTime)
                    _timeHeaderSeparator.RemoveFromClassList("hidden");
                else
                    _timeHeaderSeparator.AddToClassList("hidden");
            }

            if (_frameHeaderSeparator != null)
            {
                if (showFrame)
                    _frameHeaderSeparator.RemoveFromClassList("hidden");
                else
                    _frameHeaderSeparator.AddToClassList("hidden");
            }

            if (_objectHeaderSeparator != null)
            {
                if (showObject)
                    _objectHeaderSeparator.RemoveFromClassList("hidden");
                else
                    _objectHeaderSeparator.AddToClassList("hidden");
            }
        }

        public void ScrollToBottom()
        {
            if (_logList != null && _displayedLogs.Count > 0)
            {
                _logList.ScrollToItem(_displayedLogs.Count - 1);
            }
        }

        private void SetupLogItemContextMenu(VisualElement element_, LogItem logItem_)
        {
            // Store the log item reference for context menu
            element_.userData = logItem_;

            element_.RegisterCallback<MouseDownEvent>(OnLogItemMouseDown);
            element_.AddManipulator(
                new ContextualMenuManipulator(BuildLogItemMenu)
            );
            return;

            void OnLogItemMouseDown(MouseDownEvent evt_)
            {
                if (evt_.clickCount == 2 && evt_.button == 0)
                {
                    if ((evt_.target as VisualElement)?.userData is LogItem logItem)
                        OnLogItemMenuOpenSourceFile(logItem);
                }
            }

            void BuildLogItemMenu(ContextualMenuPopulateEvent evt_)
            {
                if ((evt_.target as VisualElement)?.userData is not LogItem logItem) return;

                evt_.menu.MenuItems().Clear();
                evt_.menu.AppendAction("Show Log", _ => OnLogItemMenuShowLog(logItem), DropdownMenuAction.AlwaysEnabled);
                evt_.menu.AppendAction("Copy Log", _ => OnLogItemMenuCopyLog(logItem), DropdownMenuAction.AlwaysEnabled);

                if (!string.IsNullOrEmpty(logItem.FilePath) && File.Exists(logItem.FilePath))
                    evt_.menu.AppendAction("Open Source File", _ => OnLogItemMenuOpenSourceFile(logItem), DropdownMenuAction.AlwaysEnabled);
            }
        }

        private void OnLogItemMenuShowLog(LogItem logItem_)
        {
            if (logItem_ == null) return;

            // Open ExtendLogEditorWindow - same as IMGUI version
            var window = EditorWindow.GetWindow<ExtendLogEditorWindow>();
            window.ShowUtility();

            // Position the window relative to the console window (approximate position)
            var consoleWindow = EditorWindow.focusedWindow;
            if (consoleWindow != null)
            {
                var consolePos = consoleWindow.position;
                var newPos = new Rect(
                    consolePos.center.x - 200, // Center horizontally relative to console
                    consolePos.y + 100,        // Offset from top
                    window.position.width,
                    window.position.height
                );
                window.position = newPos;
            }

            window.SetLogItem(logItem_);
        }

        private void OnLogItemMenuCopyLog(LogItem logItem_)
        {
            if (logItem_ == null) return;

            // Copy log data to system clipboard - same as IMGUI version
            EditorGUIUtility.systemCopyBuffer = logItem_.LogData;
        }

        private void OnLogItemMenuOpenSourceFile(LogItem logItem_)
        {
            if (logItem_ == null) return;

            // Open source file at specific line - same as IMGUI version
            LogItem.OpenStackTraceFile(logItem_.FilePath, logItem_.LineNumber);
        }

        public void Cleanup()
        {
            // Unsubscribe from events
            if (_timeHeaderResizer != null)
                _timeHeaderResizer.OnColumnResized -= OnColumnResized;
            if (_frameHeaderResizer != null)
                _frameHeaderResizer.OnColumnResized -= OnColumnResized;
            if (_objectHeaderResizer != null)
                _objectHeaderResizer.OnColumnResized -= OnColumnResized;
        }
    }
}